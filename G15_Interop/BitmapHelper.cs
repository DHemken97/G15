using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;


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
            var mono = new Bitmap(160, 43);


            try
            {
                for (int x = 0; x < mono.Width; x++)
                    for (int y = 0; y < mono.Height; y++)
                    {
                        var pix = bitmap.GetPixel(x, y);
                        var avg = (pix.R + pix.G + pix.B) / 3;
                        mono.SetPixel(x, y, Color.FromArgb(avg, avg, avg));
                    }
            }
            catch { }

            return mono;

        }


        public static byte[] ConvertTo1bpp(Bitmap originalBitmap)
        {

            var data = new byte[160 * 43];


                for (int y = 0; y < originalBitmap.Height; y++)
                for (int x = 0; x < originalBitmap.Width; x++)
                {
                    var pix = originalBitmap.GetPixel(x, y);
                    var correction = 32;
                    var value = (((pix.R + pix.B + pix.G) / 3) / correction ) * correction; 
                    data[x  + (y * 160)] = Convert.ToByte(254-value);
                            }
            return data;



        }

    }

}
