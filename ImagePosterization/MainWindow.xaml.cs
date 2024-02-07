using Microsoft.Win32;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ImagePosterization
{
    public partial class MainWindow : Window
    {
        // Paths to the DLLs
        private const string asmDllPath = @"E:\Repos\ImagePosterization\x64\Debug\PosterizationAsmLib.dll";
        private const string cppDllPath = @"E:\Repos\ImagePosterization\x64\Debug\PosterizationCppLib.dll";

        // Variables for image processing
        private string selectedImagePath = "";
        private int posterizationLevel = 2; 
        private int numberOfThreads;

        // External method declarations for DLL imports
        [DllImport(asmDllPath, EntryPoint = "posterize")]
        private static extern void posterizeASM(byte[] image, int start, int end, int level);

        [DllImport(cppDllPath, EntryPoint = "posterize")]
        private static extern void posterizeCPP(byte[] image, int start, int end, int level);

        public MainWindow()
        {
            InitializeComponent();
            // Initialize the number of threads to the number of processors
            numberOfThreads = Environment.ProcessorCount;
            threadNumber.Value = numberOfThreads; 
        }

        // Event handler for loading an image
        private void Load_Image(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp",
                Title = "Select an Image File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Get the selected image file path
                selectedImagePath = openFileDialog.FileName;
                // Load the image and display it
                BitmapImage bitmapImage = new BitmapImage(new Uri(selectedImagePath, UriKind.RelativeOrAbsolute));
                importedImage.Source = bitmapImage;
            }
        }

        // Event handler for posterizing the image
        private void Posterize_Image(object sender, RoutedEventArgs e)
        {
            // Check if an image is loaded
            if (string.IsNullOrEmpty(selectedImagePath))
                return;

            // Load the image into a Bitmap object
            Bitmap bitmap = new Bitmap(selectedImagePath);

            // Lock the bitmap data
            BitmapData bmpData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            // Calculate the number of bytes in the image
            int bytes = Math.Abs(bmpData.Stride) * bitmap.Height ;
            // Allocate memory for the RGBA values of the image
            byte[] rgbValues = new byte[bytes];
            // Copy the image data into the byte array
            Marshal.Copy(bmpData.Scan0, rgbValues, 0, bytes);

            // Determine which posterization method to use
            Action<byte[], int, int, int> posterizeMethod = AsmButton.IsChecked == true ? posterizeASM : posterizeCPP;

            Stopwatch stopwatch = new Stopwatch();
                
            stopwatch.Start();

            // Parallel processing of image bytes using multiple threads
            Parallel.For(0, numberOfThreads, i =>
            {
                int start = i * bytes / numberOfThreads;
                int end = (i + 1) * bytes / numberOfThreads;

                posterizeMethod(rgbValues, start, end, posterizationLevel);
            });

            stopwatch.Stop();
            timeElapsed_textBox.Text = stopwatch.Elapsed.ToString();

            // Copy the modified byte array back to Bitmap
            Marshal.Copy(rgbValues, 0, bmpData.Scan0, bytes);
            bitmap.UnlockBits(bmpData);

            // Convert the bitmap to a PNG image and display it
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png); // Save as PNG
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            processedImage.Source = image;
        }

        // Event handler for window loaded event
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the window size relative to the screen size
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            Width = screenWidth * 0.8;
            Height = screenHeight * 0.8;
        }

        // Event handler for thread number value changed event
        private void threadNumber_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.numberOfThreads = (int)threadNumber.Value;
        }

        // Event handler for level value changed event
        private void level_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.posterizationLevel = (int)level.Value;
        }
    }
}