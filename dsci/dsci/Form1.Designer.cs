namespace dsci
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.contentDirectory = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.zipFiles = new System.Windows.Forms.TextBox();
            this.installButton = new System.Windows.Forms.Button();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.zipFilesButton = new System.Windows.Forms.Button();
            this.progress = new System.Windows.Forms.ProgressBar();
            this.label3 = new System.Windows.Forms.Label();
            this.cancel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(105, 14);
            this.label1.TabIndex = 0;
            this.label1.Text = "Content Directory";
            // 
            // contentDirectory
            // 
            this.contentDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.contentDirectory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.contentDirectory.FormattingEnabled = true;
            this.contentDirectory.Location = new System.Drawing.Point(37, 28);
            this.contentDirectory.Name = "contentDirectory";
            this.contentDirectory.Size = new System.Drawing.Size(395, 22);
            this.contentDirectory.TabIndex = 1;
            this.contentDirectory.SelectedIndexChanged += new System.EventHandler(this.contentDirectory_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 73);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 14);
            this.label2.TabIndex = 2;
            this.label2.Text = "Content ZIP files";
            // 
            // zipFiles
            // 
            this.zipFiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.zipFiles.Location = new System.Drawing.Point(37, 91);
            this.zipFiles.Name = "zipFiles";
            this.zipFiles.ReadOnly = true;
            this.zipFiles.Size = new System.Drawing.Size(349, 22);
            this.zipFiles.TabIndex = 3;
            this.zipFiles.TextChanged += new System.EventHandler(this.zipFiles_TextChanged);
            // 
            // installButton
            // 
            this.installButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.installButton.Location = new System.Drawing.Point(345, 202);
            this.installButton.Name = "installButton";
            this.installButton.Size = new System.Drawing.Size(87, 27);
            this.installButton.TabIndex = 5;
            this.installButton.Text = "Install";
            this.installButton.UseVisualStyleBackColor = true;
            this.installButton.Click += new System.EventHandler(this.installButton_Click);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.AddExtension = false;
            this.openFileDialog1.Filter = "ZIP files|*.zip";
            this.openFileDialog1.Multiselect = true;
            // 
            // zipFilesButton
            // 
            this.zipFilesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.zipFilesButton.Location = new System.Drawing.Point(392, 91);
            this.zipFilesButton.Name = "zipFilesButton";
            this.zipFilesButton.Size = new System.Drawing.Size(40, 22);
            this.zipFilesButton.TabIndex = 7;
            this.zipFilesButton.Text = "...";
            this.zipFilesButton.UseVisualStyleBackColor = true;
            this.zipFilesButton.Click += new System.EventHandler(this.zipFilesButton_Click);
            // 
            // progress
            // 
            this.progress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progress.Location = new System.Drawing.Point(37, 149);
            this.progress.Name = "progress";
            this.progress.Size = new System.Drawing.Size(395, 23);
            this.progress.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 132);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 14);
            this.label3.TabIndex = 9;
            this.label3.Text = "Progress";
            // 
            // cancel
            // 
            this.cancel.AutoSize = true;
            this.cancel.Location = new System.Drawing.Point(34, 208);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(42, 14);
            this.cancel.TabIndex = 10;
            this.cancel.Text = "Cancel";
            this.cancel.Visible = false;
            // 
            // Form1
            // 
            this.AcceptButton = this.installButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(444, 241);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.progress);
            this.Controls.Add(this.zipFilesButton);
            this.Controls.Add(this.installButton);
            this.Controls.Add(this.zipFiles);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.contentDirectory);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.MinimumSize = new System.Drawing.Size(460, 280);
            this.Name = "Form1";
            this.Text = "DS Content Installer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox contentDirectory;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox zipFiles;
        private System.Windows.Forms.Button installButton;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button zipFilesButton;
        private System.Windows.Forms.ProgressBar progress;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label cancel;
    }
}

