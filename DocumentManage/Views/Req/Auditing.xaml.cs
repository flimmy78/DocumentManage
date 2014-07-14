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
    //归档申请 -- 审批中页面
    public partial class Auditing : Page
    {
        private readonly WorkflowDomainContext flowContext = new WorkflowDomainContext();
        private readonly EnumToStringValueConverter enumToStringConverter = new EnumToStringValueConverter();
        private PagedCollectionView pcv;
        public Auditing()
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
                        if(NavigationService != null)
                            NavigationService.Navigate(new Uri("Req/ViewApplication?ID=" + flow.FlowId, UriKind.Relative));
                        else
                        {
                            BusyIndicator1.IsBusy = true;
                            BusyIndicator1.BusyContent = "正在读取流程信息...";
                            flowContext.GetArchiveWorkflow(flow.FlowId, obj =>
                            {
                                BusyIndicator1.IsBusy = false;
                                if (Utility.Utility.CheckInvokeOperation(obj))
                                {
                                    var child = new ChildWindow { Title = "查看归档流程-" + flow.FlowTitle };
                                    var win = new ViewApplication
                                    {
                                        ArchiveFlow = obj.Value,
                                        Margin = new Thickness(0, 0, 0, 20)
                                    };
                                    child.Content = win;
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
            LoadArchiveWorkflow(AuditStatus.Auditing);
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
                    LoadArchiveWorkflow(AuditStatus.Auditing);
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
    }
}
