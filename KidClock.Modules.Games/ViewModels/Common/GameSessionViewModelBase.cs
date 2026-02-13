using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace KidClock.Modules.Games.ViewModels.Common
{
    public enum GameMode
    {
        Practice = 0,
        Challenge = 1
    }

    public abstract partial class GameSessionViewModelBase : ObservableObject
    {
        private readonly DispatcherTimer _secondTimer;
        private readonly DispatcherTimer _feedbackTimer;
        private readonly Queue<bool> _recentResults = new();
        private const int RecentWindowSize = 10;
        private const int MinResultsBeforeAdjust = 5;

        [ObservableProperty]
        private GameMode _mode = GameMode.Practice;

        [ObservableProperty]
        private bool _isRunning;

        [ObservableProperty]
        private bool _isPaused;

        [ObservableProperty]
        private int _remainingSeconds;

        [ObservableProperty]
        private int _difficultyTier = 1;

        [ObservableProperty]
        private int _score;

        [ObservableProperty]
        private int _correctCount;

        [ObservableProperty]
        private int _totalCount;

        [ObservableProperty]
        private int _starRating;

        [ObservableProperty]
        private string _statusMessage = "准备开始";

        [ObservableProperty]
        private bool _showSuccessFeedback;

        [ObservableProperty]
        private bool _showHintFeedback;

        public double Accuracy => TotalCount <= 0 ? 0 : (double)CorrectCount / TotalCount;

        protected virtual int ChallengeDurationSeconds => 90;

        protected GameSessionViewModelBase()
        {
            _secondTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _secondTimer.Tick += (_, _) => OnSecondTick();

            _feedbackTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(450)
            };
            _feedbackTimer.Tick += (_, _) =>
            {
                _feedbackTimer.Stop();
                ShowSuccessFeedback = false;
                ShowHintFeedback = false;
            };
        }

        [RelayCommand]
        public void StartPractice()
        {
            Mode = GameMode.Practice;
            StartInternal();
        }

        [RelayCommand]
        public void StartChallenge()
        {
            Mode = GameMode.Challenge;
            RemainingSeconds = ChallengeDurationSeconds;
            StartInternal();
        }

        [RelayCommand]
        public void Pause()
        {
            if (!IsRunning || IsPaused) return;
            IsPaused = true;
            _secondTimer.Stop();
            OnPaused();
        }

        [RelayCommand]
        public void Resume()
        {
            if (!IsRunning || !IsPaused) return;
            IsPaused = false;
            if (Mode == GameMode.Challenge)
            {
                _secondTimer.Start();
            }
            OnResumed();
        }

        [RelayCommand]
        public void Reset()
        {
            StopInternal();
            DifficultyTier = 1;
            ClearProgress();
            StatusMessage = "准备开始";
            OnResetCore();
        }

        protected void RecordAnswer(bool isCorrect, int scoreDeltaIfCorrect)
        {
            TotalCount++;
            if (isCorrect)
            {
                CorrectCount++;
                Score += scoreDeltaIfCorrect;
            }

            _recentResults.Enqueue(isCorrect);
            while (_recentResults.Count > RecentWindowSize)
            {
                _recentResults.Dequeue();
            }

            TryAdjustDifficulty();
        }

        protected void TriggerSuccess()
        {
            ShowHintFeedback = false;
            ShowSuccessFeedback = true;
            _feedbackTimer.Stop();
            _feedbackTimer.Start();
        }

        protected void TriggerHint()
        {
            ShowSuccessFeedback = false;
            ShowHintFeedback = true;
            _feedbackTimer.Stop();
            _feedbackTimer.Start();
        }

        protected void EndSession(string message)
        {
            StopInternal();
            StarRating = CalculateStars();
            StatusMessage = message;
            OnSessionEnded();
        }

        private void StartInternal()
        {
            StopInternal();
            ClearProgress();
            IsRunning = true;
            IsPaused = false;
            StarRating = 0;
            StatusMessage = Mode == GameMode.Practice ? "练习模式 - 开始吧！" : "挑战模式 - 90秒倒计时！";

            if (Mode == GameMode.Challenge)
            {
                if (RemainingSeconds <= 0) RemainingSeconds = ChallengeDurationSeconds;
                _secondTimer.Start();
            }

            OnStartedCore();
        }

        private void StopInternal()
        {
            _secondTimer.Stop();
            IsRunning = false;
            IsPaused = false;
            OnStopped();
        }

        private void OnSecondTick()
        {
            if (!IsRunning || IsPaused) return;
            if (Mode != GameMode.Challenge) return;

            RemainingSeconds--;
            if (RemainingSeconds <= 0)
            {
                RemainingSeconds = 0;
                EndSession("挑战结束！");
            }
        }

        private void ClearProgress()
        {
            Score = 0;
            CorrectCount = 0;
            TotalCount = 0;
            StarRating = 0;
            _recentResults.Clear();
        }

        private void TryAdjustDifficulty()
        {
            if (_recentResults.Count < MinResultsBeforeAdjust) return;

            int correct = 0;
            foreach (var r in _recentResults)
            {
                if (r) correct++;
            }

            double recentAccuracy = (double)correct / _recentResults.Count;
            if (recentAccuracy >= 0.85)
            {
                DifficultyTier = Math.Min(3, DifficultyTier + 1);
                _recentResults.Clear();
                OnDifficultyChanged();
            }
            else if (recentAccuracy <= 0.60)
            {
                DifficultyTier = Math.Max(1, DifficultyTier - 1);
                _recentResults.Clear();
                OnDifficultyChanged();
            }
        }

        private int CalculateStars()
        {
            if (Mode != GameMode.Challenge) return 0;
            if (TotalCount <= 0) return 1;

            if (Accuracy >= 0.90 && TotalCount >= 8) return 3;
            if (Accuracy >= 0.75 && TotalCount >= 5) return 2;
            return 1;
        }

        protected virtual void OnStartedCore() { }
        protected virtual void OnResetCore() { }
        protected virtual void OnStopped() { }
        protected virtual void OnPaused() { }
        protected virtual void OnResumed() { }
        protected virtual void OnSessionEnded() { }
        protected virtual void OnDifficultyChanged() { }
    }
}
