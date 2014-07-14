using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ServiceModel.DomainServices.Client;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DocumentManage.Entities;
using DocumentManage.Utility;
using DocumentManage.Views;
using DocumentManageService.Web;

namespace DocumentManage
{
    public partial class MainPage : UserControl
    {
        //�����ڳ�ʼ��
        public MainPage()
        {
            InitializeComponent();
            MouseRightButtonDown += (o, e) => { e.Handled = true; };
            //Ӧ�ó��������������ⲿִ���ҵ�ǰӦ�ý������
            if (Application.Current.IsRunningOutOfBrowser || Application.Current.InstallState == InstallState.Installed)
            {
                btnInstall.Visibility = Visibility.Collapsed;
            }
        }

        //�����û�ģ��, ��ͬ�û�Ȩ�޲�һ�������صĹ���ģ��Ҳ��һ��
        private void LoadNavModules()
        {
            var context = new SystemModuleDomainContext();
            //��ȡ�û�ģ�飬���ص�����
            context.GetUserModuleList("10", AuthenticateStatus.CurrentUser.UserId, OnGetNavModuleCompleted, null);
        }

        //��ȡ�û�ģ����ɣ��ص�
        private void OnGetNavModuleCompleted(InvokeOperation<List<SystemModule>> obj)
        {
            if (Utility.Utility.CheckInvokeOperation(obj)){     //�������Ƿ���ȷִ��
                foreach (SystemModule module in obj.Value){   //�����û���Ӧ��ģ�飬 ��ͬ���û�Ȩ�޿ɲ鿴��ģ������һ��
                    var btn = new HyperlinkButton();                            //�½�һ�������Ӱ�ť
                    btn.Style = Application.Current.Resources["LinkStyle"] as Style;//����Ϊ��������
                    btn.Content = module.ModuleName;  //��ʾģ����
                    Debug.WriteLine("ModuleName: " + btn.Content);
                    btn.DataContext = module;        //��ģ������
                    Debug.WriteLine("Uri: " + module.NavigateUri);
                    if (!string.IsNullOrEmpty(module.NavigateUri)){  //ģ���NavigateUri���Էǿ�
                        btn.NavigateUri = new Uri(module.NavigateUri, UriKind.Relative); //��ģ���Uri����һ����Ե����ӵ�ַ����ť��
                        btn.TargetName = "ContentFrame"; //Ŀ����
                    }
                    btn.Click += OnNavLinkButtonClick;   //���ӵ����¼�������󵼺�����Ӧ�Ľ���
                    //���û�ģ�鰴ť��ӵ�"��ҳ"��"����֮��"��NavLinksStackPanel.Children.Count-1 -- ������2����ť���
                    NavLinksStackPanel.Children.Insert(NavLinksStackPanel.Children.Count-1, btn); //�������Ӱ�ť��ӵ�StackPanel�ؼ�
                }
            }
        }

        //��ʾ���˵�,��ʾ��ҳ����Ӳ˵�
        private void ShowLeftMenu(bool show)
        {
            Debug.WriteLine("show"+show);
            LeftMenuBackgroundBorder.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            LinksStackPanel.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            Grid.SetColumn(ContentBorder, show ? 1 : 0);
            Grid.SetColumnSpan(ContentBorder, show ? 1 : 2);
        }

        //��ȡ�û�ģ����ģ����ɣ��ص�
        private void OnGetSubModuleCompleted(InvokeOperation<List<SystemModule>> obj)
        {
            if (Utility.Utility.CheckInvokeOperation(obj)){  //�������Ƿ���ȷִ��
                LinksStackPanel.Children.Clear();   //
                var navUrl = string.Empty;
                foreach (SystemModule module in obj.Value){
                    if (string.IsNullOrEmpty(navUrl)) navUrl = module.NavigateUri; //ģ���еĵ�����ַ�Ƿ�Ϊ��
                    var btn = new HyperlinkButton();
                    btn.Style = Application.Current.Resources["SubLinkStyle"] as Style; //����Ϊ����������
                    btn.Content = module.ModuleName;    //����ģ������
                    btn.DataContext = module;           //��ģ������
                    if (!string.IsNullOrEmpty(module.NavigateUri)) btn.NavigateUri = new Uri(module.NavigateUri, UriKind.Relative);
                    btn.TargetName = "ContentFrame";
                    LinksStackPanel.Children.Add(btn);  //��ӵ�StackPanel��
                }
                //��ContentFrame��������Ե�ַ������ʾ
                if (!string.IsNullOrEmpty(navUrl)) ContentFrame.Navigate(new Uri(navUrl, UriKind.Relative));
            }
        }

        ///////////////////////////////////////////////////�¼�����////////////////////////////////////////////////////
        //Navigated���������¼�����
        // After the Frame navigates, ensure the HyperlinkButton representing the current page is selected
        //����(navigation)��� -- ��������¼�, Frame���������Ӧҳ����ʾ����
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            //����StackPanel����NavLinksStackPanel�е��Ӳ���,��������ҳ�����͡�������2�������Ӱ�ť
            foreach (UIElement child in NavLinksStackPanel.Children){
                var hb = child as HyperlinkButton;  //�����Ӱ�ť
                if (hb != null && hb.NavigateUri != null){
                    if (ContentFrame.UriMapper.MapUri(e.Uri).ToString().Equals(ContentFrame.UriMapper.MapUri(hb.NavigateUri).ToString())){
                        VisualStateManager.GoToState(hb, "ActiveLink", true); //��ť����
                    }else{
                        VisualStateManager.GoToState(hb, "InactiveLink", true); //��ť���
                    }
                }
            }
            //����StackPanel����LinksStackPanel�е��Ӳ����� ����һ��navigation������������Frame
            foreach (UIElement child in LinksStackPanel.Children){
                var hb = child as HyperlinkButton;  //�����Ӱ�ť
                if (hb != null && hb.NavigateUri != null){
                    if (ContentFrame.UriMapper.MapUri(e.Uri).ToString().Equals(ContentFrame.UriMapper.MapUri(hb.NavigateUri).ToString())){
                        VisualStateManager.GoToState(hb, "ActiveLink", true); //��ť����
                    }else{
                        VisualStateManager.GoToState(hb, "InactiveLink", true); //��ť���
                    }
                }
            }
        }

        //����(navigation)��� -- ����ʧ���¼�
        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            e.Handled = true;
            ChildWindow errorWin = new ErrorWindow(e.Uri);

            errorWin.Show();
        }

        //MainPage�����¼�
        private void OnPageLoaded(object sender, EventArgs e)
        {
            lblWelcome.Content = string.Format("��ӭ��{0}", AuthenticateStatus.CurrentUser.RealName);
            //Application.Current.Host.Content.IsFullScreen = false;
            LoadNavModules();
            ShowLeftMenu(false);  //Ĭ�ϵĲ˵�
        }

        //�����Ӱ�ť�����¼�, ��������Ӧ��ҳ��
        private void OnNavLinkButtonClick(object sender, RoutedEventArgs e)
        {
            var link = sender as HyperlinkButton;  //��ȡ�����Ӱ�ť
            if (link == null) return;

            foreach (UIElement child in NavLinksStackPanel.Children){  //StackPanel�в��ҳ����Ӱ�ť
                var hb = child as HyperlinkButton;  
                if (hb != null){  // �ҵ���Ч�ĳ����Ӱ�ť�����ü���򲻻״̬
                    VisualStateManager.GoToState(hb, ReferenceEquals(hb, link) ? "ActiveLink" : "InactiveLink", true);
                }
            }

            if (link.NavigateUri != null){  //��ť�ĵ�����ַ��Ϊ��
                ShowLeftMenu(false);
            }else{
                ShowLeftMenu(true);    //��ʾ������Ŀ
                var module = link.DataContext as SystemModule; //��ȡ�����Ӱ󶨵�ģ������
                if (module != null){
                    var context = new SystemModuleDomainContext(); //��ȡģ�����ݣ����ص�!!!!!!!!!!!!!!!!!!!!  ��ģ����� 
                    context.GetUserModuleList(module.ModuleId, AuthenticateStatus.CurrentUser.UserId, OnGetSubModuleCompleted, null);
                }
            }
        }

        //ע���¼�
        private void OnLogoutButtonClick(object sender, RoutedEventArgs e)
        {
            if (!CustomMessageBox.Ask("ȷ��Ҫ�˳�ϵͳ��")) return;
            if (Application.Current.IsRunningOutOfBrowser){
                Application.Current.MainWindow.Close();
            }else{
                ContentFrame.Navigate(new Uri("", UriKind.Relative));
                AuthenticateStatus.CurrentUser = null;
                Content = new Login();
            }
        }

        //��װ�¼�
        private void OnInstallButtonClick(object sender, RoutedEventArgs e)
        {
            btnInstall.IsEnabled = false;
            if (Application.Current.InstallState == InstallState.NotInstalled || Application.Current.InstallState == InstallState.InstallFailed)
                Application.Current.Install();
        }

    }
}