using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Globalization;
using System.ComponentModel;

namespace dsci
{
    public class Installer
    {
        private static readonly string[] TopDirectories = Properties.Settings.Default.Array<string>("TopDirectory");

        private static readonly string[] ParentDirectories = Properties.Settings.Default.Array<string>("ParentDirectory");

        private static readonly string[] Ignorables = Properties.Settings.Default.Array<string>("Ignorable");

        public event ConfirmEventHandler ConfirmRequired;

        public void Install(string content_directory, string[] zip_files, Progress progress)
        {
            progress = progress.Divide(zip_files.Length);
            foreach (var file in zip_files)
            {
                try
                {
                    Install(content_directory, file, progress);
                }
                catch (ZipSkippedException)
                {
                }
                catch (IOException e)
                {
                    Ask(Confirm.IOError, ConfirmChoices.Ok, file, e.Message);
                }
                progress.Advance();
            }
        }

        private static readonly char[] SEPARATORS = { '/', '\\' };

        private void Install(string content_directory, string zip_filename, Progress progress)
        {
            using (var zip = new ZipArchive(File.OpenRead(zip_filename)))
            {
                progress = progress.Divide(zip.Entries.Count + 1);

                var preamble = DetectPreamble(zip, zip_filename);
                progress.Advance();

                var contents = new List<ZipArchiveEntry>();
                var others = new List<ZipArchiveEntry>();
                int ignored = 0;
                foreach (var entry in zip.Entries)
                {
                    if (entry.Name.Length == 0 ||
                        Ignorables.Any(n => 0 == string.Compare(n, entry.Name, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        ++ignored;
                    }
                    else if (preamble != null
                        && entry.FullName.StartsWith(preamble)
                        && entry.FullName.IndexOfAny(SEPARATORS, preamble.Length) >= 0)
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
                        Extract(content_directory, entry.FullName.Substring(preamble.Length), entry, zip_filename, others);
                        progress.Advance();
                    }
                }
                else
                {
                    var resp = Ask(Confirm.NoCompatibleContents, ConfirmChoices.YesNoCancel, zip_filename);
                    if (resp == ConfirmResponse.No)
                    {
                        throw new ZipSkippedException();
                    }
                }

                if (others.Count > 0)
                {
                    var folder = Path.Combine(
                        content_directory,
                        Properties.Settings.Default.OtherFilesDirectory,
                        "ZIP-" + Path.GetFileNameWithoutExtension(zip_filename));
                    Directory.CreateDirectory(folder);
                    foreach (var entry in others)
                    {
                        Extract(folder, entry.FullName, entry, zip_filename, null);
                        progress.Advance();
                    }
                }
            }
        }

        private static readonly Regex PathComponentRE = new Regex(@"[^/\\]+(?=[/\\])");

        private string DetectPreamble(ZipArchive zip, string zip_filename)
        {
            foreach (var entry in zip.Entries)
            {
                var path = entry.FullName;
                if (path.StartsWith("/") || path.StartsWith("\\") || path.IndexOf(':') >= 0)
                {
                    Ask(Confirm.AbsolutePathInZip, ConfirmChoices.OkCancel, zip_filename);
                    throw new ZipSkippedException();
                }

                foreach (Match match in PathComponentRE.Matches(path))
                {
                    var component = match.Value;
                    if (Ignorables.Any(e => 0 == string.Compare(e, component, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        continue;
                    }
                    if (ParentDirectories.Any(e => 0 == string.Compare(e, component, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        return path.Substring(0, match.Index + match.Length + 1);
                    }
                    if (TopDirectories.Any(e => 0 == string.Compare(e, component, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        return path.Substring(0, match.Index);
                    }
                }
            }
            return null;
        }

        private void Extract(string folder, string path, ZipArchiveEntry entry, string zip_filename, List<ZipArchiveEntry> others)
        {
            var extract_path = Path.Combine(folder, path);
            if (extract_path == path)
            {
                // The path was absolute.  It should never happen, since we have already checked the case.
                throw new InternalErrorException("Absolute ZIP path in Extract.");
            }

            // Take care of the cases that the file exists at the extract_path.
            if (File.Exists(extract_path))
            {
                if (Identical(entry, extract_path, zip_filename)) return;

                if (others != null)
                {
                    // When extracting a content, the user gets a chance to extract the duplicate file as a "other" file.
                    var resp = Ask(Confirm.FileExists, ConfirmChoices.YesNoCancel, zip_filename, entry.Name);
                    if (resp != ConfirmResponse.Yes)
                    {
                        others.Add(entry);
                        return;
                    }
                }
                else
                {
                    // When extracting a "other" file, the options are "overwrite" and "discard".
                    var resp = Ask(Confirm.FileExists2, ConfirmChoices.YesNoCancel, zip_filename, entry.Name);
                    if (resp != ConfirmResponse.Yes)
                    {
                        return;
                    }
                }
            }

            // Extract the file, overwriting any existing file.
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(extract_path));
                entry.ExtractToFile(extract_path, true);
            }
            catch (InvalidDataException e)
            {
                var resp = Ask(Confirm.InvalidZipData, ConfirmChoices.YesNoCancel, zip_filename, entry.Name);
                if (resp != ConfirmResponse.Yes)
                {
                    throw new ZipSkippedException("", e);
                }
            }
        }

        private bool Identical(ZipArchiveEntry entry, string path, string zip_filename)
        {
            try
            {
                using (Stream s1 = entry.Open(), s2 = File.OpenRead(path))
                {
                    // if (s1.Length != s2.Length) return false;
                    if (entry.Length != s2.Length) return false;
                    // Ah, no.  We should not do it this way...
                    for (;;)
                    {
                        var c1 = s1.ReadByte();
                        var c2 = s2.ReadByte();
                        if (c1 != c2) return false;
                        if (c1 == -1) return true;
                    }
                }
            }
            catch (InvalidDataException e)
            {
                var resp = Ask(Confirm.InvalidZipData, ConfirmChoices.YesNoCancel, zip_filename, entry.Name);
                if (resp != ConfirmResponse.Yes)
                {
                    throw new ZipSkippedException("", e);
                }
                // Returning true makes the caller to silently ignore this file.
                return true;
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
