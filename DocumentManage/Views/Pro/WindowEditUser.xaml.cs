using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel.DomainServices.Client;
using System.Windows;
using System.Windows.Controls;
using DocumentManage.Entities;
using DocumentManage.Utility;
using DocumentManage.Views.Sys;
using DocumentManageService.Web;

namespace DocumentManage.Views.Pro
{
    public partial class WindowEditUser : ChildWindow
    {
        private readonly SystemModuleDomainContext moduleContext = new SystemModuleDomainContext();
        private readonly SystemUserDomainContext userContext = new SystemUserDomainContext();
        private bool userInfoChanged;
        public WindowEditUser()
        {
            this.Resources.Add("ActiveStatusNames", EnumHelper.GetNames(typeof(ActiveStatus)));
            this.Resources.Add("GenderNames", EnumHelper.GetNames(typeof(Gender)));
            InitializeComponent();
        }

        public SystemUser UserInfo { get; set; }
        public List<UserRoleRel> SelectedRoles { get; set; }
        public List<UserOrgRel> SelectedOrganizations { get; set; }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (userInfoChanged && EditUserInfoForm.ValidateItem())
            {
                if (SelectedRoles == null || SelectedRoles.Count < 0)
                {
                    CustomMessageBox.Show("请给用户分配一个角色！");
                    return;
                }
                if (SelectedOrganizations == null || SelectedOrganizations.Count < 0)
                {
                    CustomMessageBox.Show("请给用户分配一个组织！");
                    return;
                }
                BusyIndicator1.IsBusy = true;
                BusyIndicator1.BusyContent = "正在验证登陆名...";
                userContext.HasUserNameUsed(UserInfo.UserName, UserInfo.UserId, (obj) =>
                    {
                        BusyIndicator1.IsBusy = false;
                        if (Utility.Utility.CheckInvokeOperation(obj))
                        {
                            if (obj.Value)
                            {
                                CustomMessageBox.Show("登陆名已经存在，请重新指定登陆名！");
                            }
                            else
                            {
                                BusyIndicator1.IsBusy = true;
                                BusyIndicator1.BusyContent = "正在提交更改...";
                                if (UserInfo.UserId > 0)
                                    userContext.UpdateUser(UserInfo, SelectedRoles, SelectedOrganizations, OnCreateUserCompleted, null);
                                else
                                    userContext.CreateUser(UserInfo, SelectedRoles, SelectedOrganizations, OnCreateUserCompleted, null);
                            }
                        }
                    }, null);
            }
        }

        private void OnCreateUserCompleted(InvokeOperation<int> obj)
        {
            BusyIndicator1.IsBusy = false;
            if (Utility.Utility.CheckInvokeOperation(obj))
            {
                if (obj.Value > 0)
                {
                    this.DialogResult = true;
                }
                else
                {
                    CustomMessageBox.Show("添加/修改用户信息失败，请重试！");
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (UserInfo == null)
                UserInfo = new SystemUser();
            EditUserInfoForm.CurrentItem = UserInfo;
            userContext.GetUserRoleRel(UserInfo.UserId, (obj) =>
                {
                    if (Utility.Utility.CheckInvokeOperation(obj))
                    {
                        SelectedRoles = obj.Value;
                        UserRolesList.ItemsSource = obj.Value;
                    }
                }, null);
            userContext.GetUserOrgRel(UserInfo.UserId, (obj) =>
                {
                    if (Utility.Utility.CheckInvokeOperation(obj))
                    {
                        SelectedOrganizations = obj.Value;
                        UserOrgList.ItemsSource = obj.Value;
                    }
                }, null);
            moduleContext.GetSystemModuleTree(OnGetSystemModuleCompleted, null);
        }

        private void OnGetSystemModuleCompleted(InvokeOperation<SystemModule> obj)
        {
            if (Utility.Utility.CheckInvokeOperation(obj))
            {
                foreach (var module in obj.Value.Children)
                {
                    AddModuleItemToTree(null, module);
                }
            }
        }

        private void AddModuleItemToTree(TreeViewItem parent, SystemModule module)
        {
            var item = new TreeViewItem();
            var ckb = new CheckBox();
            ckb.IsChecked = false;
            ckb.Content = module.ModuleName;
            item.Header = ckb;
            item.DataContext = module;
            if (parent == null)
            {
                UserRightsTree.Items.Add(item);
            }
            else
            {
                parent.Items.Add(item);
            }
            if (module.Children != null && module.Children.Count > 0)
            {
                foreach (var child in module.Children)
                {
                    AddModuleItemToTree(item, child);
                }
                item.IsExpanded = true;
            }
        }

        private void OnAddUserRoleButtonClick(object sender, RoutedEventArgs e)
        {
            var win = new WindowSelectRole();
            win.Closed += (o, ea) =>
                {
                    if (win.DialogResult.HasValue && win.DialogResult.Value)
                    {
                        userInfoChanged = true;
                        if (SelectedRoles == null)
                            SelectedRoles = new List<UserRoleRel>();
                        foreach (var c in win.SelectedRole)
                        {
                            if (SelectedRoles.Any(uo => uo.RoleId == c.RoleId))
                                continue;
                            SelectedRoles.Add(new UserRoleRel
                            {
                                Id = 0,
                                UserId = AuthenticateStatus.CurrentUser.UserId,
                                RoleId = c.RoleId,
                                Role = c,
                                StartTime = DateTime.Now.Date,
                                ExpireTime = DateTime.Now.Date.AddYears(10),
                                CreatedBy = AuthenticateStatus.CurrentUser.UserId,
                                CreateTime = DateTime.Now,
                                LastUpdateTime = DateTime.Now,
                                LastUpdatedBy = AuthenticateStatus.CurrentUser.UserId,
                            });
                        }
                        UserRolesList.ItemsSource = null;
                        UserRolesList.ItemsSource = SelectedRoles;
                    }
                };
            win.Show();
        }

        private void OnDeleteUseRoelButtonClick(object sender, RoutedEventArgs e)
        {
            if (UserRolesList.SelectedItem != null && CustomMessageBox.Ask("确定要删除该用户的角色吗？"))
            {
                SelectedRoles.Remove(UserRolesList.SelectedItem as UserRoleRel);
                UserRolesList.ItemsSource = null;
                UserRolesList.ItemsSource = SelectedRoles;
            }
        }

        private void OnSelectOrgButtonClick(object sender, RoutedEventArgs e)
        {
            var win = new WindowSelectOrg();
            win.SelectedOrg = SelectedOrganizations;
            win.Closed += (o, ea) =>
                {
                    if (win.DialogResult.HasValue && win.DialogResult.Value)
                    {
                        userInfoChanged = true;
                        if (SelectedOrganizations == null)
                            SelectedOrganizations = new List<UserOrgRel>();
                        SelectedOrganizations = win.SelectedOrg;
                        UserOrgList.ItemsSource = null;
                        UserOrgList.ItemsSource = SelectedOrganizations;
                    }
                };
            win.Show();
        }

        private void OnUserRightsTabLoaded(object sender, RoutedEventArgs e)
        {
            LoadUserRights();
        }

        private void LoadUserRights()
        {

        }

        private void OnFormItemChanged(object sender, EventArgs e)
        {
            userInfoChanged = true;
        }
    }
}

