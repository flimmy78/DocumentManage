
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using DocumentManage.Entities;
using DocumentManage.Utility;

namespace DocumentManageService.Web
{
    using System.Collections.Generic;
    using System.ServiceModel.DomainServices.Hosting;
    using System.ServiceModel.DomainServices.Server;


    // TODO: 创建包含应用程序逻辑的方法。
    [EnableClientAccess()]
    public class SystemModuleDomainService : DomainService
    {
        private static SystemModule systemModuleTree;
        //获取系统模块树
        public SystemModule GetSystemModuleTree()
        {
            if (systemModuleTree == null)
            {
                systemModuleTree = new SystemModule
                                       {
                                           ModuleId = "1",
                                           ParentId = "0",
                                           ModuleType = ModuleType.System,
                                           ModuleCode = "ROOT",
                                           ModuleName = "ROOT"
                                       };
                GetModuleChildren(ref systemModuleTree);
            }

            return systemModuleTree;
        }
        //获取顶层模块列表
        public List<SystemModule> GetTopModuleList()
        {
            return GetSubModuleList("10");
        }
        //获取子模块列表
        public List<SystemModule> GetSubModuleList(string parentId)
        {
            IList children = new List<SystemModule>();
            SqlHelper.SearchByField("ParentId", parentId, ref children);
            return children as List<SystemModule>;
        }
        //获取用户模块列表
        public List<SystemModule> GetUserModuleList(string parendId, int userId)
        {
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@ParentModuleId", SqlDbType.VarChar, 32) {Value = parendId},
                    new SqlParameter("@UserId", SqlDbType.Int) {Value = userId}
                };
            return SqlHelper.ExecuteStoredProcedure<SystemModule>("PROC_GETUSERMODULES", parameters);
        }
        //获取模块子成员
        private void GetModuleChildren(ref SystemModule module)
        {
            IList children = new List<SystemModule>();
            SqlHelper.SearchByField("ParentId", module.ModuleId, ref children);
            foreach (var t in children)
            {
                var tmp = t as SystemModule;
                if (module.Children == null)
                    module.Children = new List<SystemModule>();
                module.Children.Add(tmp);
                GetModuleChildren(ref tmp);
            }
        }
    }
}


