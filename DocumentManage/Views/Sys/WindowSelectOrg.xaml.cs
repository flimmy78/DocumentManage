using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.DomainServices.Client;
using System.Windows;
using System.Windows.Controls;
using DocumentManage.Entities;
using DocumentManage.Utility;
using DocumentManageService.Web;

namespace DocumentManage.Views.Sys
{
    public partial class WindowSelectOrg : ChildWindow
    {
        private readonly OrganizationDomainContext orgContext = new OrganizationDomainContext();
        public List<UserOrgRel> SelectedOrg { get; set; }

        public WindowSelectOrg()
        {
            InitializeComponent();
            orgContext.GetOrganizationTree(OnGetOrgTreeCompleted, null);
        }

        private void OnGetOrgTreeCompleted(InvokeOperation<Organization> obj)
        {
            if (Utility.Utility.CheckInvokeOperation(obj))
            {
                foreach (var module in obj.Value.Children)
                {
                    AddOrgItemToTree(null, module);
                }
                foreach (TreeViewItem item in OrganizationTree.Items)
                {
                    SetSelectedOrg(item);
                }
            }
        }

        private void SetSelectedOrg(TreeViewItem item)
        {
            if (SelectedOrg == null)
                return;
            var ckb = item.Header as CheckBox;
            var uor = item.DataContext as UserOrgRel;
            if (ckb != null && uor != null)
            {
                var vuor = SelectedOrg.FirstOrDefault((o) => o.OrganizationId == uor.OrganizationId);
                if (vuor != null)
                {
                    ckb.IsChecked = null;
                    uor.IncludeChildOrg = vuor.IncludeChildOrg;
                    if (vuor.IncludeChildOrg)
                    {
                        ckb.IsChecked = true;
                        CheckChildTreeViewItems(item, true);
                        return;
                    }
                }
            }
            foreach (TreeViewItem child in item.Items)
            {
                SetSelectedOrg(child);
            }
        }

        private void GetSelectedOrg(TreeViewItem item)
        {
            if (item == null)
                return;
            if (SelectedOrg == null)
                SelectedOrg = new List<UserOrgRel>();
            var ckb = item.Header as CheckBox;
            if (ckb != null)
            {
                var uor = ckb.DataContext as UserOrgRel;
                if (uor != null)
                {
                    if (uor.IncludeChildOrg)
                    {
                        SelectedOrg.Add(uor);
                    }
                    else if (!ckb.IsChecked.HasValue)
                    {
                        SelectedOrg.Add(uor);
                        foreach (TreeViewItem child in item.Items)
                        {
                            GetSelectedOrg(child);
                        }
                    }
                }
            }
        }

        private void AddOrgItemToTree(TreeViewItem parent, Organization org)
        {
            var item = new TreeViewItem();
            var ckb = new CheckBox();
            ckb.IsChecked = false;
            ckb.IsThreeState = true;
            ckb.Content = org.Name;
            ckb.Click += OnTreeViewItemClicked;
            item.Header = ckb;
            item.DataContext = new UserOrgRel
                {
                    Id = 0,
                    UserId = AuthenticateStatus.CurrentUser.UserId,
                    OrganizationId = org.Id,
                    Organization = org,
                    IncludeChildOrg = false,
                    StartTime = DateTime.Now.Date,
                    ExpireTime = DateTime.Now.Date.AddYears(10),
                    CreatedBy = AuthenticateStatus.CurrentUser.UserId,
                    CreateTime = DateTime.Now,
                    LastUpdatedBy = AuthenticateStatus.CurrentUser.UserId,
                    LastUpdateTime = DateTime.Now
                };
            if (parent == null)
            {
                OrganizationTree.Items.Add(item);
            }
            else
            {
                parent.Items.Add(item);
            }
            if (org.Children != null && org.Children.Count > 0)
            {
                foreach (var child in org.Children)
                {
                    AddOrgItemToTree(item, child);
                }
                item.IsExpanded = true;
            }
        }

        private void CheckChildTreeViewItems(TreeViewItem item, bool state)
        {
            var ckb = item.Header as CheckBox;
            var uor = item.DataContext as UserOrgRel;
            if (ckb != null)
            {
                ckb.IsChecked = state;
                if (item.HasItems)
                {
                    foreach (TreeViewItem child in item.Items)
                    {
                        CheckChildTreeViewItems(child, state);
                    }
                }
                if (uor != null)
                {
                    uor.IncludeChildOrg = state;
                }
            }
        }

        private void FreshParentTreeViewItemState(TreeViewItem item)
        {
            var parent = item.GetParentTreeViewItem();
            if (parent != null)
            {
                var parentCkb = parent.Header as CheckBox;
                var parentUor = parent.DataContext as UserOrgRel;
                if (parentCkb != null)
                {
                    bool hasChecked = false;
                    bool allChecked = true;
                    foreach (TreeViewItem child in parent.Items)
                    {
                        var ckb = child.Header as CheckBox;
                        if (ckb != null)
                        {
                            if (ckb.IsChecked.HasValue && ckb.IsChecked.Value)
                            {
                                if (!hasChecked)
                                    hasChecked = true;
                            }
                            else
                            {
                                if (!ckb.IsChecked.HasValue && !hasChecked)
                                    hasChecked = true;

                                if (allChecked)
                                    allChecked = false;
                            }
                        }
                    }
                    if (allChecked)
                        parentCkb.IsChecked = true;
                    else if (!hasChecked)
                        parentCkb.IsChecked = false;
                    else
                        parentCkb.IsChecked = null;
                    if (parentUor != null)
                        parentUor.IncludeChildOrg = parentCkb.IsChecked.HasValue && parentCkb.IsChecked.Value;
                }
                FreshParentTreeViewItemState(parent);
            }
        }

        private void OnTreeViewItemClicked(object sender, RoutedEventArgs e)
        {
            var ckb = sender as CheckBox;
            var item = OrganizationTree.SelectedItem as TreeViewItem;
            if (ckb != null && item != null)
            {
                var includeChild = ckb.IsChecked.HasValue && ckb.IsChecked.Value;
                CheckChildTreeViewItems(item, includeChild);
                FreshParentTreeViewItemState(item);
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedOrg != null)
                SelectedOrg.Clear();
            foreach (TreeViewItem item in OrganizationTree.Items)
                GetSelectedOrg(item);
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

