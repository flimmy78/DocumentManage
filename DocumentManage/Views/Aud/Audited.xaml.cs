using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;
using DocumentManage.Entities;
using DocumentManage.Utility;
using DocumentManage.Views.Req;
using DocumentManageService.Web;
//归档审核
namespace DocumentManage.Views.Aud
{   
    //归档审核 -- 已审批页面
    public partial class Audited : Page
    {
        private readonly WorkflowDomainContext flowContext = new WorkflowDomainContext();
        private readonly EnumToStringValueConverter enumToStringConverter = new EnumToStringValueConverter();
        private PagedCollectionView pcv;

        public Audited()
        {
            InitializeComponent();
            ArchiveFlowGrid.LoadingRow += ArchiveFlowGridLoadingRow;  //添加DataGrid加载行事件
        }

        private void ArchiveFlowGridLoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.MouseLeftButtonUp += OnGridRowLeftButtonUp;   //添加了GataGrid左键双击事件
        }

        //点击审批项目时处理
        private void OnGridRowLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TimeSpan t = DateTime.Now.TimeOfDay;  //获取当前时间

            if (ArchiveFlowGrid.Tag != null){
                var oldT = (TimeSpan)ArchiveFlowGrid.Tag;
                if ((t - oldT) < TimeSpan.FromMilliseconds(300)){  // 延时300ms
                    var flow = ArchiveFlowGrid.SelectedItem as ArchiveWorkflow;
                    if (flow != null){
                        if (NavigationService != null){
                            //导航到相对页面
                            NavigationService.Navigate(new Uri("Req/ViewApplication?ID=" + flow.FlowId, UriKind.Relative));
                        }else{
                            BusyIndicator1.IsBusy = true;
                            BusyIndicator1.BusyContent = "正在读取流程信息...";
                            flowContext.GetArchiveWorkflow(flow.FlowId, obj =>
                            {
                                BusyIndicator1.IsBusy = false;
                                if (Utility.Utility.CheckInvokeOperation(obj)){
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

        // 导航到 "已审批" 子页面
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LoadArchiveWorkflow(AuditStatus.Audited);
        }

        //加载文档工作流
        private void LoadArchiveWorkflow(AuditStatus status)
        {
            flowContext.GetAuditFlowByUser(AuthenticateStatus.CurrentUser.UserId, status, obj =>
            {
                if (Utility.Utility.CheckInvokeOperation(obj))
                {
                    pcv = new PagedCollectionView(obj.Value) { PageSize = ArchiveFlowPager.PageSize };
                    ArchiveFlowGrid.ItemsSource = pcv;
                    ArchiveFlowPager.Source = pcv;
                }
            }, null);

        }

        //DataGrid自动产生列事件
        private void OnDataGridAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType.IsEnum){
                e.Column = new DataGridTextColumn
                {
                    Header = e.Column.Header,
                    Binding = new Binding(e.PropertyName) { Converter = enumToStringConverter }
                };
            }
        }
        //单击搜索按钮
        private void OnSearchButtonClick(object sender, RoutedEventArgs e)
        {
            if (pcv != null)
            {
                if (string.IsNullOrEmpty(txtSearchKey.Text))
                    LoadArchiveWorkflow(AuditStatus.Audited);
                else
                    pcv.Filter = FilterWorkflowByTitle;
            }
        }

        //工作流通过标题过滤
        private bool FilterWorkflowByTitle(object obj)
        {
            var flow = obj as ArchiveWorkflow;
            if (flow != null)
                return flow.FlowTitle.IndexOf(txtSearchKey.Text, StringComparison.Ordinal) > -1;
            return true;
        }
    }
}
