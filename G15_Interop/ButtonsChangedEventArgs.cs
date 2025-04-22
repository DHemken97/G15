using Logitech_LCD;
using System;

namespace G15_Interop
{
    public class ButtonsChangedEventArgs : EventArgs
    {
        public ButtonsChangedEventArgs(Buttons button,bool isPressed)
        {
            Button = button;
            IsPressed = isPressed;
        }

        public bool IsPressed { get; }
        public Buttons Button { get; }
    }
}
