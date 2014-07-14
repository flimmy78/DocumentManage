
using System.Collections;
using DocumentManage.Entities;
using DocumentManage.Utility;

namespace DocumentManageService.Web
{
    using System.Linq;
    using System.Collections.Generic;
    using System.ServiceModel.DomainServices.Hosting;
    using System.ServiceModel.DomainServices.Server;


    // TODO: 创建包含应用程序逻辑的方法。
    [EnableClientAccess()]
    public class FileDescDomainService : DomainService
    {
        //获取文件描述列表
        public List<FileDescTemplate> GetDescTemplateByType(DocumentType type)
        {
            IList list = new List<FileDescTemplate>();
            SqlHelper.SearchByField("DocType", type, ref list);
            return (from o in list.Cast<FileDescTemplate>()
                   orderby o.DisplayIndex
                   select o).ToList();
        }
        //获取活动文件描述列表
        public List<FileDescTemplate> GetActiveTemplateByType(DocumentType type)
        {
            IList list = new List<FileDescTemplate>();
            SqlHelper.SearchByField("DocType", type, ref list);
            return (from o in list.Cast<FileDescTemplate>()
                   where o.Status == ActiveStatus.Active
                   orderby o.DisplayIndex
                   select o).ToList();
        }
        //创建模板
        public int CreateTemplate(FileDescTemplate template)
        {
            return SqlHelper.Insert(template);
        }
        //更新模板
        public int UpdateTemplate(FileDescTemplate template)
        {
            return SqlHelper.Update(template);
        }
        //获取所有模板
        public List<FileDescTemplate> GetAllTemplates()
        {
            return SqlHelper.GetAllEntities<FileDescTemplate>();
        }
    }
}


