using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using DocumentManage.Utility;

namespace DocumentManage.Entities
{
    //SQL文件描述模板
    [EntityTableName("T_DOC_DESC_TEMPLATE")]
    public partial class FileDescTemplate : IBaseEntity
    {
        [Display(Name = "Identity", AutoGenerateField = false, Order = 0)]  
        public int Identity
        {
            get { return TemplateId; }
            set { TemplateId = value; }
        }

        [Display(Name = "TemplateId", AutoGenerateField = false, Order = 1)]
        [EntityColumn("TEMPLATE_ID", true, DbType.Int32)]  //数据库表项
        public int TemplateId { get; set; }

        [Display(Name = "文件类型", AutoGenerateField = true, Order = 2)]
        [EntityColumn("DOC_TYPE", DbType.Int16)]      //数据库表项
        public DocumentType DocType { get; set; }

        [Display(Name = "标题", AutoGenerateField = true, Order = 3)]
        [EntityColumn("HEADER", DbType.String)]    //数据库表项
        public string Header { get; set; }

        [Display(Name = "数据类型", AutoGenerateField = true, Order = 4)]
        [EntityColumn("DATA_TYPE", DbType.Int16)]    //数据库表项
        public CustomDataType DataType { get; set; }

        [Display(Name = "最小值", AutoGenerateField = true, Order = 5)]
        [EntityColumn("MIN_VALUE", DbType.String)]  //数据库表项
        public string MinimumValue { get; set; }

        [Display(Name = "最大值", AutoGenerateField = true, Order = 6)]
        [EntityColumn("MAX_VALUE", DbType.String)]  //数据库表项
        public string MaximumValue { get; set; }  

        [Display(Name = "默认值", AutoGenerateField = true, Order = 7)]
        [EntityColumn("DEFALUT_VALUE", DbType.String)]  //数据库表项
        public string DefaultValue { get; set; }

        [Display(Name = "固定长度", AutoGenerateField = true, Order = 8, Description = "固定长度字符串，如果不限长度，请输入0")]
        [EntityColumn("VALUE_LEN", DbType.Int16)]  //数据库表项
        public short ValueLength { get; set; }

        [Display(Name = "必填项", AutoGenerateField = true, Order = 9)]
        [EntityColumn("ISREQUIRED", DbType.Boolean)]   //数据库表项
        public bool IsRequired { get; set; }

        [Display(Name = "显示顺序", AutoGenerateField = true, Order = 10, Description = "按从小到大顺序显示")]
        [EntityColumn("DISPLAY_INDEX", DbType.Int32)] //数据库表项
        public int DisplayIndex { get; set; }

        [Display(Name = "状态", AutoGenerateField = true, Order = 11)]
        [EntityColumn("TEMPLATE_STATUS", DbType.Int16)]  //数据库表项
        public ActiveStatus Status { get; set; }
    }
}
