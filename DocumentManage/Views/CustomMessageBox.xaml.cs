using System.Windows;
using System.Windows.Browser;

namespace DocumentManage.Views
{
    public partial class CustomMessageBox
    {
        public CustomMessageBox()
        {
            InitializeComponent();
        }

        public CustomMessageBox(string caption, string msg, bool isAsk)
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(caption))
                Title = caption;
            txtMessage.Text = msg;
            CancelButton.Visibility = isAsk ? Visibility.Visible : Visibility.Collapsed;
        }

        public static bool Ask(string caption, string msg)
        {
            return MessageBox.Show(msg, caption, MessageBoxButton.OKCancel) == MessageBoxResult.OK;
        }

        public static void Alert(string str)
        {
            Show("系统警告", str);
        }

        public static string Prompt(string caption)
        {
            if (Application.Current.IsRunningOutOfBrowser)
                return string.Empty;
            return HtmlPage.Window.Prompt(caption);
        }

        public static bool Ask(string msg)
        {
            return Ask(string.Empty, msg);
        }

        public static void Show(string caption, string msg)
        {
            Show(caption, msg, false);
        }

        public static void Show(string msg)
        {
            Show(string.Empty, msg);
        }

        private static void Show(string caption, string msg, bool isAsk)
        {
            var dialog = new CustomMessageBox(caption, msg, isAsk);
            dialog.Show();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

