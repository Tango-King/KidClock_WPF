using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;
using System.Windows.Media;

namespace KidClock.Modules.Games.ViewModels
{
    public partial class TimeDetectiveViewModel : ObservableObject
    {
        private Random _random = new Random();

        [ObservableProperty]
        private string _targetTimeDisplay = string.Empty;

        [ObservableProperty]
        private double _hourAngle;

        [ObservableProperty]
        private double _minuteAngle;

        [ObservableProperty]
        private string _feedbackMessage = string.Empty;

        [ObservableProperty]
        private Brush _feedbackColor;

        private TimeSpan _targetTime;
        private TimeSpan _currentTime;

        public TimeDetectiveViewModel()
        {
            FeedbackColor = Brushes.Black;
            GenerateNewLevel();
        }

        [RelayCommand]
        private void GenerateNewLevel()
        {
            // 随机生成时间 (只生成整分或5分倍数，降低难度)
            int hour = _random.Next(1, 13);
            int minute = _random.Next(0, 12) * 5; 
            _targetTime = new TimeSpan(hour, minute, 0);
            TargetTimeDisplay = DateTime.Today.Add(_targetTime).ToString("hh:mm");

            // 重置当前时间为 12:00
            _currentTime = new TimeSpan(12, 0, 0);
            UpdateAngles();
            
            FeedbackMessage = "请拖动指针，将时钟拨到上面的时间！";
            FeedbackColor = Brushes.Black;
        }

        [RelayCommand]
        private void CheckAnswer()
        {
            // 允许误差 2分钟
            double diff = Math.Abs((_currentTime.TotalMinutes % 720) - (_targetTime.TotalMinutes % 720));
            if (diff > 710) diff = 720 - diff; // Handle wrap around

            if (diff <= 2)
            {
                FeedbackMessage = "太棒了！你答对了！";
                FeedbackColor = Brushes.Green;
                // 播放音效 TODO
            }
            else
            {
                FeedbackMessage = "还没对哦，再试一次吧！";
                FeedbackColor = Brushes.Red;
            }
        }

        public void AddMinutes(double minutes)
        {
            _currentTime = _currentTime.Add(TimeSpan.FromMinutes(minutes));
            UpdateAngles();
        }

        private void UpdateAngles()
        {
            double totalHours = _currentTime.TotalHours;
            double totalMinutes = _currentTime.TotalMinutes;

            MinuteAngle = totalMinutes * 6; // 360 / 60 = 6
            HourAngle = (totalHours % 12) * 30; // 360 / 12 = 30
        }
    }
}