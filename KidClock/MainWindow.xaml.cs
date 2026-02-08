using System.Windows;

namespace KidClock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext == null)
            {
                MessageBox.Show("错误：MainWindow 的 DataContext 为 null！导航功能将无法使用。\n请检查 ViewModelLocator 配置。", "严重错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}