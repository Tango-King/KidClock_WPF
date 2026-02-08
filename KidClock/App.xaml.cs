using Prism.Ioc;
using Prism.DryIoc;
using Prism.Modularity;
using System.Windows;
using KidClock.Core.Interfaces;
using KidClock.Services;
using KidClock.Modules.ClockFace;
using KidClock.Modules.Education;
using KidClock.Modules.Games;
using Prism.Regions;
using KidClock.Core;
using Prism.Mvvm;
using KidClock.ViewModels;

namespace KidClock
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 注册服务
            containerRegistry.RegisterSingleton<IDataService, SqliteDataService>();
        }

        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();
            
            // 显式注册 MainWindow 的 ViewModel 映射，防止自动查找失败
            ViewModelLocationProvider.Register<MainWindow, MainWindowViewModel>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            // 初始化数据库
            var dataService = Container.Resolve<IDataService>();
            dataService.Initialize();

            // 手动请求一次初始导航，确保 MainRegion 被激活，并加载默认视图
            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RequestNavigate(RegionNames.MainRegion, "ClockFaceView");
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            // 注册模块
            moduleCatalog.AddModule<ClockFaceModule>();
            moduleCatalog.AddModule<EducationModule>();
            moduleCatalog.AddModule<GamesModule>();
        }
    }
}