namespace OpenCvTemplateMatcher.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class HalfConverter : IValueConverter
    {
        public static readonly HalfConverter Default = new HalfConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int i)
            {
                return 0.5 * i;
            }

            if (value is float f)
            {
                return 0.5 * f;
            }

            if (value is double d)
            {
                return 0.5 * d;
            }

            throw new ArgumentException(nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
