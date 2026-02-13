using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KidClock.Core.Interfaces;
using KidClock.Core.Models;
using KidClock.Modules.Games.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace KidClock.Modules.Games.ViewModels.Logic
{
    public record CategoryBucket(string Key, string DisplayName, Color Color);
    public record CategoryItem(string Name, string BucketKey, string MistakeTag);

    public partial class CategoryMarketViewModel : GameSessionViewModelBase
    {
        private readonly IDataService _dataService;
        private readonly Random _random = new();
        private DateTime _sessionStartUtc;
        private readonly ObservableCollection<string> _mistakeTags = new();
        private List<CategoryItem> _pool = new();

        [ObservableProperty]
        private ObservableCollection<CategoryBucket> _buckets = new();

        [ObservableProperty]
        private CategoryItem? _currentItem;

        [ObservableProperty]
        private string _bigPrompt = "把物品放到正确的篮子里";

        public CategoryMarketViewModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        protected override void OnStartedCore()
        {
            _sessionStartUtc = DateTime.UtcNow;
            _mistakeTags.Clear();
            BuildBucketsAndPool();
            NextItem();
        }

        protected override void OnResetCore()
        {
            _mistakeTags.Clear();
            Buckets.Clear();
            CurrentItem = null;
            BigPrompt = "把物品放到正确的篮子里";
        }

        protected override void OnDifficultyChanged()
        {
            if (IsRunning)
            {
                BuildBucketsAndPool();
                NextItem();
            }
        }

        protected override void OnSessionEnded()
        {
            SaveSession();
        }

        [RelayCommand]
        public void ChooseBucket(CategoryBucket bucket)
        {
            if (!IsRunning || IsPaused) return;
            if (CurrentItem == null) return;

            bool isCorrect = bucket.Key == CurrentItem.BucketKey;
            if (Mode == GameMode.Practice && !isCorrect)
            {
                BigPrompt = $"再试一次：想想“{bucket.DisplayName}”适合什么";
                AddMistakeTag(CurrentItem.MistakeTag);
                TriggerHint();
                return;
            }

            RecordAnswer(isCorrect, 10);
            if (isCorrect)
            {
                BigPrompt = "放对啦！继续";
                TriggerSuccess();
            }
            else
            {
                BigPrompt = $"没关系，正确篮子是：{Buckets.First(b => b.Key == CurrentItem.BucketKey).DisplayName}";
                AddMistakeTag(CurrentItem.MistakeTag);
                TriggerHint();
            }

            NextItem();
        }

        private void NextItem()
        {
            if (_pool.Count == 0)
            {
                BuildBucketsAndPool();
            }

            CurrentItem = _pool[_random.Next(_pool.Count)];
            BigPrompt = Mode == GameMode.Practice ? "把物品放到正确的篮子里（看类别/属性）" : "把物品放到正确的篮子里";
        }

        private void BuildBucketsAndPool()
        {
            Buckets.Clear();

            if (DifficultyTier == 1)
            {
                Buckets.Add(new CategoryBucket("fruit", "水果", Colors.Orange));
                Buckets.Add(new CategoryBucket("stationery", "文具", Colors.DodgerBlue));

                _pool = new List<CategoryItem>
                {
                    new("苹果", "fruit", "水果"),
                    new("香蕉", "fruit", "水果"),
                    new("葡萄", "fruit", "水果"),
                    new("铅笔", "stationery", "文具"),
                    new("橡皮", "stationery", "文具"),
                    new("尺子", "stationery", "文具"),
                };
            }
            else if (DifficultyTier == 2)
            {
                Buckets.Add(new CategoryBucket("animal", "动物", Colors.LimeGreen));
                Buckets.Add(new CategoryBucket("vehicle", "交通", Colors.MediumPurple));
                Buckets.Add(new CategoryBucket("food", "食物", Colors.OrangeRed));

                _pool = new List<CategoryItem>
                {
                    new("小猫", "animal", "动物"),
                    new("小狗", "animal", "动物"),
                    new("小鸟", "animal", "动物"),
                    new("汽车", "vehicle", "交通"),
                    new("公交", "vehicle", "交通"),
                    new("自行车", "vehicle", "交通"),
                    new("面包", "food", "食物"),
                    new("牛奶", "food", "食物"),
                    new("鸡蛋", "food", "食物"),
                };
            }
            else
            {
                Buckets.Add(new CategoryBucket("wheels", "有轮子", Colors.SteelBlue));
                Buckets.Add(new CategoryBucket("can_eat", "能吃", Colors.Orange));
                Buckets.Add(new CategoryBucket("can_fly", "会飞", Colors.MediumPurple));
                Buckets.Add(new CategoryBucket("animal", "是动物", Colors.LimeGreen));

                _pool = new List<CategoryItem>
                {
                    new("汽车", "wheels", "有轮子"),
                    new("自行车", "wheels", "有轮子"),
                    new("火车", "wheels", "有轮子"),
                    new("苹果", "can_eat", "能吃"),
                    new("面包", "can_eat", "能吃"),
                    new("鸡蛋", "can_eat", "能吃"),
                    new("小鸟", "can_fly", "会飞"),
                    new("蝴蝶", "can_fly", "会飞"),
                    new("飞机", "can_fly", "会飞"),
                    new("小猫", "animal", "是动物"),
                    new("小狗", "animal", "是动物"),
                    new("小兔", "animal", "是动物"),
                };
            }

            _pool = _pool.OrderBy(_ => _random.Next()).ToList();
        }

        private void AddMistakeTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return;
            if (_mistakeTags.Count >= 50) return;
            _mistakeTags.Add(tag);
        }

        private void SaveSession()
        {
            int duration = (int)Math.Max(0, (DateTime.UtcNow - _sessionStartUtc).TotalSeconds);
            var result = new GameSessionResult(
                LogicGameKeys.CategoryMarket,
                Mode.ToString(),
                DifficultyTier,
                CorrectCount,
                TotalCount,
                duration,
                StarRating,
                DateTime.UtcNow.ToString("O"),
                string.Join(",", _mistakeTags)
            );
            _dataService.SaveGameSession(result);
        }
    }
}
