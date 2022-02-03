using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Media.Imaging;

namespace WpfObjectSearch
{
    public static class BitmapConversion
    {

        public static Bitmap ToWinFormsBitmap(this BitmapSource bitmapsource)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(stream);

                using (var tempBitmap = new Bitmap(stream))
                {
                    return new Bitmap(tempBitmap);
                }
            }
        }

        public static BitmapSource ToWpfBitmap(this Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Bmp);

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }
        private static List<byte> colorList = new List<byte>();
        private static string hash;

        public static string GetImageHash(this Bitmap bmpSource)
        {
            colorList.Clear();
            int i, j;
            Bitmap bmpMin = new Bitmap(bmpSource, new Size(16, 16)); //create new image with 16x16 pixel
            for (j = 0; j < bmpMin.Height; j++)
            {
                for (i = 0; i < bmpMin.Width; i++)
                {
                    colorList.Add(bmpMin.GetPixel(i, j).R);
                }
            }
            SHA1Managed sha = new SHA1Managed();
            byte[] checksum = sha.ComputeHash(colorList.ToArray());
            hash = BitConverter.ToString(checksum).Replace("-", String.Empty);
            sha.Dispose();
            bmpMin.Dispose();
            return hash;
        }
        public static Bitmap CropImage(this Bitmap source, Rectangle section)
        {
            var bitmap = new Bitmap(section.Width, section.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);
                return bitmap;
            }
        }
    }
}