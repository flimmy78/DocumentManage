
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using DocumentManage.Entities;
using DocumentManage.Utility;

namespace DocumentManageService.Web
{
    using System.Collections.Generic;
    using System.ServiceModel.DomainServices.Hosting;
    using System.ServiceModel.DomainServices.Server;


    // TODO: 创建包含应用程序逻辑的方法。
    [EnableClientAccess()]
    public class SysMessageDomainService : DomainService
    {
        //创建消息
        public int CreateMessage(SystemMessage msg, List<SystemMessageOrg> orgs, List<SystemMessageUser> users)
        {
            int nRtn = SqlHelper.Insert(msg);
            if (nRtn > 0)
            {
                foreach (var o in orgs)
                {
                    o.MessageId = msg.MessageId;
                    nRtn += SqlHelper.Insert(o);
                }
                foreach (var u in users)
                {
                    u.MessageId = msg.MessageId;
                    nRtn += SqlHelper.Insert(u);
                }
            }
            return nRtn;
        }
        //获取用户消息列表
        public List<SystemMessage> GetUserMessagesList(int userId)
        {
            var param = new List<SqlParameter>();
            param.Add(new SqlParameter("@UserID", SqlDbType.Int) {Value = userId});
            return SqlHelper.ExecuteStoredProcedure<SystemMessage>("PROC_GETUSERMESSAGELIST", param);
        }
        //获取组消息列表
        public List<SystemMessage> GetPopupMessageList(int userId)
        {
            var param = new List<SqlParameter>();
            param.Add(new SqlParameter("@UserID", SqlDbType.Int) { Value = userId });
            return SqlHelper.ExecuteStoredProcedure<SystemMessage>("PROC_GETPOPUPMESSAGELIST", param);
        }
        //获取消息信息
        public SystemMessage GetMessageInfo(int messageId)
        {
            IBaseEntity msg = new SystemMessage();
            SqlHelper.GetSingleEntity(messageId, ref msg);
            return msg as SystemMessage;
        }
        //获取消息列表
        public List<string> GetMessageReleaseInfo(int messageId)
        {
            var param = new SqlParameter("@MessageId", SqlDbType.Int) {Value = messageId};
            return SqlHelper.ExecuteProcedure("PROC_GETMSGRELEASEINFO", param);
        }
        //用户重复提醒消息
        public void UserReviewSysMessage(int userId, int messageId)
        {
            var filters = new EntityFilters { And = true };
            filters.Add(new EntityFilter { Operator = FilterOperator.Equal, PropertyName = "UserId", Value = userId });
            filters.Add(new EntityFilter { Operator = FilterOperator.Equal, PropertyName = "MessageId", Value = messageId });
            var list = SqlHelper.Filter<SystemMessageReview>(filters);
            if (list == null || list.Count < 1)
            {
                SqlHelper.Insert(new SystemMessageReview
                    {
                        Identity = -1,
                        UserId = userId,
                        MessageId = messageId,
                        ReviewTimes = 1
                    });
            }
            else
            {
                var smr = list[0];
                smr.ReviewTimes += 1;
                SqlHelper.Update(smr);
            }
        }
    }
}


