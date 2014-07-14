using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using DocumentManage.Utility;

namespace DocumentManage.Entities
{
    //SQL 工作流程步骤
    [EntityTableName("T_WF_AUDIT_STEPS")]
    public class WorkflowAuditStep : IBaseEntity
    {
        [Display(Name = "Identity", AutoGenerateField = false, Order = 0)]
        public int Identity
        {
            get { return StepId; }
            set { StepId = value; }
        }

        [Display(Name = "StepId", AutoGenerateField = false, Order = 1)]
        [EntityColumn("STEP_ID", true, DbType.Int32)]
        public int StepId { get; set; }

        [Display(Name = "工作流ID", AutoGenerateField = false, Order = 3)]
        [EntityColumn("FLOW_TYPE", DbType.Int32)]
        public int FlowType { get; set; }

        [Display(Name = "序号", AutoGenerateField = true, Order = 2)]
        [EntityColumn("AUDIT_INDEX", DbType.Int32)]
        public int AuditIndex { get; set; }

        [Display(Name = "审核类型", AutoGenerateField = true, Order = 4)]
        [EntityColumn("AUDIT_TYPE", DbType.Int16)]
        public AuditType AuditType { get; set; }

        [Display(Name = "步骤名称", AutoGenerateField = true, Order = 5)]
        [EntityColumn("STEP_TEXT", DbType.String)]
        public string StepText { get; set; }

        [Display(Name = "审核部门ID", AutoGenerateField = false, Order = 6)]
        [EntityColumn("AUDIT_ORG", DbType.String)]
        public string AuditOrgId { get; set; }

        [Display(Name="审核部门", AutoGenerateField = true, Order = 6)]
        [EntityColumn(Foreign = true, ForeignKey = "AuditOrgId")]
        public Organization AuditOrganization { get; set; }

        [Display(Name = "审核用户ID", AutoGenerateField = false, Order = 7)]
        [EntityColumn("AUDIT_USER", DbType.Int32)]
        public int AuditUserId { get; set; }

        [Display(Name = "审核用户", AutoGenerateField = true, Order = 7)]
        [EntityColumn(Foreign = true, ForeignKey = "AuditUserId")]
        public SystemUser AuditUser { get; set; }

        [Display(Name = "审核组织", AutoGenerateField = true, Order = 8)]
        [EntityColumn("AUDIT_ORG_TYPE", DbType.Int16)]
        public OrganizationType AuditOrgType { get; set; }

        [Display(Name = "审核角色ID", AutoGenerateField = false, Order = 9)]
        [EntityColumn("AUDIT_ROLE", DbType.Int32)]
        public int AuditRoleId { get; set; }

        [Display(Name = "审核角色", AutoGenerateField = true, Order = 9)]
        [EntityColumn(Foreign = true, ForeignKey = "AuditRoleId")]
        public SystemRole AuditRole { get; set; }

        [Display(Name = "步骤状态", AutoGenerateField = true, Order = 10)]
        [EntityColumn("AUDIT_STATUS", DbType.Int16)]
        public ActiveStatus Status { get; set; }

        [Display(Name = "会审用户列表", AutoGenerateField = false, Order = 11)]
        [EntityColumn(Children = true, ChildKey = "StepId", GetChildForeigns = false)]
        public List<JointCheckupUser> JcUsers { get; set; }

        public object Clone()
        {
            return new WorkflowAuditStep
                {
                    StepId = StepId,
                    FlowType = FlowType,
                    AuditType = AuditType,
                    StepText = StepText,
                    AuditOrgId = AuditOrgId,
                    AuditUser = AuditUser,
                    AuditOrgType = AuditOrgType,
                    AuditRoleId = AuditRoleId,
                    AuditIndex = AuditIndex,
                    Status = Status
                };
        }
    }
}
