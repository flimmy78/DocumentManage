using System;

namespace DocumentManage.Utility
{
    //DocumentManage.Utility -- 空间 克隆接口
    public interface IBaseEntity : ICloneable    //继承系统接口, 实现拷贝功能
    {
        int Identity { get; set; }   //身份
    }

}
