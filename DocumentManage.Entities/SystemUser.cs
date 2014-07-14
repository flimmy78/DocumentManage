using System.ComponentModel.DataAnnotations;
using System.Data;
using DocumentManage.Utility;

namespace DocumentManage.Entities
{
    //SQL 系统用户定义
    [EntityTableName("T_SYS_USERS")]
    public partial class SystemUser : IBaseEntity
    {
        [Display(AutoGenerateField = false)]
        public int Identity     
        {
            get { return UserId; } 
            set { UserId = value; }
        }

        [Display(AutoGenerateField = false)]
        [EntityColumn("USER_ID", true, DbType.Int32)]
        public int UserId { get; set; }   //设置/读取用户ID

        [Display(AutoGenerateField = true, Name = "工号", Order = 1)]
        [Required(ErrorMessage = "请输入员工工号")]
        [EntityColumn("USER_CODE", DbType.AnsiString)]
        public string UserCode { get; set; }

        [Display(AutoGenerateField = true, Name = "登陆名", Order = 2)]
        [Required(ErrorMessage = "请输入登陆名")]
        [RegularExpression(@"^[a-zA-Z0-9_-]*$", ErrorMessage = "登陆名只能由数字、字母下划线组成")]
        [StringLength(32, ErrorMessage = "登陆名长度在4到32个字符之间", MinimumLength = 4)]
        [EntityColumn("USER_NAME", DbType.AnsiString, Unique = true)]
        public string UserName { get; set; }

        [Display(AutoGenerateField = false, Name = "密码", Order = 3)]
        [EntityColumn("USER_PWD", DbType.AnsiString)]
        public string UserPassword { get; set; }

        [Display(AutoGenerateField = true, Name = "性别", Order = 4)]
        [EntityColumn("GENDER", DbType.Int16)]
        public Gender Gender { get; set; }

        [Display(AutoGenerateField = true, Name = "真实姓名", Order = 5)]
        [EntityColumn("REAL_NAME", DbType.String)]
        public string RealName { get; set; }

        [Display(AutoGenerateField = true, Name = "电话", Order = 6)]
        [EntityColumn("TELPHONE", DbType.String)]
        public string Telphone { get; set; }

        [Display(AutoGenerateField = true, Name = "手机", Order = 7)]
        [EntityColumn("MOBILE", DbType.String)]
        public string Mobile { get; set; }

        [Display(AutoGenerateField = false, Name = "传真", Order = 8)]
        [EntityColumn("FAX", DbType.String)]
        public string Fax { get; set; }

        [Display(AutoGenerateField = false, Name = "QQ", Order = 9)]
        [EntityColumn("QQ", DbType.String)]
        public string QQ { get; set; }

        [Display(AutoGenerateField = true, Name = "邮箱", Order = 10)]
        [EntityColumn("EMAIL", DbType.String)]
        public string Email { get; set; }

        [Display(AutoGenerateField = false, Name = "地址", Order = 11)]
        [EntityColumn("ADDRESS", DbType.String)]
        public string Address { get; set; }

        [Display(AutoGenerateField = false, Name = "个人描述", Order = 12)]
        [EntityColumn("DESCRIPTION", DbType.String)]
        public string Description { get; set; }

        [Display(AutoGenerateField = true, Name = "状态", Order = 13)]
        [EntityColumn("USER_STATUS", DbType.String)]
        public ActiveStatus Status { get; set; }

        public override string ToString()
        {
            return RealName;
        }
    }
}
