using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;

namespace KidClock.Modules.Games.ViewModels
{
    public partial class PinyinPairingViewModel : ObservableObject
    {
        private Random _random = new Random();
        private DispatcherTimer _gameTimer;
        private int _mistakeCount = 0;
        private const int MISCLICK_DELAY_MS = 300;
        private DateTime _lastClickTime = DateTime.MinValue;

        [ObservableProperty]
        private ObservableCollection<PinyinCard> _floatingCards = new ObservableCollection<PinyinCard>();

        [ObservableProperty]
        private string _targetLetter = ""; // e.g., "A"

        [ObservableProperty]
        private string _gameStatus = "准备开始";

        [ObservableProperty]
        private int _score = 0;

        [ObservableProperty]
        private int _level = 1; // 1: 5 pairs, 2: 10 pairs...

        [ObservableProperty]
        private bool _isGameRunning = false;

        [ObservableProperty]
        private bool _isFindLowercase = true; // Mode: True = Uppercase -> Lowercase, False = Lowercase -> Uppercase

        [ObservableProperty]
        private double _moveSpeed = 2.0;

        // Pinyin data (A-Z + Ü)
        // Simplified for this demo: A-Z
        private string[] _allLetters = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,W,X,Y,Z".Split(',');
        // Note: V is usually ü in Pinyin input, but visual representation needs font support for ü. 
        // We will stick to basic Latin for simplicity in this text-based implementation.

        public PinyinPairingViewModel()
        {
            _gameTimer = new DispatcherTimer();
            _gameTimer.Interval = TimeSpan.FromMilliseconds(50); // 20 FPS
            _gameTimer.Tick += GameLoop;
        }

        [RelayCommand]
        public void ToggleGameMode()
        {
            IsFindLowercase = !IsFindLowercase;
            // Restart game to apply changes if running
            if (IsGameRunning)
            {
                StopGame();
                StartGame(Level.ToString());
            }
        }

        [RelayCommand]
        public void StartGame(string levelStr)
        {
            if (int.TryParse(levelStr, out int level))
            {
                Level = level;
            }
            else
            {
                Level = 1;
            }

            Score = 0;
            _mistakeCount = 0;
            MoveSpeed = 2.0;
            IsGameRunning = true;
            GameStatus = IsFindLowercase ? "游戏中 - 找出对应的小写字母！" : "游戏中 - 找出对应的大写字母！";
            
            FloatingCards.Clear();
            SpawnNextRound();
            _gameTimer.Start();
        }

        [RelayCommand]
        public void StopGame()
        {
            IsGameRunning = false;
            _gameTimer.Stop();
            GameStatus = "游戏已停止";
        }

        private void SpawnNextRound()
        {
            FloatingCards.Clear();
            
            // Pick a random target letter
            string rawTarget = _allLetters[_random.Next(_allLetters.Length)];
            
            // If FindLowercase (Default): Target is Upper (A), we find Lower (a)
            // If FindUppercase: Target is Lower (a), we find Upper (A)
            
            TargetLetter = IsFindLowercase ? rawTarget : rawTarget.ToLower();

            // Determine number of distractors based on level
            int totalCards = 5 + (Level * 2); 

            // Create Correct Answer
            string correctContent = IsFindLowercase ? rawTarget.ToLower() : rawTarget;
            var correctCard = new PinyinCard(correctContent, true);
            RandomizePosition(correctCard, FloatingCards);
            FloatingCards.Add(correctCard);

            // Create Distractors
            for (int i = 0; i < totalCards - 1; i++)
            {
                string distractorLetter = _allLetters[_random.Next(_allLetters.Length)];
                // Ensure distractor is not the target letter (regardless of case)
                while (distractorLetter == rawTarget)
                {
                    distractorLetter = _allLetters[_random.Next(_allLetters.Length)];
                }

                // Randomly choose case for distractor to add confusion, OR stick to the answer type?
                // Usually "Find Lowercase" implies all floating cards are lowercase options?
                // Or mixed? Let's make all floating cards the Target Type (Answer Type).
                // e.g. Find Lowercase -> All floating cards are lowercase.
                
                string distractorContent = IsFindLowercase ? distractorLetter.ToLower() : distractorLetter;
                
                var card = new PinyinCard(distractorContent, false);
                RandomizePosition(card, FloatingCards);
                FloatingCards.Add(card);
            }
        }

        private void RandomizePosition(PinyinCard card, ObservableCollection<PinyinCard> existingCards)
        {
            // Canvas size 800x600 roughly
            // Card size ~60x60
            
            int maxAttempts = 50;
            for (int i = 0; i < maxAttempts; i++)
            {
                double x = _random.Next(50, 700);
                double y = _random.Next(50, 450); // Reduced height to avoid overlap with bottom bar
                
                bool overlap = false;
                foreach (var other in existingCards)
                {
                    double dx = x - other.X;
                    double dy = y - other.Y;
                    double dist = Math.Sqrt(dx*dx + dy*dy);
                    if (dist < 80) // 60 width + margin
                    {
                        overlap = true;
                        break;
                    }
                }
                
                if (!overlap)
                {
                    card.X = x;
                    card.Y = y;
                    break;
                }
            }
            
            // Random direction
            card.VelX = (_random.NextDouble() - 0.5) * MoveSpeed;
            card.VelY = (_random.NextDouble() - 0.5) * MoveSpeed;
            
            // Random Color
            card.BgColor = PickRandomColor();
        }

        private System.Windows.Media.Brush PickRandomColor()
        {
            byte r = (byte)_random.Next(100, 255);
            byte g = (byte)_random.Next(100, 255);
            byte b = (byte)_random.Next(100, 255);
            return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(r, g, b));
        }

        private void GameLoop(object? sender, EventArgs e)
        {
            if (!IsGameRunning) return;

            // Move cards
            foreach (var card in FloatingCards)
            {
                card.X += card.VelX;
                card.Y += card.VelY;

                // Bounce off walls (assuming 800x600 boundary for simplicity, View should constrain properly)
                if (card.X < 0 || card.X > 750) card.VelX = -card.VelX;
                if (card.Y < 0 || card.Y > 550) card.VelY = -card.VelY;
            }
        }

        [RelayCommand]
        public void CardClicked(PinyinCard card)
        {
            // Anti-misclick check
            if ((DateTime.Now - _lastClickTime).TotalMilliseconds < MISCLICK_DELAY_MS) return;
            _lastClickTime = DateTime.Now;

            if (card.IsTarget)
            {
                // Correct!
                Score += 10;
                GameStatus = "配对成功！+10分";
                // Particle effect trigger would go here (via event or property)
                
                // Play Sound (Placeholder)
                // System.Media.SystemSounds.Beep.Play(); 

                SpawnNextRound();
            }
            else
            {
                // Wrong!
                _mistakeCount++;
                GameStatus = "找错了，再试试！";
                
                // Dynamic Difficulty Adjustment
                if (_mistakeCount >= 3)
                {
                    MoveSpeed *= 0.8; // Slow down
                    GameStatus += " (速度已降低)";
                    _mistakeCount = 0; // Reset counter
                    
                    // Apply new speed to existing cards
                    foreach(var c in FloatingCards)
                    {
                        c.VelX *= 0.8;
                        c.VelY *= 0.8;
                    }
                }
            }
        }
    }

    public partial class PinyinCard : ObservableObject
    {
        [ObservableProperty]
        private double _x;
        [ObservableProperty]
        private double _y;
        
        [ObservableProperty]
        private System.Windows.Media.Brush _bgColor = System.Windows.Media.Brushes.White;

        public double VelX { get; set; }
        public double VelY { get; set; }

        public string Content { get; }
        public bool IsTarget { get; }

        public PinyinCard(string content, bool isTarget)
        {
            Content = content;
            IsTarget = isTarget;
        }
    }
}
