using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DocumentManage.Entities;
using DocumentManage.Utility;
using DocumentManage.Views.Doc;
using DocumentManageService.Web;

namespace DocumentManage.Views.Req
{
    public partial class NewApplication : Page
    {
        private readonly WorkflowDomainContext flowContext = new WorkflowDomainContext();
        private readonly DocumentDomainContext docContext = new DocumentDomainContext();

        public string OrgId { get; set; }

        public int FolderId { get; set; }

        public NewApplication()
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
            new DocumentDomainContext().GetFolder(FolderId, obj =>
            {
                if (Utility.Utility.CheckInvokeOperation(obj))
                {
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
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }

        private void OnBackToBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            Content = new Browser { OrgId = OrgId, FolderId = FolderId };
        }

        private void OnSaveWrokflowButtonClick(object sender, RoutedEventArgs e)
        {
            if (ValidateWorkflowInfo())
            {
                var flow = LayoutRoot.DataContext as ArchiveWorkflow;
                foreach (var file in flow.Files)
                {
                    if(file.DocumentInfo == null)
                        continue;
                    BusyIndicator1.IsBusy = true;
                    BusyIndicator1.BusyContent = string.Format("正在上传: {0}", file.DocumentInfo.FileName);
                    WorkflowFileInfo file1 = file;
                    docContext.UploadFile(file.DocumentInfo, obj =>
                                                                 {
                                                                     BusyIndicator1.IsBusy = false;
                                                                     if(Utility.Utility.CheckInvokeOperation(obj))
                                                                     {
                                                                     }
                                                                 }, null);
                }
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
            //foreach (var file in flow.Files)
            //{
            //    var sb = new StringBuilder();
            //    sb.AppendFormat("FileName:{0}{1}Length:{2}", file.DocumentInfo.FileName, Environment.NewLine,
            //                    file.DocumentInfo.Content.Length);
            //    if (file.DocumentInfo.Descriptions != null)
            //        foreach (var desc in file.DocumentInfo.Descriptions)
            //        {
            //            sb.AppendFormat("{0}{1}={2}", Environment.NewLine, desc.Header, desc.Description);
            //        }
            //    MessageBox.Show(sb.ToString());
            //}
            return true;
        }

        private void OnSubmitWorkflowButtonClick(object sender, RoutedEventArgs e)
        {
            if (ValidateWorkflowInfo())
            {

            }
        }

        private void OnUploadFileButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                foreach (var file in dialog.Files)
                {
                    var doc = new Document
                                  {
                                      CreateTime = DateTime.Now,
                                      CreatedBy = AuthenticateStatus.CurrentUser.UserId,
                                      DocumentId = 0,
                                      FolderId = FolderId,
                                      OrganizationId = OrgId,
                                      FileName = file.Name,
                                      OrignalName = file.Name,
                                      FilePath = string.Empty,
                                      UniqeName = Guid.NewGuid(),
                                      FileType = Utility.Utility.GetFileTypeByExtension(file.Extension),
                                      Revision = 1,
                                      Status = DocumentStatus.Draft,
                                      LastUpdateTime = DateTime.Now,
                                      LastUpdatedBy = AuthenticateStatus.CurrentUser.UserId,
                                      Content = new byte[file.Length]
                                  };
                    using (var stream = file.OpenRead())
                        stream.Read(doc.Content, 0, doc.Content.Length);

                    var source = UploadedFilesList.ItemsSource as List<WorkflowFileInfo> ?? new List<WorkflowFileInfo>();

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

        private void OnUploadedFileDeleted(object sender, EventArgs e)
        {
            var file = fileDescriber1.FileInfo;
            var source = UploadedFilesList.ItemsSource as List<WorkflowFileInfo>;
            if (file != null && source != null)
            {
                var removeItem =
                    source.FirstOrDefault(o => o.DocumentInfo != null && o.DocumentInfo.UniqeName == file.UniqeName);
                if (removeItem != null)
                    source.Remove(removeItem);
                UploadedFilesList.ItemsSource = null;
                UploadedFilesList.ItemsSource = source;
            }
        }
    }
}
