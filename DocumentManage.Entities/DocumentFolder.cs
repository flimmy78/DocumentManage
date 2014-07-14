using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using DocumentManage.Utility;

namespace DocumentManage.Entities
{
    //SQL 文件夾描述表
    [EntityTableName("T_DOC_FOLDER")]
    public class DocumentFolder : IBaseEntity
    {
        [Display(Name = "Identity", AutoGenerateField = false, Order = 0)]
        public int Identity
        {
            get { return FolderId; }
            set { FolderId = value; }
        }

        [Display(Name = "FolderId", AutoGenerateField = false, Order = 1)]
        [EntityColumn("FOLDER_ID", true, DbType.Int32)]
        public int FolderId { get; set; }

        [Display(Name = "ParentId", AutoGenerateField = false, Order = 2)]
        [EntityColumn("PARENT_ID", DbType.Int32)]
        public int ParentId { get; set; }

        [EntityColumn(Foreign = true, ForeignKey = "ParentId", GetChildForeigns = false)]
        public DocumentFolder ParentFolder { get; set; }

        [Display(Name = "组织ID", AutoGenerateField = false, Order = 2)]
        [EntityColumn("ORG_ID", DbType.String)]
        public string OrganizationId { get; set; }

        [Display(Name = "文件夹名", AutoGenerateField = true, Order = 3)]
        [EntityColumn("FOLDER_NAME", DbType.String)]
        public string Name { get; set; }

        [Display(Name = "完整路径", AutoGenerateField = true, Order = 4)]
        [EntityColumn("FULL_PATH", DbType.String)]
        public string FullName { get; set; }

        [Display(Name = "创建人ID", AutoGenerateField = false, Order = 5)]
        [EntityColumn("CREATED_BY", DbType.Int32)]
        public int CreatedBy { get; set; }

        [Display(Name = "创建时间", AutoGenerateField = true, Order = 6)]
        [EntityColumn("CREATE_TIME", DbType.DateTime)]
        public DateTime CreateTime { get; set; }

        [Display(Name = "状态", AutoGenerateField = true, Order = 7)]
        [EntityColumn("FOLDER_STATUS", DbType.Int16)]
        public ActiveStatus Status { get; set; }

        [Display(Name = "子文件夹", AutoGenerateField = false, Order = 8)]
        [EntityColumn(Children = true, ChildKey = "ParentId", GetChildForeigns = false)]
        public List<DocumentFolder> Children { get; set; }

        public object Clone()
        {
            return new DocumentFolder
                {
                    FolderId = FolderId,
                    ParentId = ParentId,
                    OrganizationId = OrganizationId,
                    Name = Name,
                    FullName = FullName,
                    CreatedBy = CreatedBy,
                    CreateTime = CreateTime,
                    Status = Status
                };
        }
    }
}
