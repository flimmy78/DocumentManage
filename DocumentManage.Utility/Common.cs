using System.Windows;
using System.Windows.Media;

namespace DocumentManage.Utility
{
    public static class Common
    {
        //泛型， 查找虚拟父目标
        public static T FindVisualParent<T>(DependencyObject obj) where T : class
        {
            while (obj != null)
            {
                if (obj is T)
                    return obj as T;

                obj = VisualTreeHelper.GetParent(obj);
            }

            return null;
        }
    }
}
