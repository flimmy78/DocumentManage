using System;
using System.Data;

namespace DocumentManage.Utility
{
    //DocumentManage.Utility 空间 -- 自定义属性接口类，需要实例化来调用
    [AttributeUsage(AttributeTargets.Property)]
    public class EntityColumnAttribute : Attribute
    {
        public EntityColumnAttribute() { }
        //列属性
        public EntityColumnAttribute(string field)
        {
            Field = field;
            Key = false;
            DataType = DbType.AnsiString;
        }
        //列属性
        public EntityColumnAttribute(string field, DbType type)
        {
            Field = field;
            Key = false;
            DataType = type;
        }
        //列属性
        public EntityColumnAttribute(string field, bool key, DbType type)
        {
            Field = field;
            Key = key;
            DataType = type;
        }

        public bool ShowInGrid { get; set; }
        public string Header { get; set; }
        public string ValueFormat { get; set; }

        public bool Key { get; set; }
        public bool Unique { get; set; }

        public bool Foreign { get; set; }
        public string ForeignKey { get; set; }

        public bool Children { get; set; }
        public bool GetChildForeigns { get; set; }
        public string ChildKey { get; set; }

        public string Field { get; set; }
        public DbType DataType { get; set; }
    }

    //DocumentManage.Utility 空间 -- 列验证类，需要实例化来调用
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnValidatorAttribute : Attribute
    {
        public ColumnValidatorAttribute(bool required)
        {
            Required = required;
        }

        public bool Required { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public string Pattern { get; set; }
        public string ErrorMessage { get; set; }
        public object MinValue { get; set; }
        public object MaxValue { get; set; }
    }

    //DocumentManage.Utility 空间 -- 表名称类，需要实例化来调用
    [AttributeUsage(AttributeTargets.Class)]
    public class EntityTableNameAttribute : Attribute
    {
        public EntityTableNameAttribute(string table)
        {
            TableName = table;
        }

        public string TableName { get; set; }
    }

}
