using Logitech_LCD;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Timers;
using static System.Net.Mime.MediaTypeNames;


namespace G15_Interop
{
    public interface IG15_Display
    {
        event EventHandler<ScreenUpdatedEventArgs> ScreenUpdated;
        event EventHandler<ButtonsChangedEventArgs> ButtonsChanged;

        Dictionary<Buttons, bool> ButtonStates { get; } 
        void Clear();
        Bitmap DrawScreen();
        void RefreshScreen();
        void SetLine(int line, string text);
        void SetBackground(Bitmap bitmap);
    }
    public class G15_Display : IG15_Display
    {
        public event EventHandler<ScreenUpdatedEventArgs> ScreenUpdated;
        public event EventHandler<ButtonsChangedEventArgs> ButtonsChanged;

        private string[] Lines = new string[4];
        public Dictionary<Buttons, bool> ButtonStates { get; private set; } 
        private string updateMessage;
        private Bitmap background;
        private Logitech_LCD.LogitechLcd Lcd;
        private bool HoldEvent;

        public G15_Display(bool isDummy = false)
        {
            ButtonStates = new Dictionary<Buttons, bool>
            {
                {Buttons.MonoButton0,false},
                {Buttons.MonoButton1,false},
                {Buttons.MonoButton2,false},
                {Buttons.MonoButton3,false}
            };
            background = new Bitmap(160, 43);

            if (isDummy) return;
            InitScreen();


            var timer = new System.Timers.Timer();
            timer.Interval = 100;
            timer.Elapsed += PollButtons;
            timer.AutoReset = true;
            timer.Start();

            


        }
        private void InitScreen()
        {
            LogitechLcd.Instance.Init("My Winform App", LcdType.Mono);
            LogitechLcd.Instance.MonoSetText(0, "----------------"); // Example: Top line separator
            LogitechLcd.Instance.MonoSetText(1, " Welcome! "); // Your text on the second line
            LogitechLcd.Instance.MonoSetText(2, "Starting Service..."); // Example: Show time
            LogitechLcd.Instance.MonoSetText(3, "----------------"); // Example: Bottom line separator
            LogitechLcd.Instance.Update();
            Lcd = Logitech_LCD.LogitechLcd.Instance;

            Thread.Sleep(5000);
            Clear();
        }

        private void PollButtons(object sender, ElapsedEventArgs e)
        {
            if (Lcd == null) { 
                ((System.Windows.Forms.Timer)sender).Stop(); return;
            };
            var buttons = ButtonStates.Select(x => x.Key).ToArray();
            foreach (var button in buttons) {

                var value = ButtonStates[button];
                var current = Lcd.IsButtonPressed(button);
                if (current != value)
                {
                    ButtonStates[button] = current;
                    
                    ButtonsChanged?.Invoke(this,new ButtonsChangedEventArgs(button, current));
                }
            }
            
        }

        public virtual void Clear()
        {
            HoldEvent = true;
            SetLine(0,string.Empty);
            SetLine(1,string.Empty);
            SetLine(2,string.Empty);
            SetLine(3,string.Empty);

            background?.Dispose();
            background = new Bitmap(160,43);
            updateMessage = "Screen Cleared";
            HoldEvent = false;
            RefreshScreen();
        }

        public virtual Bitmap DrawScreen()
        {

            var img = ((background?.Height??0) == 160 && (background?.Height??0)==43) ? background : new Bitmap(160,43);         
            var gfx = Graphics.FromImage(img);
            var lineHeight = img.Height / 4;
            var lineMargin = Math.Max( (int)(lineHeight * 0.05),1);
            Font font = new Font("MS Gothic", (int)lineHeight-(2*lineMargin), FontStyle.Bold);
            Brush brush = Brushes.Black;
            gfx.DrawString(Lines[0], font, brush, new PointF(lineMargin, lineMargin));
            gfx.DrawString(Lines[1], font, brush, new PointF(lineMargin, (1*lineHeight)+lineMargin));
            gfx.DrawString(Lines[2], font, brush, new PointF(lineMargin, (2*lineHeight)+lineMargin));
            gfx.DrawString(Lines[3], font, brush, new PointF(lineMargin, (3*lineHeight)+lineMargin));

            return img;

        }

        public virtual void RefreshScreen()
        {
            Lcd?.Update();
            RaiseScreenUpdatedEvent();
        }
        protected void RaiseScreenUpdatedEvent()
        {
            if (HoldEvent) return;
            var screenPreview = DrawScreen();
            ScreenUpdated?.Invoke(this, new ScreenUpdatedEventArgs(screenPreview, updateMessage??"Screen Refreshed"));
            updateMessage = null;
        }

        public void SetLine(int line, string text)
        {
            if (Lines[line] == text) return;
            Lcd?.MonoSetText(line, text);
            Lines[line] = text;
            updateMessage = $"Line {line} set to '{text}'";
            RefreshScreen();
        }

        public void SetBackground(Bitmap bitmap)
        {
            var mono = BitmapHelper.MakeMono(bitmap);
            bitmap.Dispose();
            var bytes = BitmapHelper.BitmapToBytes(mono);
            Lcd?.MonoSetBackground(bytes);
            background = bitmap;
            updateMessage = $"Background Updated";
            RefreshScreen();
        }
    }
}
