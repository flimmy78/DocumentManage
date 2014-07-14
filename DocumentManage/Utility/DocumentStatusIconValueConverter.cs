using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using DocumentManage.Entities;

namespace DocumentManage.Utility
{
    public class DocumentStatusIconValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fse = value as Document;
            string strUri = string.Empty;
            if (fse != null)
            {
                strUri = string.Format("/DocumentManage;component/Images/fs_{0}.png", fse.Status.ToString().ToLower());
                return new BitmapImage { UriSource = new Uri(strUri, UriKind.Relative) };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
