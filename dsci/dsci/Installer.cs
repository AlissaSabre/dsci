using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Globalization;
using System.ComponentModel;
using SevenZipExtractor;

namespace dsci
{
    public class Installer
    {
        private static readonly string[] TopDirectories = Properties.Settings.Default.Array<string>("TopDirectory");

        private static readonly string[] ParentDirectories = Properties.Settings.Default.Array<string>("ParentDirectory");

        private static readonly string[] Ignorables = Properties.Settings.Default.Array<string>("Ignorable");

        private static readonly Regex OtherFilesAliasRE =
            new Regex(Properties.Settings.Default.OtherFilesAliasRE, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly string[] ArchiveExtensions = Properties.Settings.Default.ArchiveExtensions.Split(new[] { ';', '*' }, StringSplitOptions.RemoveEmptyEntries);

        public event ConfirmEventHandler ConfirmRequired;

        public string ContentDirectory { get; set; }

        public void Install(string[] zip_files, Progress progress)
        {
            progress = progress.Divide(zip_files.Length);
            foreach (var filename in zip_files)
            {
                try
                {
                    using (var archive_stream = File.OpenRead(filename))
                    {
                        Install(archive_stream, filename, progress);
                    }
                }
                catch (IOException e)
                {
                    Ask(Confirm.IOError, ConfirmChoices.OkCancel, filename, e.Message);
                }
                progress.Advance();
            }
        }

        private static readonly char[] SEPARATORS = { '/', '\\' };

        private void Install(Stream archive_stream, string filename, Progress progress)
        {
            try
            {
                using (var archive = new ArchiveFile(archive_stream))
                {
                    progress = progress.Divide(archive.Entries.Count + 1);

                    var preambles = DetectPreambles(archive.Entries.Select(e => e.FileName), filename);
                    progress.Advance();

                    var contents = new List<Entry>();
                    var others = new List<Entry>();
                    int ignored = 0;
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.IsFolder || IsIgnorable(entry.FileName))
                        {
                            ++ignored;
                        }
                        else if (entry.Size > (ulong)Properties.Settings.Default.MaxFileLength)
                        {
                            var resp = Ask(Confirm.EntryTooLong, ConfirmChoices.YesNoCancel, filename, Path.GetFileName(entry.FileName));
                            if (resp == ConfirmResponse.No)
                            {
                                throw new ZipSkippedException();
                            }
                        }
                        else if (HasPreamble(entry.FileName, preambles))
                        {
                            contents.Add(entry);
                        }
                        else
                        {
                            others.Add(entry);
                        }
                    }

                    progress.Advance(ignored);

                    if (contents.Count > 0)
                    {
                        foreach (var entry in contents)
                        {
                            Extract(ContentDirectory, AdjustPath(entry.FileName, preambles), entry, filename, others);
                            progress.Advance();
                        }
                    }
                    else if (others.Any(e => IsArchive(e.FileName)))
                    {
                        foreach (var archive_entry in others.Where(e => IsArchive(e.FileName)).ToList())
                        {
                            var archive_entry_name = string.Format("{0}/{1}", filename, Path.GetFileName(archive_entry.FileName));
                            using (var stream = new MemoryStream())
                            {
                                archive_entry.Extract(stream);
                                stream.Seek(0, SeekOrigin.Begin);
                                Install(stream, archive_entry_name, progress);
                            }
                            others.Remove(archive_entry);
                            progress.Advance();
                        }
                    }
                    else
                    {
                        var resp = Ask(Confirm.NoCompatibleContents, ConfirmChoices.YesNoCancel, filename);
                        if (resp == ConfirmResponse.No)
                        {
                            throw new ZipSkippedException();
                        }
                    }

                    if (others.Count > 0)
                    {
                        var folder = Path.Combine(
                                ContentDirectory,
                                Properties.Settings.Default.OtherFilesDirectory,
                                string.Format("Other files from {0} ({1})",
                                    Path.GetFileNameWithoutExtension(filename),
                                    Path.GetExtension(filename).Substring(1)));
                        foreach (var entry in others)
                        {
                            Extract(folder, Path.GetFileName(entry.FileName), entry, filename, null);
                            progress.Advance();
                        }
                    }
                }

            }
            catch (ZipSkippedException)
            {
            }
            catch (IOException e)
            {
                Ask(Confirm.IOError, ConfirmChoices.OkCancel, filename, e.Message);
            }
            catch (SevenZipException e)
            {
                Ask(Confirm.ArchiveError, ConfirmChoices.OkCancel, filename, e.Message);
            }
        }

        private static bool IsIgnorable(string path)
        {
            return PathComponentRE.Matches(path).Cast<Match>().Select(m => FileNameEquals(m.Value)).Any(Ignorables.Any);
        }

        private static readonly Regex PathComponentRE = new Regex(@"[^/\\]+(?=[/\\])");

        private string[] DetectPreambles(IEnumerable<string> paths, string zip_filename)
        {
            var candidates1 = new SortedSet<string>(StringComparer.InvariantCultureIgnoreCase);
            var candidates2 = new SortedSet<string>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var path in paths)
            {
                if (path.StartsWith("/") || path.StartsWith("\\") || path.IndexOf(':') >= 0)
                {
                    Ask(Confirm.AbsolutePathInZip, ConfirmChoices.OkCancel, zip_filename);
                    throw new ZipSkippedException();
                }

                foreach (Match match in PathComponentRE.Matches(path))
                {
                    var pred = FileNameEquals(match.Value);
                    if (Ignorables.Any(pred))
                    {
                        break;
                    }
                    if (TopDirectories.Any(pred))
                    {
                        candidates1.Add(path.Substring(0, match.Index));
                        break;
                    }
                    if (ParentDirectories.Any(pred))
                    {
                        candidates2.Add(path.Substring(0, match.Index + match.Length + 1));
                    }
                }
            }

            if (candidates1.Count > 0)
            {
                // eliminate prefixed ones
                foreach (var s in candidates1.ToList())
                {
                    if (candidates1.Any(t => t.Length < s.Length && FileNameEquals(t, s.Substring(0, t.Length))))
                    {
                        candidates1.Remove(s);
                    }
                }
                return candidates1.ToArray();
            }
            if (candidates2.Count == 1)
            {
                return candidates2.ToArray();
            }
            return new string[0];
        }

        private bool HasPreamble(string path, string[] preambles)
        {
            foreach (var p in preambles)
            {
                // each preamble ends with a path separator char.
                // we don't want a file (perhaps readme.txt) be at a root of the content directory, so return false for the case.
                if (FileNamePrefix(p, path)) return path.IndexOfAny(SEPARATORS, p.Length) > 0;
            }
            return false;
        }

        private string GetLocalPath(string path, string[] preambles)
        {
            foreach (var p in preambles)
            {
                if (FileNamePrefix(p, path)) return path.Substring(p.Length);
            }
            // should never come here.
            throw new ApplicationException();
        }

        private static Func<string, bool> FileNameEquals(string name)
        {
            var n = name.Replace('\\', '/');
            return s => n.Equals(s.Replace('\\', '/'), StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool FileNameEquals(string s, string t)
        {
            return FileNameEquals(s)(t);
        }

        private static Func<string, bool> FileNamePrefix(string prefix)
        {
            var n = prefix.Replace('\\', '/');
            return s => s.Length >= n.Length && n.Equals(s.Substring(0, n.Length).Replace('\\', '/'), StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool FileNamePrefix(string prefix, string s)
        {
            return FileNamePrefix(prefix)(s);
        }

        private string AdjustPath(string fullname, string[] preambles)
        {
            var p = GetLocalPath(fullname, preambles);
            return OtherFilesAliasRE.Replace(p, Properties.Settings.Default.OtherFilesDirectory);
        }

        private bool IsArchive(string s)
        {
            var t = Path.GetExtension(s);
            foreach (var ext in ArchiveExtensions)
            {
                if (t.Equals(ext, StringComparison.InvariantCultureIgnoreCase)) return true;
            }
            return false;
        }

        private void Extract(string folder, string path, Entry entry, string zip_filename, List<Entry> others)
        {
            var extract_path = Path.Combine(folder, path);
            if (extract_path == path)
            {
                // The path was absolute.  It should never happen, since we have already checked the case.
                throw new InternalErrorException("Absolute archive member path in Extract.");
            }

            // Take care of the cases that the file exists at the extract_path.
            if (File.Exists(extract_path))
            {
                if (Identical(entry, extract_path, zip_filename)) return;

                if (others != null)
                {
                    // When extracting a content, the user gets a chance to extract the duplicate file as a "other" file.
                    var resp = Ask(Confirm.FileExists, ConfirmChoices.YesNoCancel, zip_filename, Path.GetFileName(entry.FileName));
                    if (resp != ConfirmResponse.Yes)
                    {
                        others.Add(entry);
                        return;
                    }
                }
                else
                {
                    // When extracting a "other" file, the options are "overwrite" and "discard".
                    var resp = Ask(Confirm.FileExists2, ConfirmChoices.YesNoCancel, zip_filename, Path.GetFileName(entry.FileName));
                    if (resp != ConfirmResponse.Yes)
                    {
                        return;
                    }
                }
            }

            // Extract the file, overwriting any existing file.
            try
            {
                entry.Extract(extract_path);
            }
            catch (Exception e)
            {
                var resp = Ask(Confirm.ExtractFailed, ConfirmChoices.YesNoCancel, zip_filename, Path.GetFileName(entry.FileName), e.Message);
                if (resp != ConfirmResponse.Yes)
                {
                    throw new ZipSkippedException("", e);
                }
            }
        }

        private bool Identical(Entry entry, string path, string zip_filename)
        {
            using (var s = File.OpenRead(path))
            {
                if ((long)entry.Size != s.Length) return false;
                var m = new ComparingOutputStream(s, true);
                try
                {
                    entry.Extract(m);
                }
                catch (Exception)
                {
                    return false;
                }
                return m.IsEqual;
            }
        }

        private ConfirmResponse Ask(Confirm confirm, ConfirmChoices choices, params object[] args)
        {
            var e = new ConfirmEventArgs()
            {
                Confirm = confirm,
                Args = args,
                Choices = choices,
                Response = ConfirmResponse.None,
            };
            ConfirmRequired(this, e);
            if (e.Response == ConfirmResponse.Cancel)
            {
                throw new UserCancelException();
            }
            return e.Response;
        }
    }
}
