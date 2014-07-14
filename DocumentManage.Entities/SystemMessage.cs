using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using DocumentManage.Utility;

namespace DocumentManage.Entities
{
    //SQL  系统消息
    [EntityTableName("T_SYS_MSG")]
    public class SystemMessage : IBaseEntity
    {
        [Display(Name = "Identity", AutoGenerateField = false)]
        public int Identity
        {
            get { return MessageId; }
            set { MessageId = value; }
        }

        [Display(Name = "消息ID", AutoGenerateField = false)]
        [EntityColumn("MSG_ID", true, DbType.Int32)]
        public int MessageId { get; set; }

        [Display(Name = "标题", AutoGenerateField = true, Order=1)]
        [EntityColumn("MSG_TITLE", DbType.String)]
        public string MessageTitle { get; set; }

        [Display(Name = "内容", AutoGenerateField = false, Order = 2)]
        [EntityColumn("MSG_CONTENT", DbType.String)]
        public string MessageContent { get; set; }

        [Display(Name = "创建人ID", AutoGenerateField = false)]
        [EntityColumn("CREATED_BY", DbType.Int32)]
        public int CreatedBy { get; set; }

        [Display(Name="发布人", AutoGenerateField=true, Order = 3)]
        [EntityColumn(Foreign = true, ForeignKey = "CreatedBy", GetChildForeigns = false)]
        public SystemUser CreateUser { get; set; }

        [Display(Name="创建日期", AutoGenerateField = true, Order = 4)]
        [EntityColumn("CREATE_TIME", DbType.DateTime)]
        public DateTime CreateTime { get; set; }

        [Display(Name = "提醒次数", AutoGenerateField = true, Order = 5)]
        [EntityColumn("REMIND_TIMES", DbType.Int32)]
        public int RemindTimes { get; set; }

        public object Clone()
        {
            return new SystemMessage
                {
                    Identity = Identity,
                    MessageTitle = MessageTitle,
                    MessageContent = MessageContent,
                    CreatedBy = CreatedBy,
                    CreateUser= CreateUser,
                    CreateTime = CreateTime,
                    RemindTimes = RemindTimes
                };
        }
    }
}
