using System.ComponentModel.DataAnnotations;
using System.Data;
using DocumentManage.Utility;

namespace DocumentManage.Entities
{
    //SQL 系统消息用户
    [EntityTableName("T_SYS_MSG_USER")]
    public class SystemMessageUser : IBaseEntity
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

        [Display(Name = "用户ID", AutoGenerateField = false)]
        [EntityColumn("USER_ID", DbType.Int32)]
        public int UserId { get; set; }

        public object Clone()
        {
            return new SystemMessageUser
            {
                Identity = Identity,
                MessageId = MessageId,
                UserId = UserId
            };
        }
    }
}
