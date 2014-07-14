using System.Data;
using DocumentManage.Utility;

namespace DocumentManage.Entities
{   
    //SQL 工作流文件
    [EntityTableName("T_WORKFLOW_FILES")]
    public class WorkflowFileInfo : IBaseEntity
    {
        public int Identity
        {
            get { return Id; }
            set { Id = value; }
        }

        [EntityColumn("ID", true, DbType.Int32)]
        public int Id { get; set; }

        [EntityColumn("FLOW_ID", DbType.Int32)]
        public int FlowId { get; set; }

        [EntityColumn("DOC_ID", DbType.Int32)]
        public int DocumentId { get; set; }

        [EntityColumn(Foreign = true, ForeignKey = "DocumentId", GetChildForeigns = false)]
        public Document DocumentInfo { get; set; }

        public object Clone()
        {
            return new WorkflowFileInfo
                {
                    Id = Id,
                    FlowId = FlowId,
                    DocumentId = DocumentId,
                    DocumentInfo = DocumentInfo == null ? null : DocumentInfo.Clone() as Document
                };
        }
    }
}
