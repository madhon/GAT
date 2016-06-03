namespace Gat.WPFSample
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AnalyticsHelper.Current.LogEvent("/SampleApp/TestButton1/", "Just some event text");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            AnalyticsHelper.Current.LogEvent("/SampleApp/TestButton2/SubAction", "Just some other event text");
        }
    }
}
