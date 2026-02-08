using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Threading;

namespace KidClock.Modules.Education.ViewModels
{
    public partial class TimeRelationViewModel : ObservableObject
    {
        private DispatcherTimer _timer;
        private double _speedMultiplier = 1.0;
        private DateTime _currentTime;

        [ObservableProperty]
        private double _hourAngle;

        [ObservableProperty]
        private double _minuteAngle;

        [ObservableProperty]
        private double _secondAngle;

        [ObservableProperty]
        private string _statusMessage;

        [ObservableProperty]
        private bool _isTrailVisible;

        public TimeRelationViewModel()
        {
            _currentTime = DateTime.Now.Date + new TimeSpan(12, 0, 0); // Start at 12:00:00
            UpdateAngles();
            StatusMessage = "点击按钮开始演示时间魔法！";

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(16); // ~60fps
            _timer.Tick += OnTimerTick;
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            // 增加时间
            // 基础速度是实时的，但在演示模式下我们需要加速
            // 比如 60倍速 (1秒实际时间 = 1分钟虚拟时间)
            
            double elapsedSeconds = _timer.Interval.TotalSeconds * _speedMultiplier;
            _currentTime = _currentTime.AddSeconds(elapsedSeconds);
            UpdateAngles();
        }

        private void UpdateAngles()
        {
            // 毫秒级别的平滑移动
            // double totalSeconds = _currentTime.Second + _currentTime.Millisecond / 1000.0;
            // SecondAngle = totalSeconds * 6; // 0-360 per minute
            
            // 重新计算累积角度
            double totalHours = (_currentTime - DateTime.Today).TotalHours;
            
            // 秒针：每分钟转一圈(360度)。
            // SecondAngle = TotalSeconds * 6
            // 使用 double 类型 TotalSeconds 来保证毫秒级平滑
            double totalSeconds = (_currentTime - DateTime.Today).TotalSeconds;
            SecondAngle = totalSeconds * 6;

            // 分针：每小时转一圈(360度)。
            MinuteAngle = totalHours * 360;

            // 时针：每12小时转一圈(360度)。
            HourAngle = (totalHours / 12.0) * 360;
        }

        [RelayCommand]
        private void StartOneMinuteMagic()
        {
            // 演示：秒针转1圈，分针走1格
            // 我们可以加速时间，使得这个过程在 2-3 秒内完成
            _speedMultiplier = 20; // 20倍速 -> 1分钟只需 3秒
            StatusMessage = "正在加速... 观察秒针和分针的关系！";
            _timer.Start();
        }

        [RelayCommand]
        private void StartOneHourMagic()
        {
            // 演示：分针转1圈，时针走1大格
            // 需要更快，比如 1小时在 5秒内完成
            // 1小时 = 3600秒。 3600 / 5 = 720倍速
            _speedMultiplier = 720;
            StatusMessage = "超级加速！观察分针和时针的关系！";
            _timer.Start();
        }

        [RelayCommand]
        private void StopMagic()
        {
            _timer.Stop();
            StatusMessage = "时间暂停。";
            _speedMultiplier = 1.0;
        }

        [RelayCommand]
        private void ResetTime()
        {
            StopMagic();
            _currentTime = DateTime.Now.Date + new TimeSpan(12, 0, 0);
            UpdateAngles();
            StatusMessage = "时间已重置为 12:00";
        }
        
        [RelayCommand]
        private void ToggleTrails()
        {
            IsTrailVisible = !IsTrailVisible;
        }
    }
}