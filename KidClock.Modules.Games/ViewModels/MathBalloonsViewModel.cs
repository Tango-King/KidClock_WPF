using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;

namespace KidClock.Modules.Games.ViewModels
{
    public partial class MathBalloonsViewModel : ObservableObject
    {
        private Random _random = new Random();
        private DispatcherTimer _gameTimer;
        private int _maxRange = 20;

        [ObservableProperty]
        private ObservableCollection<BalloonItem> _balloons = new ObservableCollection<BalloonItem>();

        [ObservableProperty]
        private string _currentQuestion = "准备开始";

        [ObservableProperty]
        private int _targetAnswer;

        [ObservableProperty]
        private int _score = 0;

        [ObservableProperty]
        private bool _isGameRunning = false;

        [ObservableProperty]
        private ObservableCollection<string> _difficultyOptions = new ObservableCollection<string> { 
            "20以内", "30以内", "40以内", "50以内", 
            "60以内", "70以内", "80以内", "90以内", "100以内" 
        };

        private string _selectedDifficulty = "20以内";

        public string SelectedDifficulty
        {
            get => _selectedDifficulty;
            set
            {
                if (SetProperty(ref _selectedDifficulty, value))
                {
                    UpdateDifficulty();
                }
            }
        }

        private void UpdateDifficulty()
        {
            // Remove "以内" if present to parse
            string cleanValue = _selectedDifficulty.Replace("以内", "").Trim();
            
            if (int.TryParse(cleanValue, out int range))
            {
                if (range > 0)
                {
                    _maxRange = range;
                    // If game is running, immediately restart round with new difficulty
                    if (IsGameRunning)
                    {
                        GenerateQuestion(); 
                    }
                }
            }
        }

        public MathBalloonsViewModel()
        {
            _gameTimer = new DispatcherTimer();
            _gameTimer.Interval = TimeSpan.FromMilliseconds(50);
            _gameTimer.Tick += GameLoop;
        }

        [RelayCommand]
        public void StartGame()
        {
            Score = 0;
            IsGameRunning = true;
            Balloons.Clear();
            GenerateQuestion();
            _gameTimer.Start();
        }

        [RelayCommand]
        public void StopGame()
        {
            IsGameRunning = false;
            _gameTimer.Stop();
            CurrentQuestion = "游戏已停止";
        }

        private void GenerateQuestion()
        {
            // Simple Add/Sub
            bool isAdd = _random.Next(2) == 0;
            int a, b;

            if (isAdd)
            {
                a = _random.Next(1, _maxRange);
                b = _random.Next(1, _maxRange - a); // Ensure sum <= maxRange
                TargetAnswer = a + b;
                CurrentQuestion = $"{a} + {b} = ?";
            }
            else
            {
                a = _random.Next(1, _maxRange);
                b = _random.Next(1, a); // Ensure diff >= 0
                TargetAnswer = a - b;
                CurrentQuestion = $"{a} - {b} = ?";
            }

            SpawnBalloons();
        }

        private void SpawnBalloons()
        {
            Balloons.Clear();
            int count = 5; 
            int correctIndex = _random.Next(count);

            for (int i = 0; i < count; i++)
            {
                int val;
                if (i == correctIndex)
                {
                    val = TargetAnswer;
                }
                else
                {
                    // Random wrong answer
                    do
                    {
                        val = _random.Next(0, _maxRange);
                    } while (val == TargetAnswer);
                }

                // Random Color
                byte r = (byte)_random.Next(100, 255);
                byte g = (byte)_random.Next(100, 255);
                byte b = (byte)_random.Next(100, 255);
                var color = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(r, g, b));

                var balloon = new BalloonItem(val, color);
                balloon.X = 100 + i * 150; // Spread out horizontally
                balloon.Y = 600 + _random.Next(0, 100); // Start below screen
                // Faster speed 5-10
                balloon.Speed = _random.Next(5, 11); 
                Balloons.Add(balloon);
            }
        }

        private void GameLoop(object? sender, EventArgs e)
        {
            if (!IsGameRunning) return;

            // Use a for-loop to avoid enumeration issues if modification happens (though we are only modifying properties)
            // But modifying ObservableCollection triggers UI updates which might be slow.
            // Better to iterate and update.
            
            for (int i = 0; i < Balloons.Count; i++)
            {
                var b = Balloons[i];
                b.Y -= b.Speed;
                
                // Reset if off top screen
                if (b.Y < -100)
                {
                    b.Y = 600; // Reset to bottom
                    b.X = _random.Next(50, 750); // Randomize X slightly
                }
            }
        }

        [RelayCommand]
        public void BalloonClick(BalloonItem balloon)
        {
            if (balloon.Value == TargetAnswer)
            {
                Score += 10;
                // Pop effect?
                GenerateQuestion();
            }
            else
            {
                // Wrong answer penalty?
            }
        }
    }

    public partial class BalloonItem : ObservableObject
    {
        [ObservableProperty]
        private double _x;
        [ObservableProperty]
        private double _y;
        
        public double Speed { get; set; }
        public int Value { get; }
        public System.Windows.Media.Brush Color { get; }

        public BalloonItem(int value, System.Windows.Media.Brush color)
        {
            Value = value;
            Color = color;
        }
    }
}
