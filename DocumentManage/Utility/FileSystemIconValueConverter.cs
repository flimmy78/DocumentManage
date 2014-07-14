using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using DocumentManage.Entities;

namespace DocumentManage.Utility
{
    public class FileSystemIconValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fse = value as FileSystemEntity;
            string strUri = "/DocumentManage;component/Images/unknown.png";
            if (fse != null)
            {
                strUri = fse.Type == FileSystemEntityType.Folder
                    ? "/DocumentManage;component/Images/folder.png"
                    : string.Format("/DocumentManage;component/Images/{0}.png", fse.FileType.ToString().ToLower());
            }
            else
            {
                var file = value as Document;
                if (file != null)
                {
                    strUri =  string.Format("/DocumentManage;component/Images/{0}.png", file.FileType.ToString().ToLower());
                }
            }
            var bmp = new BitmapImage{UriSource = new Uri(strUri, UriKind.Relative)};
            return bmp;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
