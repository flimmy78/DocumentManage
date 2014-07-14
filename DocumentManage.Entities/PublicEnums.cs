using System;
using System.ComponentModel.DataAnnotations;

//DocumentManage.Entities 空间 -- 定义的公共类型
namespace DocumentManage.Entities
{
    public enum ActiveStatus : short
    {
        [Display(Name = "使用中")]
        Active,
        [Display(Name = "已停用")]
        InActive,
        [Display(Name = "已过期")]
        Expired,
        [Display(Name = "已删除")]
        Deleted
    }


    public enum Gender : short
    {
        [Display(Name = "男")]
        Male,
        [Display(Name = "女")]
        Female
    }

    public enum ModuleType : short
    {
        [Display(Name = "请选择")]
        None,
        [Display(Name = "系统")]
        System,
        [Display(Name = "模块")]
        Module,
        [Display(Name = "菜单")]
        Menu,
        [Display(Name = "功能")]
        Function
    }

    public enum OrganizationType : short
    {
        [Display(Name = "请选择")]
        None,
        [Display(Name = "总公司")]
        Company,
        [Display(Name = "部门")]
        Department,
        [Display(Name = "组")]
        Team,
        [Display(Name = "工厂")]
        Factory,
        [Display(Name = "车间")]
        WorkShop,
        [Display(Name = "班次")]
        Thread
    }

    public enum FileSaveMethod : short
    {
        [Display(Name = "文件系统")]
        FileSystem,
        [Display(Name = "数据库")]
        Database
    }

    public enum FileReadMethod : short
    {
        [Display(Name = "在线浏览")]
        Online,
        [Display(Name = "下载浏览")]
        Download
    }

    public enum DocumentType : short
    {
        Unknown,
        TXT,
        CSV,
        XML,
        BMP,
        JPG,
        GIF,
        PNG,
        DOC,
        DOCX,
        XLS,
        XLSX,
        PPT,
        PPTX,
        PDF,
        DWG,
        DWF,
        Folder = 99
    }

    public enum CustomDataType : short
    {
        [Display(Name = "字符串")]
        String,
        [Display(Name = "整数")]
        Integer,
        [Display(Name = "浮点数")]
        Float,
        [Display(Name = "日期")]
        Date,
        [Display(Name = "时间")]
        Time,
        [Display(Name = "日期+时间")]
        DateTime
    }

    public enum AuditType : short
    {
        [Display(Name = "特定用户")]
        ConstUser,
        [Display(Name = "特定部门")]
        ConstOrg,
        [Display(Name = "组织类型")]
        Organization,
        [Display(Name = "系统角色")]
        Role,
        [Display(Name = "多人会审")]
        JointCheckup,
        [Display(Name = "最终审批")]
        FinalJudgement
    }

    public enum AuditStatus : short
    {
        [Display(Name = "新建")]
        New,

        [Display(Name = "草稿箱")]
        Draft,

        [Display(Name = "已提交")]
        Submitted,

        [Display(Name = "审批中")]
        Auditing,

        [Display(Name = "已审批")]
        Audited,

        [Display(Name = "未通过")]
        Rejected,

        [Display(Name = "已驳回")]
        Returned
    }

    public enum FileSystemEntityType : short
    {
        [Display(Name = "文件夹")]
        Folder,
        [Display(Name = "文件")]
        File
    }

    public enum DocumentReleaseType:short
    {
        [Display(Name="组织")]
        Organization,
        [Display(Name = "用户")]
        SystemUser
    }

    public enum AuditOperation : short
    {
        [Display(Name = "提交")]
        Submit,
        [Display(Name = "审核")]
        Audit,
        [Display(Name = "退回")]
        Reject,
        [Display(Name = "驳回")]
        Return,
        [Display(Name = "会审")]
        JointCheck,
        [Display(Name = "审批")]
        Final
    }

    public enum DocumentStatus : short
    {
        Draft,
        Pending,
        Public,
        Hide,
        Readonly,
        FinalVersion,
        Deleted
    }
}
