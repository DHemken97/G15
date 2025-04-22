using System;
using System.Drawing;

namespace G15_Interop
{
    public class ScreenUpdatedEventArgs : EventArgs
    {
        public ScreenUpdatedEventArgs(Bitmap preview, string updateMessage)
        {
            Preview = preview;
            UpdateMessage = updateMessage;
        }

        public Bitmap Preview { get; }
        public string UpdateMessage { get; }
    }
}
