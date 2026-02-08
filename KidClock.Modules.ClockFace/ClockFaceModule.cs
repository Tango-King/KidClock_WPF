using KidClock.Core;
using KidClock.Modules.ClockFace.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using IRegionManager = Prism.Regions.IRegionManager;

namespace KidClock.Modules.ClockFace
{
    public class ClockFaceModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public ClockFaceModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(ClockFaceView));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 显式注册名称
            containerRegistry.RegisterForNavigation<ClockFaceView>("ClockFaceView");
        }
    }
}