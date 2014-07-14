using System;
using System.Globalization;
using System.Windows.Data;

namespace DocumentManage.Utility
{
    //DocumentManage.Utility 空间 -- 接口
    public class IconConverter : IValueConverter    //对象转换接口实现
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
                return null;
            return IconExtractor.GetIcon(string.Empty, true, false);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
