using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DocumentManage.Utility;

namespace DocumentManage.Entities
{
    //SQL 系统日志
    [EntityTableName("T_SYS_LOGS")]
    public class SystemLog : IBaseEntity
    {
        public int Identity
        {
            get { return Id; }
            set { Id = value; }
        }

        [EntityColumn("ID", true, DbType.Int32)]
        public int Id { get; set; }

        [EntityColumn("LOG_TYPE", DbType.Int16)]
        public LogType LogType { get; set; }

        [EntityColumn("LOG_TIME", DbType.DateTime)]
        public DateTime LogTime { get; set; }

        [EntityColumn("ORG_ID", DbType.String)]
        public string OrgId { get; set; }

        [EntityColumn("USER_ID", DbType.Int32)]
        public int UserId { get; set; }

        [EntityColumn("LOG_INFO", DbType.String)]
        public string LogInfo { get; set; }

        public object Clone()
        {
            return new SystemLog
                {
                    Identity = Identity,
                    LogType = LogType,
                    LogTime = LogTime,
                    OrgId = OrgId,
                    UserId = UserId,
                    LogInfo = LogInfo
                };
        }
    }
}
