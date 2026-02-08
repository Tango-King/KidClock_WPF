using CommunityToolkit.Mvvm.ComponentModel;
using Prism.Mvvm;
using System;
using System.Windows.Threading;

namespace KidClock.Modules.ClockFace.ViewModels
{
    public partial class ClockFaceViewModel : ObservableObject
    {
        private DispatcherTimer _timer;

        [ObservableProperty]
        private double _hourAngle;

        [ObservableProperty]
        private double _minuteAngle;

        [ObservableProperty]
        private double _secondAngle;

        [ObservableProperty]
        private string _digitalTime = string.Empty;

        public ClockFaceViewModel()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += OnTimerTick;
            _timer.Start();

            UpdateClock(DateTime.Now);
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            UpdateClock(DateTime.Now);
        }

        private void UpdateClock(DateTime time)
        {
            // 秒针：一圈60秒，每秒6度
            // 为了平滑移动，可以使用 Millisecond
            double totalSeconds = time.Second + time.Millisecond / 1000.0;
            SecondAngle = totalSeconds * 6;

            // 分针：一圈60分，每分6度。加上秒的偏移
            double totalMinutes = time.Minute + totalSeconds / 60.0;
            MinuteAngle = totalMinutes * 6;

            // 时针：一圈12小时，每小时30度。加上分的偏移
            double totalHours = (time.Hour % 12) + totalMinutes / 60.0;
            HourAngle = totalHours * 30;

            DigitalTime = time.ToString("HH:mm:ss");
        }
    }
}