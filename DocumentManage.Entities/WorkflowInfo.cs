using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using DocumentManage.Utility;

namespace DocumentManage.Entities
{
    //SQL 工作流信息
    [EntityTableName("T_WF_DEFINE")]
    public class WorkflowInfo : IBaseEntity
    {
        [Display(Name = "Identity", AutoGenerateField = false, Order = 0)]
        public int Identity
        {
            get { return FlowType; }
            set { FlowType = value; }
        }

        [Display(Name = "ID", AutoGenerateField = false, Order = 1)]
        [EntityColumn("FLOW_TYPE", true, DbType.Int32)]
        public int FlowType { get; set; }

        [Display(Name = "名称", AutoGenerateField = true, Order = 2)]
        [EntityColumn("FLOW_NAME", DbType.String)]
        public string FlowName { get; set; }

        [Display(Name = "添加人ID", AutoGenerateField = false, Order = 3)]
        [EntityColumn("CREATED_BY", DbType.Int32)]
        public int CreatedBy { get; set; }

        [Display(Name = "添加日期", AutoGenerateField = false, Order = 4)]
        [EntityColumn("CREATE_TIME", DbType.DateTime)]
        public DateTime CreateTime { get; set; }

        [Display(Name = "最后更新人ID", AutoGenerateField = false, Order = 5)]
        [EntityColumn("LAST_UPDATED_BY", DbType.Int32)]
        public int LastUpdatedBy { get; set; }

        [Display(Name = "最后更新时间", AutoGenerateField = false, Order = 6)]
        [EntityColumn("LAST_UPDATE_TIME", DbType.DateTime)]
        public DateTime LastUpdateTime { get; set; }

        [Display(Name = "审批步骤", AutoGenerateField = true, Order = 7)]
        [EntityColumn(Children = true, ChildKey = "FlowType", GetChildForeigns = true)]
        public List<WorkflowAuditStep> AuditSteps { get; set; } 

        [Display(Name = "状态", AutoGenerateField = true, Order = 8)]
        [EntityColumn("FLOW_STATUS", DbType.Int16)]
        public ActiveStatus Status { get; set; }

        public object Clone()
        {
            return new WorkflowInfo
                {
                    FlowType = FlowType,
                    FlowName = FlowName,
                    CreateTime = CreateTime,
                    CreatedBy = CreatedBy,
                    LastUpdateTime = LastUpdateTime,
                    LastUpdatedBy = LastUpdatedBy,
                    Status = Status
                };
        }
    }
}
