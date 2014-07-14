using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace DocumentManage.Entities
{
    [Flags]
    public enum LogType : short
    {
        [Display(Name = "默认")]
        Default = 0x01,
        [Display(Name = "系统")]
        System = 0x02,
        [Display(Name = "历史")]
        History = 0x04,
        [Display(Name = "文档")]
        Document = 0x08,
        [Display(Name = "浏览")]
        View = 0x10
    }
}
