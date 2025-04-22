using System;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;


namespace G15_Interop
{
    public interface IG15_Display
    {
        event EventHandler<ScreenUpdatedEventArgs> ScreenUpdated;

        void Clear();
        Bitmap DrawScreen();
        void RefreshScreen();
        void SetLine(int line, string text);
        void SetBackground(Bitmap bitmap);
    }
    public class G15_Display : IG15_Display
    {
        public event EventHandler<ScreenUpdatedEventArgs> ScreenUpdated;

        private string[] Lines = new string[4];
        private string updateMessage;
        private Bitmap background;
        private Logitech_LCD.LogitechLcd Lcd;


        public G15_Display(bool isDummy = false)
        {
            if (isDummy) return;
            Lcd = Logitech_LCD.LogitechLcd.Instance;
        }

        public virtual void Clear()
        {
            Lines = new string[4];
            background?.Dispose();
            background = new Bitmap(160,43);
            updateMessage = "Screen Cleared";
            RefreshScreen();
        }

        public virtual Bitmap DrawScreen()
        {

            var img =(Bitmap) background?.Clone() ?? new Bitmap(160, 43);            
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
            var bytes = BitmapHelper.BitmapToBytes(mono);
            Lcd?.MonoSetBackground(bytes);
            background = bitmap;
            updateMessage = $"Background Updated";
            RefreshScreen();
        }
    }
}
