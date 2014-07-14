using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using DocumentManage.Entities;
using DocumentManage.Utility;
using DocumentManageService.Web;

namespace DocumentManage.Views.Sys
{
    public partial class DefineWorkflow : Page
    {
        private readonly WorkflowDomainContext flowContext = new WorkflowDomainContext();
        public DefineWorkflow()
        {
            Resources.Add("ActiveStatuNames", EnumHelper.GetNames(typeof(ActiveStatus)));
            Resources.Add("AuditTypeNames", EnumHelper.GetNames(typeof(AuditType)));
            Resources.Add("OrganizationTypeNames", EnumHelper.GetNames(typeof(OrganizationType)));
            InitializeComponent();
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            flowContext.GetAllWorkflows((obj) =>
                                            {
                                                if (Utility.Utility.CheckInvokeOperation(obj))
                                                {
                                                    CurrentWorkflowList.ItemsSource = obj.Value;
                                                }
                                            }, null);
        }

        private void OnAuditStepsAutoGenerateColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType.IsEnum)
            {
                e.Column = new DataGridTextColumn
                {
                    Header = e.Column.Header,
                    Binding = new Binding(e.PropertyName) { Converter = Resources["EnumStringConverter"] as EnumToStringValueConverter }
                };
            }
            else if (e.PropertyName == "AuditUser")
            {
                e.Column = new DataGridTextColumn
                    {
                        Header = e.Column.Header,
                        Binding = new Binding("AuditUser.RealName")
                    };
            }
            else if (e.PropertyName == "AuditOrganization")
            {
                e.Column = new DataGridTextColumn
                {
                    Header = e.Column.Header,
                    Binding = new Binding("AuditOrganization.Name")
                };
            }
            else if (e.PropertyName == "AuditRole")
            {
                e.Column = new DataGridTextColumn
                {
                    Header = e.Column.Header,
                    Binding = new Binding("AuditRole.RoleName")
                };
            }
            e.Column.IsReadOnly = true;
        }

        private void OnAuditTypeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cmb = sender as ComboBox;
            if (cmb != null)
            {
                var parent = Utility.Utility.FindVisualParent<DataForm>(cmb);
                if (parent != null)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        var el = parent.FindNameInContent("AutoHideField" + i) as DataField;
                        if (el != null && el.Tag != null)
                        {
                            if (el.Tag.ToString().IndexOf(cmb.SelectedIndex.ToString(), StringComparison.Ordinal) > -1)
                            {
                                el.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                el.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                }
            }
        }

        private void OnUserAutoCompleteBoxPopulating(object sender, PopulatingEventArgs e)
        {
            var acb = sender as AutoCompleteBox;
            if (acb != null)
            {
                var context = new SystemUserDomainContext();
                context.SearchUser(e.Parameter, (obj) =>
                    {
                        if (Utility.Utility.CheckInvokeOperation(obj))
                            acb.ItemsSource = obj.Value;
                    }, null);
            }
        }

        private void OnOrgnizationCompleteBoxPopulating(object sender, PopulatingEventArgs e)
        {
            var acb = sender as AutoCompleteBox;
            if (acb != null)
            {
                var context = new OrganizationDomainContext();
                context.SearchOrganization(e.Parameter, (obj) =>
                {
                    if (Utility.Utility.CheckInvokeOperation(obj))
                        acb.ItemsSource = obj.Value;
                }, null);
            }
        }

        private void OnRoleAutoCompleteBoxPopulating(object sender, PopulatingEventArgs e)
        {
            var acb = sender as AutoCompleteBox;
            if (acb != null)
            {
                var context = new SystemRoleDomainContext();
                context.SearchRole(e.Parameter, (obj) =>
                {
                    if (Utility.Utility.CheckInvokeOperation(obj))
                        acb.ItemsSource = obj.Value;
                }, null);
            }
        }

        private void OnAddJcUserButtonClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
            {
                var parent = Utility.Utility.FindVisualParent<Grid>(btn);
                if (parent != null)
                {
                    var acb = parent.FindName("JcUsersAutoCompleteBox") as AutoCompleteBox;
                    var lsb = parent.FindName("JcUsersListBox") as ListBox;
                    if (acb != null && lsb != null && acb.SelectedItem != null && acb.SelectedItem is SystemUser)
                    {
                        var user = acb.SelectedItem as SystemUser;
                        var item = new JointCheckupUser { Id = 0, StepId = 0, UserId = user.UserId, UserInfo = user.Clone() as SystemUser };
                        var source = lsb.ItemsSource as List<JointCheckupUser> ?? new List<JointCheckupUser>();
                        source.Add(item);
                        lsb.ItemsSource = null;
                        lsb.ItemsSource = source;
                    }
                }
            }
        }

        private void OnWorkflowEditEnding(object sender, DataFormEditEndingEventArgs e)
        {
            if (e.EditAction == DataFormEditAction.Commit)
            {
                var workflow = FormDefineWorkflow.CurrentItem as WorkflowInfo;
                if (workflow != null)
                {
                    switch (FormDefineWorkflow.Mode)
                    {
                        case DataFormMode.AddNew:
                            flowContext.CreateWorkflow(workflow, (obj) =>
                                                                     {
                                                                         if (Utility.Utility.CheckInvokeOperation(obj))
                                                                         {
                                                                             CustomMessageBox.Show("新增审批工作流成功");
                                                                         }
                                                                     }, null);
                            break;
                        case DataFormMode.Edit:
                            flowContext.UpdateWorkflow(workflow, (obj) =>
                            {
                                if (Utility.Utility.CheckInvokeOperation(obj))
                                {
                                    CustomMessageBox.Show("修改审批工作流成功");
                                }
                            }, null);
                            break;
                    }
                }
            }
        }

        private void OnWorkflowStepEditEnding(object sender, DataFormEditEndingEventArgs e)
        {

        }

        private void OnWorkflowItemDeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void OnWorkflowDeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void OnWorkflowValidatingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }

    }
}
