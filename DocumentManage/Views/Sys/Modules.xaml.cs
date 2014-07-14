using System.ServiceModel.DomainServices.Client;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DocumentManage.Entities;
using DocumentManageService.Web;

namespace DocumentManage.Views.Sys
{
    public partial class Modules : Page
    {
        private readonly SystemModuleDomainContext moduleContext = new SystemModuleDomainContext();
        public Modules()
        {
            InitializeComponent();
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
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
    }
}
