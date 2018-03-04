using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dsci
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            contentDirectory.Items.AddRange(DsConfig.ContentDirectories);
            openFileDialog1.Filter += Properties.Settings.Default.ArchiveExtensions;
            Application.Idle += Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            installButton.Enabled = contentDirectory.SelectedIndex >= 0 && zipFiles.Text.Length > 0;
        }

        private void zipFilesButton_Click(object sender, EventArgs e)
        {
            zipFiles.Select(0, 0);
            if (DialogResult.OK == openFileDialog1.ShowDialog(this))
            {
                if (openFileDialog1.FileNames.Length == 1)
                {
                    zipFiles.Text = openFileDialog1.SafeFileName;
                }
                else
                {
                    // Limit the feedback length to workaround a strange behaviour of TextBox.
                    var remaining = 4000;
                    var names = openFileDialog1.SafeFileNames.TakeWhile(s => (remaining -= s.Length + 3) > 0).ToList();
                    var text = '"' + string.Join("\" \"", names) + '"';
                    if (names.Count < openFileDialog1.SafeFileNames.Length)
                    {
                        text = string.Format("{0} files: {1} ...", openFileDialog1.SafeFileNames.Length, text);
                    }
                    zipFiles.Text = text;
                }
            }
        }

        private void contentDirectory_SelectedIndexChanged(object sender, EventArgs e)
        {
            progress.Value = progress.Minimum;
        }

        private void zipFiles_TextChanged(object sender, EventArgs e)
        {
            progress.Value = progress.Minimum;
        }

        private void installButton_Click(object sender, EventArgs e)
        {
            AutoConfirms.Clear();
            progress.Value = progress.Minimum;
            var p = new WorkerParams();
            p.ContentDirectory = (string)contentDirectory.SelectedItem;
            p.ZipFiles = openFileDialog1.FileNames;
            backgroundWorker1.RunWorkerAsync(p);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var args = (WorkerParams)e.Argument;
            var progress = new Progress(0, 100, Installer_ProgressUpdated);
            var installer = new Installer();
            installer.ConfirmRequired += Installer_Confirm;
            installer.ContentDirectory = args.ContentDirectory;
            installer.Install(args.ZipFiles, progress);
        }

        private void Installer_ProgressUpdated(int progress)
        {
            backgroundWorker1.ReportProgress(progress, null);
        }

        private readonly Dictionary<Confirm, ConfirmResponse> AutoConfirms = new Dictionary<Confirm, ConfirmResponse>(); 

        private void Installer_Confirm(object sender, ConfirmEventArgs args)
        {
            bool dont;
            lock (AutoConfirms)
            {
                dont = AutoConfirms.TryGetValue(args.Confirm, out args.Response);
            }
            if (!dont)
            {
                Invoke((Action)delegate ()
                {
                    if (!AutoConfirms.TryGetValue(args.Confirm, out args.Response))
                    {
                        using (var dlg = new ConfirmDialog())
                        {
                            dlg.EventArgs = args;
                            dlg.ShowDialog(this);
                            if (dlg.DontAsk)
                            {
                                lock (AutoConfirms)
                                {
                                    AutoConfirms[args.Confirm] = args.Response;
                                }
                            }
                        }
                    }
                });
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progress.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
            }
            else if (e.Error is UserCancelException)
            {
                MessageBox.Show(this, "Cancelled.");
            }
            else
            {
                MessageBox.Show(this, e.Error.ToString(), "Exception");
            }
        }
    }

    class WorkerParams
    {
        public string ContentDirectory;
        public string[] ZipFiles;
    }
}
