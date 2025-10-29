using System;
using System.Globalization;
using System.Windows.Data;

namespace ArabicCompilerIDE
{
    public class BooleanToExecutionModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "🔧 حقيقي" : "🎮 محاكاة";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}