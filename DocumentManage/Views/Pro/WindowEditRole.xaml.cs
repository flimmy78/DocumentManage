using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.DomainServices.Client;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using DocumentManage.Entities;
using DocumentManage.Utility;
using DocumentManageService.Web;

namespace DocumentManage.Views.Pro
{
    public partial class WindowEditRole : ChildWindow
    {
        private readonly SystemModuleDomainContext moduleContext = new SystemModuleDomainContext();
        private readonly SystemRoleDomainContext roleContext = new SystemRoleDomainContext();
        private readonly EnumToStringValueConverter enumConverter = new EnumToStringValueConverter();
        private bool hasChenged;
        public SystemRole RoleInfo { get; set; }
        public List<RoleModuleRel> RoleModules { get; set; }

        public WindowEditRole()
        {
            InitializeComponent();
            Loaded += OnWindowLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (RoleInfo == null)
                RoleInfo = new SystemRole{CreateTime = DateTime.Now, CreatedBy = AuthenticateStatus.CurrentUser.UserId};

            RoleInfo.LastUpdateTime = DateTime.Now;
            RoleInfo.LastUpdatedBy = AuthenticateStatus.CurrentUser.UserId;

            FormEditRole.CurrentItem = RoleInfo;
            moduleContext.GetSystemModuleTree(OnGetSystemModuleTreeComplete, null);
        }

        private void OnGetSystemModuleTreeComplete(InvokeOperation<SystemModule> obj)
        {
            if (Utility.Utility.CheckInvokeOperation(obj))
            {
                foreach (var module in obj.Value.Children)
                {
                    AddModuleItemToTree(null, module);
                }
                GetRoleModule();
            }
        }

        private void GetRoleModule()
        {
            if (RoleInfo.RoleId > 0)
            {
                roleContext.GetRoleModules(RoleInfo.RoleId, OnGetRoleModuleCompleted, null);
            }
        }

        private void OnGetRoleModuleCompleted(InvokeOperation<List<RoleModuleRel>> obj)
        {
            if (Utility.Utility.CheckInvokeOperation(obj))
            {
                RoleModules = obj.Value;
                foreach (TreeViewItem item in SysModulesTree.Items)
                {
                    SetRoleModuleToTree(item);
                }
            }
        }

        private void SetRoleModuleToTree(TreeViewItem item)
        {
            if (RoleModules == null)
                return;
            var ckb = item.Header as CheckBox;
            var rmr = item.DataContext as RoleModuleRel;
            if (ckb != null && rmr != null)
            {
                var vrmr = RoleModules.FirstOrDefault(o => o.ModuleId == rmr.ModuleId);
                if (vrmr != null)
                {
                    ckb.IsChecked = null;
                    rmr.IncludeChild = vrmr.IncludeChild;
                    if (vrmr.IncludeChild)
                    {
                        ckb.IsChecked = true;
                        CheckChildTreeViewItems(item, true);
                        return;
                    }
                }
            }
            foreach (TreeViewItem child in item.Items)
            {
                SetRoleModuleToTree(child);
            }
        }

        #region 树操作

        private void AddModuleItemToTree(TreeViewItem parent, SystemModule module)
        {
            var item = new TreeViewItem();
            var ckb = new CheckBox();
            ckb.IsThreeState = true;
            ckb.IsChecked = false;
            ckb.Content = module.ModuleName;
            ckb.Click += OnTreeViewItemClicked;
            ckb.Foreground = new SolidColorBrush(Colors.Black);
            item.Header = ckb;
            item.DataContext = new RoleModuleRel
                {
                    Id = 0,
                    RoleId = RoleInfo == null ? 0 : RoleInfo.RoleId,
                    ModuleId = module.ModuleId,
                    StartTime = DateTime.Now.Date,
                    ExpireTime = DateTime.Now.Date.AddYears(10),
                    IncludeChild = false,
                    CreateTime = DateTime.Now,
                    LastUpdateTime = DateTime.Now,
                    CreatedBy = AuthenticateStatus.CurrentUser.UserId,
                    LastUpdatedBy = AuthenticateStatus.CurrentUser.UserId
                };
            if (parent == null)
            {
                SysModulesTree.Items.Add(item);
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

        private void OnTreeViewItemClicked(object sender, RoutedEventArgs e)
        {
            hasChenged = true;
            var ckb = sender as CheckBox;
            var item = SysModulesTree.SelectedItem as TreeViewItem;
            if (ckb != null && item != null)
            {
                var includeChild = ckb.IsChecked.HasValue && ckb.IsChecked.Value;
                CheckChildTreeViewItems(item, includeChild);
                FreshParentTreeViewItemState(item);
            }
        }

        private void CheckChildTreeViewItems(TreeViewItem item, bool state)
        {
            var ckb = item.Header as CheckBox;
            var uor = item.DataContext as RoleModuleRel;
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
                    uor.IncludeChild = state;
                }
            }
        }

        private void FreshParentTreeViewItemState(TreeViewItem item)
        {
            var parent = item.GetParentTreeViewItem();
            if (parent != null)
            {
                var parentCkb = parent.Header as CheckBox;
                var parentUor = parent.DataContext as RoleModuleRel;
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
                        parentUor.IncludeChild = parentCkb.IsChecked.HasValue && parentCkb.IsChecked.Value;
                }
                FreshParentTreeViewItemState(parent);
            }
        }

        #endregion

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (FormEditRole.ValidateItem())
            {
                GetRoleModules();
                if (RoleModules == null || RoleModules.Count < 1)
                {
                    CustomMessageBox.Show("请给角色分配权限");
                    return;
                }
                BusyIndicator1.IsBusy = true;
                BusyIndicator1.BusyContent = "正在验证角色编码...";
                roleContext.HasRoleCodeUsed(RoleInfo.RoleCode, RoleInfo.RoleId, (obj) =>
                    {
                        BusyIndicator1.IsBusy = false;
                        if (Utility.Utility.CheckInvokeOperation(obj))
                        {
                            if (obj.Value)
                            {
                                CustomMessageBox.Show("角色编码已经存在，请重新指定！");
                            }
                            else
                            {
                                if (RoleInfo.RoleId > 0)
                                    roleContext.UpdateRole(RoleInfo, RoleModules, OnEditRoleInfoCompleted, null);
                                else
                                    roleContext.CreateRole(RoleInfo, RoleModules, OnEditRoleInfoCompleted, null);
                            }
                        }
                    }, null);
            }
        }

        private void GetRoleModules()
        {
            if (RoleModules == null)
                RoleModules = new List<RoleModuleRel>();
            RoleModules.Clear();

            foreach (TreeViewItem item in SysModulesTree.Items)
                GetSelectedModule(item);
        }

        private void GetSelectedModule(TreeViewItem item)
        {
            if (item == null)
                return;
            if (RoleModules == null)
                RoleModules = new List<RoleModuleRel>();
            var ckb = item.Header as CheckBox;
            if (ckb != null)
            {
                var uor = ckb.DataContext as RoleModuleRel;
                if (uor != null)
                {
                    if (uor.IncludeChild)
                    {
                        RoleModules.Add(uor);
                    }
                    else if (!ckb.IsChecked.HasValue)
                    {
                        RoleModules.Add(uor);
                        foreach (TreeViewItem child in item.Items)
                        {
                            GetSelectedModule(child);
                        }
                    }
                }
            }
        }

        private void OnEditRoleInfoCompleted(InvokeOperation<int> obj)
        {
            if (Utility.Utility.CheckInvokeOperation(obj))
            {
                if (obj.Value > 0)
                {
                    DialogResult = true;
                }
                else
                {
                    CustomMessageBox.Show("添加/修改角色资料失败！");
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void OnDataFormAutoGenerateField(object sender, DataFormAutoGeneratingFieldEventArgs e)
        {
            if (e.PropertyType == typeof(SystemUser) || e.PropertyType == typeof(DateTime))
            {
                e.Cancel = true;
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
    }
}

