using System.Windows;
using System.Windows.Controls;
using DocumentManage.Entities;
using DocumentManageService.Web;

namespace DocumentManage.Views.Doc
{
    public partial class WindowReleaseHistory : ChildWindow
    {
        private readonly DocumentDomainContext docContext = new DocumentDomainContext();
        public FileSystemEntity FileEntity { get; set; }

        public WindowReleaseHistory()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void OnDeleteRecordButtonClick(object sender, RoutedEventArgs e)
        {
            if (!CustomMessageBox.Ask("您确定要删除该文档发布记录吗？该操作不可恢复！"))
                return;
            var btn = sender as Button;
            if (btn != null && btn.DataContext != null)
            {
                var dr = btn.DataContext as DocumentRelease;
                if (dr != null)
                {
                    docContext.RemoveDocumentRelease(dr, obj => LoadReleaseHistory(),null);
                }
            }
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            LoadReleaseHistory();
        }

        private void LoadReleaseHistory()
        {
            if (FileEntity != null)
            {
                BusyIndicator1.IsBusy = true;
                BusyIndicator1.BusyContent = "正在读取数据...";
                docContext.GetReleaseHistory(FileEntity, obj =>
                    {
                        BusyIndicator1.IsBusy = false;
                        if (Utility.Utility.CheckInvokeOperation(obj))
                        {
                            DocumentReleaseHistoryGrid.ItemsSource = obj.Value;
                        }
                    }, null);
            }
        }
    }
}

