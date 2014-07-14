using System;
using System.Windows;
using System.Windows.Controls;

namespace DocumentManage
{
    public partial class App : Application
    {
        //应用启动
        public App()
        {
            //检查
            if (IsRunningOutOfBrowser)  //检查应用属性： 是否允许在浏览器外执行
            {
                CheckAndDownloadUpdateCompleted += OnCheckAndDownloadUpdateComplated; //更新完成事件
                CheckAndDownloadUpdateAsync(); //应用成员函数：启动异步进程下载更新应用程序版本, 
            }
            //启动事件处理
            this.Startup += this.Application_Startup;   
            //异常事件处理
            this.UnhandledException += this.Application_UnhandledException;
            InitializeComponent();
        }

        ////////////////////////////////////////系统事件句柄的具体实现////////////////////////////////////////////
        //检查并下载更新完成
        private void OnCheckAndDownloadUpdateComplated(object sender, CheckAndDownloadUpdateCompletedEventArgs e)
        {
            if (e.UpdateAvailable && e.Error == null)
            {
                MessageBox.Show("应用新版本已经下载成功，将在下次启动时生效。");
            }
            else if (e.Error != null)
            {
                MessageBox.Show("在检测应用更新时, 出现以下错误信息:" + Environment.NewLine + Environment.NewLine + e.Error.Message);
            }
        }

        //启动应用程序
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (Current.IsRunningOutOfBrowser)
            {
                this.MainWindow.WindowState = WindowState.Maximized;
                Utility.Utility.ClearTempLocalFolder();
            }
            this.RootVisual = new Login();   //显示登录框
        }

        //应用程序异常处理
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // If the app is running outside of the debugger then report the exception using
            // a ChildWindow control.
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                // NOTE: This will allow the application to continue running after an exception has been thrown
                // but not handled. 
                // For production applications this error handling should be replaced with something that will 
                // report the error to the website and stop the application.
                e.Handled = true;
                ChildWindow errorWin = new ErrorWindow(e.ExceptionObject);
                errorWin.Show();    //显示出错窗口
            }
        }
    }
}