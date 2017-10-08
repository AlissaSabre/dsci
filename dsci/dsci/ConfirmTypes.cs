using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dsci
{
    public enum ConfirmChoices
    {
        Ok,
        OkCancel,
        YesNo,
        YesNoCancel,
    }

    public enum ConfirmResponse
    {
        None = 0,
        Ok,
        Yes,
        No,
        Cancel,
    }

    public delegate void ConfirmEventHandler(object sender, ConfirmEventArgs args);

    public class ConfirmEventArgs : EventArgs
    {
        public Confirm Confirm;

        public object[] Args;

        public ConfirmChoices Choices;

        public ConfirmResponse Response;
    }
}
