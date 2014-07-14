using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;
using DocumentManage.Entities;
using DocumentManage.Utility;
using DocumentManageService.Web;

namespace DocumentManage.Views.Sys
{
    public partial class Config : Page
    {
        private readonly EnumToStringValueConverter enumConverter = new EnumToStringValueConverter();
        private readonly SystemConfigDomainContext configContext = new SystemConfigDomainContext();
        public Config()
        {
            Resources.Add("FileSaveMethodNames", EnumHelper.GetNames(typeof(FileSaveMethod)));
            Resources.Add("FileReadMethodNames", EnumHelper.GetNames(typeof(FileReadMethod)));
            InitializeComponent();
            InitLogTypeList();
        }

        private void InitLogTypeList()
        {
            foreach (var name in EnumHelper.GetNames(typeof(LogType)))
            {
                LogTypeStackPanel.Children.Add(new CheckBox { Content = name, Width = 70, Foreground = new SolidColorBrush(Colors.Black) });
            }
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            configContext.GetSystemConfig((obj) =>
                                              {
                                                  if (Utility.Utility.CheckInvokeOperation(obj))
                                                  {
                                                      DataContext = obj.Value;
                                                      SetLogTypeCheckBox(obj.Value.WriteLog);
                                                  }
                                              }, null);
        }

        private void SetLogTypeCheckBox(int logs)
        {
            if (logs == 0)
                return;

            foreach (CheckBox ckb in LogTypeStackPanel.Children)
            {
                var logType = (int)((LogType)EnumHelper.GetValue(typeof(LogType), ckb.Content.ToString())); 
                ckb.IsChecked = ((logType & logs) == logType);
            }
        }

        private int GetSelectedLogType()
        {
            int logs = 0;
            foreach (CheckBox ckb in LogTypeStackPanel.Children)
            {
                if (ckb.IsChecked.HasValue && ckb.IsChecked.Value)
                {
                    var logType = (int)((LogType)EnumHelper.GetValue(typeof(LogType), ckb.Content.ToString()));
                    logs |= logType;
                }
            }
            return logs;
        }

        private void OnSaveConfigButtonClick(object sender, RoutedEventArgs e)
        {
            var config = DataContext as SystemConfig;
            if (config == null)
                return;
            if (string.IsNullOrEmpty(config.ApplicationName))
            {
                CustomMessageBox.Show("请输入系统名称");
            }
            else
            {
                config.WriteLog = GetSelectedLogType();
                configContext.SaveSystemConfig(config, (obj) =>
                    {
                        if (Utility.Utility.CheckInvokeOperation(obj))
                        {
                            CustomMessageBox.Show(obj.Value > 0 ? "保存成功！" : "保存系统设置时出现未知错误，请重试！");
                        }
                    }, null);
            }
        }

    }
}
