using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.ServiceModel.DomainServices.Client;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Input;
using DocumentManage.Entities;
using DocumentManage.Utility;
using DocumentManage.Views;
using DocumentManageService.Web;

namespace DocumentManage
{
    public partial class Login : UserControl
    {
        //申请一个域用户类实例
        private readonly SystemUserDomainContext userContext = new SystemUserDomainContext();
        //构造
        public Login()
        {
            InitializeComponent();    //初始化窗体
            KeyUp += OnPageKeyUp;      //添加按键离开事件
            MouseRightButtonDown += (o, e) => { e.Handled = true; }; //右键按键事件
        }

        /////////////////////////////////事件处理///////////////////////////////////////////
        //点击取消
        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            if (Application.Current.IsRunningOutOfBrowser){
                Application.Current.MainWindow.Close();     //关闭应用程序窗口
            }else{
                HtmlPage.Window.Eval("window.close()");     //关闭浏览器当前窗口
            }
        }
        //点击登陆
        private void OnLoginButtonClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtUserName.Text) || string.IsNullOrEmpty(txtPassword.Password)){
                //CustomMessageBox.Show("请输入用户名和密码");
                MessageBox.Show("请输入用户名和密码");
                return;
            }

            busyIndicator1.IsBusy = true;     //登录进度条忙
            btnLogin.IsEnabled = false;       //登录按钮失效

            if (IsolatedStorageFile.IsEnabled) {    //独立存储使能
                using (var storage = IsolatedStorageFile.GetUserStoreForSite()) { //获取独立存储器
                    if (true == chkRemmeberMe.IsChecked){  //单选框选中
                        using (var stream = storage.CreateFile("userInfo.txt")){  
                            var sw = new StreamWriter(stream);        //写入用户名和密码
                            sw.WriteLine(txtUserName.Text);
                            sw.WriteLine(txtPassword.Password);
                            sw.Flush();
                            sw.Close();
                        }
                    }else{
                        if (storage.FileExists("userInfo.txt")){    //如果文件村子则删除
                            storage.DeleteFile("userInfo.txt");
                        }
                    }
                }
            }

            /////////////////////通过域用户类SystemUserDomainContext的Login()方法登录服务器 !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            userContext.Login(txtUserName.Text, txtPassword.Password, OnLoginCompleted, null);
        }
        //登陆完成操作
        private void OnLoginCompleted(InvokeOperation<SystemUser> ortn)
        {
            busyIndicator1.IsBusy = false;      //登录进度条空闲
            btnLogin.IsEnabled = true;          //登录按钮有效
            //Debug.WriteLine("x"); 
            if (Utility.Utility.CheckInvokeOperation(ortn)){ //调用Utility空间Utility类的CheckInvokeOperation（）方法
                if (ortn.Value != null){         
                    AuthenticateStatus.CurrentUser = ortn.Value;   //成功登录,返回当前的用户!!!!!!!!!!!!
                    Debug.WriteLine(AuthenticateStatus.CurrentUser.UserName);   //打印出登录的用户名
                    Debug.WriteLine(AuthenticateStatus.CurrentUser.UserPassword); //打印出登录的密码     
                    Debug.WriteLine(AuthenticateStatus.CurrentUser.RealName); //打印出登录的真实名字 
                    GetUserModules();           //加载用户模块 !!!!!!!!!!!!!!!!!!!!!!!!!!
                }else{
                    CustomMessageBox.Show("用户名或者密码错误!");
                }
            }
        }
        //按键离开
        private void OnPageKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter){  //判断如果是回车键，执行登录操作

                OnLoginButtonClick(this, null);
            }
        }
        //用户登录控件加载事件, 如果已经登录则自动加载之前的用户名和密码
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsolatedStorageFile.IsEnabled){  //如果使能了独立存储
                using (var storage = IsolatedStorageFile.GetUserStoreForSite()){
                    if (storage.FileExists("userInfo.txt")){        //判断"userInfo.txt"是否存在
                        using(var stream = storage.OpenFile("userInfo.txt", FileMode.Open)){  //打开并且读取用户名和密码
                            var sr = new StreamReader(stream);
                            txtUserName.Text = sr.ReadLine();
                            txtPassword.Password = sr.ReadLine();
                            sr.Close();
                        }
                        chkRemmeberMe.IsChecked = true;         //使能单选框
                    }
                }
            }
        }
        //////////////////////////////////其他数据的加载//////////////////////////////
        //AuthenticateStatus 自定义用户验证类
        //SystemModuleDomainContext，SystemUserDomainContext，都继承自DomainContext, 通过这两个子类获取用户模型和用户角色，并加入到AuthenticateStatus类中
        //获取用户模型
        private void GetUserModules()
        {
            if (AuthenticateStatus.HasLogin){ //已登录 
                //通过用户ID，使用类DomainContext来获取用户模型
                new SystemModuleDomainContext().GetUserModuleList(string.Empty, AuthenticateStatus.CurrentUser.UserId, obj =>
                {
                    if (Utility.Utility.CheckInvokeOperation(obj)){ //检查调用操作是否完成
                        AuthenticateStatus.UserModules = obj.Value;   
                        GetUserRoles();    //获取用户组
                    }
                }, null);
            }
        }
 
        //获取用户角色
        private void GetUserRoles()
        {
            if (AuthenticateStatus.HasLogin){  //已登录 
                //通过用户ID，使用类DomainContext获取用户角色
                userContext.GetUserRoleRel(AuthenticateStatus.CurrentUser.UserId, obj =>
                //new SystemUserDomainContext().GetUserRoleRel(AuthenticateStatus.CurrentUser.UserId, obj =>
                {
                    if (Utility.Utility.CheckInvokeOperation(obj)){ //检查调用操作是否完成
                        AuthenticateStatus.UserRoles = obj.Value;
                        GetUserOrgs();   //获取用户组
                    }
                }, null);
            }
        }
        //获取用户组
        private void GetUserOrgs()
        {
            if (AuthenticateStatus.HasLogin){ //已登录 
                //通过用户ID，使用类DomainContext获取用户组织
                userContext.GetUserOrgRel(AuthenticateStatus.CurrentUser.UserId, obj =>
                {
                    if (Utility.Utility.CheckInvokeOperation(obj)){ //检查调用操作是否完成
                        AuthenticateStatus.UserOrgs = obj.Value;
                        Content = new MainPage();       //创建主页！！！！！！！！！！！！！！！！
                        KeyUp -= OnPageKeyUp;           //去掉按键离开事件
                    }
                }, null);
            }
        }
    }
}
