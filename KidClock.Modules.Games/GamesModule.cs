using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using KidClock.Modules.Games.Views;

namespace KidClock.Modules.Games
{
    public class GamesModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 显式注册名称
            containerRegistry.RegisterForNavigation<TimeDetectiveView>("TimeDetectiveView");
            containerRegistry.RegisterForNavigation<PuzzleView>("PuzzleView");
        }
    }
}