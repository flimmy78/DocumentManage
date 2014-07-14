using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using DocumentManage.Utility;

namespace DocumentManage.Entities
{
    //SQL 文件描述表
    [EntityTableName("T_DOC_RELEASE")]
    public class DocumentRelease : IBaseEntity
    {
        [Display(Name = "Identity", AutoGenerateField = false, Order = 0)]
        public int Identity
        {
            get { return ReleaseId; }
            set { ReleaseId = value; }
        }

        [Display(Name = "ReleaseId", AutoGenerateField = false, Order = 1)]
        [EntityColumn("RELEASE_ID", true, DbType.Int32)]
        public int ReleaseId { get; set; }

        [Display(Name = "发布类型", AutoGenerateField = true, Order = 2)]
        [EntityColumn("RELEASE_TYPE", DbType.Int16)]
        public DocumentReleaseType ReleaseType { get; set; }

        [Display(Name = "文件夹ID", AutoGenerateField = false, Order = 3)]
        [EntityColumn("FOLDER_ID", DbType.Int32)]
        public int FolderId { get; set; }

        [Display(Name = "文档ID", AutoGenerateField = false, Order = 5)]
        [EntityColumn("FILE_NAME", DbType.Guid)]
        public Guid DocumentId { get; set; }

        [Display(Name = "浏览用户ID", AutoGenerateField = false, Order = 7)]
        [EntityColumn("REVIEW_USER", DbType.Int32)]
        public int ReviewUserId { get; set; }

        [Display(Name = "浏览用户", AutoGenerateField = true, Order = 8)]
        [EntityColumn(Foreign = true, ForeignKey = "ReviewUserId", GetChildForeigns = false)]
        public SystemUser ReviewUser { get; set; }

        [Display(Name = "浏览组织ID", AutoGenerateField = false, Order = 9)]
        [EntityColumn("REVIEW_ORG", DbType.String)]
        public string ReviewOrgId { get; set; }

        [Display(Name = "浏览组织", AutoGenerateField = true, Order = 10)]
        [EntityColumn(Foreign = true, ForeignKey = "ReviewOrgId", GetChildForeigns = false)]
        public Organization ReviewOrg { get; set; }

        [Display(Name = "发布时间", AutoGenerateField = true, Order = 11)]
        [EntityColumn("RELEASE_TIME", DbType.DateTime)]
        public DateTime ReleaseTime { get; set; }

        [Display(Name = "发布用户ID", AutoGenerateField = false, Order = 12)]
        [EntityColumn("RELEASED_BY", DbType.Int32)]
        public int ReleasedBy { get; set; }

        [Display(Name = "发布用户", AutoGenerateField = true, Order = 13)]
        [EntityColumn(Foreign = true, ForeignKey = "ReleasedBy", GetChildForeigns = false)]
        public SystemUser ReleaseUser { get; set; }

        public object Clone()
        {
            return new DocumentRelease
                {
                    Identity = Identity,
                    ReleaseType = ReleaseType,
                    DocumentId = DocumentId,
                    FolderId = FolderId,
                    ReviewUserId = ReviewUserId,
                    ReviewOrgId = ReviewOrgId,
                    ReleaseTime = ReleaseTime,
                    ReleasedBy = ReleasedBy
                };
        }
    }
}
