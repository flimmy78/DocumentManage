using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DocumentManageService.Web
{
    using System.ServiceModel.DomainServices.Hosting;
    using System.ServiceModel.DomainServices.Server;
    using DocumentManage.Entities;
    using DocumentManage.Utility;


    [EnableClientAccess()]
    public class SystemUserDomainService : DomainService
    {   
        //获取页用户列表
        public List<SystemUser> GetUsersByPage(int pageSize, int pageIndex)
        {
            if (pageIndex < 1)
                pageIndex = 1;
            int recordCount, pageCount;
            return SqlHelper.GetEntityByPage<SystemUser>(pageSize, pageIndex, out recordCount, out pageCount);
        }
        //获取组织用户列表
        public List<UserOrgRel> GetUserOrgRel(int userId)
        {
            if (userId < 1)
                return null;
            IList list = new List<UserOrgRel>();
            SqlHelper.SearchByField("UserId", userId, ref list, true, false);
            return (List<UserOrgRel>)list;
        }
        //获取用户角色
        public List<UserRoleRel> GetUserRoleRel(int userId)
        {
            if (userId < 1)
                return null;
            IList list = new List<UserRoleRel>();
            SqlHelper.SearchByField("UserId", userId, ref list, true, false);
            return (List<UserRoleRel>)list;
        }
        //获取用户数
        public int GetUserCount()
        {
            return SqlHelper.GetRecordCount<SystemUser>();
        }
        //登陆
        public SystemUser Login(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return null;
            IBaseEntity entity = new SystemUser();
            SqlHelper.GetSingleEntity(userName, ref entity);
            var user = (SystemUser)entity;
            if (userName.Equals(user.UserName, StringComparison.CurrentCulture) &&
                Md5Encrypt.GetMd5Hash(password).Equals(user.UserPassword, StringComparison.CurrentCulture))
            {
                return user;
            }
            return null;
        }
        //用户名被使用？
        public bool HasUserNameUsed(string userName, int userId)
        {
            IList users = new List<SystemUser>();
            SqlHelper.SearchByField("UserName", userName, ref users);
            if (userId < 1 && users.Count > 0)
                return true;

            return users.Cast<SystemUser>().Any(user => user.UserId != userId);
        }
        //查找用户
        public List<SystemUser> SearchUser(string searchKey)
        {
            var filters = new EntityFilters();
            filters.And = false;
            filters.Add(new EntityFilter { Operator = FilterOperator.Like, PropertyName = "UserCode", Value = searchKey });
            filters.Add(new EntityFilter { Operator = FilterOperator.Like, PropertyName = "UserName", Value = searchKey });
            filters.Add(new EntityFilter { Operator = FilterOperator.Like, PropertyName = "RealName", Value = searchKey });
            return SqlHelper.Filter<SystemUser>(filters);
        }
        //创建用户
        public int CreateUser(SystemUser user, List<UserRoleRel> roles, List<UserOrgRel> orgs)
        {
            if (user == null || string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.UserPassword))
            {
                return -2;
            }
            user.UserPassword = Md5Encrypt.GetMd5Hash(user.UserPassword);
            if (SqlHelper.Insert(user) > 0)
            {
                foreach (var role in roles)
                {
                    role.UserId = user.UserId;
                    SqlHelper.Insert(role);
                }
                if (orgs != null)
                {
                    foreach (var org in orgs)
                    {
                        org.UserId = user.UserId;
                        SqlHelper.Insert(org);
                    }
                }
                return 1;
            }
            return -1;
        }
        //删除用户
        public int DeleteUser(SystemUser user)
        {
            if (user != null)
            {
                user.Status = ActiveStatus.Deleted;
                return SqlHelper.Update(user);
            }
            return -1;
        }
        //更新用户
        public int UpdateUser(SystemUser user, List<UserRoleRel> roles, List<UserOrgRel> orgs)
        {
            if (user == null)
                return -2;

            var oldUser = GetUser(user.UserId);
            if (oldUser == null)
                return -3;

            if (oldUser.UserPassword != user.UserPassword)
                user.UserPassword = Md5Encrypt.GetMd5Hash(user.UserPassword);

            if (roles.Count > 0 && SqlHelper.Update(user) > 0)
            {
                SqlHelper.DeleteByField<UserRoleRel>("UserId", user.UserId);
                SqlHelper.DeleteByField<UserOrgRel>("UserId", user.UserId);
                foreach (var role in roles)
                {
                    role.UserId = user.UserId;
                    SqlHelper.Insert(role);
                }
                if (orgs != null)
                {
                    foreach (var org in orgs)
                    {
                        org.UserId = user.UserId;
                        SqlHelper.Insert(org);
                    }
                }
                return 1;
            }
            return -1;
        }
        //获取用户
        public SystemUser GetUser(int userId)
        {
            IBaseEntity userInfo = new SystemUser();
            SqlHelper.GetSingleEntity(userId, ref userInfo);
            return userInfo as SystemUser;
        }
    }
}


