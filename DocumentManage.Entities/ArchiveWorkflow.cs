using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using DocumentManage.Utility;

namespace DocumentManage.Entities
{
    [EntityTableName("T_ARCHIVE_WORKFLOW")]
    public class ArchiveWorkflow : IBaseEntity
    {
        [Display(Name = "Identity", AutoGenerateField = false, Order = 0)]
        public int Identity
        {
            get { return FlowId; }
            set { FlowId = value; }
        }

        [Display(Name = "FlowId", AutoGenerateField = false, Order = 1)]
        [EntityColumn("FLOW_ID", true, DbType.Int32)]
        public int FlowId { get; set; }

        [Display(Name = "FolderId", AutoGenerateField = false, Order = 1)]
        [EntityColumn("FOLDER_ID", DbType.Int32)]
        public int FolderId { get; set; }

        [Display(Name = "FlowType", AutoGenerateField = false, Order = 2)]
        [EntityColumn("FLOW_TYPE", DbType.Int32)]
        public int FlowType { get; set; }

        [Display(Name = "修订流程", AutoGenerateField = true, Order = 2)]
        [EntityColumn("IS_REVISE", DbType.Boolean)]
        public bool IsRevise { get; set; }

        [Display(Name = "流程标题", AutoGenerateField = true, Order = 3)]
        [EntityColumn("FLOW_TITLE", DbType.String)]
        public string FlowTitle { get; set; }

        [Display(Name = "提交用户ID", AutoGenerateField = false, Order = 4)]
        [EntityColumn("SUBMIT_USER", DbType.Int32)]
        public int SubmitUserId { get; set; }

        [Display(Name = "提交时间", AutoGenerateField = true, Order = 5)]
        [EntityColumn("SUBMIT_TIME", DbType.DateTime)]
        public DateTime SubmitTime { get; set; }

        [Display(Name = "描述内容", AutoGenerateField = false, Order = 6)]
        [EntityColumn("SUBMIT_DESC", DbType.String)]
        public string SubmitDescription { get; set; }

        [Display(Name = "CurrentStep", AutoGenerateField = false, Order = 7)]
        [EntityColumn("CURRENT_STEP", DbType.Int32)]
        public int CurrentStep { get; set; }

        [Display(Name = "当前状态", AutoGenerateField = true, Order = 8)]
        [EntityColumn("AUDIT_STATUS", DbType.Int16)]
        public AuditStatus Status { get; set; }

        [Display(Name = "文件列表", AutoGenerateField = false, Order = 9)]
        [EntityColumn(Children = true, ChildKey = "FlowId", GetChildForeigns = false)]
        public List<WorkflowFileInfo> Files { get; set; }

        public object Clone()
        {
            return new ArchiveWorkflow
                {
                    FlowId = FlowId,
                    FlowType = FlowType,
                    FlowTitle = FlowTitle,
                    IsRevise = IsRevise,
                    SubmitUserId = SubmitUserId,
                    SubmitTime = SubmitTime,
                    SubmitDescription = SubmitDescription,
                    CurrentStep = CurrentStep,
                    Status = Status,
                };
        }
    }
}
