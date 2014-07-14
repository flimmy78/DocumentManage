using System;
using System.Collections.Generic;
using System.Linq;

namespace DocumentManage.Utility
{
    //DocumentManage.Utility 空间 -- 过滤类表
    public class EntityFilters : List<EntityFilter>
    {
        public bool And { get; set; }
        public EntityFilter Get(string name)
        {
            return this.FirstOrDefault((o) => o.PropertyName.Equals(name, StringComparison.CurrentCulture));
        }
    }

    //DocumentManage.Utility 空间 -- 过滤累
    public class EntityFilter
    {
        public string PropertyName { get; set; }        //名称属性
        public FilterOperator Operator { get; set; }  //操作属性
        public object Value { get; set; }            //值属性
    }

    //DocumentManage.Utility 空间 -- 过滤项
    public enum FilterOperator
    {
        Equal,              // 等于
        EqualOrLess,        // 小于等于
        EqualOrGreater,     // 大于等于
        Like,               // 类似
        StartWith,          // 以起始
        EndWith             // 以结束
    }
}
