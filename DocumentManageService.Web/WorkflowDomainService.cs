using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using DocumentManage.Entities;
using DocumentManage.Utility;

namespace DocumentManageService.Web
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel.DomainServices.Hosting;
    using System.ServiceModel.DomainServices.Server;


    // TODO: 创建包含应用程序逻辑的方法。
    [EnableClientAccess()]
    public class WorkflowDomainService : DomainService
    {
        //获取所有工作流
        public List<WorkflowInfo> GetAllWorkflows()
        {
            var list = SqlHelper.GetAllEntities<WorkflowInfo>();
            foreach (var flow in list)
            {
                IList steps = new List<WorkflowAuditStep>();
                SqlHelper.SearchByField("FlowType", flow.FlowType, ref steps, true, true);
                flow.AuditSteps = steps as List<WorkflowAuditStep>;
                if (flow.AuditSteps != null)
                {
                    foreach (var step in flow.AuditSteps)
                    {
                        if (step.AuditType == AuditType.JointCheckup && step.JcUsers != null && step.JcUsers.Count > 0)
                        {
                            foreach (var jcu in step.JcUsers)
                            {
                                IBaseEntity entity = jcu.UserInfo ?? new SystemUser();
                                SqlHelper.GetSingleEntity(jcu.UserId, ref entity);
                                jcu.UserInfo = entity as SystemUser;
                            }
                        }
                    }
                }
            }
            return list;
        }
        //获取工作流信息
        public WorkflowInfo GetWorkflowInfo(int flowType)
        {
            IBaseEntity entity = new WorkflowInfo();
            SqlHelper.GetSingleEntity(flowType, ref entity, false, true);
            return entity as WorkflowInfo;
        }
        // 
        public List<JointCheckupUser> GetStepJointCheckupUsers(int stepId)
        {
            IList list = new List<JointCheckupUser>();
            SqlHelper.SearchByField("StepId", stepId, ref list);
            return list as List<JointCheckupUser>;
        }
        //创建工作流
        public int CreateWorkflow(WorkflowInfo flow)
        {
            flow.CreateTime = DateTime.Now;
            flow.LastUpdateTime = DateTime.Now;

            if (flow.AuditSteps != null && flow.AuditSteps.Count > 0)
            {
                if (SqlHelper.Insert(flow) > 0)
                {
                    foreach (var step in flow.AuditSteps)
                    {
                        step.FlowType = flow.FlowType;
                        if (step.AuditUserId < 1 && step.AuditUser != null)
                            step.AuditUserId = step.AuditUser.UserId;
                        if (step.AuditRoleId < 1 && step.AuditRole != null)
                            step.AuditRoleId = step.AuditRole.RoleId;
                        if (string.IsNullOrEmpty(step.AuditOrgId) && step.AuditOrganization != null)
                            step.AuditOrgId = step.AuditOrganization.Id;
                        if (step.AuditType == AuditType.JointCheckup)
                        {
                            if (step.JcUsers != null && step.JcUsers.Count > 0)
                            {
                                SqlHelper.Insert(step);
                                foreach (var jcu in step.JcUsers)
                                {
                                    jcu.StepId = step.StepId;
                                    if (jcu.UserId < 1 && jcu.UserInfo != null)
                                        jcu.UserId = jcu.UserInfo.UserId;
                                    SqlHelper.Insert(jcu);
                                }
                            }
                        }
                        else
                        {
                            SqlHelper.Insert(step);
                        }
                    }
                    return 1;
                }
                return -1;
            }
            return -2;
        }
        //更新工作流
        public int UpdateWorkflow(WorkflowInfo flow)
        {
            if (flow.CreateTime < DateTime.Now.AddYears(-10))
                flow.CreateTime = DateTime.Now;
            flow.LastUpdateTime = DateTime.Now;
            if (flow.AuditSteps != null && flow.AuditSteps.Count > 0)
            {
                if (SqlHelper.Update(flow) > 0)
                {
                    DeleteWorkfowSteps(flow.FlowType);

                    foreach (var step in flow.AuditSteps)
                    {
                        step.FlowType = flow.FlowType;
                        if (step.AuditUserId < 1 && step.AuditUser != null)
                            step.AuditUserId = step.AuditUser.UserId;
                        if (step.AuditRoleId < 1 && step.AuditRole != null)
                            step.AuditRoleId = step.AuditRole.RoleId;
                        if (string.IsNullOrEmpty(step.AuditOrgId) && step.AuditOrganization != null)
                            step.AuditOrgId = step.AuditOrganization.Id;
                        if (step.AuditType == AuditType.JointCheckup)
                        {
                            if (step.JcUsers != null && step.JcUsers.Count > 0)
                            {
                                SqlHelper.Insert(step);
                                foreach (var jcu in step.JcUsers)
                                {
                                    jcu.StepId = step.StepId;
                                    if (jcu.UserId < 1 && jcu.UserInfo != null)
                                        jcu.UserId = jcu.UserInfo.UserId;
                                    SqlHelper.Insert(jcu);
                                }
                            }
                        }
                        else
                        {
                            SqlHelper.Insert(step);
                        }
                    }

                    return 1;
                }
                return -1;
            }
            return -2;
        }
        //保存存档工作流
        public ArchiveWorkflow SaveWorkflow(ArchiveWorkflow flow)
        {
            if (flow != null && !string.IsNullOrEmpty(flow.FlowTitle) && flow.Files != null && flow.Files.Count > 0)
            {
                flow.SubmitTime = DateTime.Now;
                FlowAuditRecord record = null;
                if (flow.Status == AuditStatus.Submitted)
                {
                    record = new FlowAuditRecord
                                         {
                                             Id = 0,
                                             FlowId = flow.FlowId,
                                             AuditUserId = flow.SubmitUserId,
                                             AuditTime = DateTime.Now,
                                             AuditDescription = flow.SubmitDescription
                                         };
                    var wf = GetWorkflowInfo(flow.FlowType);
                    if (wf != null && wf.AuditSteps != null && wf.AuditSteps.Count > 0)
                    {
                        flow.Status = AuditStatus.Auditing;
                        flow.CurrentStep = wf.AuditSteps.OrderBy(o => o.AuditIndex).First().StepId;
                    }
                }
                int n = flow.FlowId > 0 ? SqlHelper.Update(flow) : SqlHelper.Insert(flow);
                if (n > 0)
                {
                    if (record != null)
                    {
                        record.FlowId = flow.FlowId;
                        SqlHelper.Insert(record);
                    }

                    var docService = new DocumentDomainService();
                    var rootFolder = docService.GetFolder(flow.FolderId);
                    foreach (var file in flow.Files)
                    {
                        file.FlowId = flow.FlowId;
                        file.DocumentId = file.DocumentInfo.Identity;
                        if (file.DocumentInfo.Status == DocumentStatus.Deleted)
                        {
                            int tmpFolderId = file.DocumentInfo.FolderId;
                            file.DocumentInfo.FolderId = (rootFolder != null && rootFolder.Identity > 0) ? flow.FolderId : 0;
                            docService.CheckAndCreateFolder(file.DocumentInfo);
                            if (tmpFolderId != file.DocumentInfo.FolderId)
                                SqlHelper.Update(file.DocumentInfo);
                        }
                        n = file.Id > 0 ? SqlHelper.Update(file) : SqlHelper.Insert(file);
                    }
                    docService.Dispose();
                    return flow;
                }
            }
            return null;
        }
        //删除工作流
        public int DeleteArchiveWorkFlow(int flowId)
        {
            var flow = GetArchiveWorkflow(flowId);
            if (flow == null || flow.FlowId != flowId)
                return -1;
            int nRtn = 0;
            nRtn += SqlHelper.DeleteByField<WorkflowFileInfo>("FlowId", flowId);
            foreach (var file in flow.Files)
            {
                nRtn += SqlHelper.DeleteByField<DocumentDesc>("DocumentId", file.Identity);
                nRtn += SqlHelper.Delete(file);
                if (file.DocumentInfo != null && !string.IsNullOrEmpty(file.DocumentInfo.FilePath))
                {
                    try
                    {
                        File.Delete(file.DocumentInfo.FilePath);
                    }
                    catch{}
                }
            }
            nRtn += SqlHelper.DeleteByField<FlowAuditRecord>("FlowId", flowId);
            nRtn += SqlHelper.Delete(flow);
            return nRtn;
        }
        //通过用户获取存档工作流
        public List<ArchiveWorkflow> GetArchiveFlowByUser(int userId, AuditStatus status)
        {
            var filters = new EntityFilters { And = true };
            filters.Add(new EntityFilter { Operator = FilterOperator.Equal, PropertyName = "SubmitUserId", Value = userId });
            filters.Add(new EntityFilter { Operator = FilterOperator.Equal, PropertyName = "Status", Value = status });
            var list = SqlHelper.Filter<ArchiveWorkflow>(filters);
            if (status == AuditStatus.Draft)
            {
                filters[1].Value = AuditStatus.Rejected;
                list.AddRange(SqlHelper.Filter<ArchiveWorkflow>(filters));
            }
            return list;
        }
        //通过用户获取审核流
        public List<ArchiveWorkflow> GetAuditFlowByUser(int userId, AuditStatus status)
        {
            var parmeters = new List<SqlParameter>();
            parmeters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
            parmeters.Add(new SqlParameter("@AuditStatus", SqlDbType.SmallInt) { Value = status });
            return SqlHelper.ExecuteStoredProcedure<ArchiveWorkflow>("PROC_GETUSERAUDITWORKFLOWS", parmeters);
        }
        //获取存档工作流
        public ArchiveWorkflow GetArchiveWorkflow(int flowId)
        {
            IBaseEntity entity = new ArchiveWorkflow();
            SqlHelper.GetSingleEntity(flowId, ref entity, true, true);
            var flow = entity as ArchiveWorkflow;
            if (flow != null && flow.Files != null)
            {
                foreach (var file in flow.Files)
                {
                    IBaseEntity tmp = new Document();
                    SqlHelper.GetSingleEntity(file.DocumentId, ref tmp, true, true);
                    var doc = tmp as Document;
                    if (doc != null)
                        file.DocumentId = doc.DocumentId;
                    file.DocumentInfo = doc;
                }
            }
            return flow;
        }
        //用户审计工作流使能
        public bool CanUserAuditWorkflow(int flowId, AuditStatus status, int userId)
        {
            if (status != AuditStatus.Auditing)
                return false;
            return GetAuditFlowByUser(userId, status).Any(o => o.FlowId == flowId);
        }
        //获取审计记录列表
        public List<FlowAuditRecord> GetFlowAuditRecords(int flowId)
        {
            IList list = new List<FlowAuditRecord>();
            SqlHelper.SearchByField("FlowId", flowId, ref list, true, false);
            return list.Cast<FlowAuditRecord>().OrderBy(o => o.AuditTime).ToList();
        }
        //审计存档工作流
        public int AuditArchiveWorkflow(FlowAuditRecord auditRecord)
        {
            if (auditRecord == null || auditRecord.FlowId < 1)
                return -2;
            var awf = GetArchiveWorkflow(auditRecord.FlowId);
            if (awf == null || awf.FlowId != auditRecord.FlowId)
                return -3;
            var twf = GetWorkflowInfo(awf.FlowType);
            WorkflowAuditStep curStep = null, nextStep = null;
            foreach (var step in twf.AuditSteps)
            {
                if (step.StepId == awf.CurrentStep)
                {
                    curStep = step;
                    continue;
                }
                if (curStep != null)
                {
                    nextStep = step;
                    break;
                }
            }
            if (curStep == null)
                return -4;

            SqlHelper.Insert(auditRecord);

            if (curStep.AuditType == AuditType.JointCheckup)
            {
                var records = GetFlowAuditRecords(awf.FlowId);
                foreach (var user in GetStepJointCheckupUsers(curStep.StepId))
                {
                    JointCheckupUser user1 = user;
                    if (!records.Exists(o => user1.UserId == o.AuditUserId))
                    {
                        nextStep = curStep;
                        break;
                    }
                }
            }
            switch (auditRecord.Operation)
            {
                case AuditOperation.Audit:
                case AuditOperation.Final:
                case AuditOperation.JointCheck:
                    if (nextStep != null && nextStep != curStep)
                    {
                        awf.CurrentStep = nextStep.StepId;
                        awf.Status = AuditStatus.Auditing;
                    }
                    else if (nextStep == null)
                    {
                        awf.Status = AuditStatus.Audited;
                        foreach (var file in awf.Files)
                        {
                            if (file.DocumentInfo != null)
                            {
                                file.DocumentInfo.Status = DocumentStatus.FinalVersion;
                                SqlHelper.Update(file.DocumentInfo);
                            }
                        }
                    }
                    break;
                case AuditOperation.Return:
                    awf.Status = AuditStatus.Returned;
                    foreach (var file in awf.Files)
                    {
                        if (file.DocumentInfo != null)
                        {
                            file.DocumentInfo.Status = DocumentStatus.Hide;
                            SqlHelper.Update(file.DocumentInfo);
                        }
                    }
                    break;
                case AuditOperation.Reject:
                    awf.CurrentStep = twf.AuditSteps.OrderBy(o => o.AuditIndex).First().StepId;
                    awf.Status = AuditStatus.Rejected;
                    break;
            }
            return SqlHelper.Update(awf);
        }
        //删除工作流步骤
        private static void DeleteWorkfowSteps(int flowType)
        {
            IList stepList = new List<WorkflowAuditStep>();
            SqlHelper.SearchByField("FlowType", flowType, ref stepList);
            foreach (WorkflowAuditStep step in stepList)
            {
                if (step.AuditType == AuditType.JointCheckup)
                    SqlHelper.DeleteByField<JointCheckupUser>("StepId", step.StepId);
            }
            SqlHelper.DeleteByField<WorkflowAuditStep>("FlowType", flowType);
        }
        //删除工作流
        public int DeleteWorkflow(WorkflowInfo flow)
        {
            if (flow != null)
            {
                flow.LastUpdateTime = DateTime.Now;
                flow.Status = ActiveStatus.Deleted;
                return SqlHelper.Update(flow);
            }
            return -2;
        }
    }
}


