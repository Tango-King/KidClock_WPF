using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace KidClock.Modules.Games.ViewModels
{
    public partial class MemoryMatchViewModel : ObservableObject
    {
        private Random _random = new Random();
        private MemoryCard? _firstSelected;
        private bool _isProcessing;

        [ObservableProperty]
        private int _gridRows = 4;

        [ObservableProperty]
        private int _gridColumns = 4;

        [ObservableProperty]
        private ObservableCollection<MemoryCard> _cards = new ObservableCollection<MemoryCard>();

        [ObservableProperty]
        private string _gameStatus = "准备开始";

        [ObservableProperty]
        private int _score = 0;

        [ObservableProperty]
        private string _difficulty = "4x4"; // 4x4, 6x6, 8x8

        public MemoryMatchViewModel()
        {
            StartGameCommand.Execute("4"); // Default 4x4
        }

        [RelayCommand]
        public void StartGame(string sizeStr)
        {
            if (int.TryParse(sizeStr, out int size))
            {
                // Validate size (must be even total for pairs)
                if (size != 4 && size != 6 && size != 8) size = 4;
                
                // Clear old cards first to ensure UI clears
                Cards = new ObservableCollection<MemoryCard>();
                
                GridRows = size;
                GridColumns = size;
                Difficulty = $"{size}x{size}";
            }

            ResetGame();
        }

        private void ResetGame()
        {
            Score = 0;
            GameStatus = "游戏中 - 点击翻牌！";
            _firstSelected = null;
            _isProcessing = false;
            Cards.Clear();

            int totalPairs = (GridRows * GridColumns) / 2;
            
            // Generate pairs (Clock time 1:00 to 12:00, reusing if needed)
            for (int i = 0; i < totalPairs; i++)
            {
                int hour = (i % 12) + 1;
                string id = Guid.NewGuid().ToString();
                
                // Card 1: Digital Text
                Cards.Add(new MemoryCard(id, hour, true));
                // Card 2: Analog Clock (we will render this differently in View based on IsClock property)
                Cards.Add(new MemoryCard(id, hour, false));
            }

            // Shuffle
            var shuffled = Cards.OrderBy(x => _random.Next()).ToList();
            
            // Re-assign a new ObservableCollection to force full UI refresh
            Cards = new ObservableCollection<MemoryCard>(shuffled);
        }

        [RelayCommand]
        public async Task CardClick(MemoryCard card)
        {
            if (_isProcessing || card.IsMatched || card.IsFlipped) return;

            card.IsFlipped = true;

            if (_firstSelected == null)
            {
                _firstSelected = card;
            }
            else
            {
                _isProcessing = true; // Block input
                
                // Check Match
                if (_firstSelected.MatchId == card.MatchId)
                {
                    // Match!
                    _firstSelected.IsMatched = true;
                    card.IsMatched = true;
                    Score += 10;
                    GameStatus = "配对成功！";
                    
                    if (Cards.All(c => c.IsMatched))
                    {
                        GameStatus = "恭喜！全部完成！";
                    }
                }
                else
                {
                    // No Match
                    GameStatus = "不匹配...";
                    await Task.Delay(1000);
                    _firstSelected.IsFlipped = false;
                    card.IsFlipped = false;
                }

                _firstSelected = null;
                _isProcessing = false;
            }
        }
    }

    public partial class MemoryCard : ObservableObject
    {
        public string MatchId { get; }
        public int Hour { get; } // 1-12
        public bool IsDigital { get; } // True = "01:00", False = Clock Face

        [ObservableProperty]
        private bool _isFlipped;

        [ObservableProperty]
        private bool _isMatched;

        public MemoryCard(string matchId, int hour, bool isDigital)
        {
            MatchId = matchId;
            Hour = hour;
            IsDigital = isDigital;
        }
    }
}
