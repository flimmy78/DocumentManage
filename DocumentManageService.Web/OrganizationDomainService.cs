
using System;
using System.Collections;
using System.Collections.ObjectModel;
using DocumentManage.Entities;
using DocumentManage.Utility;

namespace DocumentManageService.Web
{
    using System.Collections.Generic;
    using System.ServiceModel.DomainServices.Hosting;
    using System.ServiceModel.DomainServices.Server;


    // TODO: 创建包含应用程序逻辑的方法。
    [EnableClientAccess()]
    public class OrganizationDomainService : DomainService
    {
        private static Organization organizationTree = new Organization
                                       {
                                           Id = "1",
                                           ParentId = "0",
                                           Type = OrganizationType.Company,
                                           Code = "ROOT",
                                           Name = "ROOT"
                                       };
        
        private static bool orgChanged = true;
        //组织树
        public Organization GetOrganizationTree()
        {
            if (orgChanged)
            {
                GetOrgChildren(ref organizationTree);
                orgChanged = false;
            }
            return organizationTree;
        }
        //搜索组织
        public List<Organization> SearchOrganization(string strKey)
        {
            var filters = new EntityFilters();
            filters.And = false;
            filters.Add(new EntityFilter { Operator = FilterOperator.StartWith, PropertyName = "Id", Value = strKey });
            filters.Add(new EntityFilter { Operator = FilterOperator.StartWith, PropertyName = "ParentId", Value = strKey });
            filters.Add(new EntityFilter { Operator = FilterOperator.Like, PropertyName = "Code", Value = strKey });
            filters.Add(new EntityFilter { Operator = FilterOperator.Like, PropertyName = "Name", Value = strKey });
            return SqlHelper.Filter<Organization>(filters);
        }
        //获取组织子成员
        public void GetOrgChildren(ref Organization org)
        {
            if (org.Children == null)
                org.Children = new ObservableCollection<Organization>();
            org.Children.Clear();
            IList children = new List<Organization>();
            SqlHelper.SearchByField("ParentId", org.Id, ref children);
            foreach (var t in children)
            {
                var tmp = t as Organization;
                org.Children.Add(tmp);
                GetOrgChildren(ref tmp);
            }
        }
        //保存组织信息
        [Invoke]
        public int SaveOrganizationInfo(Organization org)
        {
            IList list = new List<Organization>();
            SqlHelper.SearchByField("Id", org.Id, ref list);
            orgChanged = true;
            if (list.Count > 0)
                return SqlHelper.Update(org);
            return SqlHelper.Insert(org);
        }
        //获取组织信息
        public Organization GetOrganizationInfo(string orgId)
        {
            IBaseEntity org = new Organization();
            SqlHelper.GetSingleEntity(orgId, ref org);
            return org as Organization;
        }
    }
}


