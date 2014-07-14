using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using DocumentManage.Utility;

namespace DocumentManage.Entities
{   
    //SQL 审核记录
    [EntityTableName("T_AW_AUDIT_RECORD")]
    public class FlowAuditRecord:IBaseEntity
    {
        [Display(Name = "Identity", AutoGenerateField = false, Order = 0)]
        public int Identity
        {
            get { return Id; }
            set { Id = value; }
        }

        [Display(Name = "ID", AutoGenerateField = false, Order = 1)]
        [EntityColumn("ID", true, DbType.Int32)]
        public int Id { get; set; }

        [Display(Name = "流程ID", AutoGenerateField = false, Order = 2)]
        [EntityColumn("FLOW_ID", DbType.Int32)]
        public int FlowId { get; set; }

        [Display(Name = "流程", AutoGenerateField = true, Order = 3)]
        [EntityColumn(Foreign = true, ForeignKey = "FlowId", GetChildForeigns = false)]
        public ArchiveWorkflow ArchiveFlow { get; set; }

        [Display(Name = "用户ID", AutoGenerateField = false, Order = 4)]
        [EntityColumn("AUDIT_USER", DbType.Int32)]
        public int AuditUserId { get; set; }

        [Display(Name = "用户", AutoGenerateField = true, Order = 5)]
        [EntityColumn(Foreign = true, ForeignKey = "AuditUserId", GetChildForeigns = false)]
        public SystemUser AuditUser { get; set; }

        [Display(Name="时间", AutoGenerateField = true, Order = 6)]
        [EntityColumn("AUDIT_TIME", DbType.DateTime)]
        public DateTime AuditTime { get; set; }

        [Display(Name = "操作", AutoGenerateField = true, Order = 7)]
        [EntityColumn("AUDIT_OPERATION", DbType.Int16)]
        public AuditOperation Operation { get; set; }

        [Display(Name = "说明", AutoGenerateField = true, Order = 8)]
        [EntityColumn("AUDIT_MSG", DbType.String)]
        public string AuditDescription { get; set; }

        public object Clone()
        {
            return new FlowAuditRecord
                       {
                           Id = Id,
                           FlowId = FlowId,
                           AuditUserId = AuditUserId,
                           AuditUser = AuditUser == null ? null : AuditUser.Clone() as SystemUser,
                           AuditTime = AuditTime,
                           AuditDescription = AuditDescription
                       };
        }
    }
}
