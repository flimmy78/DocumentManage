using System.Collections.Generic;

namespace DocumentManage.Entities
{
    public partial class WorkflowInfo
    {
        partial void OnCreated()
        {
            if (this.AuditSteps == null)
                this.AuditSteps = new List<WorkflowAuditStep>();
        }
    }
}
