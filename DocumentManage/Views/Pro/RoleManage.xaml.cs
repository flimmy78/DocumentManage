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
    public partial class RoleManage : Page
    {
        private readonly SystemRoleDomainContext roleContext = new SystemRoleDomainContext();
        private readonly EnumToStringValueConverter enumToStringConverter = new EnumToStringValueConverter();

        public RoleManage()
        {
            InitializeComponent();
            SysRoleGrid.LoadingRow += SysRoleGridLoadingRow;
        }

        private void SysRoleGridLoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.MouseLeftButtonUp += OnGridRowLeftButtonUp;
        }

        private void OnGridRowLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TimeSpan t = DateTime.Now.TimeOfDay;
            if (SysRoleGrid.Tag != null)
            {
                var oldT = (TimeSpan)SysRoleGrid.Tag;
                if ((t - oldT) < TimeSpan.FromMilliseconds(300))
                {
                    var role = SysRoleGrid.SelectedItem as SystemRole;
                    if (role != null)
                    {
                        var win = new WindowEditRole();
                        win.RoleInfo = role;
                        win.Closed += (oo, ee) => LoadSystemRoles(SysRolePager.PageIndex + 1);
                        win.Show();
                    }
                }
            }
            SysRoleGrid.Tag = t;
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LoadSystemRoles(1);
            roleContext.GetRolesCount(OnGetRolesCountCompleted, null);
        }

        private void OnGetRolesCountCompleted(InvokeOperation<int> obj)
        {
            if (Utility.Utility.CheckInvokeOperation(obj))
            {
                SysRolePager.BindSource(obj.Value, SysRolePager.PageSize);
            }
        }

        private void LoadSystemRoles(int page)
        {
            roleContext.GetRolesByPage(SysRolePager.PageSize, page, OnGetSystemRoleCompleted, null);
        }

        private void OnGetSystemRoleCompleted(InvokeOperation<List<SystemRole>> obj)
        {
            if (Utility.Utility.CheckInvokeOperation(obj))
            {
                SysRoleGrid.ItemsSource = obj.Value;
            }
        }

        private void OnCreateRoleButtonClick(object sender, RoutedEventArgs e)
        {
            var win = new WindowEditRole();
            win.Closed += (oo, ee) =>
                {
                    LoadSystemRoles(1);
                    roleContext.GetRolesCount(OnGetRolesCountCompleted, null);
                };
            win.Show();
        }

        private void OnDeleteRoleButtonClick(object sender, RoutedEventArgs e)
        {
            if (SysRoleGrid.SelectedItem != null && CustomMessageBox.Ask("确定要删除该角色吗？"))
            {
                var role = SysRoleGrid.SelectedItem as SystemRole;
                if (role != null)
                {
                    roleContext.DeleteRole(role, (obj) => LoadSystemRoles(SysRolePager.PageIndex + 1), null);
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

        private void ChangeRoleGridPage(object sender, EventArgs e)
        {
            LoadSystemRoles(SysRolePager.PageIndex + 1);
        }

        private void OnSearchUserButtonClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSearchKey.Text))
            {
                BusyIndicator1.IsBusy = true;
                BusyIndicator1.BusyContent = "正在查询...";
                roleContext.SearchRole(txtSearchKey.Text, (obj) =>
                {
                    BusyIndicator1.IsBusy = false;
                    if (Utility.Utility.CheckInvokeOperation(obj))
                    {
                        SysRoleGrid.ItemsSource = obj.Value;
                        SysRolePager.Source = obj.Value;
                    }
                }, null);
            }
        }

    }
}
