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
                zipFiles.Text =
                    openFileDialog1.FileNames.Length > 1
                    ? '"' + string.Join("\", \"", openFileDialog1.SafeFileNames) + '"'
                    : openFileDialog1.SafeFileName;
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
            var p = new WorkerParams();
            p.ContentDirectory = (string)contentDirectory.SelectedItem;
            p.ZipFiles = openFileDialog1.FileNames;
            backgroundWorker1.RunWorkerAsync(p);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var args = (WorkerParams)e.Argument;
            var installer = new Installer();
            installer.ProgressChanged += Installer_ProgressChanged;
            installer.ConfirmRequired += Installer_Confirm;
            installer.Install(args.ContentDirectory, args.ZipFiles);
        }

        private void Installer_ProgressChanged(object sender, ProgressChangedEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void Installer_Confirm(object sender, ConfirmEventArgs args)
        {
            Invoke((Action)delegate ()
            {
                using (var dlg = new ConfirmDialog())
                {
                    dlg.EventArgs = args;
                    dlg.ShowDialog();
                }
            });
        }

        private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {

        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                MessageBox.Show(this, "Done.");
            }
            else if (e.Error is UserCancelException)
            {
                MessageBox.Show(this, "Cancelled.");
            }
            else
            {
                MessageBox.Show(this, e.ToString(), "Exception");
            }
        }
    }

    class WorkerParams
    {
        public string ContentDirectory;
        public string[] ZipFiles;
    }
}
