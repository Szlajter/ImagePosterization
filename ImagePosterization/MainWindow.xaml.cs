using Microsoft.Win32;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace ImagePosterization
{
    public partial class MainWindow : Window
    {
        private const string asmDllPath = @"E:\Repos\ImagePosterization\x64\Debug\PosterizationAsmLib.dll";
        private const string cppDllPath = @"E:\Repos\ImagePosterization\x64\Debug\PosterizationCppLib.dll";

        private string selectedImagePath = "";
        private int posterizationLevel = 2; 
        private int numberOfThreads;


        [DllImport(asmDllPath, EntryPoint = "posterize")]
        private static extern void posterizeASM(byte[] image, int start, int end, int level);

        [DllImport(cppDllPath, EntryPoint = "posterize")]
        private static extern void posterizeCPP(byte[] image, int start, int end, int level);

        public MainWindow()
        {
            InitializeComponent();
            numberOfThreads = Environment.ProcessorCount;
            threadNumber.Value = numberOfThreads;
        }

        private void Load_Image(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp",
                Title = "Select an Image File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                selectedImagePath = openFileDialog.FileName;
                BitmapImage bitmapImage = new BitmapImage(new Uri(selectedImagePath, UriKind.RelativeOrAbsolute));
                importedImage.Source = bitmapImage;
            }
        }

        private void Posterize_Image(object sender, RoutedEventArgs e)
        {
            if (selectedImagePath == "")
                return;

            Bitmap bitmap = new Bitmap(selectedImagePath); 

            BitmapData bmpData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            int bytes = Math.Abs(bmpData.Stride) * bitmap.Height;
            byte[] rgbaValues = new byte[bytes];
            Marshal.Copy(bmpData.Scan0, rgbaValues, 0, bytes);

            if (AsmButton.IsChecked == true)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                Parallel.For(0, numberOfThreads, i =>
                {
                    int start = i * bytes / numberOfThreads;
                    int end = (i + 1) * bytes / numberOfThreads;

                    posterizeASM(rgbaValues, start, end, posterizationLevel);
                });

                stopwatch.Stop();

                timeElapsed_textBox.Text = stopwatch.Elapsed.ToString();
            }
            else
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                Parallel.For(0, numberOfThreads, i =>
                {
                    int start = i * bytes / numberOfThreads;
                    int end = (i + 1) * bytes / numberOfThreads;

                    posterizeCPP(rgbaValues, start, end, posterizationLevel);
                });

                stopwatch.Stop();

                timeElapsed_textBox.Text = stopwatch.Elapsed.ToString();
            }

            // Copy the modified byte array back to Bitmap
            Marshal.Copy(rgbaValues, 0, bmpData.Scan0, bytes);
            bitmap.UnlockBits(bmpData);

            // Display the modified image in WPF Image control
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png); // Save as PNG
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            processedImage.Source = image;
        }

        // Sets relative window's size 
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            Width = screenWidth * 0.8;
            Height = screenHeight * 0.8;
        }

        private void threadNumber_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.numberOfThreads = (int)threadNumber.Value;
        }

        private void level_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.posterizationLevel = (int)level.Value;
        }
    }
}