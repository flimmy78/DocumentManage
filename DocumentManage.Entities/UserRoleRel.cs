using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using DocumentManage.Utility;

namespace DocumentManage.Entities
{
    //用户角色组定义
    [EntityTableName("T_USER_ROLE_REL")]
    public class UserRoleRel : IBaseEntity
    {
        [Display(Name = "Identity", AutoGenerateField = false)]
        public int Identity
        {
            get { return Id; }
            set { Id = value; }
        }

        [Display(Name = "ID", AutoGenerateField = false, Order = 0)]
        [EntityColumn("ID", true, DbType.Int32)]
        public int Id { get; set; }

        [Display(Name = "用户ID", AutoGenerateField = false, Order = 1)]
        [EntityColumn("USER_ID", DbType.Int32)]
        public int UserId { get; set; }

        [Display(Name="角色ID", AutoGenerateField = false, Order=2)]
        [EntityColumn("ROLE_ID", DbType.Int32)]
        public int RoleId { get; set; }

        [Display(Name="角色", AutoGenerateField = true, Order=3)]
        [EntityColumn(Foreign = true, ForeignKey = "RoleId", GetChildForeigns = false)]
        public SystemRole Role { get; set; }

        [Display(Name="启用日期", AutoGenerateField = true, Order =4)]
        [EntityColumn("START_DATE", DbType.DateTime)]
        public DateTime StartTime { get; set; }

        [Display(Name = "停用日期", AutoGenerateField = true, Order = 5)]
        [EntityColumn("EXPIRE_DATE", DbType.DateTime)]
        public DateTime ExpireTime { get; set; }

        [Display(Name = "添加人ID", AutoGenerateField = false, Order = 6)]
        [EntityColumn("CREATED_BY", DbType.Int32)]
        public int CreatedBy { get; set; }

        [Display(Name = "添加日期", AutoGenerateField = false, Order = 7)]
        [EntityColumn("CREATE_TIME", DbType.DateTime)]
        public DateTime CreateTime { get; set; }

        [Display(Name = "最后更新人ID", AutoGenerateField = false, Order = 8)]
        [EntityColumn("LAST_UPDATED_BY", DbType.Int32)]
        public int LastUpdatedBy { get; set; }

        [Display(Name = "最后更新时间", AutoGenerateField = false, Order = 9)]
        [EntityColumn("LAST_UPDATE_TIME", DbType.DateTime)]
        public DateTime LastUpdateTime { get; set; }

        public object Clone()
        {
            return new UserRoleRel
                {
                    Id = Id,
                    UserId = UserId,
                    RoleId = RoleId,
                    StartTime = StartTime,
                    ExpireTime = ExpireTime,
                    CreatedBy = CreatedBy,
                    CreateTime = CreateTime,
                    LastUpdatedBy = LastUpdatedBy,
                    LastUpdateTime = LastUpdateTime
                };
        }
    }
}
