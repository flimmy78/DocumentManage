
using DocumentManage.Entities;
using DocumentManage.Utility;

namespace DocumentManageService.Web
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.ServiceModel.DomainServices.Hosting;
    using System.ServiceModel.DomainServices.Server;


    // TODO: 创建包含应用程序逻辑的方法。
    [EnableClientAccess()]
    public class SystemConfigDomainService : DomainService
    {
        private static SystemConfig configInstance;
        //获取系统配置
        public SystemConfig GetSystemConfig()
        {
            if (configInstance == null)
            {
                var list = SqlHelper.GetAllEntities<SystemConfig>();
                if (list != null && list.Count > 0)
                    configInstance = list[0];
                if (configInstance == null)
                {
                    configInstance = new SystemConfig
                                         {
                                             Id = 1,
                                             ApplicationName = "文档管理系统",
                                             WriteLog = (int)LogType.Default,
                                             RecordAudit = false,
                                             SaveMethod = FileSaveMethod.FileSystem,
                                             ReadMethod = FileReadMethod.Online,
                                             FileSaveUrl = "F:\\Documents",
                                             ServerUserName = string.Empty,
                                             ServerPassword = string.Empty
                                         };
                    SqlHelper.Insert(configInstance);
                }
            }
            return configInstance;
        }
        //保存系统配置
        public int SaveSystemConfig(SystemConfig config)
        {
            config.Id = configInstance.Id;
            int n = SqlHelper.Update(config);
            if (n > 0)
            {
                configInstance = config;
                return n;
            }
            return -1;
        }
    }
}


