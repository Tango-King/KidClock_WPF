using KidClock.Modules.Games.ViewModels.Logic;
using System;
using System.Globalization;
using System.Windows.Data;

namespace KidClock.Modules.Games.Converters
{
    public class MazeCommandToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MazeCommand cmd)
            {
                return cmd switch
                {
                    MazeCommand.Forward => "↑",
                    MazeCommand.TurnLeft => "↺",
                    MazeCommand.TurnRight => "↻",
                    _ => "?"
                };
            }
            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return MazeCommand.Forward;
        }
    }
}

