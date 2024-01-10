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
        private const string asmDllPath = @"E:\Repos\ImagePosterization\x64\Debug\PosterizationAsmLib.dll";
        private const string cppDllPath = @"E:\Repos\ImagePosterization\x64\Debug\PosterizationCppLib.dll";

        private string selectedImagePath = "";
        private int posterizationLevel = 2;
        private int numberOfThreads = 1;


        [DllImport(asmDllPath, EntryPoint = "posterize")]
        private static extern void posterizeASM(byte[] image, int width, int height, int level);


        [DllImport(cppDllPath, EntryPoint = "posterize")]
        private static extern void posterizeCPP(byte[] image, int width, int height, int level);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Load_Image(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp|All files (*.*)|*.*",
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

            // Convert Bitmap to byte array
            BitmapData bmpData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            int bytes = Math.Abs(bmpData.Stride) * bitmap.Height;
            byte[] rgbValues = new byte[bytes];
            Marshal.Copy(bmpData.Scan0, rgbValues, 0, bytes);

            // int stripHeight = bitmap.Height / numberOfThreads;

            //Parallel.For(0, numberOfThreads, i =>
            //{
            //    posterize(rgbValues, bitmap.Width, stripHeight * (i+1), posterizationLevel);
            //});


            if (AsmButton.IsChecked == true)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                posterizeASM(rgbValues, bitmap.Width, bitmap.Height, posterizationLevel);

                stopwatch.Stop();

                timeElapsed_textBox.Text= stopwatch.Elapsed.ToString();
            }
            else
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                posterizeCPP(rgbValues, bitmap.Width, bitmap.Height, posterizationLevel);

                stopwatch.Stop();

                timeElapsed_textBox.Text = stopwatch.Elapsed.ToString();
            }

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
            processedImage.Source = image;
        }

        // Sets relative window's size 
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            Width = screenHeight * 1.2;
            Height = screenHeight * 0.7;
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