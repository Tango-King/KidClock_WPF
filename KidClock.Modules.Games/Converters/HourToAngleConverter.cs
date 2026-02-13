using System;
using System.Globalization;
using System.Windows.Data;

namespace KidClock.Modules.Games.Converters
{
    public class HourToAngleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int hour)
            {
                // Hour 12 -> 0 degrees (or 360)
                // Hour 3 -> 90 degrees
                // Formula: (hour % 12) * 30
                return (hour % 12) * 30.0;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
