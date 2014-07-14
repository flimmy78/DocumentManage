using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel.DomainServices.Client;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Navigation;
using DocumentManage.Entities;
using DocumentManage.Utility;
using DocumentManageService.Web;

namespace DocumentManage.Views.Sys
{
    public partial class OrganizationMange : Page
    {
        private readonly OrganizationDomainContext orgContext = new OrganizationDomainContext();
        private readonly EnumToStringValueConverter enumConverter = new EnumToStringValueConverter();
        private Organization organizationTree;
        private List<Organization> pendingOrgList = new List<Organization>();

        public OrganizationMange()
        {
            InitializeComponent();
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            orgContext.GetOrganizationTree(OnGetOrgnizationTreeCompleted, null);
        }

        private void OnGetOrgnizationTreeCompleted(InvokeOperation<Organization> obj)
        {
            if (Utility.Utility.CheckInvokeOperation(obj))
            {
                organizationTree = obj.Value;
                SysOrgTree.ItemsSource = organizationTree.Children;
            }
        }

        private void BindAndSelectTreeView(object selected)
        {
            if (!SysOrgTree.SelectItem(selected))
            {
                MessageBox.Show("Select Failed!");
            }
        }

        private void OnDataFormAutoGenerateField(object sender, DataFormAutoGeneratingFieldEventArgs e)
        {
            if (e.PropertyName.Equals("Id", StringComparison.CurrentCulture)
                || e.PropertyName.Equals("ParentId", StringComparison.CurrentCulture))
            {
                e.Field.Mode = DataFieldMode.ReadOnly;
                e.Field.IsReadOnly = true;
            }
            if (e.PropertyType.IsEnum)
            {
                var combbox = new ComboBox();
                combbox.ItemsSource = EnumHelper.GetNames(e.PropertyType);
                combbox.SelectedIndex = 0;
                combbox.SetBinding(Selector.SelectedItemProperty, new Binding
                {
                    Path = new PropertyPath(e.PropertyName),
                    Mode = BindingMode.TwoWay,
                    Converter = enumConverter,
                    ValidatesOnDataErrors = true
                });
                e.Field.Content = combbox;
            }
        }

        private void OnCreateSiblingButtonClick(object sender, RoutedEventArgs e)
        {
            Organization org;
            if (GetSelectedOrg(out org))
            {
                var parentItem = FindParentOrganization(org);
                if (parentItem != null)
                {
                    var newItem = new Organization
                        {
                            Id = GetNewItemId(parentItem),
                            ParentId = parentItem.Id,
                            Type = org.Type,
                            Name = "未命名",
                            Status = ActiveStatus.InActive
                        };
                    pendingOrgList.Add(newItem);
                    parentItem.Children.Add(newItem);
                    BindAndSelectTreeView(newItem);
                }
            }
        }

        private void OnCreateChildButtonClick(object sender, RoutedEventArgs e)
        {
            Organization org;
            if (GetSelectedOrg(out org))
            {
                var newItem = new Organization
                {
                    Id = GetNewItemId(org),
                    ParentId = org.Id,
                    Type = org.Type,
                    Name = "未命名",
                    Status = ActiveStatus.Active
                };
                if (org.Children == null)
                    org.Children = new ObservableCollection<Organization>();
                pendingOrgList.Add(newItem);
                org.Children.Add(newItem);
                BindAndSelectTreeView(org);
            }
        }

        private void OnDeleteOrgButtonClick(object sender, RoutedEventArgs e)
        {
            Organization org;
            if (GetSelectedOrg(out org))
            {
                org.Status = ActiveStatus.Deleted;
            }
        }

        private Organization FindParentOrganization(Organization org)
        {
            return FindParentOrganization(organizationTree, org);
        }

        private Organization FindParentOrganization(Organization parent, Organization org)
        {
            if (parent.Id.Equals(org.ParentId, StringComparison.Ordinal))
                return parent;
            if (parent.Children != null)
                return parent.Children.Select(o => FindParentOrganization(o, org)).FirstOrDefault(tmp => tmp != null);
            return null;
        }

        private void OnOrgTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (FormEditOrgInfo.Mode == DataFormMode.Edit)
            {
                FormEditOrgInfo.CancelEdit();
                var oldOrg = e.OldValue as Organization;
                if (oldOrg != null && pendingOrgList.Any(o => o.Id == oldOrg.Id))
                {
                    var oldParent = FindParentOrganization(oldOrg);
                    if (oldParent != null && oldParent.Children != null)
                    {
                        oldParent.Children.Remove(oldOrg);
                        pendingOrgList.Remove(oldOrg);
                    }
                }
            }
            var org = e.NewValue as Organization;
            if (org != null)
            {
                btnCreateChild.IsEnabled = true;
                btnDeleteOrg.IsEnabled = btnCreateSibling.IsEnabled = !org.ParentId.Equals("1");
                FormEditOrgInfo.CurrentItem = org;
            }
            else
            {
                btnCreateChild.IsEnabled = btnCreateSibling.IsEnabled = btnDeleteOrg.IsEnabled = false;
            }
        }

        private bool GetSelectedOrg(out Organization org)
        {
            org = SysOrgTree.SelectedItem as Organization;
            return org != null;
        }

        private string GetNewItemId(Organization org)
        {
            long maxId = 1;
            if (org.Children == null || org.Children.Count < 1)
            {
                return string.Format("{0}01", org.Id);
            }
            foreach (var o in org.Children)
            {
                var tmp = Convert.ToInt32(o.Id.Substring(o.Id.Length - 2));
                if (tmp > maxId)
                    maxId = tmp;
            }
            maxId += 1;
            return org.Id + maxId.ToString().PadLeft(2, '0');
        }

        private void OnDataFormEditEnding(object sender, DataFormEditEndingEventArgs e)
        {
            if (e.EditAction == DataFormEditAction.Commit && FormEditOrgInfo.ValidateItem())
            {
                var org = FormEditOrgInfo.CurrentItem as Organization;
                if (org != null)
                {
                    orgContext.SaveOrganizationInfo(org, obj =>
                        {
                            if (Utility.Utility.CheckInvokeOperation(obj))
                            {
                                if (obj.Value > 0)
                                {
                                    CustomMessageBox.Show("组织已成功保存！");
                                    pendingOrgList.Remove(org);
                                }
                                else
                                {
                                    CustomMessageBox.Show("保存组织失败，请重试！");
                                }
                            }
                        }, null);
                }
            }
        }

        private void OnDataFormBeginningEdit(object sender, CancelEventArgs e)
        {

        }
    }
}
