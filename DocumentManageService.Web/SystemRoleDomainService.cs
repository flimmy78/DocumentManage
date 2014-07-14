
using System;
using System.Collections;
using DocumentManage.Entities;
using DocumentManage.Utility;

namespace DocumentManageService.Web
{
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel.DomainServices.Hosting;
    using System.ServiceModel.DomainServices.Server;


    // TODO: 创建包含应用程序逻辑的方法。
    [EnableClientAccess()]
    public class SystemRoleDomainService : DomainService
    {
        //获取页角色
        public List<SystemRole> GetRolesByPage(int pageSize, int pageIndex)
        {
            int recordCount, pageCount;
            return SqlHelper.GetEntityByPage<SystemRole>(pageSize, pageIndex, out recordCount, out pageCount);
        }
        //获取角色个数
        public int GetRolesCount()
        {
            return SqlHelper.GetRecordCount<SystemRole>();
        }
        //角色代表被使用？
        public bool HasRoleCodeUsed(string code, int roleId)
        {
            IList users = new List<SystemRole>();
            SqlHelper.SearchByField("RoleCode", code, ref users);
            if (roleId < 1 && users.Count > 0)
                return true;

            return users.Cast<SystemRole>().Any(user => user.RoleId != roleId);
        }
        //查找角色
        public List<SystemRole> SearchRole(string searchKey)
        {
            var filters = new EntityFilters();
            filters.And = false;
            filters.Add(new EntityFilter { Operator = FilterOperator.Like, PropertyName = "RoleCode", Value = searchKey });
            filters.Add(new EntityFilter { Operator = FilterOperator.Like, PropertyName = "RoleName", Value = searchKey });
            return SqlHelper.Filter<SystemRole>(filters);
        }
        //删除角色
        public int DeleteRole(SystemRole role)
        {
            if (role == null)
                return -2;
            role.Status = ActiveStatus.Deleted;
            return SqlHelper.Update(role);
        }
        //获取角色模型
        public List<RoleModuleRel> GetRoleModules(int roleId)
        {
            IList list = new List<RoleModuleRel>();
            SqlHelper.SearchByField("RoleId", roleId, ref list);
            return list as List<RoleModuleRel>;
        }
        //获取所有角色
        public List<SystemRole> GetAllRoles()
        {
            var list = SqlHelper.GetAllEntities<SystemRole>(true, false).Where(o => o.Status == ActiveStatus.Active).ToList();
            return list;
        }
        //更新角色
        public int UpdateRole(SystemRole roleInfo, List<RoleModuleRel> modules)
        {
            if (roleInfo == null || string.IsNullOrEmpty(roleInfo.RoleCode) && roleInfo.RoleId > 0)
            {
                return -2;
            }
            roleInfo.LastUpdateTime = DateTime.Now;
            if (SqlHelper.Update(roleInfo) > 0)
            {
                SqlHelper.DeleteByField<RoleModuleRel>("RoleId", roleInfo.RoleId);
                foreach (var rel in modules)
                {
                    rel.RoleId = roleInfo.RoleId;
                    SqlHelper.Insert(rel);
                }
                return 1;
            }
            return -1;
        }
        //创建角色
        public int CreateRole(SystemRole roleInfo, List<RoleModuleRel> modules)
        {
            if (roleInfo == null || string.IsNullOrEmpty(roleInfo.RoleCode))
            {
                return -2;
            }
            roleInfo.CreateTime = DateTime.Now;
            roleInfo.LastUpdateTime = DateTime.Now;
            if (SqlHelper.Insert(roleInfo) > 0)
            {
                foreach (var rel in modules)
                {
                    rel.RoleId = roleInfo.RoleId;
                    SqlHelper.Insert(rel);
                }
                return 1;
            }
            return -1;
        }
    }
}


