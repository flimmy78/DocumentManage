using System;
using System.Globalization;
using System.Windows.Data;

namespace DocumentManage.Utility
{
    public class EnumToStringValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
          object parameter, CultureInfo culture)
        {
            return EnumHelper.GetName(value.GetType(), value);
        }

        public object ConvertBack(object value, Type targetType,
          object parameter, CultureInfo culture)
        {
            return EnumHelper.GetValue(targetType, value.ToString());
        }
    }
}
