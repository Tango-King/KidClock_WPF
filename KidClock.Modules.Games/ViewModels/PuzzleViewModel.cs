using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Media;

namespace KidClock.Modules.Games.ViewModels
{
    public partial class PuzzleViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _statusMessage = "请将右侧的数字拖到表盘上的正确位置！";

        [ObservableProperty]
        private string _scoreDisplay = "进度: 0 / 12";

        [ObservableProperty]
        private bool _isComplete;

        [ObservableProperty]
        private Brush _statusColor = Brushes.Black;

        [ObservableProperty]
        private int _resetTrigger; // Acts as a signal for the View

        private int _currentScore = 0;

        public PuzzleViewModel()
        {
            ResetGame();
        }

        [RelayCommand]
        public void ResetGame()
        {
            _currentScore = 0;
            ScoreDisplay = "进度: 0 / 12";
            StatusMessage = "请将右侧的数字拖到表盘上的正确位置！";
            StatusColor = Brushes.Black;
            IsComplete = false;
            
            // Increment trigger to notify View
            ResetTrigger++;
        }

        public void IncrementScore()
        {
            _currentScore++;
            ScoreDisplay = $"进度: {_currentScore} / 12";

            if (_currentScore >= 12)
            {
                IsComplete = true;
                StatusMessage = "太棒了！你拼好了整个时钟！";
                StatusColor = Brushes.Green;
            }
        }
    }
}
