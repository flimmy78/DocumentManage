using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.DomainServices.Client;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using DocumentManage.Entities;
using DocumentManage.Utility;
using DocumentManage.Views.Pro;
using DocumentManage.Views.Req;
using DocumentManage.Views.Sys;
using DocumentManageService.Web;

namespace DocumentManage.Views.Doc
{
    //文件及文件夹右键功能
    public partial class FileBrowser : Page
    {
        private readonly DocumentDomainContext docContext = new DocumentDomainContext();
        private FrameworkElement mouseElement;
        private readonly List<MenuItem> contextMenuiItems = new List<MenuItem>();

        public int FolderId { get; set; }

        public FileBrowser()
        {
            FolderId = -1;
            InitializeComponent();
            SearchFilePanel.Visibility = AuthenticateStatus.GetModuleVisibility("100101");
            btnCreateFolder.Visibility = AuthenticateStatus.GetModuleVisibility("100104");
            btnNewApplication.Visibility = AuthenticateStatus.GetModuleVisibility("100105");
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
            var item6 = new MenuItem { Header = "发布到组织" };
            item6.Click += OnReleaseOrgContextMenuClick;
            contextMenuiItems.Add(item6);
            context.Items.Add(item6);
            var item7 = new MenuItem { Header = "发布到个人" };
            item7.Click += OnReleaseUserContextMenuClick;
            contextMenuiItems.Add(item7);
            context.Items.Add(item7);
            var item8 = new MenuItem { Header = "查看访问权限" };
            item8.Click += OnViewReleaseContextMenuClick;
            contextMenuiItems.Add(item8);
            context.Items.Add(item8);
            context.Opened += OnContextMenuOpend;
            ContextMenuService.SetContextMenu(FileBrowseListBox, context);
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
                FileBrowseListBox.SelectedItem = fse;
                bool isFolder = fse.Type == FileSystemEntityType.Folder;
                contextMenuiItems[0].Visibility = isFolder && AuthenticateStatus.CheckModuleAccess("100102") ? Visibility.Visible : Visibility.Collapsed;
                contextMenuiItems[1].Visibility = isFolder || !AuthenticateStatus.CheckModuleAccess("100102") ? Visibility.Collapsed : Visibility.Visible;
                contextMenuiItems[2].Visibility = (isFolder && AuthenticateStatus.CheckModuleAccess("100106")
                        || (!isFolder && AuthenticateStatus.CheckModuleAccess("100107"))) ? Visibility.Visible : Visibility.Collapsed;
                contextMenuiItems[3].Visibility = isFolder && AuthenticateStatus.CheckModuleAccess("100108") ? Visibility.Visible : Visibility.Collapsed;
                contextMenuiItems[4].Visibility = isFolder || !AuthenticateStatus.CheckModuleAccess("100103") ? Visibility.Collapsed : Visibility.Visible;
                contextMenuiItems[5].Visibility = (isFolder && AuthenticateStatus.CheckModuleAccess("100401")
                        || (!isFolder && AuthenticateStatus.CheckModuleAccess("100403"))) ? Visibility.Visible : Visibility.Collapsed;
                contextMenuiItems[6].Visibility = (isFolder && AuthenticateStatus.CheckModuleAccess("100402")
                        || (!isFolder && AuthenticateStatus.CheckModuleAccess("100404"))) ? Visibility.Visible : Visibility.Collapsed;
                contextMenuiItems[7].Visibility = AuthenticateStatus.CheckModuleAccess("100405") ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            InitContextMenu();
            LoadUserRootFolders();
        }

        private void LoadUserRootFolders()
        {
            var rootElement = new TreeViewItem
            {
                Header = new Label { Content = "泰禾光电", Foreground = new SolidColorBrush(Colors.White) },
                IsExpanded = true,
                IsSelected = true,
                DataContext = new FileSystemEntity { Type = FileSystemEntityType.Folder, FolderId = -1, OrgId = "10" }
            };
            UserFoldersTree.Items.Add(rootElement);
        }

        private void OnUserFolderSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selectedItem = UserFoldersTree.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                var fse = selectedItem.DataContext as FileSystemEntity;
                if (fse != null)
                {
                    FolderId = fse.FolderId;
                    OpenFolder(FolderId);
                }
            }
        }

        private void BindFileSystemEntityList(InvokeOperation<List<FileSystemEntity>> obj)
        {
            BusyIndicator1.IsBusy = false;
            if (Utility.Utility.CheckInvokeOperation(obj))
            {
                FileBrowseListBox.ItemsSource = obj.Value;

                var selectedItem = UserFoldersTree.SelectedItem as TreeViewItem;
                if (selectedItem != null)
                {
                    selectedItem.Items.Clear();
                    foreach (var item in obj.Value)
                    {
                        if (item.Type == FileSystemEntityType.Folder)
                        {
                            selectedItem.Items.Add(new TreeViewItem
                            {
                                Header = new Label
                                {
                                    Content = item.Name,
                                    Foreground = new SolidColorBrush(Colors.White)
                                },
                                DataContext = item
                            });
                        }
                    }
                    selectedItem.IsExpanded = true;
                }
            }
        }

        private void OnDeleteContextMenuClick(object sender, RoutedEventArgs e)
        {
            var fse = mouseElement.DataContext as FileSystemEntity;
            if (fse == null)
                return;

            if (!CustomMessageBox.Ask(string.Format("您确定要删除{0}[{1}]吗？该操作不可恢复，请谨慎操作！", EnumHelper.GetName(fse.Type.GetType(), fse.Type), fse.Name)))
                return;
            BusyIndicator1.IsBusy = true;
            BusyIndicator1.BusyContent = "正在执行...";
            if (fse.Type == FileSystemEntityType.Folder)
            {
                docContext.DeleteFolder(fse.FolderId, AuthenticateStatus.CurrentUser.UserId, obj =>
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
                docContext.DeleteFile(fse.FileId, fse.FileRevision, obj =>
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
            var dlg = new PromptWindow();
            dlg.SetTitle("请输入新的文件夹名称");
            dlg.OnOk += (oo, ee) =>
                {
                    string strName = dlg.InputText;
                    if (string.IsNullOrEmpty(strName) || strName.Equals(fse.Name, StringComparison.CurrentCultureIgnoreCase))
                        return;
                    var source = FileBrowseListBox.ItemsSource as List<FileSystemEntity>;
                    if (source != null && source.Any(o => o.Type == FileSystemEntityType.Folder
                        && o.Name.Equals(strName, StringComparison.CurrentCultureIgnoreCase)
                        && o.FolderId != fse.FolderId))
                    {
                        CustomMessageBox.Alert("新的文件夹名称与其他文件夹名重复！");
                        return;
                    }
                    BusyIndicator1.IsBusy = true;
                    BusyIndicator1.BusyContent = "正在执行...";
                    docContext.RenameFolder(fse.FolderId, strName, obj =>
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
                };
            dlg.Show();
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

        private void OnOpenContextMenuClick(object sender, RoutedEventArgs e)
        {
            var fse = mouseElement.DataContext as FileSystemEntity;
            if (fse == null)
                return;
            SelectUserFolderTreeItem(fse);
        }

        private void OnBrowseContextMenuClick(object sender, RoutedEventArgs e)
        {
            var fse = mouseElement.DataContext as FileSystemEntity;
            if (fse == null)
                return;
            BrowseFileInfo(fse);
        }

        private void OnReleaseOrgContextMenuClick(object sender, RoutedEventArgs e)
        {
            var fse = mouseElement.DataContext as FileSystemEntity;
            if (fse == null)
                return;
            var win = new WindowSelectOrg();
            win.Closed += (o, ea) =>
            {
                if (win.DialogResult.HasValue && win.DialogResult.Value)
                {
                    foreach (var c in win.SelectedOrg)
                    {
                        if (c.IncludeChildOrg)
                        {
                            var dr = new DocumentRelease
                            {
                                ReleaseType = DocumentReleaseType.Organization,
                                DocumentId = fse.Type == FileSystemEntityType.Folder ? Guid.Empty : fse.FileId,
                                FolderId = fse.Type == FileSystemEntityType.Folder ? fse.FolderId : 0,
                                ReviewUserId = 0,
                                ReviewOrgId = c.OrganizationId,
                                ReleaseTime = DateTime.Now,
                                ReleasedBy = AuthenticateStatus.CurrentUser.UserId
                            };
                            docContext.ReleaseDocument(dr);
                        }
                    }
                }
            };
            win.Show();
        }

        private void OnReleaseUserContextMenuClick(object sender, RoutedEventArgs e)
        {
            var fse = mouseElement.DataContext as FileSystemEntity;
            if (fse == null)
                return;
            var win = new WindowSelectUser();
            win.Closed += (o, ea) =>
            {
                if (win.DialogResult.HasValue && win.DialogResult.Value)
                {
                    foreach (var c in win.SelectedUsers)
                    {
                        var dr = new DocumentRelease
                        {
                            ReleaseType = DocumentReleaseType.SystemUser,
                            DocumentId = fse.Type == FileSystemEntityType.Folder ? Guid.Empty : fse.FileId,
                            FolderId = fse.Type == FileSystemEntityType.Folder ? fse.FolderId : 0,
                            ReviewUserId = c.UserId,
                            ReviewOrgId = string.Empty,
                            ReleaseTime = DateTime.Now,
                            ReleasedBy = AuthenticateStatus.CurrentUser.UserId
                        };
                        docContext.ReleaseDocument(dr);
                    }
                }
            };
            win.Show();
        }

        private void OnViewReleaseContextMenuClick(object sender, RoutedEventArgs e)
        {
            var fse = mouseElement.DataContext as FileSystemEntity;
            if (fse == null)
                return;
            var win = new WindowReleaseHistory { FileEntity = fse };
            win.Show();
        }

        private void OpenFolder(int parentId)
        {
            if (parentId < 0) parentId = 0;
            BusyIndicator1.IsBusy = true;
            BusyIndicator1.BusyContent = "正在加载...";
            FolderId = parentId;
            docContext.GetFileSystemEntityByFolder(parentId, AuthenticateStatus.CurrentUser.UserId, BindFileSystemEntityList, null);
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

        private void OnListBoxMouseMove(object sender, MouseEventArgs e)
        {
            mouseElement = e.OriginalSource as FrameworkElement;
        }

        private void OnFileSystemEntityClick(object sender, MouseButtonEventArgs e)
        {
            var stack = sender as StackPanel;
            e.Handled = true;

            if (stack == null)
                return;

            if (stack.Tag == null)
            {
                stack.Tag = DateTime.Now.TimeOfDay;
                return;
            }

            var ts = (TimeSpan)stack.Tag;
            var span = DateTime.Now.TimeOfDay - ts;
            if (span.TotalMilliseconds < 300)
            {
                var fse = stack.DataContext as FileSystemEntity;
                if (fse == null)
                    return;

                if (fse.Type == FileSystemEntityType.Folder)
                {
                    SelectUserFolderTreeItem(fse);
                }
                else
                {
                    BrowseFileInfo(fse);
                }
            }
            stack.Tag = DateTime.Now.TimeOfDay;
        }

        private void SelectUserFolderTreeItem(FileSystemEntity fse)
        {
            if (fse == null)
                return;
            var sItem = UserFoldersTree.SelectedItem as TreeViewItem;
            if (sItem != null && sItem.Items != null && sItem.Items.Count > 0)
            {
                foreach (TreeViewItem item in sItem.Items)
                {
                    var tmp = item.DataContext as FileSystemEntity;
                    if (tmp != null && (ReferenceEquals(tmp, fse) || tmp.FolderId == fse.FolderId))
                    {
                        item.IsSelected = true;
                        break;
                    }
                }
            }
        }

        private void OnCreateApplicationButtonClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null)
            {
                NavigationService.Navigate(
                    new Uri(string.Format("Req/EditApplication?FolderId={0}", FolderId),
                            UriKind.Relative));
            }
            else
            {
                Content = new EditApplication { OrgId = string.Empty, FolderId = FolderId, BackUrl = "Browser" };
            }
        }

        private void OnInputFolderNameOk(object sender, EventArgs e)
        {
            var prompt = sender as PromptWindow;
            if (prompt != null)
            {
                prompt.OnOk -= OnInputFolderNameOk;
                string strFolderName = prompt.InputText;
                if (string.IsNullOrEmpty(strFolderName))
                    return;
                var source = FileBrowseListBox.ItemsSource as List<FileSystemEntity> ?? new List<FileSystemEntity>();
                if (source.Any(o => o.Name == strFolderName && o.Type == FileSystemEntityType.Folder))
                {
                    if (CustomMessageBox.Ask("文件夹名重复，是否重新指定？"))
                        OnCreateFolderButtonClick(null, null);
                }
                else
                {
                    var folder = new DocumentFolder
                    {
                        FolderId = 0,
                        ParentId = FolderId < 0 ? 0 : FolderId,
                        Name = strFolderName,
                        CreatedBy = AuthenticateStatus.CurrentUser.UserId,
                        CreateTime = DateTime.Now,
                        OrganizationId = null,
                        Status = ActiveStatus.Active
                    };
                    BusyIndicator1.IsBusy = true;
                    BusyIndicator1.BusyContent = "正在创建文件夹...";
                    docContext.CreateFolder(folder, obj =>
                    {
                        BusyIndicator1.IsBusy = false;
                        if (Utility.Utility.CheckInvokeOperation(obj))
                        {
                            OpenFolder(FolderId);
                        }
                    }, null);
                }
                prompt.Close();
            }
        }

        private void OnCreateFolderButtonClick(object sender, RoutedEventArgs e)
        {
            var prompt = new PromptWindow();
            prompt.SetTitle("请输入文件夹名称");
            prompt.OnOk += OnInputFolderNameOk;
            prompt.Show();
        }

        private void OnSearchFileButtonClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSearchKey.Text))
            {
                BusyIndicator1.IsBusy = true;
                BusyIndicator1.BusyContent = "正在搜索...";
                docContext.SearchFile(txtSearchKey.Text, string.Empty, FolderId == 0 ? -1 : FolderId, AuthenticateStatus.CurrentUser.UserId, obj =>
                {
                    BusyIndicator1.IsBusy = false;
                    if (Utility.Utility.CheckInvokeOperation(obj))
                        FileBrowseListBox.ItemsSource = obj.Value;
                }, null);
            }
        }

    }
}
