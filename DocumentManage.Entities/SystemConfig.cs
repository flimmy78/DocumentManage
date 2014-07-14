using System.ComponentModel.DataAnnotations;
using System.Data;
using DocumentManage.Utility;

namespace DocumentManage.Entities
{   
    //SQL 系统设置
    [EntityTableName("T_SYS_CONFIG")]
    public class SystemConfig : IBaseEntity
    {
        [Display(Name = "Identity", AutoGenerateField = false, Order = 0)]
        public int Identity
        {
            get { return Id; }
            set { Id = value; }
        }

        [Display(Name = "Id", AutoGenerateField = false, Order = 1)]
        [EntityColumn("ID", true, DbType.Int32)]
        public int Id { get; set; }

        [Display(Name = "系统名称", AutoGenerateField = true, Order = 2)]
        [EntityColumn("APP_NAME", DbType.String)]
        public string ApplicationName { get; set; }

        [Display(Name="记录日志", AutoGenerateField = true, Order=3)]
        [EntityColumn("WRITE_LOG", DbType.Int32)]
        public int WriteLog { get; set; }

        [Display(Name="数据审计", AutoGenerateField = true, Order= 4)]
        [EntityColumn("RECORD_AUDIT", DbType.Boolean)]
        public bool RecordAudit { get; set; }

        [Display(Name="文件保存方式", AutoGenerateField = true, Order=5)]
        [EntityColumn("FILE_SAVE_TYPE", DbType.Int16)]
        public FileSaveMethod SaveMethod { get; set; }

        [Display(Name = "文件阅读方式", AutoGenerateField = true, Order = 6)]
        [EntityColumn("FILE_READ_TYPE", DbType.Int16)]
        public FileReadMethod ReadMethod { get; set; }

        [Display(Name = "文件保存地址", AutoGenerateField = true, Order = 7)]
        [EntityColumn("FILE_SAVE_URL", DbType.String)]
        public string FileSaveUrl { get; set; }

        [Display(Name = "文件服务器用户名", AutoGenerateField = true, Order = 8)]
        [EntityColumn("SERVER_USER", DbType.String)]
        public string ServerUserName { get; set; }

        [Display(Name = "文件服务器密码", AutoGenerateField = true, Order = 9)]
        [EntityColumn("SERVER_PWD", DbType.String)]
        public string ServerPassword { get; set; }

        public object Clone()
        {
            return new SystemConfig
                       {
                           Id = Id,
                           ApplicationName = ApplicationName,
                           WriteLog = WriteLog,
                           RecordAudit = RecordAudit,
                           SaveMethod = SaveMethod,
                           ReadMethod = ReadMethod,
                           FileSaveUrl = FileSaveUrl,
                           ServerUserName = ServerUserName,
                           ServerPassword = ServerPassword
                       };
        }
    }
}
