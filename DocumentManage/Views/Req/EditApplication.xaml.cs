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
using DocumentManage.Views.Doc;
using DocumentManageService.Web;

namespace DocumentManage.Views.Req
{
    public partial class EditApplication : Page
    {
        private readonly WorkflowDomainContext flowContext = new WorkflowDomainContext();
        private readonly DocumentDomainContext docContext = new DocumentDomainContext();
        private string strParentFolder = string.Empty;
        private bool archiveFlowSaved;

        public string BackUrl { get; set; }

        public string OrgId { get; set; }

        public int FolderId { get; set; }

        public bool IsRevise { get; set; }

        public Document ReviseFile { get; set; }

        public ArchiveWorkflow ArchiveFlow { get; set; }

        public EditApplication()
        {
            InitializeComponent();
            Loaded += OnPageLoaded;
            flowContext.GetAllWorkflows(obj =>
            {
                if (Utility.Utility.CheckInvokeOperation(obj))
                {
                    ArchiveFlowCombBox.ItemsSource = obj.Value.Where(o => o.Status == ActiveStatus.Active);
                    if (ArchiveFlowCombBox.Items.Count > 0)
                        ArchiveFlowCombBox.SelectedIndex = 0;
                }
            }, null);
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            if (NavigationService == null)
                BindWorkflowInfo();
        }

        private void BindWorkflowInfo()
        {
            new DocumentDomainContext().GetFolder(FolderId, obj =>
                {
                    if (Utility.Utility.CheckInvokeOperation(obj))
                    {
                        ArchiveFlow.FolderId = FolderId;
                        CurrentFolderLabel.DataContext = obj.Value;
                    }
                }, null);
            new OrganizationDomainContext().GetOrganizationInfo(OrgId, obj =>
                {
                    if (Utility.Utility.CheckInvokeOperation(obj))
                    {
                        CurrentOrganizationLabel.DataContext = obj.Value;
                    }
                }, null);
            if (ArchiveFlow == null)
                ArchiveFlow = new ArchiveWorkflow();
            IsRevise = ArchiveFlow.IsRevise;
            btnUploadFile.Content = IsRevise ? "更新文件" : "添加文件";
            if (IsRevise && ArchiveFlow.Files != null && ArchiveFlow.Files.Count > 0)
            {
                ReviseFile = ArchiveFlow.Files[0].DocumentInfo;
            }
            Title = "编辑归档流程 - " + ArchiveFlow.FlowTitle;
            LayoutRoot.DataContext = ArchiveFlow;
            UploadedFilesList.ItemsSource = ArchiveFlow.Files;
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (NavigationService != null)
            {
                int flowId = NavigationContext.QueryString.ContainsKey("FlowId")
                    ? Convert.ToInt32(NavigationContext.QueryString["FlowId"])
                    : 0;
                Guid fileId = Guid.Empty;
                if (flowId > 0)
                {
                    BusyIndicator1.IsBusy = true;
                    BusyIndicator1.BusyContent = "正在加载数据...";
                    flowContext.GetArchiveWorkflow(flowId, obj =>
                        {
                            BusyIndicator1.IsBusy = false;
                            if (Utility.Utility.CheckInvokeOperation(obj))
                            {
                                ArchiveFlow = obj.Value;
                                var doc = ArchiveFlow.Files == null || ArchiveFlow.Files.Count < 1 ? null : ArchiveFlow.Files[0];
                                if (doc != null)
                                {
                                    FolderId = NavigationContext.QueryString.ContainsKey("FolderId")
                                        ? Convert.ToInt32(NavigationContext.QueryString["FolderId"])
                                        : ArchiveFlow.FolderId;

                                    OrgId = NavigationContext.QueryString.ContainsKey("OrgId")
                                        ? NavigationContext.QueryString["OrgId"]
                                        : doc.DocumentInfo.OrganizationId;
                                }
                                BindWorkflowInfo();
                            }
                        }, null);
                }
                else if (NavigationContext.QueryString.ContainsKey("ReviseFile") &&
                         Guid.TryParse(NavigationContext.QueryString["ReviseFile"], out fileId))
                {
                    ArchiveFlow = new ArchiveWorkflow { IsRevise = true };
                    IsRevise = true;
                    docContext.DownloadLatestFile(fileId, obj =>
                        {
                            if (Utility.Utility.CheckInvokeOperation(obj))
                            {
                                var doc = obj.Value;
                                if (doc == null)
                                {
                                    CustomMessageBox.Alert("您要修订的文件不存在，请您确认已经正确操作！");
                                    NavigationService.Navigate(new Uri(string.Empty, UriKind.Relative));
                                }
                                else
                                {
                                    if (ArchiveFlow.Files == null)
                                        ArchiveFlow.Files = new List<WorkflowFileInfo>();
                                    ArchiveFlow.Files.Clear();
                                    doc.Identity = 0;
                                    ArchiveFlow.Files.Add(new WorkflowFileInfo { DocumentId = 0, DocumentInfo = doc });
                                    ArchiveFlow.FlowTitle = string.Format("修订文档-{0}", doc.FileName);
                                    OrgId = doc.OrganizationId;
                                    FolderId = doc.FolderId;
                                    BindWorkflowInfo();
                                }
                            }
                        }, null);
                }
                else
                {
                    FolderId = NavigationContext.QueryString.ContainsKey("FolderId")
                        ? Convert.ToInt32(NavigationContext.QueryString["FolderId"])
                        : 0;
                    OrgId = NavigationContext.QueryString.ContainsKey("OrgId")
                     ? NavigationContext.QueryString["OrgId"]
                     : AuthenticateStatus.DefaultOrganization;
                    BindWorkflowInfo();
                }
            }
        }

        private void OnBackToBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
                return;
            }
            switch (BackUrl)
            {
                case "Browser":
                    Content = new FileBrowser { FolderId = FolderId };
                    break;
                default:
                    var child = Parent as ChildWindow;
                    if (child != null)
                        child.DialogResult = false;
                    break;
            }
        }

        private void OnSaveWrokflowButtonClick(object sender, RoutedEventArgs e)
        {
            if (ValidateWorkflowInfo())
            {
                archiveFlowSaved = false;
                SaveArchiveWorkflowInfo(AuditStatus.Draft);
            }
        }

        private void OnSubmitWorkflowButtonClick(object sender, RoutedEventArgs e)
        {
            if (ValidateWorkflowInfo())
            {
                archiveFlowSaved = false;
                SaveArchiveWorkflowInfo(AuditStatus.Submitted);
            }
        }

        private void SaveArchiveWorkflowInfo(AuditStatus status)
        {
            var flow = LayoutRoot.DataContext as ArchiveWorkflow;
            if (status == AuditStatus.Submitted && flow != null && ArchiveFlow.Files != null)
            {
                foreach (var file in ArchiveFlow.Files)
                {
                    file.DocumentInfo.Status = DocumentStatus.Pending;
                }
            }
            UploadFileAndSaveFlow(status, flow);
        }

        private void DoSaveArchiveWorkflow(AuditStatus status, ArchiveWorkflow flow)
        {
            archiveFlowSaved = true;
            BusyIndicator1.IsBusy = true;
            BusyIndicator1.BusyContent = "正在保存流程...";
            flow.SubmitUserId = AuthenticateStatus.CurrentUser.UserId;
            flow.Status = status;
            flowContext.SaveWorkflow(flow, obj =>
                {
                    BusyIndicator1.IsBusy = false;
                    if (Utility.Utility.CheckInvokeOperation(obj))
                    {
                        if (obj.Value == null)
                        {
                            CustomMessageBox.Show("流程保存失败，请您检查输入是否正确！");
                        }
                        else
                        {
                            ArchiveFlow = obj.Value;
                            if (ArchiveFlow.Status == AuditStatus.Draft)
                            {
                                LayoutRoot.DataContext = ArchiveFlow;
                            }
                            else if (ArchiveFlow.Status == AuditStatus.Auditing)
                            {
                                CustomMessageBox.Alert("流程提交成功！");
                                OnBackToBrowseButtonClick(this, null);
                            }
                        }
                    }
                }, null);
        }

        private WorkflowFileInfo GetNextUploadFile(ArchiveWorkflow flow)
        {
            return flow.Files.FirstOrDefault(o => o.DocumentInfo.Identity < 1);
        }

        private void UploadFileAndSaveFlow(AuditStatus status, ArchiveWorkflow flow)
        {
            var file = GetNextUploadFile(flow);
            if (file != null)
            {
                BusyIndicator1.IsBusy = true;
                BusyIndicator1.BusyContent = string.Format("正在{0}: {1}", 
                    file.DocumentInfo.FileType == DocumentType.Folder ? "创建文件夹" : "上传文件",
                    file.DocumentInfo.FileName);
                if (file.DocumentInfo.Content == null && !string.IsNullOrEmpty(file.DocumentInfo.FilePath))
                {
                    using (var stream = new FileStream(file.DocumentInfo.FilePath, FileMode.Open))
                    {
                        int fileLenght = (int)stream.Length;
                        file.DocumentInfo.Content = new byte[fileLenght];
                        stream.Read(file.DocumentInfo.Content, 0, fileLenght);
                        file.DocumentInfo.FilePath = string.Empty;
                    }
                }
                WorkflowFileInfo file1 = file;
                docContext.UploadFile(file.DocumentInfo, obj =>
                {
                    if (Utility.Utility.CheckInvokeOperation(obj))
                    {
                        if (obj.Value != null)
                        {
                            file1.DocumentId = obj.Value.Identity;
                            file1.DocumentInfo.Identity = obj.Value.Identity;
                        }
                        file1.DocumentInfo = obj.Value;
                        UploadFileAndSaveFlow(status, flow);
                    }
                }, null);
            }
            else
            {
                BusyIndicator1.IsBusy = false;
                DoSaveArchiveWorkflow(status, flow);
            }
        }

        private bool ValidateWorkflowInfo()
        {
            var flow = LayoutRoot.DataContext as ArchiveWorkflow;
            if (flow == null)
            {
                CustomMessageBox.Alert("获取工作流实例失败，请重试！");
                return false;
            }
            if (string.IsNullOrEmpty(flow.FlowTitle))
            {
                CustomMessageBox.Alert("请输入工作流标题！");
                txtFlowTitle.Focus();
                return false;
            }
            if (flow.Files == null || flow.Files.Count < 1)
            {
                CustomMessageBox.Alert("请选择您要提交的文件！");
                return false;
            }
            if (ArchiveFlowCombBox.SelectedIndex == -1)
            {
                CustomMessageBox.Alert("请选择归档流程！");
                ArchiveFlowCombBox.Focus();
                return false;
            }
            if (flow.FlowType < 1)
            {
                flow.FlowType = Convert.ToInt32(ArchiveFlowCombBox.SelectedValue);
            }
            var doc = flow.Files[0].DocumentInfo;
            if (IsRevise && doc.Identity < 1)
            {
                doc.Revision = ReviseFile.Revision + 1;
                doc.CreatedBy = AuthenticateStatus.CurrentUser.UserId;
                doc.CreateTime = DateTime.Now;
                doc.FilePath = string.Empty;
                doc.Status = DocumentStatus.Draft;
                if (doc.Descriptions != null)
                {
                    foreach (var desc in doc.Descriptions)
                    {
                        desc.Identity = 0;
                    }
                }
            }
            return true;
        }

        private void OnUploadFileButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = !IsRevise;
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                foreach (var file in dialog.Files)
                {
                    if (file.Length > 1024 * 1024 * 30)
                    {
                        MessageBox.Show(string.Format("文件[{0}]的大小超过30M，系统不允许上传大于30M的文件！",
                            Application.Current.IsRunningOutOfBrowser ? file.FullName : file.Name));
                        continue;
                    }
                    var doc = new Document
                                  {
                                      CreateTime = DateTime.Now,
                                      CreatedBy = AuthenticateStatus.CurrentUser.UserId,
                                      Identity = 0,
                                      FolderId = FolderId,
                                      OrganizationId = OrgId,
                                      FileName = file.Name,
                                      OrignalName = file.Name,
                                      FilePath = Application.Current.IsRunningOutOfBrowser ? file.FullName : string.Empty,
                                      UniqeName = IsRevise && ReviseFile != null ? ReviseFile.UniqeName : Guid.NewGuid(),
                                      FileType = Utility.Utility.GetFileTypeByExtension(file.Extension),
                                      Revision = IsRevise && ReviseFile != null ? ReviseFile.Revision : 1,
                                      Status = DocumentStatus.Draft,
                                      LastUpdateTime = DateTime.Now,
                                      LastUpdatedBy = AuthenticateStatus.CurrentUser.UserId,
                                      Descriptions = IsRevise && ReviseFile != null ? ReviseFile.Descriptions : null,
                                      Content = null
                                  };

                    if (ArchiveFlow.Files == null)
                        ArchiveFlow.Files = new List<WorkflowFileInfo>();
                    var source = ArchiveFlow.Files;
                    if (IsRevise && source.Count > 0)
                    {
                        if (source[0].DocumentInfo != null)
                            docContext.DeleteFile(source[0].DocumentInfo.UniqeName, source[0].DocumentInfo.Revision);
                        source.Clear();
                    }

                    source.Add(new WorkflowFileInfo
                    {
                        DocumentId = doc.DocumentId,
                        DocumentInfo = doc
                    });
                    UploadedFilesList.ItemsSource = null;
                    UploadedFilesList.ItemsSource = source;
                }
            }
        }

        private void OnFlowTypeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var flowType = Convert.ToInt32(ArchiveFlowCombBox.SelectedValue);
            if (ArchiveFlow != null)
                ArchiveFlow.FlowType = flowType;
        }

        private void OnFilesMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var lb = sender as ListBox;
            var sl = e.OriginalSource as FrameworkElement;
            if (lb == null || sl == null)
                return;
            if (lb.Tag != null)
            {
                var t1 = (TimeSpan)lb.Tag;
                if ((DateTime.Now.TimeOfDay - t1) < TimeSpan.FromMilliseconds(300))
                {
                    var wfile = sl.DataContext as WorkflowFileInfo;
                    if (wfile == null)
                        return;
                    Utility.Utility.BrowseFileInfo(wfile.DocumentInfo, BusyIndicator1);
                }
            }
            lb.Tag = DateTime.Now.TimeOfDay;

        }

        private void OnUploadFolderButtonClick(object sender, RoutedEventArgs e)
        {
            if (IsRevise)
            {
                CustomMessageBox.Show("修订文件时不能上传文件夹");
                return;
            }
            var dialog = new FolderBrowser();
            dialog.OnOk += OnSelectFolderCompleted;
            dialog.Show();
        }

        private void OnSelectFolderCompleted(object sender, EventArgs e)
        {
            var dlg = sender as FolderBrowser;
            if (dlg != null)
            {
                dlg.OnOk -= OnSelectFolderCompleted;
                string strFolder = dlg.SelectedFolder;
                var dir = new DirectoryInfo(strFolder);
                strParentFolder = dir.Parent.FullName;
                FetchFolderDocuments(dir);
                UploadedFilesList.ItemsSource = null;
                UploadedFilesList.ItemsSource = ArchiveFlow.Files;
            }
        }

        private void FetchFolderDocuments(DirectoryInfo dir)
        {
            if (ArchiveFlow.Files != null && ArchiveFlow.Files.Count > 1000)
            {
                CustomMessageBox.Show("请不要一次上传超过1000个文件(夹)");
                return;
            }
            string strFolder = dir.FullName.Remove(0, strParentFolder.Length);
            var folder = new Document
            {
                CreateTime = DateTime.Now,
                CreatedBy = AuthenticateStatus.CurrentUser.UserId,
                Identity = 0,
                FolderId = FolderId,
                OrganizationId = OrgId,
                FileName = strFolder,
                OrignalName = strFolder,
                FilePath = string.Empty,
                UniqeName = Guid.Empty,
                FileType = DocumentType.Folder,
                Revision = 1,
                Status = DocumentStatus.Draft,
                LastUpdateTime = DateTime.Now,
                LastUpdatedBy = AuthenticateStatus.CurrentUser.UserId,
                Descriptions = null,
                Content = null
            };
            if (ArchiveFlow.Files == null)
                ArchiveFlow.Files = new List<WorkflowFileInfo>();
            ArchiveFlow.Files.Add(new WorkflowFileInfo { DocumentId = folder.DocumentId, DocumentInfo = folder });

            foreach (var file in dir.EnumerateFiles())
            {
                if (!file.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    if (file.Length > 1024 * 1024 * 30)
                    {
                        MessageBox.Show(string.Format("文件[{0}]的大小超过30M，系统不允许上传大于30M的文件！",
                            Application.Current.IsRunningOutOfBrowser ? file.FullName : file.Name));
                        continue;
                    }
                    var tmpFile = new Document
                     {
                         CreateTime = DateTime.Now,
                         CreatedBy = AuthenticateStatus.CurrentUser.UserId,
                         Identity = 0,
                         FolderId = FolderId,
                         OrganizationId = OrgId,
                         FileName = Path.GetFileNameWithoutExtension(file.FullName),
                         OrignalName = file.FullName.Remove(0, strParentFolder.Length),
                         FilePath = file.FullName,
                         UniqeName = IsRevise && ReviseFile != null ? ReviseFile.UniqeName : Guid.NewGuid(),
                         FileType = Utility.Utility.GetFileTypeByExtension(file.Extension),
                         Revision = IsRevise && ReviseFile != null ? ReviseFile.Revision : 1,
                         Status = DocumentStatus.Draft,
                         LastUpdateTime = DateTime.Now,
                         LastUpdatedBy = AuthenticateStatus.CurrentUser.UserId,
                         Descriptions = IsRevise && ReviseFile != null ? ReviseFile.Descriptions : null,
                         Content = null
                     };

                    ArchiveFlow.Files.Add(new WorkflowFileInfo { DocumentId = tmpFile.DocumentId, DocumentInfo = tmpFile });
                }
            }
            foreach (var subFolder in dir.EnumerateDirectories())
            {
                FetchFolderDocuments(subFolder);
                if (ArchiveFlow.Files.Count > 1000)
                    break;
            }
        }

        private void OnDeleteFileButtonClick(object sender, RoutedEventArgs e)
        {
            if (UploadedFilesList.SelectedItems.Count < 1)
                return;
            var wfile = UploadedFilesList.SelectedItem as WorkflowFileInfo;
            if (wfile != null && wfile.DocumentInfo != null)
            {
                var doc = wfile.DocumentInfo;
                var removeItem =
                    ArchiveFlow.Files.FirstOrDefault(o => o.DocumentInfo.FileName != null && o.DocumentInfo.UniqeName == doc.UniqeName);
                if (removeItem != null)
                    ArchiveFlow.Files.Remove(removeItem);
                if (doc.Identity > 0 && CustomMessageBox.Ask(string.Format("确定要删除文件{0}吗", doc.FileName)))
                {
                    docContext.DeleteFile(doc.UniqeName, doc.Revision);
                }
                UploadedFilesList.ItemsSource = null;
                UploadedFilesList.ItemsSource = ArchiveFlow.Files;
            }
        }
    }
}
