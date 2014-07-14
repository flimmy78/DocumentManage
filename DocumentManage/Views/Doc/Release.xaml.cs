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
using DocumentManage.Views.Pro;
using DocumentManage.Views.Sys;
using DocumentManageService.Web;

namespace DocumentManage.Views.Doc
{
    public partial class Release : Page
    {
        private readonly OrganizationDomainContext orgContext = new OrganizationDomainContext();
        private readonly DocumentDomainContext documentContext = new DocumentDomainContext();
        private FrameworkElement mouseElement;
        private readonly List<MenuItem> contextMenuiItems = new List<MenuItem>();

        public string OrgId { get; set; }

        public int FolderId { get; set; }

        public Release()
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
            var item3 = new MenuItem { Header = "发布到组织" };
            item3.Click += OnReleaseOrgContextMenuClick;
            contextMenuiItems.Add(item3);
            context.Items.Add(item3);
            var item4 = new MenuItem { Header = "发布到个人" };
            item4.Click += OnReleaseUserContextMenuClick;
            contextMenuiItems.Add(item4);
            context.Items.Add(item4);
            var item5 = new MenuItem { Header = "查看访问权限" };
            item5.Click += OnViewReleaseContextMenuClick;
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
                            documentContext.ReleaseDocument(dr);
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
                        documentContext.ReleaseDocument(dr);
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
                contextMenuiItems[0].Visibility = isFolder ? Visibility.Visible : Visibility.Collapsed;
                contextMenuiItems[1].Visibility = isFolder ? Visibility.Collapsed : Visibility.Visible;
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
                documentContext.SearchFile(txtSearchKey.Text, OrgId, FolderId, AuthenticateStatus.CurrentUser.UserId, obj =>
                {
                    BusyIndicator1.IsBusy = false;
                    if (Utility.Utility.CheckInvokeOperation(obj))
                    {
                        FileEntityListBox.ItemsSource = obj.Value;
                        btnBackToParentFolder.Visibility
                                = Visibility.Collapsed;
                    }
                }, null);
            }
        }
    }
}
