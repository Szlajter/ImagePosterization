using Microsoft.Win32;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;

namespace ImagePosterization
{
    public partial class MainWindow : Window
    {
        private const string asmDllPath = @"E:\Repos\Assembler\ImagePosterization\x64\Debug\PosterizationAsmLib.dll";
        private const string cppDllPath = @"E:\Repos\Assembler\ImagePosterization\x64\Debug\PosterizationCppLib.dll";

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
            InitializeThreads();
        }

        private void InitializeThreads()
        {
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
                ImageHelper.DisplayImage(selectedImagePath, importedImage);
            }
        }

        private void Posterize_Image(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedImagePath))
                return;

            Bitmap bitmap = ImageHelper.LoadBitmap(selectedImagePath);
            byte[] rgbValues = ImageHelper.ExtractBitmapBytes(bitmap);

            Action<byte[], int, int, int> posterizeMethod = AsmButton.IsChecked == true ? posterizeASM : posterizeCPP;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            ImageHelper.ProcessImageBytes(rgbValues, posterizeMethod, numberOfThreads, posterizationLevel);

            stopwatch.Stop();
            UpdateElapsedTime(stopwatch.Elapsed);

            ImageHelper.CopyModifiedBytesToBitmap(bitmap, rgbValues);
            ImageHelper.DisplayProcessedBitmap(bitmap, processedImage);
        }

        private void UpdateElapsedTime(TimeSpan elapsed)
        {
            timeElapsed_textBox.Text = elapsed.ToString();
            string trimmedVariable = elapsed.ToString().Substring(6);
            Clipboard.SetText(trimmedVariable);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetWindowSize();
        }

        private void SetWindowSize()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            Width = screenWidth * 0.8;
            Height = screenHeight * 0.8;
        }

        private void threadNumber_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            numberOfThreads = (int)threadNumber.Value;
        }

        private void level_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            posterizationLevel = (int)level.Value;
        }
    }
}
