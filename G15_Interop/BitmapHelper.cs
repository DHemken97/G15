using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;


namespace G15_Interop
{

    public static class BitmapHelper
    {
        public static byte[] BitmapToBytes(Bitmap bitmap)
        {
            MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png); // You can change to Bmp, Jpeg, etc.
            var bytes = stream.ToArray();
            stream.Dispose();
            return bytes;
        }

        internal static Bitmap MakeMono(Bitmap bitmap)
        {
            for (int x=0 ; x<bitmap.Width; x++)
                for (int y=0 ; y<bitmap.Height ; y++)
                {
                    var pix = bitmap.GetPixel(x, y);
                    var avg = (pix.R + pix.G + pix.B) / 3;
                    bitmap.SetPixel(x, y, Color.FromArgb(avg,avg,avg));
                }
            return bitmap;

        }
    }

}
