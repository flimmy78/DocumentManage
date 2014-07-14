using System.ComponentModel.DataAnnotations;
using System.Data;
using DocumentManage.Utility;

namespace DocumentManage.Entities
{
    //SQL 系统消息组织
    [EntityTableName("T_SYS_MSG_ORG")]
    public class SystemMessageOrg : IBaseEntity
    {
        public int Identity
        {
            get { return Id; }
            set { Id = value; }
        }

        [Display(Name = "ID", AutoGenerateField = false)]
        [EntityColumn("ID", true, DbType.Int32)]
        public int Id { get; set; }

        [Display(Name = "消息ID", AutoGenerateField = false)]
        [EntityColumn("MSG_ID", DbType.Int32)]
        public int MessageId { get; set; }

        [Display(Name = "组织ID", AutoGenerateField = false)]
        [EntityColumn("ORG_ID", DbType.String)]
        public string OrganizationId { get; set; }

        public object Clone()
        {
            return new SystemMessageOrg
                {
                    Identity = Identity,
                    MessageId = MessageId,
                    OrganizationId = OrganizationId
                };
        }
    }
}
