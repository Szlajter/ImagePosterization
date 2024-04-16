using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Threading.Tasks; // Add this namespace for Parallel.For

namespace ImagePosterization
{
    public static class ImageHelper
    {
        // Method to load a Bitmap from a file path
        public static Bitmap LoadBitmap(string path)
        {
            return new Bitmap(path);
        }

        // Method to extract the raw byte data from a Bitmap
        public static byte[] ExtractBitmapBytes(Bitmap bitmap)
        {
            // Lock the bitmap data to safely access its bytes
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int bytes = Math.Abs(bmpData.Stride) * bitmap.Height;
            byte[] rgbValues = new byte[bytes];
            Marshal.Copy(bmpData.Scan0, rgbValues, 0, bytes);

            // Unlock the bitmap data
            bitmap.UnlockBits(bmpData);
            return rgbValues;
        }

        // Method to display an image in a WPF Image control
        public static void DisplayImage(string path, System.Windows.Controls.Image imageControl)
        {
            BitmapImage bitmapImage = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
            imageControl.Source = bitmapImage;
        }

        // Method to process the raw byte data of an image using parallel processing
        public static void ProcessImageBytes(byte[] rgbValues, Action<byte[], int, int, int> posterizeMethod, int numberOfThreads, int posterizationLevel)
        {
            Parallel.For(0, numberOfThreads, i =>
            {
                int start = i * rgbValues.Length / numberOfThreads;
                int end = (i + 1) * rgbValues.Length / numberOfThreads;
                posterizeMethod(rgbValues, start, end, posterizationLevel);
            });
        }

        // Method to copy modified byte data back to a Bitmap
        public static void CopyModifiedBytesToBitmap(Bitmap bitmap, byte[] rgbValues)
        {
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            Marshal.Copy(rgbValues, 0, bmpData.Scan0, rgbValues.Length);
            bitmap.UnlockBits(bmpData);
        }

        // Method to display a processed Bitmap in a WPF Image control
        public static void DisplayProcessedBitmap(Bitmap bitmap, System.Windows.Controls.Image imageControl)
        {
            // Convert the Bitmap to a BitmapImage and display it in the Image control
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            imageControl.Source = image;
        }
    }
}
