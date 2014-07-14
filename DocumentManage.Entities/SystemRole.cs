using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using DocumentManage.Utility;

namespace DocumentManage.Entities
{
    //SQL 系统角色定义
    [EntityTableName("T_SYS_ROLES")]
    public class SystemRole : IBaseEntity
    {
        [Display(Name="Identity", AutoGenerateField = false)]
        public int Identity
        {
            get { return RoleId; }
            set { RoleId = value; }
        }

        [Display(Name = "角色ID", AutoGenerateField = false)]
        [EntityColumn("ROLE_ID", true, DbType.Int32)]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "请填写角色编码")]
        [StringLength(32, ErrorMessage = "角色编码长度需在4~32个字符之间", MinimumLength = 4)]
        [RegularExpression(@"^[a-zA-Z0-9_-]*$", ErrorMessage = "角色编码只能由数字、字母和下划线组成")]
        [Display(Name="角色编码", AutoGenerateField = true, Order = 1)]
        [EntityColumn("ROLE_CODE", DbType.String)]
        public string RoleCode { get; set; }

        [Display(Name="角色名", AutoGenerateField = true, Order=2)]
        [EntityColumn("ROLE_NAME", DbType.String)]
        public string RoleName { get; set; }

        [Display(Name="添加人ID", AutoGenerateField = false, Order = 3)]
        [EntityColumn("CREATED_BY", DbType.Int32)]
        public int CreatedBy { get; set; }

        [Display(Name="添加人", AutoGenerateField = true, Order=4)]
        [EntityColumn(Foreign = true, ForeignKey = "CreatedBy", GetChildForeigns = false)]
        public SystemUser CreateUser { get; set; }

        [Display(Name="添加时间", AutoGenerateField = true, Order=5)]
        [DisplayFormat(DataFormatString = "yyyy-MM-dd HH:mm:ss")]
        [EntityColumn("CREATE_TIME", DbType.DateTime)]
        public DateTime CreateTime { get; set; }

        [Display(Name="最后更新人ID", AutoGenerateField = false, Order=6)]
        [EntityColumn("LAST_UPDATED_BY", DbType.Int32)]
        public int LastUpdatedBy { get; set; }

        [Display(Name="最后更新人", AutoGenerateField = true, Order=7)]
        [EntityColumn(Foreign = true, ForeignKey = "LastUpdatedBy", GetChildForeigns = false)]
        public SystemUser LastUpdateUser { get; set; }

        [Display(Name="最新更新时间", AutoGenerateField = true, Order=8)]
        [DisplayFormat(DataFormatString = "yyyy-MM-dd HH:mm:ss")]
        [EntityColumn("LAST_UPDATE_TIME", DbType.DateTime)]
        public DateTime LastUpdateTime { get; set; }

        [Display(Name="状态", AutoGenerateField = true, Order=9)]
        [EntityColumn("ROLE_STATUS", DbType.Int16)]
        public ActiveStatus Status { get; set; }

        public object Clone()
        {
            return new SystemRole
                {
                    RoleId = RoleId,
                    RoleCode = RoleCode,
                    RoleName=  RoleName,
                    CreatedBy = CreatedBy,
                    CreateTime = CreateTime,
                    LastUpdatedBy = LastUpdatedBy,
                    LastUpdateTime = LastUpdateTime,
                    Status = Status,
                    CreateUser = CreateUser.Clone() as SystemUser,
                    LastUpdateUser = LastUpdateUser.Clone() as SystemUser
                };
        }
    }
}
