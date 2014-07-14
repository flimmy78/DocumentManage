using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DocumentManage.Entities;

namespace DocumentManage.Utility
{
    //身份验证类
    public class AuthenticateStatus
    {
        static AuthenticateStatus()
        {
            DefaultOrganization = "10";
        }

        public static bool HasLogin
        {
            get
            {
                if (CurrentUser != null && CurrentUser.UserId > 0 && !string.IsNullOrEmpty(CurrentUser.UserCode))
                {
                    return true;
                }
                return false;
            }
        }                             //登录状态

        public static bool CheckModuleAccess(string moduleId)
        {
            return UserModules != null && UserModules.Any(o => o.ModuleId.Equals(moduleId, StringComparison.Ordinal));
        }   //检查模块权限

        public static Visibility GetModuleVisibility(string moduleId)
        {
            return CheckModuleAccess(moduleId) ? Visibility.Visible : Visibility.Collapsed;
        }

        public static bool CheckUserOrgAccess(string orgId)
        {
            if (HasLogin && UserOrgs != null && UserOrgs.Count > 0)
            {
                return UserOrgs.Any(uor => uor.OrganizationId == orgId || (uor.IncludeChildOrg && orgId.StartsWith(uor.OrganizationId)));
            }
            return false;
        }      //检查用户组权限

        public static SystemUser CurrentUser { get; set; }          //当前用户信息  定义在SystemUser.cs中

        public static List<UserRoleRel> UserRoles { get; set; }     //用户角色列表

        public static List<UserOrgRel> UserOrgs { get; set; }       //用户组列表

        public static List<SystemModule> UserModules { get; set; }  //用户模型列表

        public static string DefaultOrganization { get; set; }      //默认组织
    }
}
