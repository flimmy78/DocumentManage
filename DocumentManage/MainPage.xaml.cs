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
        //主窗口初始化
        public MainPage()
        {
            InitializeComponent();
            MouseRightButtonDown += (o, e) => { e.Handled = true; };
            //应用程序可以再浏览器外部执行且当前应用建立完成
            if (Application.Current.IsRunningOutOfBrowser || Application.Current.InstallState == InstallState.Installed)
            {
                btnInstall.Visibility = Visibility.Collapsed;
            }
        }

        //加载用户模块, 不同用户权限不一样，加载的功能模块也不一样
        private void LoadNavModules()
        {
            var context = new SystemModuleDomainContext();
            //获取用户模块，并回调处理
            context.GetUserModuleList("10", AuthenticateStatus.CurrentUser.UserId, OnGetNavModuleCompleted, null);
        }

        //获取用户模块完成，回调
        private void OnGetNavModuleCompleted(InvokeOperation<List<SystemModule>> obj)
        {
            if (Utility.Utility.CheckInvokeOperation(obj)){     //检查操作是否正确执行
                foreach (SystemModule module in obj.Value){   //查找用户对应的模块， 不同的用户权限可查看的模块数不一样
                    var btn = new HyperlinkButton();                            //新建一个超链接按钮
                    btn.Style = Application.Current.Resources["LinkStyle"] as Style;//设置为链接类型
                    btn.Content = module.ModuleName;  //显示模块名
                    Debug.WriteLine("ModuleName: " + btn.Content);
                    btn.DataContext = module;        //绑定模块数据
                    Debug.WriteLine("Uri: " + module.NavigateUri);
                    if (!string.IsNullOrEmpty(module.NavigateUri)){  //模块的NavigateUri属性非空
                        btn.NavigateUri = new Uri(module.NavigateUri, UriKind.Relative); //以模块的Uri创建一个相对的链接地址到按钮上
                        btn.TargetName = "ContentFrame"; //目标名
                    }
                    btn.Click += OnNavLinkButtonClick;   //增加单击事件，点击后导航到相应的界面
                    //将用户模块按钮添加到"首页"和"帮助之间"，NavLinksStackPanel.Children.Count-1 -- 倒数第2个按钮编号
                    NavLinksStackPanel.Children.Insert(NavLinksStackPanel.Children.Count-1, btn); //将超链接按钮添加到StackPanel控件
                }
            }
        }

        //显示左侧菜单,显示该页面的子菜单
        private void ShowLeftMenu(bool show)
        {
            Debug.WriteLine("show"+show);
            LeftMenuBackgroundBorder.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            LinksStackPanel.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            Grid.SetColumn(ContentBorder, show ? 1 : 0);
            Grid.SetColumnSpan(ContentBorder, show ? 1 : 2);
        }

        //获取用户模块子模块完成，回调
        private void OnGetSubModuleCompleted(InvokeOperation<List<SystemModule>> obj)
        {
            if (Utility.Utility.CheckInvokeOperation(obj)){  //检查操作是否正确执行
                LinksStackPanel.Children.Clear();   //
                var navUrl = string.Empty;
                foreach (SystemModule module in obj.Value){
                    if (string.IsNullOrEmpty(navUrl)) navUrl = module.NavigateUri; //模块中的导航地址是否为空
                    var btn = new HyperlinkButton();
                    btn.Style = Application.Current.Resources["SubLinkStyle"] as Style; //设置为子链接类型
                    btn.Content = module.ModuleName;    //设置模块名称
                    btn.DataContext = module;           //绑定模块数据
                    if (!string.IsNullOrEmpty(module.NavigateUri)) btn.NavigateUri = new Uri(module.NavigateUri, UriKind.Relative);
                    btn.TargetName = "ContentFrame";
                    LinksStackPanel.Children.Add(btn);  //添加到StackPanel中
                }
                //由ContentFrame导航到相对地址，并显示
                if (!string.IsNullOrEmpty(navUrl)) ContentFrame.Navigate(new Uri(navUrl, UriKind.Relative));
            }
        }

        ///////////////////////////////////////////////////事件处理////////////////////////////////////////////////////
        //Navigated（导航）事件处理
        // After the Frame navigates, ensure the HyperlinkButton representing the current page is selected
        //导航(navigation)框架 -- 导航完成事件, Frame窗体承载相应页面显示内容
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            //查找StackPanel部件NavLinksStackPanel中的子部件,包含“首页”，和“帮助”2个超链接按钮
            foreach (UIElement child in NavLinksStackPanel.Children){
                var hb = child as HyperlinkButton;  //超链接按钮
                if (hb != null && hb.NavigateUri != null){
                    if (ContentFrame.UriMapper.MapUri(e.Uri).ToString().Equals(ContentFrame.UriMapper.MapUri(hb.NavigateUri).ToString())){
                        VisualStateManager.GoToState(hb, "ActiveLink", true); //按钮激活
                    }else{
                        VisualStateManager.GoToState(hb, "InactiveLink", true); //按钮不活动
                    }
                }
            }
            //查找StackPanel部件LinksStackPanel中的子部件， 包含一个navigation（导航）窗体Frame
            foreach (UIElement child in LinksStackPanel.Children){
                var hb = child as HyperlinkButton;  //超链接按钮
                if (hb != null && hb.NavigateUri != null){
                    if (ContentFrame.UriMapper.MapUri(e.Uri).ToString().Equals(ContentFrame.UriMapper.MapUri(hb.NavigateUri).ToString())){
                        VisualStateManager.GoToState(hb, "ActiveLink", true); //按钮激活
                    }else{
                        VisualStateManager.GoToState(hb, "InactiveLink", true); //按钮不活动
                    }
                }
            }
        }

        //导航(navigation)框架 -- 导航失败事件
        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            e.Handled = true;
            ChildWindow errorWin = new ErrorWindow(e.Uri);

            errorWin.Show();
        }

        //MainPage加载事件
        private void OnPageLoaded(object sender, EventArgs e)
        {
            lblWelcome.Content = string.Format("欢迎：{0}", AuthenticateStatus.CurrentUser.RealName);
            //Application.Current.Host.Content.IsFullScreen = false;
            LoadNavModules();
            ShowLeftMenu(false);  //默认的菜单
        }

        //超链接按钮单击事件, 导航到相应的页面
        private void OnNavLinkButtonClick(object sender, RoutedEventArgs e)
        {
            var link = sender as HyperlinkButton;  //获取超链接按钮
            if (link == null) return;

            foreach (UIElement child in NavLinksStackPanel.Children){  //StackPanel中查找超链接按钮
                var hb = child as HyperlinkButton;  
                if (hb != null){  // 找到有效的超链接按钮，设置激活或不活动状态
                    VisualStateManager.GoToState(hb, ReferenceEquals(hb, link) ? "ActiveLink" : "InactiveLink", true);
                }
            }

            if (link.NavigateUri != null){  //按钮的导航地址不为空
                ShowLeftMenu(false);
            }else{
                ShowLeftMenu(true);    //显示导航项目
                var module = link.DataContext as SystemModule; //获取超链接绑定的模块数据
                if (module != null){
                    var context = new SystemModuleDomainContext(); //获取模块内容，并回调!!!!!!!!!!!!!!!!!!!!  子模块加载 
                    context.GetUserModuleList(module.ModuleId, AuthenticateStatus.CurrentUser.UserId, OnGetSubModuleCompleted, null);
                }
            }
        }

        //注销事件
        private void OnLogoutButtonClick(object sender, RoutedEventArgs e)
        {
            if (!CustomMessageBox.Ask("确定要退出系统吗？")) return;
            if (Application.Current.IsRunningOutOfBrowser){
                Application.Current.MainWindow.Close();
            }else{
                ContentFrame.Navigate(new Uri("", UriKind.Relative));
                AuthenticateStatus.CurrentUser = null;
                Content = new Login();
            }
        }

        //安装事件
        private void OnInstallButtonClick(object sender, RoutedEventArgs e)
        {
            btnInstall.IsEnabled = false;
            if (Application.Current.InstallState == InstallState.NotInstalled || Application.Current.InstallState == InstallState.InstallFailed)
                Application.Current.Install();
        }

    }
}