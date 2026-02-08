using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace KidClock.Modules.Education.ViewModels
{
    public partial class TimeCognitionViewModel : ObservableObject
    {
        [ObservableProperty]
        private double _timeSliderValue = 6.0; // Default to 6:00 AM

        [ObservableProperty]
        private string _timeDisplay = "06:00";

        [ObservableProperty]
        private string _periodName = ""; // Initialize empty to trigger update

        [ObservableProperty]
        private string _periodDescription = "太阳升起来了，新的一天开始啦！";

        [ObservableProperty]
        private Brush _backgroundBrush = Brushes.LightSkyBlue; // Placeholder for dynamic background

        [ObservableProperty]
        private ObservableCollection<ActivityItem> _currentActivities = new ObservableCollection<ActivityItem>();

        public TimeCognitionViewModel()
        {
            UpdateTimeState();
        }

        partial void OnTimeSliderValueChanged(double value)
        {
            UpdateTimeState();
        }

        private void UpdateTimeState()
        {
            // Format time string
            int hours = (int)TimeSliderValue;
            int minutes = (int)((TimeSliderValue - hours) * 60);
            TimeDisplay = $"{hours:D2}:{minutes:D2}";

            // Determine Period
            if (hours >= 6 && hours < 12)
            {
                SetMorningState();
            }
            else if (hours >= 12 && hours < 15)
            {
                SetNoonState();
            }
            else if (hours >= 15 && hours < 19)
            {
                SetAfternoonState();
            }
            else
            {
                // Night time fallback (though requirements say 6-18:59)
                PeriodName = "晚上";
                PeriodDescription = "月亮出来了，该睡觉了。";
                BackgroundBrush = Brushes.DarkBlue;
                CurrentActivities.Clear();
                CurrentActivities.Add(new ActivityItem("睡觉", "🛏️"));
            }
        }

        private void SetMorningState()
        {
            if (PeriodName != "早上")
            {
                PeriodName = "早上";
                PeriodDescription = "太阳公公咪咪笑，鸟儿枝头喳喳叫。";
                // Morning Gradient (Orange to Blue)
                BackgroundBrush = new LinearGradientBrush(
                    new GradientStopCollection { 
                        new GradientStop(Colors.LightSkyBlue, 0), 
                        new GradientStop(Colors.PeachPuff, 1) 
                    }, 90);
                
                CurrentActivities.Clear();
                CurrentActivities.Add(new ActivityItem("起床", "🌅"));
                CurrentActivities.Add(new ActivityItem("刷牙", "🪥"));
                CurrentActivities.Add(new ActivityItem("吃早餐", "🥣"));
                CurrentActivities.Add(new ActivityItem("上学", "🎒"));
            }
        }

        private void SetNoonState()
        {
            if (PeriodName != "中午")
            {
                PeriodName = "中午";
                PeriodDescription = "太阳当空照，知了声声叫。";
                // Noon Gradient (Bright Blue to Yellow)
                BackgroundBrush = new LinearGradientBrush(
                    new GradientStopCollection { 
                        new GradientStop(Colors.DeepSkyBlue, 0), 
                        new GradientStop(Colors.LightYellow, 1) 
                    }, 90);

                CurrentActivities.Clear();
                CurrentActivities.Add(new ActivityItem("吃午餐", "🍱"));
                CurrentActivities.Add(new ActivityItem("午睡", "😴"));
                CurrentActivities.Add(new ActivityItem("阅读", "📖"));
            }
        }

        private void SetAfternoonState()
        {
            if (PeriodName != "下午")
            {
                PeriodName = "下午";
                PeriodDescription = "太阳慢慢下山了，放学的铃声响起了。";
                // Afternoon Gradient (Blue to Orange/Red)
                BackgroundBrush = new LinearGradientBrush(
                    new GradientStopCollection { 
                        new GradientStop(Colors.SkyBlue, 0), 
                        new GradientStop(Colors.OrangeRed, 1) 
                    }, 90);

                CurrentActivities.Clear();
                CurrentActivities.Add(new ActivityItem("放学", "🏫"));
                CurrentActivities.Add(new ActivityItem("写作业", "✍️"));
                CurrentActivities.Add(new ActivityItem("游戏", "⚽"));
                CurrentActivities.Add(new ActivityItem("看动画", "📺"));
            }
        }
    }

    public class ActivityItem
    {
        public string Name { get; set; }
        public string Icon { get; set; } // Using Emoji as placeholder for icons

        public ActivityItem(string name, string icon)
        {
            Name = name;
            Icon = icon;
        }
    }
}
