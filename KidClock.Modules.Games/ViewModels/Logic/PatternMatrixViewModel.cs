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
    public record MatrixToken(TokenShape Shape, Color Color);
    public record MatrixCell(int Index, MatrixToken Token, bool IsBlank);

    public partial class PatternMatrixViewModel : GameSessionViewModelBase
    {
        private readonly IDataService _dataService;
        private readonly Random _random = new();
        private DateTime _sessionStartUtc;
        private readonly ObservableCollection<string> _mistakeTags = new();

        [ObservableProperty]
        private int _gridSize = 3;

        [ObservableProperty]
        private ObservableCollection<MatrixCell> _cells = new();

        [ObservableProperty]
        private ObservableCollection<MatrixToken> _options = new();

        [ObservableProperty]
        private string _bigPrompt = "选出正确的缺失图形";

        private MatrixToken _expected = new(TokenShape.Circle, Colors.Red);
        private string _expectedTag = "";

        public PatternMatrixViewModel(IDataService dataService)
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
            Cells.Clear();
            Options.Clear();
            BigPrompt = "选出正确的缺失图形";
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
        public void ChooseOption(MatrixToken chosen)
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
                BigPrompt = "做对啦！继续下一题";
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
            GridSize = DifficultyTier >= 3 ? 4 : 3;
            int count = GridSize * GridSize;

            var colors = new[] { Colors.Red, Colors.DodgerBlue, Colors.Gold, Colors.LimeGreen };
            var shapes = new[] { TokenShape.Circle, TokenShape.Square, TokenShape.Triangle, TokenShape.Star };

            int blankIndex = _random.Next(count);
            int rule = DifficultyTier switch
            {
                1 => _random.Next(2),
                2 => _random.Next(3),
                _ => _random.Next(3)
            };

            int colorOffset = _random.Next(colors.Length);
            int shapeOffset = _random.Next(shapes.Length);

            Cells.Clear();
            for (int i = 0; i < count; i++)
            {
                int row = i / GridSize;
                int col = i % GridSize;

                Color c;
                TokenShape s;

                if (rule == 0)
                {
                    c = colors[(colorOffset + col) % colors.Length];
                    s = shapes[(shapeOffset + row) % shapes.Length];
                    _expectedTag = "看颜色顺序";
                }
                else if (rule == 1)
                {
                    c = colors[(colorOffset + row) % colors.Length];
                    s = shapes[(shapeOffset + col) % shapes.Length];
                    _expectedTag = "看形状顺序";
                }
                else
                {
                    c = colors[(colorOffset + row + col) % colors.Length];
                    s = shapes[(shapeOffset + row + col) % shapes.Length];
                    _expectedTag = "看行列一起变化";
                }

                var token = new MatrixToken(s, c);
                bool isBlank = i == blankIndex;
                Cells.Add(new MatrixCell(i, token, isBlank));
            }

            _expected = Cells.First(c => c.Index == blankIndex).Token;
            BuildOptions(_expected, colors, shapes);
            BigPrompt = Mode == GameMode.Practice ? $"选出正确的缺失图形（{_expectedTag}）" : "选出正确的缺失图形";
        }

        private void BuildOptions(MatrixToken expected, Color[] colors, TokenShape[] shapes)
        {
            Options.Clear();
            Options.Add(expected);

            while (Options.Count < 4)
            {
                var c = colors[_random.Next(colors.Length)];
                var s = shapes[_random.Next(shapes.Length)];
                var t = new MatrixToken(s, c);
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
                LogicGameKeys.PatternMatrix,
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
