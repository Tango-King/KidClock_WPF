using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.Windows;

namespace KidClock.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private string _title = "KidClock - 儿童时钟认知教育";

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public DelegateCommand<string> NavigateCommand { get; private set; }

        public MainWindowViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            NavigateCommand = new DelegateCommand<string>(Navigate);
        }

        private void Navigate(string navigatePath)
        {
            if (string.IsNullOrEmpty(navigatePath))
                return;

            _regionManager.RequestNavigate("MainRegion", navigatePath, NavigationComplete);
        }

        private void NavigationComplete(NavigationResult result)
        {
            if (result.Result == false)
            {
                MessageBox.Show($"导航失败: {result.Error?.Message ?? "未知错误"}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}