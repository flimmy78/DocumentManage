using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DocumentManage.Entities;
using DocumentManage.Utility;
using DocumentManage.Views;
using DocumentManage.Views.Pro;
using DocumentManage.Views.Sys;
using DocumentManageService.Web;

namespace DocumentManage
{
    public partial class Home : Page
    {
        private List<Organization> selectedOrg = new List<Organization>();
        private List<SystemUser> selectedUser = new List<SystemUser>();
        private readonly SysMessageDomainContext msgContext = new SysMessageDomainContext();
        public Home()
        {
            InitializeComponent();
        }

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!AuthenticateStatus.CheckModuleAccess("100301"))
            {
                createMsgForm.Visibility = Visibility.Collapsed;
                Grid.SetColumnSpan(messagesStackpanel, 2);
            }
            LoadMessageList();
            ShowPopupMessages();
        }

        private void ShowPopupMessages()
        {
            msgContext.GetPopupMessageList(AuthenticateStatus.CurrentUser.UserId, obj =>
                {
                    if (Utility.Utility.CheckInvokeOperation(obj))
                    {
                        foreach (var m in obj.Value)
                        {
                            new WindowViewMessage {MessageId = m.MessageId}.Show();
                        }
                    }
                }, null);
        }

        private void OnReleaseOrgButtonClick(object sender, RoutedEventArgs e)
        {
            var win = new WindowSelectOrg();
            win.Closed += (o, ea) =>
            {
                if (true == win.DialogResult)
                {
                    foreach (var c in win.SelectedOrg)
                    {
                        if (c.IncludeChildOrg &&
                            selectedOrg.All(org => org.Id != c.OrganizationId))
                        {
                            selectedOrg.Add(c.Organization);
                        }
                    }
                    ShowMessageReleaseInfo();
                }
            };
            win.Show();
        }


        private void OnReleaseUserButtonClick(object sender, RoutedEventArgs e)
        {
            var win = new WindowSelectUser();
            win.Closed += (oo, ee) =>
                {
                    if (true == win.DialogResult)
                    {
                        foreach (var c in win.SelectedUsers)
                        {
                            if (selectedUser.All(user => user.UserId != c.UserId))
                                selectedUser.Add(c);
                        }
                        ShowMessageReleaseInfo();
                    }
                };
            win.Show();
        }

        private void ShowMessageReleaseInfo()
        {
            var sb = new StringBuilder();
            foreach (var c in selectedOrg)
            {
                sb.AppendFormat("组织：[{0}]{1}{2}", c.Id, c.Name, Environment.NewLine);
            }
            foreach (var c in selectedUser)
            {
                sb.AppendFormat("个人：[{0}]{1}{2}", c.UserName, c.RealName, Environment.NewLine);
            }
            txtMsgReleaseInfo.Text = sb.ToString();
        }

        private void OnReleaseMessageButtonClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtTitle.Text))
            {
                CustomMessageBox.Show("请输入消息标题！");
                return;
            }
            if (string.IsNullOrEmpty(txtContent.Text))
            {
                CustomMessageBox.Show("请输入消息内容！");
                return;
            }
            if (txtContent.Text.Length > 200)
            {
                CustomMessageBox.Show("消息内容最长为200个字符！");
                return;
            }
            if (selectedOrg.Count < 1 && selectedUser.Count < 1)
            {
                CustomMessageBox.Show("请为系统消息指定阅读权限！");
                return;
            }
            var msg = new SystemMessage
                {
                    MessageId = -1,
                    MessageTitle = txtTitle.Text,
                    MessageContent = txtContent.Text,
                    CreatedBy = AuthenticateStatus.CurrentUser.UserId,
                    CreateTime = DateTime.Now,
                    Identity = -1,
                    RemindTimes = (int)numRemindTimes.Value
                };
            BusyIndicator1.BusyContent = "正在发布消息....";
            BusyIndicator1.IsBusy = true;
            msgContext.CreateMessage(msg, GetMessageOrgs(), GetMessageUsers(), obj =>
                {
                    BusyIndicator1.IsBusy = false;
                    if (Utility.Utility.CheckInvokeOperation(obj))
                    {
                        ClearForm();
                        LoadMessageList();
                    }
                }, null);
        }

        private void ClearForm()
        {
            txtTitle.Text = txtContent.Text = string.Empty;
            numRemindTimes.Value = 3;
            selectedOrg.Clear();
            selectedUser.Clear();
            txtMsgReleaseInfo.Text = string.Empty;
        }

        private List<SystemMessageOrg> GetMessageOrgs()
        {
            return (from org in selectedOrg
                    select new SystemMessageOrg
                        {
                            Identity = -1,
                            MessageId = -1,
                            OrganizationId = org.Id,
                        }).ToList();
        }

        private void LoadMessageList()
        {
            BusyIndicator1.BusyContent = "正在加载消息列表....";
            BusyIndicator1.IsBusy = true;
            msgContext.GetUserMessagesList(AuthenticateStatus.CurrentUser.UserId, obj =>
                {
                    BusyIndicator1.IsBusy = false;
                    if (Utility.Utility.CheckInvokeOperation(obj))
                    {
                        gridMessages.ItemsSource = obj.Value;
                    }
                }, null);
        }

        private List<SystemMessageUser> GetMessageUsers()
        {
            return (from user in selectedUser
                    select new SystemMessageUser
                    {
                        Identity = -1,
                        MessageId = -1,
                        UserId = user.UserId,
                    }).ToList();
        }

        private void OnMessageItemClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TimeSpan t = DateTime.Now.TimeOfDay;
            if (gridMessages.Tag != null)
            {
                var oldT = (TimeSpan)gridMessages.Tag;
                if ((t - oldT) < TimeSpan.FromMilliseconds(300))
                {
                    var msg = gridMessages.SelectedItem as SystemMessage;
                    if (msg != null)
                    {
                        new WindowViewMessage { MessageId = msg.MessageId }.Show();
                    }
                }
            }
            gridMessages.Tag = t;
        }
    }
}
