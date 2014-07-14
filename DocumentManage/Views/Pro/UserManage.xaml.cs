using System;
using System.Collections.Generic;
using System.ServiceModel.DomainServices.Client;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using DocumentManage.Entities;
using DocumentManage.Utility;
using DocumentManageService.Web;

namespace DocumentManage.Views.Pro
{
    public partial class UserManage : Page
    {
        private readonly SystemUserDomainContext userContext = new SystemUserDomainContext();
        private EnumToStringValueConverter enumToStringConverter = new EnumToStringValueConverter();
        public UserManage()
        {
            InitializeComponent();
            SysUserGrid.LoadingRow += SysUserGridLoadingRow;
        }

        private void SysUserGridLoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.MouseLeftButtonUp += OnGridRowLeftButtonUp;
        }

        private void OnGridRowLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TimeSpan t = DateTime.Now.TimeOfDay;
            if (SysUserGrid.Tag != null)
            {
                var oldT = (TimeSpan)SysUserGrid.Tag;
                if ((t - oldT) < TimeSpan.FromMilliseconds(300))
                {
                    var user = SysUserGrid.SelectedItem as SystemUser;
                    if (user != null)
                    {
                        var win = new WindowEditUser();
                        win.UserInfo = user;
                        win.Closed += (oo, ee) => LoadSystemUsers(SysUserPager.PageIndex + 1);
                        win.Show();
                    }
                }
            }
            SysUserGrid.Tag = t;
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LoadSystemUsers(1);
            userContext.GetUserCount(OnGetUserCountCompleted, null);
        }

        private void LoadSystemUsers(int page)
        {
            BusyIndicator1.IsBusy = true;
            userContext.GetUsersByPage(SysUserPager.PageSize, page, OnGetPagedUserCompleted, null);
        }

        private void OnGetUserCountCompleted(InvokeOperation<int> obj)
        {
            if (Utility.Utility.CheckInvokeOperation(obj))
            {
                SysUserPager.BindSource(obj.Value, SysUserPager.PageSize);
            }
        }

        private void OnGetPagedUserCompleted(InvokeOperation<List<SystemUser>> obj)
        {
            BusyIndicator1.IsBusy = false;
            if (Utility.Utility.CheckInvokeOperation(obj))
            {
                SysUserGrid.ItemsSource = obj.Value;
            }
        }

        private void ChangeUserGridPage(object sender, EventArgs e)
        {
            LoadSystemUsers(SysUserPager.PageIndex + 1);
        }

        private void OnCreateUserButtonClick(object sender, RoutedEventArgs e)
        {
            var win = new WindowEditUser();
            win.Closed += (o, ee) =>
                {
                    LoadSystemUsers(1);
                    userContext.GetUserCount(OnGetUserCountCompleted, null);
                };
            win.Show();
        }

        private void OnDeleteUserButtonClick(object sender, RoutedEventArgs e)
        {
            if (SysUserGrid.SelectedItem != null && CustomMessageBox.Ask("确定要删除该用户吗？"))
            {
                var user = SysUserGrid.SelectedItem as SystemUser;
                if (user != null)
                {
                    userContext.DeleteUser(user, (obj) => LoadSystemUsers(SysUserPager.PageIndex + 1), null);
                } 
            }
        }

        private void OnDataGridAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType.IsEnum)
            {
                e.Column = new DataGridTextColumn
                {
                    Header = e.Column.Header,
                    Binding = new Binding(e.PropertyName) { Converter = enumToStringConverter }
                };
            }
        }

        private void OnSearchUserButtonClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSearchKey.Text))
            {
                BusyIndicator1.IsBusy = true;
                BusyIndicator1.BusyContent = "正在查询...";
                userContext.SearchUser(txtSearchKey.Text, (obj) =>
                    {
                        BusyIndicator1.IsBusy = false;
                        if (Utility.Utility.CheckInvokeOperation(obj))
                        {
                            SysUserGrid.ItemsSource = obj.Value;
                            SysUserPager.Source = obj.Value;
                        }
                    }, null);
            }
        }
    }
}
