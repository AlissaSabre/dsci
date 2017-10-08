using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dsci
{
    public partial class ConfirmDialog : Form
    {
        private static ResourceManager message_resource;

        public static string GetMessage(ConfirmEventArgs args)
        {
            if (message_resource == null)
            {
                message_resource = new ResourceManager("dsci.Confirm", typeof(ConfirmDialog).Assembly);
            }
            var fmt = message_resource.GetString(args.Confirm.ToString());
            return string.Format(fmt, args.Args ?? new object[0]);
        }

        public ConfirmDialog()
        {
            InitializeComponent();

            OkLabel = label1.Text;
            YesLabel = button1.Text;
            NoLabel = button2.Text;
            CancelLabel = button3.Text;
        }

        private string OkLabel, YesLabel, NoLabel, CancelLabel;

        private ConfirmEventArgs _EventArgs;

        public ConfirmEventArgs EventArgs
        {
            get { return _EventArgs; }
            set
            {
                _EventArgs = value;
                textBox1.Text = GetMessage(_EventArgs);
                switch (_EventArgs.Choices)
                {
                    case ConfirmChoices.Ok:
                        UpdateButtons(ConfirmResponse.None, ConfirmResponse.None, ConfirmResponse.Ok);
                        break;
                    case ConfirmChoices.OkCancel:
                        UpdateButtons(ConfirmResponse.None, ConfirmResponse.Ok, ConfirmResponse.Cancel);
                        break;
                    case ConfirmChoices.YesNo:
                        UpdateButtons(ConfirmResponse.None, ConfirmResponse.Yes, ConfirmResponse.No);
                        break;
                    case ConfirmChoices.YesNoCancel:
                        UpdateButtons(ConfirmResponse.Yes, ConfirmResponse.No, ConfirmResponse.Cancel);
                        break;
                }
                UpdateDefaultButton();
            }
        }

        private void UpdateButtons(ConfirmResponse r1, ConfirmResponse r2, ConfirmResponse r3)
        {
            UpdateButton(button1, r1);
            UpdateButton(button2, r2);
            UpdateButton(button3, r3);
        }

        private void UpdateButton(Button button, ConfirmResponse response)
        {
            switch (response)
            {
                case ConfirmResponse.None:
                    button.Visible = false;
                    break;
                case ConfirmResponse.Ok:
                    button.Visible = true;
                    button.Text = OkLabel;
                    button.DialogResult = DialogResult.OK;
                    AcceptButton = button;
                    break;
                case ConfirmResponse.Yes:
                    button.Visible = true;
                    button.Text = YesLabel;
                    button.DialogResult = DialogResult.Yes;
                    AcceptButton = button;
                    break;
                case ConfirmResponse.No:
                    button.Visible = true;
                    button.Text = NoLabel;
                    button.DialogResult = DialogResult.No;
                    CancelButton = button;
                    break;
                case ConfirmResponse.Cancel:
                    button.Visible = true;
                    button.Text = CancelLabel;
                    button.DialogResult = DialogResult.Cancel;
                    CancelButton = button;
                    break;
            }
        }

        private void ConfirmDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            switch (DialogResult)
            {
                case DialogResult.OK:
                    _EventArgs.Response = ConfirmResponse.Ok;
                    break;
                case DialogResult.Yes:
                    _EventArgs.Response = ConfirmResponse.Yes;
                    break;
                case DialogResult.No:
                    _EventArgs.Response = ConfirmResponse.No;
                    break;
                case DialogResult.Cancel:
                    _EventArgs.Response = ConfirmResponse.Cancel;
                    break;
            }
        }
    }
}
