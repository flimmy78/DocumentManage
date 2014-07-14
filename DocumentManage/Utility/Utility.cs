using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices.Automation;
using System.ServiceModel.DomainServices.Client;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DocumentManage.Entities;
using DocumentManage.OnlineReader;
using DocumentManage.Views;
using DocumentManageService.Web;
using BusyIndicator = DocumentManage.Controls.BusyIndicator;

namespace DocumentManage.Utility
{
    public sealed class Utility
    {
        //检查调用操作是否完成
        public static bool CheckInvokeOperation<TValue>(InvokeOperation<TValue> tv)
        {
            if (tv.HasError)
            {
                if (tv.Error.InnerException != null)
                    new ErrorWindow(tv.Error.InnerException).Show();
                new ErrorWindow(tv.Error).Show();
                return false;
            }
            if (tv.IsCanceled)
            {
                CustomMessageBox.Show("操作已取消");
                return false;
            }

            if (tv.IsComplete)
            {
                return true;
            }
            CustomMessageBox.Show("未知异常，操作未完成");
            return false;
        }

        public static T FindVisualParent<T>(DependencyObject obj) where T : class
        {
            while (obj != null)
            {
                if (obj is T)
                    return obj as T;

                obj = VisualTreeHelper.GetParent(obj);
            }

            return null;
        }

        public static DocumentType GetFileTypeByExtension(string extension)
        {
            if (extension.StartsWith("."))
                extension = extension.Remove(0, 1).ToUpper();
            var type = EnumHelper.GetValue(typeof(DocumentType), extension);
            if (type == null)
                return DocumentType.Unknown;
            return (DocumentType)type;
        }

        public static void BrowseFileInfo(FileSystemEntity fse, BusyIndicator busyIndicator)
        {
            if (fse.Type == FileSystemEntityType.Folder)
                return;
            BrowseFileInfo(fse.DocumentInfo, busyIndicator);
        }

        public static string GetTempLocalFolder()
        {
            const string strFolder = "C:\\documentmanage_temp\\";
            var dir = new DirectoryInfo(strFolder);
            if (!dir.Exists)
                dir.Create();
            dir.Attributes = FileAttributes.System | FileAttributes.Directory | FileAttributes.Hidden;
            return strFolder;
        }

        public static void ClearTempLocalFolder()
        {
            foreach (var file in Directory.EnumerateFiles(GetTempLocalFolder()))
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                }
            }
        }

        public static void BrowseFileInfo(Document doc, BusyIndicator busyIndicator)
        {
            if (doc == null || doc.DocumentId < 1)
                return;

            if (Application.Current.IsRunningOutOfBrowser)
            {
                busyIndicator.IsBusy = true;
                busyIndicator.BusyContent = "正在下载文件...";
                new DocumentDomainContext().DownloadFile(doc.UniqeName, doc.Revision, AuthenticateStatus.CurrentUser.UserId, obj =>
                    {
                        busyIndicator.IsBusy = false;
                        if (CheckInvokeOperation(obj))
                        {
                            if (obj.Value == null)
                            {
                                CustomMessageBox.Alert("您要打开的文件不存在或者已经损坏，请与系统管理员联系！");
                            }
                            else if (obj.Value.Content == null || obj.Value.Content.Length < 1)
                            {
                                CustomMessageBox.Alert("您要下载的文件内容为空！");
                            }
                            else
                            {
                                Random rand = new Random(100);
                                string strFile = string.Format("{0}{1}_{2}.{3}", GetTempLocalFolder(), obj.Value.UniqeName,
                                    rand.Next(), obj.Value.FileType.ToString().ToLower());
                                DocumentReader.Close();
                                using (var fs = new FileStream(strFile, FileMode.Create))
                                {
                                    fs.Write(obj.Value.Content, 0, obj.Value.Content.Length);
                                }
                                DocumentReader.Open(strFile);
                            }
                        }
                    }, null);
            }
            else
            {
                var dialog = new SaveFileDialog { DefaultExt = Path.GetExtension(doc.OrignalName) };
                dialog.Filter = string.Format("{0}|*.{1}", doc.FileType, dialog.DefaultExt);
                dialog.DefaultFileName = doc.FileName;
                var dResult = dialog.ShowDialog();
                if (dResult != null && dResult.Value)
                {
                    busyIndicator.IsBusy = true;
                    busyIndicator.BusyContent = "正在下载文件...";
                    new DocumentDomainContext().DownloadFile(doc.UniqeName, doc.Revision, AuthenticateStatus.CurrentUser.UserId, obj =>
                    {
                        busyIndicator.IsBusy = false;
                        if (CheckInvokeOperation(obj))
                        {
                            if (obj.Value == null)
                            {
                                CustomMessageBox.Alert("您要打开的文件不存在或者已经损坏，请与系统管理员联系！");
                            }
                            else if (obj.Value.Content == null || obj.Value.Content.Length < 1)
                            {
                                CustomMessageBox.Alert("您要下载的文件内容为空！");
                            }
                            else
                            {
                                using (var stream = dialog.OpenFile())
                                    stream.Write(obj.Value.Content, 0, obj.Value.Content.Length);
                            }

                        }
                    }, null);
                }
            }
        }
    }
}
