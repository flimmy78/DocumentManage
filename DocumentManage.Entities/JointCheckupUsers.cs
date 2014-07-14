using System.Data;
using DocumentManage.Utility;

namespace DocumentManage.Entities
{
    //SQL
    [EntityTableName("T_WF_JC_USERS")]
    public class JointCheckupUser : IBaseEntity
    {
        public int Identity { get { return Id; } set { Id = value; } }

        [EntityColumn("ID", true, DbType.Int32)]
        public int Id { get; set; }

        [EntityColumn("STEP_ID",  DbType.Int32)]
        public int StepId { get; set; }

        [EntityColumn("USER_ID", DbType.Int32)]
        public int UserId { get; set; }

        [EntityColumn(Foreign = true, ForeignKey = "UserId")]
        public SystemUser UserInfo { get; set; }

        public object Clone()
        {
            return new JointCheckupUser
                {
                    Id = Id,
                    StepId = StepId,
                    UserId = UserId
                };
        }
    }
}
