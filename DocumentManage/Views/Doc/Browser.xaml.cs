using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using DocumentManage.Entities;
using DocumentManage.Utility;
using DocumentManage.Views.Req;
using DocumentManageService.Web;

namespace DocumentManage.Views.Doc
{
    public partial class Browser : Page
    {
        private readonly OrganizationDomainContext orgContext = new OrganizationDomainContext();
        private readonly DocumentDomainContext documentContext = new DocumentDomainContext();
        private FrameworkElement mouseElement;
        private readonly List<MenuItem> contextMenuiItems = new List<MenuItem>();

        public string OrgId { get; set; }

        public int FolderId { get; set; }

        public Browser()
        {
            InitializeComponent();
            orgContext.GetOrganizationTree((obj) =>
            {
                if (Utility.Utility.CheckInvokeOperation(obj))
                {
                    OrganizationTreeView.ItemsSource = obj.Value.Children;
                    OpenFolder(FolderId);
                }
            }, null);
            InitContextMenu();

            SearchFilePanel.Visibility = AuthenticateStatus.GetModuleVisibility("100101");
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }

        private void InitContextMenu()
        {
            contextMenuiItems.Clear();
            var context = new ContextMenu();
            var item1 = new MenuItem { Header = "打开" };
            item1.Click += OnOpenContextMenuClick;
            contextMenuiItems.Add(item1);
            context.Items.Add(item1);
            var item2 = new MenuItem { Header = "浏览" };
            item2.Click += OnBrowseContextMenuClick;
            contextMenuiItems.Add(item2);
            context.Items.Add(item2);
            var item3 = new MenuItem { Header = "删除" };
            item3.Click += OnDeleteContextMenuClick;
            contextMenuiItems.Add(item3);
            context.Items.Add(item3);
            var item4 = new MenuItem { Header = "重命名" };
            item4.Click += OnRenameContextMenuClick;
            contextMenuiItems.Add(item4);
            context.Items.Add(item4);
            var item5 = new MenuItem { Header = "修订" };
            item5.Click += OnRevisionContextMenuClick;
            contextMenuiItems.Add(item5);
            context.Items.Add(item5);
            context.Opened += OnContextMenuOpend;
            ContextMenuService.SetContextMenu(FileEntityListBox, context);
        }

        private void OnOpenContextMenuClick(object sender, RoutedEventArgs e)
        {
            var fse = mouseElement.DataContext as FileSystemEntity;
            if (fse == null)
                return;
            OpenFolder(fse.FolderId);
        }

        private void OnBrowseContextMenuClick(object sender, RoutedEventArgs e)
        {
            var fse = mouseElement.DataContext as FileSystemEntity;
            if (fse == null)
                return;
            BrowseFileInfo(fse);
        }

        private void OnDeleteContextMenuClick(object sender, RoutedEventArgs e)
        {
            var fse = mouseElement.DataContext as FileSystemEntity;
            if (fse == null)
                return;

            if (!CustomMessageBox.Ask(string.Format("您确定要删除{0}[{1}]吗？", EnumHelper.GetName(fse.Type.GetType(), fse.Type), fse.Name)))
                return;
            BusyIndicator1.IsBusy = true;
            BusyIndicator1.BusyContent = "正在执行...";
            if (fse.Type == FileSystemEntityType.Folder)
            {
                documentContext.DeleteFolder(fse.FolderId, AuthenticateStatus.CurrentUser.UserId, obj =>
                    {
                        BusyIndicator1.IsBusy = false;
                        if (Utility.Utility.CheckInvokeOperation(obj))
                        {
                            if (obj.Value == -2)
                            {
                                CustomMessageBox.Show("当前文件夹不为空，不能删除！");
                            }
                            else if (obj.Value > 0)
                            {
                                OpenFolder(FolderId);
                            }
                            else
                            {
                                CustomMessageBox.Alert("删除文件夹失败，错误码：" + obj.Value);
                            }
                        }
                    }, null);
            }
            else
            {
                documentContext.DeleteFile(fse.FileId, fse.FileRevision, obj =>
                {
                    BusyIndicator1.IsBusy = false;
                    if (Utility.Utility.CheckInvokeOperation(obj))
                    {
                        if (obj.Value == -2)
                        {
                            CustomMessageBox.Show("没有找到要删除的文件信息！");
                        }
                        else if (obj.Value > 0)
                        {
                            OpenFolder(FolderId);
                        }
                        else
                        {
                            CustomMessageBox.Alert("删除文件失败，错误码：" + obj.Value);
                        }
                    }
                }, null);
            }
        }

        private void OnRenameContextMenuClick(object sender, RoutedEventArgs e)
        {
            var fse = mouseElement.DataContext as FileSystemEntity;
            if (fse == null)
                return;
            string strName = CustomMessageBox.Prompt("请输入新的文件夹名称:");
            if (string.IsNullOrEmpty(strName) || strName.Equals(fse.Name, StringComparison.CurrentCultureIgnoreCase))
                return;
            var source = FileEntityListBox.ItemsSource as List<FileSystemEntity>;
            if (source != null && source.Any(o => o.Type == FileSystemEntityType.Folder
                && o.Name.Equals(strName, StringComparison.CurrentCultureIgnoreCase)
                && o.FolderId != fse.FolderId))
            {
                CustomMessageBox.Alert("新的文件夹名称与其他文件夹名重复！");
                return;
            }
            BusyIndicator1.IsBusy = true;
            BusyIndicator1.BusyContent = "正在执行...";
            documentContext.RenameFolder(fse.FolderId, strName, obj =>
                {
                    BusyIndicator1.IsBusy = false;
                    if (Utility.Utility.CheckInvokeOperation(obj))
                    {
                        if (obj.Value > 0)
                            fse.Name = strName;
                        else
                            CustomMessageBox.Show("重命名失败！错误码：" + obj.Value);
                    }
                }, null);
        }

        private void OnRevisionContextMenuClick(object sender, RoutedEventArgs e)
        {
            var fse = mouseElement.DataContext as FileSystemEntity;
            if (fse == null)
                return;
            if (fse.Type == FileSystemEntityType.Folder)
                return;
            if (NavigationService != null)
                NavigationService.Navigate(
                    new Uri(string.Format("Req/EditApplication?ReviseFile={0}", fse.FileId),
                            UriKind.Relative));
        }

        private void OnContextMenuOpend(object sender, RoutedEventArgs e)
        {
            var cm = sender as ContextMenu;
            if (cm == null)
                return;
            var fse = mouseElement.DataContext as FileSystemEntity;
            if (fse == null)
            {
                cm.IsOpen = false;
            }
            else
            {
                FileEntityListBox.SelectedItem = fse;
                bool isFolder = fse.Type == FileSystemEntityType.Folder;
                contextMenuiItems[0].Visibility = isFolder && AuthenticateStatus.CheckModuleAccess("100102") ? Visibility.Visible : Visibility.Collapsed;
                contextMenuiItems[1].Visibility = isFolder || !AuthenticateStatus.CheckModuleAccess("100102") ? Visibility.Collapsed : Visibility.Visible;
                contextMenuiItems[2].Visibility = (isFolder && AuthenticateStatus.CheckModuleAccess("100106")
                        || (!isFolder && AuthenticateStatus.CheckModuleAccess("100107"))) ? Visibility.Visible : Visibility.Collapsed;
                contextMenuiItems[3].Visibility = isFolder && AuthenticateStatus.CheckModuleAccess("100108") ? Visibility.Visible : Visibility.Collapsed;
                contextMenuiItems[4].Visibility = isFolder || !AuthenticateStatus.CheckModuleAccess("100103") ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void OnCreateFolderButtonClick(object sender, RoutedEventArgs e)
        {
            var orgId = OrganizationTreeView.SelectedValue as String;
            if (!string.IsNullOrEmpty(orgId))
            {
                var source = FileEntityListBox.ItemsSource as List<FileSystemEntity> ?? new List<FileSystemEntity>();
                string strFolderName;
                while (true)
                {
                    strFolderName = CustomMessageBox.Prompt("请输入文件夹名称");
                    if (string.IsNullOrEmpty(strFolderName))
                        return;
                    if (source.Any(o => o.Name == strFolderName && o.Type == FileSystemEntityType.Folder))
                    {
                        if (CustomMessageBox.Ask("文件夹名重复，是否重新指定？"))
                            continue;
                        return;
                    }
                    break;
                }
                var folder = new DocumentFolder
                    {
                        FolderId = 0,
                        ParentId = FolderId,
                        Name = strFolderName,
                        CreatedBy = AuthenticateStatus.CurrentUser.UserId,
                        CreateTime = DateTime.Now,
                        OrganizationId = orgId,
                        Status = ActiveStatus.Active
                    };
                BusyIndicator1.IsBusy = true;
                BusyIndicator1.BusyContent = "正在创建文件夹...";
                documentContext.CreateFolder(folder, obj =>
                    {
                        BusyIndicator1.IsBusy = false;
                        if (Utility.Utility.CheckInvokeOperation(obj))
                        {
                            OpenFolder(FolderId);
                        }
                    }, null);
            }
        }

        private void OpenFolder(int parentId)
        {
            FolderId = parentId;
            btnBackToParentFolder.Visibility = FolderId < 1 ? Visibility.Collapsed : Visibility.Visible;
            if (parentId < 1)
            {
                OpenFolder(OrgId);
            }
            else
            {
                btnCreateFolder.Visibility = AuthenticateStatus.GetModuleVisibility("100104");
                btnCreateApplication.Visibility = AuthenticateStatus.GetModuleVisibility("100105");
                BusyIndicator1.IsBusy = true;
                BusyIndicator1.BusyContent = "正在加载...";
                documentContext.GetFileSystemEntityByFolder(parentId, AuthenticateStatus.CurrentUser.UserId, obj =>
                    {
                        BusyIndicator1.IsBusy = false;
                        if (Utility.Utility.CheckInvokeOperation(obj))
                        {
                            FileEntityListBox.ItemsSource = obj.Value;
                        }
                    }, null);
            }
        }

        private void OpenFolder(string orgId)
        {
            if (string.IsNullOrEmpty(orgId))
                return;
            btnCreateFolder.Visibility = AuthenticateStatus.GetModuleVisibility("100104");
            btnCreateApplication.Visibility = AuthenticateStatus.GetModuleVisibility("100105");
            OrgId = orgId;
            BusyIndicator1.IsBusy = true;
            BusyIndicator1.BusyContent = "正在加载...";
            documentContext.GetFileSystemEntityByOrg(orgId, AuthenticateStatus.CurrentUser.UserId, obj =>
            {
                BusyIndicator1.IsBusy = false;
                if (Utility.Utility.CheckInvokeOperation(obj))
                {
                    FileEntityListBox.ItemsSource = obj.Value;
                }
            }, null);
        }

        private void OnCreateApplicationButtonClick(object sender, RoutedEventArgs e)
        {
            string orgId = OrgId;
            if (string.IsNullOrEmpty(orgId))
                orgId = AuthenticateStatus.DefaultOrganization;
            if (NavigationService != null)
            {
                NavigationService.Navigate(
                    new Uri(string.Format("Req/EditApplication?OrgId={0}&FolderId={1}", orgId, FolderId),
                            UriKind.Relative));
            }
            else
            {
                Content = new EditApplication { OrgId = orgId, FolderId = FolderId, BackUrl = "Browser" };
            }
        }

        private void OnOrganizationSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            string orgId = OrganizationTreeView.SelectedValue.ToString();
            if (!string.IsNullOrEmpty(orgId))
            {
                OpenFolder(orgId);
            }
        }

        private void OnFileSystemEntityClick(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as Grid;
            e.Handled = true;

            if (grid == null)
                return;

            if (grid.Tag == null)
            {
                grid.Tag = DateTime.Now.TimeOfDay;
                return;
            }

            var ts = (TimeSpan)grid.Tag;
            var span = DateTime.Now.TimeOfDay - ts;
            if (span.TotalMilliseconds < 300)
            {
                var fse = grid.DataContext as FileSystemEntity;
                if (fse == null)
                    return;

                if (fse.Type == FileSystemEntityType.Folder)
                {
                    OpenFolder(fse.FolderId);
                }
                else
                {
                    BrowseFileInfo(fse);
                }
            }
            grid.Tag = DateTime.Now.TimeOfDay;
        }

        private void BrowseFileInfo(FileSystemEntity fse)
        {
            if (fse.Type == FileSystemEntityType.Folder && fse.FolderId > 0)
            {
                OpenFolder(fse.FolderId);
            }
            else
            {
                Utility.Utility.BrowseFileInfo(fse, BusyIndicator1);
            }
        }

        private void OnBackToParentFolderButtonClick(object sender, RoutedEventArgs e)
        {
            if (FolderId < 1)
            {
                OpenFolder(FolderId);
            }
            else
            {
                BusyIndicator1.IsBusy = true;
                BusyIndicator1.BusyContent = "正在加载...";
                documentContext.BackToParentFolder(FolderId, AuthenticateStatus.CurrentUser.UserId, obj =>
                    {
                        BusyIndicator1.IsBusy = false;
                        if (Utility.Utility.CheckInvokeOperation(obj))
                        {
                            FileEntityListBox.ItemsSource = obj.Value;
                            if (obj.Value != null && obj.Value.Count > 0)
                                FolderId = obj.Value[0].ParentFolder;
                            else
                                FolderId = 0;
                            btnBackToParentFolder.Visibility = FolderId > 0
                                                                   ? Visibility.Visible
                                                                   : Visibility.Collapsed;
                        }
                    }, null);
            }
        }

        private void OnListBoxMouseMove(object sender, MouseEventArgs e)
        {
            mouseElement = e.OriginalSource as FrameworkElement;
        }

        private void OnSearchFileButtonClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSearchKey.Text))
            {
                BusyIndicator1.IsBusy = true;
                BusyIndicator1.BusyContent = "正在搜索...";
                int searchFolde = FolderId;
                if (searchFolde == 0) searchFolde = -1;
                documentContext.SearchFile(txtSearchKey.Text, OrgId, searchFolde, AuthenticateStatus.CurrentUser.UserId, obj =>
                    {
                        BusyIndicator1.IsBusy = false;
                        if (Utility.Utility.CheckInvokeOperation(obj))
                        {
                            FileEntityListBox.ItemsSource = obj.Value;
                            btnBackToParentFolder.Visibility
                                = btnCreateFolder.Visibility
                                  = btnCreateApplication.Visibility
                                    = Visibility.Collapsed;
                        }
                    }, null);
            }
        }
    }
}
