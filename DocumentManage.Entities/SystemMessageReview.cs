using System.Data;
using DocumentManage.Utility;

namespace DocumentManage.Entities
{
    //SQL 系统消息提醒
    [EntityTableName("T_SYS_MSG_REVIEW")]
    public class SystemMessageReview : IBaseEntity
    {
        public int Identity
        {
            get { return Id; }
            set { Id = value; }
        }

        [EntityColumn("ID", true, DbType.Int32)]
        public int Id { get; set; }

        [EntityColumn("USER_ID", DbType.Int32)]
        public int UserId { get; set; }

        [EntityColumn("MSG_ID", DbType.Int32)]
        public int MessageId { get; set; }

        [EntityColumn("REVIEW_TIMES", DbType.Int32)]
        public int ReviewTimes { get; set; }

        public object Clone()
        {
            return new SystemMessageReview
                {
                    Identity = Identity,
                    UserId = UserId,
                    MessageId = MessageId,
                    ReviewTimes = 0
                };
        }
    }
}
