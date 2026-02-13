using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Mvvm;
using KidClock.Modules.Games.Views;
using KidClock.Modules.Games.ViewModels.Logic;

namespace KidClock.Modules.Games
{
    public class GamesModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            ViewModelLocationProvider.Register<LogicPackView, LogicPackViewModel>();
            ViewModelLocationProvider.Register<SequenceTrainView, SequenceTrainViewModel>();
            ViewModelLocationProvider.Register<CategoryMarketView, CategoryMarketViewModel>();
            ViewModelLocationProvider.Register<PatternMatrixView, PatternMatrixViewModel>();
            ViewModelLocationProvider.Register<CommandMazeView, CommandMazeViewModel>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 显式注册名称
            containerRegistry.RegisterForNavigation<TimeDetectiveView>("TimeDetectiveView");
            containerRegistry.RegisterForNavigation<PuzzleView>("PuzzleView");
            containerRegistry.RegisterForNavigation<PinyinPairingView>("PinyinPairingView");
            containerRegistry.RegisterForNavigation<MemoryMatchView>("MemoryMatchView");
            containerRegistry.RegisterForNavigation<MathBalloonsView>("MathBalloonsView");

            containerRegistry.RegisterForNavigation<LogicPackView>("LogicPackView");
            containerRegistry.RegisterForNavigation<SequenceTrainView>("SequenceTrainView");
            containerRegistry.RegisterForNavigation<CategoryMarketView>("CategoryMarketView");
            containerRegistry.RegisterForNavigation<PatternMatrixView>("PatternMatrixView");
            containerRegistry.RegisterForNavigation<CommandMazeView>("CommandMazeView");
        }
    }
}
