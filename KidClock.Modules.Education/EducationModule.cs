using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using KidClock.Core;
using KidClock.Modules.Education.Views;
using IRegionManager = Prism.Regions.IRegionManager;

namespace KidClock.Modules.Education
{
    public class EducationModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public EducationModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 显式注册名称
            containerRegistry.RegisterForNavigation<TimeRelationView>("TimeRelationView");
            containerRegistry.RegisterForNavigation<TimeCognitionView>("TimeCognitionView");
        }
    }
}