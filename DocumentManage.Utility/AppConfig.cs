using System;
using System.Configuration;
using System.IO;

namespace DocumentManage.Utility
{
    //DocumentManage.Utility 空间 -- 应用配置类，全是静态函数，通过类名直接调用
    public class AppConfig
    {
        private const string CONN_KEY = "DocumentManage";
        private const string DATABASE_FILE = "DatabaseFile";
        private const string PATCH_FOLDER = "PatchFolder";
        private const string DATABASE_PASSWORD = "DbPassowrd";

        //获取数据库文件名
        public static string GetDatabaseFileName()      
        {
            string str = ConfigurationManager.AppSettings[DATABASE_FILE];
            if (string.IsNullOrEmpty(str))
            {
                str = AppDomain.CurrentDomain.BaseDirectory;
            }
            return str;
        }
        //获取补丁文件夹
        public static string GetPatchFolder()
        {
            string str = ConfigurationManager.AppSettings[PATCH_FOLDER];
            if (string.IsNullOrEmpty(str))
            {
                str = string.Format("{0}Patches\\", AppDomain.CurrentDomain.BaseDirectory);
            }
            return str;
        }
        //保存
        public static bool Save(string fileName, string folder, string password)
        {
            if (!File.Exists(fileName))
            {
                CustomMessageBox.Show("数据库文件不存在！");
                return false;
            }

            if (!Directory.Exists(folder))
            {
                try
                {
                    Directory.CreateDirectory(folder);
                }
                catch
                {
                    CustomMessageBox.Show("创建版本库目录失败！");
                    return false;
                }
            }

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove(DATABASE_FILE);
            config.AppSettings.Settings.Add(DATABASE_FILE, fileName);
            config.AppSettings.Settings.Remove(PATCH_FOLDER);
            config.AppSettings.Settings.Add(PATCH_FOLDER, folder);
            config.AppSettings.Settings.Remove(DATABASE_PASSWORD);
            config.AppSettings.Settings.Add(DATABASE_PASSWORD, password);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            return true;
        }
        //保存
        public static bool Save(string fileName, string folder)
        {
            return Save(fileName, folder, string.Empty);
        }
        //设置数据库文件名
        public static void SetDatabaseFileName(string name)
        {
            if (!File.Exists(name))
            {
                CustomMessageBox.Show("数据库文件不存在！");
                return;
            }

            if (name.Equals(GetDatabaseFileName()))
                return;

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove(DATABASE_FILE);
            config.AppSettings.Settings.Add(DATABASE_FILE, name);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
        //设置补丁文件夹
        public static void SetPatchFolder(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            if (folder.Equals(GetPatchFolder()))
                return;

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove(PATCH_FOLDER);
            config.AppSettings.Settings.Add(PATCH_FOLDER, folder);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
        //获取数据库连接字符串
        public static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings[CONN_KEY].ConnectionString;
        }
        //获取数据库密码
        public static string GetDatabasePassword()
        {
            string str = ConfigurationManager.AppSettings[DATABASE_PASSWORD];
            return str;
        }
    }
}
