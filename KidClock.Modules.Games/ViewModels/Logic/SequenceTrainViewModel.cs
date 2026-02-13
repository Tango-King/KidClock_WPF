using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KidClock.Core.Interfaces;
using KidClock.Core.Models;
using KidClock.Modules.Games.ViewModels.Common;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace KidClock.Modules.Games.ViewModels.Logic
{
    public enum TokenShape
    {
        Circle = 0,
        Square = 1,
        Triangle = 2,
        Star = 3
    }

    public record SequenceToken(TokenShape Shape, Color Color);

    public partial class SequenceTrainViewModel : GameSessionViewModelBase
    {
        private readonly IDataService _dataService;
        private readonly Random _random = new();
        private DateTime _sessionStartUtc;
        private readonly ObservableCollection<string> _mistakeTags = new();

        [ObservableProperty]
        private ObservableCollection<SequenceToken> _sequence = new();

        [ObservableProperty]
        private ObservableCollection<SequenceToken> _options = new();

        [ObservableProperty]
        private SequenceToken _blankToken = new(TokenShape.Circle, Colors.Transparent);

        [ObservableProperty]
        private string _bigPrompt = "点击开始，找出下一个图形";

        private SequenceToken _expected = new(TokenShape.Circle, Colors.Red);
        private string _expectedTag = "";

        public SequenceTrainViewModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        protected override void OnStartedCore()
        {
            _sessionStartUtc = DateTime.UtcNow;
            _mistakeTags.Clear();
            GenerateRound();
        }

        protected override void OnResetCore()
        {
            _mistakeTags.Clear();
            Sequence.Clear();
            Options.Clear();
            BigPrompt = "点击开始，找出下一个图形";
        }

        protected override void OnDifficultyChanged()
        {
            if (IsRunning)
            {
                GenerateRound();
            }
        }

        protected override void OnSessionEnded()
        {
            SaveSession();
        }

        [RelayCommand]
        public void ChooseOption(SequenceToken chosen)
        {
            if (!IsRunning || IsPaused) return;

            bool isCorrect = chosen.Equals(_expected);
            if (Mode == GameMode.Practice && !isCorrect)
            {
                BigPrompt = $"再试一次：想想规律（{_expectedTag}）";
                AddMistakeTag(_expectedTag);
                TriggerHint();
                return;
            }

            RecordAnswer(isCorrect, 10);
            if (isCorrect)
            {
                BigPrompt = "太棒了！继续下一题";
                TriggerSuccess();
                GenerateRound();
            }
            else
            {
                BigPrompt = $"没关系，再来一题（提示：{_expectedTag}）";
                AddMistakeTag(_expectedTag);
                TriggerHint();
                GenerateRound();
            }
        }

        private void GenerateRound()
        {
            int length = DifficultyTier switch
            {
                1 => 4,
                2 => 5,
                _ => 6
            };

            var colors = new[] { Colors.Red, Colors.DodgerBlue, Colors.Gold, Colors.LimeGreen };
            var shapes = new[] { TokenShape.Circle, TokenShape.Square, TokenShape.Triangle, TokenShape.Star };

            bool useDualRule = DifficultyTier >= 3 && _random.Next(2) == 0;
            int cycle = DifficultyTier == 1 ? 2 : (DifficultyTier == 2 ? 3 : 4);

            int colorOffset = _random.Next(colors.Length);
            int shapeOffset = _random.Next(shapes.Length);

            Sequence.Clear();

            for (int i = 0; i < length; i++)
            {
                Color c;
                TokenShape s;

                if (useDualRule)
                {
                    c = colors[(colorOffset + i) % cycle];
                    s = shapes[(shapeOffset + (i % cycle)) % shapes.Length];
                    _expectedTag = "颜色+形状";
                }
                else
                {
                    if (_random.Next(2) == 0)
                    {
                        c = colors[(colorOffset + i) % cycle];
                        s = shapes[shapeOffset];
                        _expectedTag = "看颜色变化";
                    }
                    else
                    {
                        c = colors[colorOffset];
                        s = shapes[(shapeOffset + i) % cycle];
                        _expectedTag = "看形状变化";
                    }
                }

                Sequence.Add(new SequenceToken(s, c));
            }

            _expected = PredictNext(Sequence.ToArray(), useDualRule, colors, shapes, colorOffset, shapeOffset, cycle);
            BuildOptions(_expected, colors, shapes);

            BigPrompt = Mode == GameMode.Practice ? $"找出下一个图形（{_expectedTag}）" : "找出下一个图形";
        }

        private static SequenceToken PredictNext(SequenceToken[] seq, bool useDualRule, Color[] colors, TokenShape[] shapes, int colorOffset, int shapeOffset, int cycle)
        {
            int i = seq.Length;
            if (useDualRule)
            {
                var c = colors[(colorOffset + i) % cycle];
                var s = shapes[(shapeOffset + (i % cycle)) % shapes.Length];
                return new SequenceToken(s, c);
            }

            bool colorChanges = seq.Select(t => t.Color).Distinct().Count() > 1;
            if (colorChanges)
            {
                var c = colors[(colorOffset + i) % cycle];
                return new SequenceToken(seq[0].Shape, c);
            }

            var s2 = shapes[(shapeOffset + i) % cycle];
            return new SequenceToken(s2, seq[0].Color);
        }

        private void BuildOptions(SequenceToken expected, Color[] colors, TokenShape[] shapes)
        {
            Options.Clear();
            Options.Add(expected);

            while (Options.Count < 4)
            {
                var c = colors[_random.Next(colors.Length)];
                var s = shapes[_random.Next(shapes.Length)];
                var t = new SequenceToken(s, c);
                if (!Options.Contains(t))
                {
                    Options.Add(t);
                }
            }

            var shuffled = Options.OrderBy(_ => _random.Next()).ToArray();
            Options.Clear();
            foreach (var t in shuffled)
            {
                Options.Add(t);
            }
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
                LogicGameKeys.SequenceTrain,
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
