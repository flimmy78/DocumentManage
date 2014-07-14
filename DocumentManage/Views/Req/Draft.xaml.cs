using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using DocumentManage.Entities;
using DocumentManage.Utility;
using DocumentManageService.Web;
//归档申请
namespace DocumentManage.Views.Req
{
    public partial class Draft : Page
    {
        private readonly WorkflowDomainContext flowContext = new WorkflowDomainContext();
        private readonly EnumToStringValueConverter enumToStringConverter = new EnumToStringValueConverter();
        private PagedCollectionView pcv;

        public Draft()
        {
            InitializeComponent();
            ArchiveFlowGrid.LoadingRow += ArchiveFlowGridLoadingRow;
        }

        private void ArchiveFlowGridLoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.MouseLeftButtonUp += OnGridRowLeftButtonUp;
        }

        private void OnGridRowLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TimeSpan t = DateTime.Now.TimeOfDay;
            if (ArchiveFlowGrid.Tag != null)
            {
                var oldT = (TimeSpan)ArchiveFlowGrid.Tag;
                if ((t - oldT) < TimeSpan.FromMilliseconds(300))
                {
                    var flow = ArchiveFlowGrid.SelectedItem as ArchiveWorkflow;
                    if (flow != null)
                    {
                        if (NavigationService != null)
                            NavigationService.Navigate(
                                new Uri(string.Format("Req/EditApplication?FlowId={0}", flow.FlowId), UriKind.Relative));
                        else
                        {
                            BusyIndicator1.IsBusy = true;
                            BusyIndicator1.BusyContent = "正在读取流程信息...";
                            flowContext.GetArchiveWorkflow(flow.FlowId, obj =>
                                {
                                    BusyIndicator1.IsBusy = false;
                                    if (Utility.Utility.CheckInvokeOperation(obj))
                                    {
                                        var child = new ChildWindow { Title = "编辑流程信息" };
                                        child.Background = Application.Current.Resources["ControlBackgroundAltBrush"] as Brush;
                                        var doc = obj.Value.Files[0].DocumentInfo;
                                        var editWindow = new EditApplication
                                            {
                                                OrgId = doc.OrganizationId,
                                                FolderId = doc.FolderId,
                                                ArchiveFlow = obj.Value,
                                                Margin = new Thickness(0, 0, 0, 20)
                                            };
                                        child.Content = editWindow;
                                        child.Closed += (oo, ee) =>
                                            {
                                                if (editWindow.ArchiveFlow.Status == AuditStatus.Auditing && NavigationService != null)
                                                    NavigationService.Navigate(new Uri("Req/Auditing", UriKind.Relative));
                                            };
                                        child.Show();
                                    }
                                }, null);
                        }
                    }
                }
            }
            ArchiveFlowGrid.Tag = t;
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LoadArchiveWorkflow(AuditStatus.Draft);
        }

        private void LoadArchiveWorkflow(AuditStatus status)
        {
            flowContext.GetArchiveFlowByUser(AuthenticateStatus.CurrentUser.UserId, status, obj =>
                {
                    if (Utility.Utility.CheckInvokeOperation(obj))
                    {
                        pcv = new PagedCollectionView(obj.Value) { PageSize = ArchiveFlowPager.PageSize };
                        ArchiveFlowGrid.ItemsSource = pcv;
                        ArchiveFlowPager.Source = pcv;
                    }
                }, null);

        }

        private void OnDataGridAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType.IsEnum)
            {
                e.Column = new DataGridTextColumn
                {
                    Header = e.Column.Header,
                    Binding = new Binding(e.PropertyName) { Converter = enumToStringConverter }
                };
            }

        }

        private void OnSearchButtonClick(object sender, RoutedEventArgs e)
        {
            if (pcv != null)
            {
                if (string.IsNullOrEmpty(txtSearchKey.Text))
                    LoadArchiveWorkflow(AuditStatus.Draft);
                else
                    pcv.Filter = FilterWorkflowByTitle;
            }
        }

        private bool FilterWorkflowByTitle(object obj)
        {
            var flow = obj as ArchiveWorkflow;
            if (flow != null)
                return flow.FlowTitle.IndexOf(txtSearchKey.Text, StringComparison.Ordinal) > -1;
            return true;
        }

        private void OnDeleteFlowButtonClick(object sender, MouseButtonEventArgs e)
        {
            var img = sender as Image;
            if (img == null)
                return;
            var flow = img.DataContext as ArchiveWorkflow;
            if (flow != null && CustomMessageBox.Ask(string.Format("确定要删除流程[{0}]吗？该操作会将相应文件同时删除！", flow.FlowTitle)))
            {
                flowContext.DeleteArchiveWorkFlow(flow.FlowId, obj =>
                    {
                        if (Utility.Utility.CheckInvokeOperation(obj))
                        {
                            if (obj.Value > 0)
                            {
                                LoadArchiveWorkflow(AuditStatus.Draft);
                            }
                            else
                            {
                                CustomMessageBox.Show("删除流程失败，请与管理员联系！");
                            }
                        }
                    }, null);
            }
        }
    }
}
