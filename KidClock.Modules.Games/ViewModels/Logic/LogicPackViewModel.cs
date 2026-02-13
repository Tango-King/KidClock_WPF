using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KidClock.Core;
using KidClock.Core.Interfaces;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace KidClock.Modules.Games.ViewModels.Logic
{
    public partial class LogicGameCard : ObservableObject
    {
        public string Title { get; }
        public string ViewName { get; }
        public string GameKey { get; }

        [ObservableProperty]
        private string _bestText = "暂无记录";

        public LogicGameCard(string title, string viewName, string gameKey)
        {
            Title = title;
            ViewName = viewName;
            GameKey = gameKey;
        }
    }

    public partial class LogicPackViewModel : ObservableObject
    {
        private readonly IRegionManager _regionManager;
        private readonly IDataService _dataService;

        [ObservableProperty]
        private ObservableCollection<LogicGameCard> _games = new();

        [ObservableProperty]
        private string _weeklySummary = "本周报告：暂无数据";

        public LogicPackViewModel(IRegionManager regionManager, IDataService dataService)
        {
            _regionManager = regionManager;
            _dataService = dataService;

            Games.Add(new LogicGameCard("序列火车", "SequenceTrainView", LogicGameKeys.SequenceTrain));
            Games.Add(new LogicGameCard("分类小超市", "CategoryMarketView", LogicGameKeys.CategoryMarket));
            Games.Add(new LogicGameCard("规律拼图", "PatternMatrixView", LogicGameKeys.PatternMatrix));
            Games.Add(new LogicGameCard("指令迷宫", "CommandMazeView", LogicGameKeys.CommandMaze));

            Refresh();
        }

        [RelayCommand]
        public void NavigateTo(string viewName)
        {
            if (string.IsNullOrWhiteSpace(viewName)) return;
            _regionManager.RequestNavigate(RegionNames.MainRegion, viewName);
        }

        [RelayCommand]
        public void Refresh()
        {
            foreach (var g in Games)
            {
                var best = _dataService.GetBestRecord(g.GameKey, "Challenge");
                if (best == null)
                {
                    g.BestText = "挑战最佳：暂无记录";
                }
                else
                {
                    double acc = best.TotalCount <= 0 ? 0 : (double)best.CorrectCount / best.TotalCount;
                    g.BestText = $"挑战最佳：{best.StarRating}星  正确率{acc:P0}  题数{best.TotalCount}";
                }
            }

            string to = DateTime.UtcNow.ToString("O");
            string from = DateTime.UtcNow.AddDays(-7).ToString("O");
            var report = _dataService.GetWeeklyReport(from, to);

            if (report.Items.Length == 0)
            {
                WeeklySummary = "本周报告：暂无数据";
            }
            else
            {
                var lines = report.Items
                    .OrderBy(i => i.GameKey)
                    .Select(i => $"{ToChineseGameName(i.GameKey)}：正确率{i.Accuracy:P0}，题数{i.TotalQuestions}，平均用时{i.AvgDurationSeconds:F0}s");
                WeeklySummary = "本周报告：\n" + string.Join("\n", lines);
            }
        }

        private static string ToChineseGameName(string gameKey)
        {
            return gameKey switch
            {
                LogicGameKeys.SequenceTrain => "序列火车",
                LogicGameKeys.CategoryMarket => "分类小超市",
                LogicGameKeys.PatternMatrix => "规律拼图",
                LogicGameKeys.CommandMaze => "指令迷宫",
                _ => gameKey
            };
        }
    }
}

