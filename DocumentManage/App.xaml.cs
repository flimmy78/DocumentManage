using System;
using System.Windows;
using System.Windows.Controls;

namespace DocumentManage
{
    public partial class App : Application
    {
        //Ӧ������
        public App()
        {
            //���
            if (IsRunningOutOfBrowser)  //���Ӧ�����ԣ� �Ƿ��������������ִ��
            {
                CheckAndDownloadUpdateCompleted += OnCheckAndDownloadUpdateComplated; //��������¼�
                CheckAndDownloadUpdateAsync(); //Ӧ�ó�Ա�����������첽�������ظ���Ӧ�ó���汾, 
            }
            //�����¼�����
            this.Startup += this.Application_Startup;   
            //�쳣�¼�����
            this.UnhandledException += this.Application_UnhandledException;
            InitializeComponent();
        }

        ////////////////////////////////////////ϵͳ�¼�����ľ���ʵ��////////////////////////////////////////////
        //��鲢���ظ������
        private void OnCheckAndDownloadUpdateComplated(object sender, CheckAndDownloadUpdateCompletedEventArgs e)
        {
            if (e.UpdateAvailable && e.Error == null)
            {
                MessageBox.Show("Ӧ���°汾�Ѿ����سɹ��������´�����ʱ��Ч��");
            }
            else if (e.Error != null)
            {
                MessageBox.Show("�ڼ��Ӧ�ø���ʱ, �������´�����Ϣ:" + Environment.NewLine + Environment.NewLine + e.Error.Message);
            }
        }

        //����Ӧ�ó���
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (Current.IsRunningOutOfBrowser)
            {
                this.MainWindow.WindowState = WindowState.Maximized;
                Utility.Utility.ClearTempLocalFolder();
            }
            this.RootVisual = new Login();   //��ʾ��¼��
        }

        //Ӧ�ó����쳣����
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
                errorWin.Show();    //��ʾ������
            }
        }
    }
}