namespace OpenCvTemplateMatcher.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class DoubleConverter : IValueConverter
    {
        public static readonly DoubleConverter Default = new DoubleConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int i)
            {
                return 2 * i;
            }

            if (value is float f)
            {
                return 2 * f;
            }

            if (value is double d)
            {
                return 2 * d;
            }

            throw new ArgumentException(nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}