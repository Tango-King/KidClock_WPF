using System;
using System.Globalization;
using System.Windows.Data;

namespace KidClock.Modules.Games.Converters
{
    public class StarsToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int stars)
            {
                stars = Math.Max(0, Math.Min(3, stars));
                return new string('★', stars) + new string('☆', 3 - stars);
            }
            return "☆☆☆";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}

