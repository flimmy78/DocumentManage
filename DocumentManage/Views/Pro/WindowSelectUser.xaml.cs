using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.DomainServices.Client;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using DocumentManage.Entities;
using DocumentManage.Utility;
using DocumentManageService.Web;

namespace DocumentManage.Views.Pro
{
    public partial class WindowSelectUser : ChildWindow
    {
        private readonly SystemUserDomainContext userContext = new SystemUserDomainContext();
        private EnumToStringValueConverter enumToStringConverter = new EnumToStringValueConverter();
        public WindowSelectUser()
        {
            InitializeComponent();
            LoadSystemUsers(1);
            userContext.GetUserCount(OnGetUserCountCompleted, null);
            SysUserGrid.LoadingRow += SysUserGridLoadingRow;
        }

        public List<SystemUser> SelectedUsers { get; set; }

        private void SysUserGridLoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.MouseLeftButtonUp += OnGridRowLeftButtonUp;
        }

        private void OnGridRowLeftButtonUp(object sender, MouseButtonEventArgs e)
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
                        GetSelectedUsers();
                        DialogResult = true;
                    }
                }
            }
            SysUserGrid.Tag = t;
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

        private void GetSelectedUsers()
        {
            if (SelectedUsers == null) SelectedUsers = new List<SystemUser>();
            SelectedUsers.Clear();
            if (SysUserGrid.SelectedItems != null && SysUserGrid.SelectedItems.Count > 0)
            {
                SelectedUsers.AddRange(SysUserGrid.SelectedItems.Cast<SystemUser>());
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            GetSelectedUsers();
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
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

        private void OnWindowKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CancelButton_Click(this, null);
            }
        }
    }
}

