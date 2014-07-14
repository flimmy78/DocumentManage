using System.Data;
using DocumentManage.Utility;

namespace DocumentManage.Entities
{
    [EntityTableName("T_DOC_DESC")]      //数据库表名
    public class DocumentDesc : IBaseEntity
    {
        public int Identity
        {
            get { return Id; }
            set { Id = value; }
        }

        [EntityColumn("ID", true, DbType.Int32)]  //主键列
        public int Id{get; set;}

        [EntityColumn("DOC_ID", DbType.Int32)]     //文档ID列
        public int DocumentId { get; set; }

        [EntityColumn("TEMPLATE_ID", DbType.Int32)] //模板ID列
        public int TemplateId { get; set; }

        [EntityColumn(Foreign = true, ForeignKey="TemplateId", GetChildForeigns = false)]
        public FileDescTemplate TemplateInfo { get; set; }

        [EntityColumn("HEADER", DbType.String)]     //头列
        public string Header { get; set; }

        [EntityColumn("DESC_CONTENT", DbType.String)]  //描述列
        public string Description { get; set; }

        public object Clone()
        {
            return new DocumentDesc
                       {
                           Id = Id,
                           DocumentId = DocumentId,
                           TemplateId = TemplateId,
                           TemplateInfo = TemplateInfo == null ? null : TemplateInfo.Clone() as FileDescTemplate,
                           Description = Description
                       };
        }
    }
}
