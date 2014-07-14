using System.Collections.Generic;
using System.Data;
using DocumentManage.Utility;

namespace DocumentManage.Entities
{
    //SQL 系统模块定义
    [EntityTableName("T_SYS_MODULES")]
    public class SystemModule : IBaseEntity
    {
        [EntityColumn("MODULE_ID", true, DbType.String)]
        public string ModuleId { get; set; }

        [EntityColumn("PARENT_ID", DbType.String)]
        public string ParentId { get; set; }

        [EntityColumn("MODULE_TYPE", DbType.Int16)]
        public ModuleType ModuleType { get; set; }

        [EntityColumn("MOUDLE_CODE", DbType.String)]
        public string ModuleCode { get; set; }

        [EntityColumn("MODULE_NAME", DbType.String)]
        public string ModuleName { get; set; }

        [EntityColumn("NAV_URI", DbType.String)]
        public string NavigateUri { get; set; }

        [EntityColumn("MODULE_STATUS", DbType.Int16)]
        public ActiveStatus ModuleStatus { get; set; }

        [EntityColumn(Children = true, ChildKey = "ModuleId", GetChildForeigns = false)]
        public List<SystemModule> Children { get; set; }

        public object Clone()
        {
            return new SystemModule
                {
                    ModuleId = ModuleId,
                    ParentId = ParentId,
                    Identity = Identity,
                    ModuleType = ModuleType,
                    ModuleCode = ModuleCode,
                    ModuleName = ModuleName,
                    ModuleStatus = ModuleStatus
                };
        }

        public int Identity { get; set; }
    }
}
