using System.Windows;

namespace DocumentManage.Utility
{
    //消息提示框
    public class CustomMessageBox
    {   
        //消息提示
        public static void Show(string str, MessageBoxImage icon)
        {
            MessageBox.Show(str, "系统提示", MessageBoxButton.OK, icon);
        }
        //消息提示
        public static void Show(string str)
        {
            Show(str, MessageBoxImage.Exclamation);
        }
        //询问
        public static bool Ask(string str)
        {
            return MessageBoxResult.OK == MessageBox.Show(str, "系统提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);
        }
    }
}
