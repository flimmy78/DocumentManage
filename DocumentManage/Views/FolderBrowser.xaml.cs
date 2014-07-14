using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Automation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DocumentManage.Views
{
    public partial class FolderBrowser : ChildWindow
    {
        private static string lastFolder = "C:\\";
        private EventHandler onOkEventHandler;

        public FolderBrowser()
        {
            InitializeComponent();
            Loaded += OnWindowLoaded;
            if (Application.Current.HasElevatedPermissions)
            {
                dynamic fileSystem = AutomationFactory.CreateObject("Scripting.FileSystemObject");
                dynamic drives = fileSystem.Drives;
                string strName = "未知设备";
                foreach (var drive in drives)
                {
                    try
                    {
                        int nDriveType = drive.DriveType;
                        switch (nDriveType)
                        {
                            case 0:
                                strName = string.Format("未知磁盘{0}:", drive.DriveLetter);
                                break;
                            case 1:
                                strName = string.Format("{0} ({1}:)", string.IsNullOrEmpty(drive.VolumeName) ? "可移动硬盘" : drive.VolumeName, drive.DriveLetter);
                                break;
                            case 2:
                                strName = string.Format("{0} ({1}:)", string.IsNullOrEmpty(drive.VolumeName) ? "本地磁盘" : drive.VolumeName, drive.DriveLetter);
                                break;
                            case 3:
                                strName = string.Format("{0} ({1}:)", string.IsNullOrEmpty(drive.VolumeName) ? "网络硬盘" : drive.VolumeName, drive.DriveLetter);
                                break;
                            case 4:
                                strName = string.Format("{0} ({1}:)", string.IsNullOrEmpty(drive.VolumeName) ? "CD-ROM" : drive.VolumeName, drive.DriveLetter);
                                break;
                            case 5:
                                strName = string.Format("{0} ({1}:)", string.IsNullOrEmpty(drive.VolumeName) ? "RAM Disk" : drive.VolumeName, drive.DriveLetter);
                                break;
                        }
                        cmbDrives.Items.Add(strName);
                    }
                    catch (COMException) { }
                }
            }
        }

        public string SelectedFolder
        {
            get
            {
                if (lstFolders.SelectedIndex > 0)
                {
                    return Path.Combine(lstFolders.Items[0].ToString(), lstFolders.SelectedValue.ToString());
                }
                if (lstFolders.SelectedIndex == 0)
                {
                    return lstFolders.SelectedValue.ToString();
                }
                return string.Empty;
            }
        }

        public event EventHandler OnOk
        {
            add { onOkEventHandler += value; }
            remove
            {
                if (onOkEventHandler != null)
                    onOkEventHandler -= value;
            }
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            string curDrive = string.Format("({0})", lastFolder.Substring(0, 2));
            foreach (var strDrive in cmbDrives.Items)
            {
                if (strDrive.ToString().EndsWith(curDrive))
                {
                    cmbDrives.SelectedItem = strDrive;
                    ShowChildFolders();
                    break;
                }
            }
            cmbDrives.SelectionChanged += OnDriveSelectionChanged;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.HasElevatedPermissions && onOkEventHandler != null)
            {
                onOkEventHandler(this, null);
            }
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void OnDriveSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string strDrive = cmbDrives.SelectedValue.ToString();
            if (!string.IsNullOrEmpty(strDrive))
            {
                lastFolder = strDrive.Substring(strDrive.IndexOf('(') + 1, 2) + '\\';
                ShowChildFolders();
            }
        }

        private void OnBackToParentButtonClick(object sender, MouseButtonEventArgs e)
        {
            if (!Application.Current.HasElevatedPermissions)
                return;
            var dir = new DirectoryInfo(lastFolder);
            if (dir.Parent != null)
            {
                lstFolders.ItemsSource = null;
                lastFolder = dir.Parent.FullName;
                ShowChildFolders();
            }
        }

        private void OnFolderClicked(object sender, MouseButtonEventArgs e)
        {
            TimeSpan t = DateTime.Now.TimeOfDay;
            if (lstFolders.Tag != null)
            {
                var oldT = (TimeSpan)lstFolders.Tag;
                if ((t - oldT) < TimeSpan.FromMilliseconds(300))
                {
                    string strFolder = lstFolders.SelectedValue.ToString();
                    if (!string.IsNullOrEmpty(strFolder))
                    {
                        lastFolder = Path.Combine(lastFolder, strFolder);
                        ShowChildFolders();
                    }
                }
            }
            lstFolders.Tag = t;
        }

        private void ShowChildFolders()
        {
            var dir = new DirectoryInfo(lastFolder);
            var folders = (from folder in dir.EnumerateDirectories()
                           where !folder.Attributes.HasFlag(FileAttributes.Hidden)
                           orderby folder.Name
                           select folder.Name)
                              .ToList();
            folders.Insert(0, dir.FullName);
            lstFolders.ItemsSource = null;
            lstFolders.ItemsSource = folders;
        }
    }
}

