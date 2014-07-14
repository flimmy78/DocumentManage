using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using DocumentManage.Utility;
using DocumentManageService.Web;

namespace DocumentManage.Views.Sys
{
    public partial class WindowViewMessage : ChildWindow
    {
        private readonly SysMessageDomainContext msgContext = new SysMessageDomainContext();
        public WindowViewMessage()
        {
            InitializeComponent();
            Loaded += OnWindowLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            msgContext.GetMessageInfo(MessageId, obj =>
                {
                    if (Utility.Utility.CheckInvokeOperation(obj))
                    {
                        txtContent.Content = obj.Value.MessageContent;
                        txtTitle.Content = obj.Value.MessageTitle;
                    }
                }, null);
            msgContext.GetMessageReleaseInfo(MessageId, obj =>
                {
                    if (Utility.Utility.CheckInvokeOperation(obj))
                    {
                        var sb = new StringBuilder();
                        sb.Append("阅读权限：");
                        foreach (var s in obj.Value)
                        {
                            sb.AppendFormat("{0}，", s);
                        }
                        if (sb.Length > 6)
                            sb.Remove(sb.Length - 1, 1);
                        txtReleaseInfo.Text = sb.ToString();
                    }
                }, null);
        }

        public int MessageId { get; set; }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            msgContext.UserReviewSysMessage(AuthenticateStatus.CurrentUser.UserId, MessageId);
            DialogResult = true;
        }
    }
}

