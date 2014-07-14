using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using DocumentManage.Utility;

namespace DocumentManage.Entities
{
    //SQL 用户组定义
    [EntityTableName("T_USER_ORG_REL")]
    public class UserOrgRel : IBaseEntity
    {
        [Display(Name="Identity", AutoGenerateField = false)]
        public int Identity
        {
            get { return Id; }
            set { Id = value; }
        }

        [Display(Name="ID", AutoGenerateField = false, Order= 0)]
        [EntityColumn("ID", true, DbType.Int32)]
        public int Id { get; set; }

        [Display(Name="用户ID", AutoGenerateField = false, Order=1)]
        [EntityColumn("USER_ID", DbType.Int32)]
        public int UserId { get; set; }

        [Display(Name = "组织ID", AutoGenerateField = false, Order = 2)]
        [EntityColumn("ORG_ID", DbType.Int32)]
        public string OrganizationId { get; set; }

        [Display(Name="组织", AutoGenerateField = true, Order=3)]
        [EntityColumn(Foreign = true, ForeignKey = "OrganizationId", GetChildForeigns = false)]
        public Organization Organization { get; set; }

        [Display(Name="含下级组织", AutoGenerateField = true, Order = 4)]
        [EntityColumn("CHILD_ORG_FLAG", DbType.Boolean)]
        public bool IncludeChildOrg { get; set; }

        [Display(Name="启用日期", AutoGenerateField = true, Order=5)]
        [EntityColumn("START_DATE", DbType.DateTime)]
        public DateTime StartTime { get; set; }

        [Display(Name = "停用日期", AutoGenerateField = true, Order = 6)]
        [EntityColumn("EXPIRE_DATE", DbType.DateTime)]
        public DateTime ExpireTime { get; set; }

        [Display(Name = "添加人ID", AutoGenerateField = false, Order = 7)]
        [EntityColumn("CREATED_BY", DbType.Int32)]
        public int CreatedBy { get; set; }

        [Display(Name = "添加日期", AutoGenerateField = false, Order = 8)]
        [EntityColumn("CREATE_TIME", DbType.DateTime)]
        public DateTime CreateTime { get; set; }

        [Display(Name = "最后更新人ID", AutoGenerateField = false, Order = 9)]
        [EntityColumn("LAST_UPDATED_BY", DbType.Int32)]
        public int LastUpdatedBy { get; set; }

        [Display(Name = "最后更新时间", AutoGenerateField = false, Order = 10)]
        [EntityColumn("LAST_UPDATE_TIME", DbType.DateTime)]
        public DateTime LastUpdateTime { get; set; }

        public object Clone()
        {
            return new UserOrgRel
                {
                    Id = Id,
                    UserId = UserId,
                    OrganizationId = OrganizationId,
                    StartTime = StartTime,
                    ExpireTime = ExpireTime,
                    CreateTime = CreateTime,
                    CreatedBy = CreatedBy,
                    LastUpdateTime = LastUpdateTime,
                    LastUpdatedBy = LastUpdatedBy
                };
        }
    }
}
