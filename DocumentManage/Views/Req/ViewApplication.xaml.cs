using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using DocumentManage.Entities;
using DocumentManage.Utility;
using DocumentManageService.Web;
//归档申请
namespace DocumentManage.Views.Req
{
    public partial class ViewApplication : Page
    {
        private readonly WorkflowDomainContext flowContext = new WorkflowDomainContext();
        private static EnumToStringValueConverter enumStringConverter = new EnumToStringValueConverter();

        public ArchiveWorkflow ArchiveFlow { get; set; }

        public static readonly DependencyProperty ShowAuditCommandsProperty =
            DependencyProperty.Register("ShowAuditCommands", typeof(bool), typeof(ViewApplication),
                                        new PropertyMetadata(false, OnShowAuditCommandsChanged));

        private static void OnShowAuditCommandsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ViewApplication)d).OnShowAuditCommandsChanged(e.NewValue);
        }

        private void OnShowAuditCommandsChanged(object show)
        {
            Visibility v = Convert.ToBoolean(show) ? Visibility.Visible : Visibility.Collapsed;
            lblAuditDescription.Visibility = v;
            AuditCommandPanel.Visibility = v;
            txtAuditDescription.Visibility = v;
        }

        public bool ShowAuditCommands
        {
            get { return (bool)GetValue(ShowAuditCommandsProperty); }
            set { SetValue(ShowAuditCommandsProperty, value); }
        }

        public ViewApplication()
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
            if (NavigationService == null && ArchiveFlow != null)
            {
                BindWorkflowInfo();
            }
        }

        private void BindWorkflowInfo()
        {
            if (ArchiveFlow == null)
                return;
            Title = "查看归档流程-" + ArchiveFlow.FlowTitle;
            var folderId = ArchiveFlow.FolderId;
            var orgId = ArchiveFlow.Files[0].DocumentInfo.OrganizationId;

            new DocumentDomainContext().GetFolder(folderId, obj =>
                {
                    if (Utility.Utility.CheckInvokeOperation(obj))
                    {
                        CurrentFolderLabel.DataContext = obj.Value;
                    }
                }, null);
            new OrganizationDomainContext().GetOrganizationInfo(orgId, obj =>
                {
                    if (Utility.Utility.CheckInvokeOperation(obj))
                    {
                        CurrentOrganizationLabel.DataContext = obj.Value;
                    }
                }, null);
            flowContext.CanUserAuditWorkflow(ArchiveFlow.FlowId, ArchiveFlow.Status,
                                             AuthenticateStatus.CurrentUser.UserId, obj =>
                                                            {
                                                                if (Utility.Utility.CheckInvokeOperation(obj))
                                                                {
                                                                    ShowAuditCommands = obj.Value;
                                                                }
                                                            }, null);
            WorkflowInfoGrid.DataContext = ArchiveFlow;
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (NavigationService != null)
            {
                BackButton.Visibility = Visibility.Visible;
                int flowId = NavigationContext.QueryString.ContainsKey("ID")
                    ? Convert.ToInt32(NavigationContext.QueryString["ID"]) : 0;
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
                                BindWorkflowInfo();
                            }
                        }, null);
                    flowContext.GetFlowAuditRecords(flowId, obj =>
                    {
                        if (Utility.Utility.CheckInvokeOperation(obj))
                        {
                            AuditHistoryGrid.ItemsSource = obj.Value;
                        }
                    }, null);
                }
            }
            else
            {
                BackButton.Visibility = Visibility.Collapsed;
            }
        }

        private void OnBackButtonClick(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void OnGridAutoGenerateColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType.IsEnum)
            {
                e.Column = new DataGridTextColumn
                {
                    Header = e.Column.Header,
                    Binding = new Binding(e.PropertyName) { Converter = enumStringConverter }
                };
            }
        }

        private void OnAuditPassButtonClick(object sender, RoutedEventArgs e)
        {
            AuditWorkflow(AuditOperation.Audit);
        }

        private void OnAuditRejectButtonClick(object sender, RoutedEventArgs e)
        {
            AuditWorkflow(AuditOperation.Reject);
        }

        private void OnAuditReturnButtonClick(object sender, RoutedEventArgs e)
        {
            AuditWorkflow(AuditOperation.Return);
        }

        private void AuditWorkflow(AuditOperation op)
        {
            if (!string.IsNullOrEmpty(txtAuditDescription.Text))
            {
                var audit = new FlowAuditRecord
                                {
                                    Id = 0,
                                    FlowId = ArchiveFlow.FlowId,
                                    AuditUserId = AuthenticateStatus.CurrentUser.UserId,
                                    AuditTime = DateTime.Now,
                                    Operation = op,
                                    AuditDescription = txtAuditDescription.Text
                                };
                flowContext.AuditArchiveWorkflow(audit, obj =>
                                                            {
                                                                if (Utility.Utility.CheckInvokeOperation(obj))
                                                                {
                                                                    if (obj.Value > 0)
                                                                        MessageBox.Show("操作成功！");
                                                                    else
                                                                        MessageBox.Show("审核操作失败！错误码：" + obj.Value);
                                                                }
                                                            }, null);
            }
            else
            {
                CustomMessageBox.Show("请输入审核意见");
                txtAuditDescription.Focus();
            }
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
    }
}
