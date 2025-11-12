using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace GameAletheiaCross.Converters
{
    public class BooleanNegationConverter : IValueConverter
    {
        public static readonly BooleanNegationConverter Instance = new BooleanNegationConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            return Avalonia.Data.BindingOperations.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            return Avalonia.Data.BindingOperations.DoNothing;
        }
    }
}
