using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DocumentManage.Entities;
using DocumentManage.Utility;
using DocumentManageService.Web;

namespace DocumentManage.Views.Pro
{
    public partial class WindowSelectRole : ChildWindow
    {
        private EnumToStringValueConverter enumConverter = new EnumToStringValueConverter();

        public List<SystemRole> SelectedRole { get; set; }

        public WindowSelectRole()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            GetSelectedRoles();
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            var context = new SystemRoleDomainContext();
            LoadRoleBusyIndicatory.IsBusy = true;
            context.GetAllRoles((obj) =>
                {
                    LoadRoleBusyIndicatory.IsBusy = false;
                    if (Utility.Utility.CheckInvokeOperation(obj))
                    {
                        SystemRolesList.ItemsSource = obj.Value;
                    }
                }, null);
        }

        private void GetSelectedRoles()
        {
            if (SelectedRole == null)
                SelectedRole = new List<SystemRole>();
            foreach (SystemRole role in SystemRolesList.SelectedItems)
            {
                SelectedRole.Add(role);
            }
        }

        private void OnRolesAutoGenerateColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType.IsEnum)
            {
                e.Column = new DataGridTextColumn
                {
                    Header = e.Column.Header,
                    Binding = new Binding(e.PropertyName) { Converter = enumConverter }
                };
            }
            else if (e.PropertyType == typeof(SystemUser))
            {
                e.Column = new DataGridTextColumn
                {
                    Header = e.Column.Header,
                    Binding = new Binding(string.Format("{0}.RealName", e.PropertyName))
                };
            }
            else if (e.PropertyType == typeof(DateTime))
            {
                e.Cancel = true;
            }
        }

    }
}

