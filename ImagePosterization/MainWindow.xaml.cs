using Microsoft.Win32;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImagePosterization
{
    public partial class MainWindow : Window
    {
       //private const string dllPath = @"E:\Repos\ImagePosterization\x64\Debug\PosterizationCppLib.dll";
       private const string dllPath = @"E:\Repos\ImagePosterization\x64\Debug\PosterizationAsmLib.dll";

        [DllImport(dllPath)]
        private static extern void posterize(byte[] image, int width, int height, int level);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp|All files (*.*)|*.*",
                Title = "Select an Image File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedImagePath = openFileDialog.FileName;
                DisplayImage(selectedImagePath);
            }
        }

        private void DisplayImage(string imagePath)
        {
            Bitmap bitmap = new Bitmap(imagePath);

            // Convert Bitmap to byte array
            BitmapData bmpData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            int bytes = Math.Abs(bmpData.Stride) * bitmap.Height;
            byte[] rgbValues = new byte[bytes];
            Marshal.Copy(bmpData.Scan0, rgbValues, 0, bytes);
            // 24 bytes pixels without alpha channel
            // Call C++ DLL to perform posterization
            int posterizeLevel = 2;// You can adjust the levels as needed
            posterize(rgbValues, bitmap.Width, bitmap.Height, posterizeLevel);

            // Copy the modified byte array back to Bitmap
                Marshal.Copy(rgbValues, 0, bmpData.Scan0, bytes);
            bitmap.UnlockBits(bmpData);

            // Display the modified image in WPF Image control
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            importedImage.Source = image;
        }
    }
}