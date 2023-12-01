using System.Runtime.InteropServices;
using System.Windows;

namespace ImagePosterization
{
    public partial class MainWindow : Window
    {
        private const string dllPath = @"E:\Repos\ImagePosterization\x64\Debug\PosterizationCppLib.dll";

        [DllImport(dllPath)]
        private static extern int posterization_init();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int value = posterization_init();
            resultLabel.Content = value;
        }
    }
}