using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using DocumentManage.Utility;

namespace DocumentManage.Entities
{
    //SQL 系统组织表
    [EntityTableName("T_SYS_ORG")]
    public class Organization : IBaseEntity
    {
        [Display(Name = "Identity", AutoGenerateField = false, Order = 0)]
        public int Identity { get; set; }

        [Display(Name = "组织ID", AutoGenerateField = true, Order = 1)]
        [EntityColumn("ORG_ID", DbType.String, Unique = true)]
        public string Id { get; set; }

        [Display(Name = "上级组织ID", AutoGenerateField = true, Order = 2)]
        [EntityColumn("PARENT_ID", DbType.String)]
        public string ParentId { get; set; }

        [Display(Name = "类型", AutoGenerateField = true, Order = 3)]
        [EntityColumn("ORG_TYPE", DbType.Int16)]
        public OrganizationType Type { get; set; }

        [Display(Name = "编码", AutoGenerateField = true, Order = 4)]
        [Required(ErrorMessage="请输入编码！")]
        [EntityColumn("ORG_CODE", DbType.String)]
        public string Code { get; set; }

        [Display(Name = "名称", AutoGenerateField = true, Order = 5)]
        [EntityColumn("ORG_NAME", DbType.String)]
        public string Name { get; set; }

        [Display(Name = "电话", AutoGenerateField = true, Order = 6)]
        [EntityColumn("ORG_TELPHONE", DbType.String)]
        public string Telphone { get; set; }

        [Display(Name = "传真", AutoGenerateField = true, Order = 7)]
        [EntityColumn("ORG_FAX", DbType.String)]
        public string Fax { get; set; }

        [Display(Name = "邮箱", AutoGenerateField = true, Order = 8)]
        [EntityColumn("ORG_MAIL", DbType.String)]
        public string Email { get; set; }

        [Display(Name = "地址", AutoGenerateField = true, Order = 9)]
        [EntityColumn("ORG_ADDRESS", DbType.String)]
        public string Address { get; set; }

        [Display(Name = "简介", AutoGenerateField = true, Order = 10)]
        [EntityColumn("ORG_DESCRIPTION", DbType.String)]
        public string Description { get; set; }

        [Display(Name = "状态", AutoGenerateField = true, Order = 11)]
        [EntityColumn("ORG_STATUS", DbType.Int16)]
        public ActiveStatus Status { get; set; }

        [Display(Name = "下级组织", AutoGenerateField = false, Order = 12)]
        [EntityColumn(Children = true, ChildKey = "Id", GetChildForeigns = false)]
        public ObservableCollection<Organization> Children { get; set; }

        public object Clone()
        {
            return new Organization
                {
                    Id = Id,
                    ParentId = ParentId,
                    Type = Type,
                    Code = Code,
                    Name = Name,
                    Telphone = Telphone,
                    Fax = Fax,
                    Email = Email,
                    Address = Address,
                    Description = Description,
                    Status = Status
                };
        }
    }
}
