using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using DocumentManage.Utility;

namespace DocumentManage.Entities
{
    [EntityTableName("T_DOCUMENTS")]
    public class Document : IBaseEntity
    {
        public int Identity
        {
            get { return DocumentId; }
            set { DocumentId = value; }
        }

        [EntityColumn("DOC_ID", true, DbType.Int32)]
        public int DocumentId { get; set; }

        [EntityColumn("FOLDER_ID", DbType.Int32)]
        public int FolderId { get; set; }

        [EntityColumn("ORG_ID", DbType.String)]
        public string OrganizationId { get; set; }

        [EntityColumn("DOC_TYPE", DbType.Int16)]
        public DocumentType FileType { get; set; }

        [EntityColumn("FILE_NAME", DbType.Guid)]
        public Guid UniqeName { get; set; }

        [EntityColumn("DOC_NAME", DbType.String)]
        public string FileName { get; set; }

        [EntityColumn("FILE_PATH", DbType.String)]
        public string FilePath { get; set; }

        [EntityColumn("ORIGNAL_NAME", DbType.String)]
        public string OrignalName { get; set; }

        [EntityColumn("REVISION", DbType.Int32)]
        public int Revision { get; set; }

        [EntityColumn(Children = true, ChildKey = "DocumentId", GetChildForeigns = false)]
        public List<DocumentDesc> Descriptions { get; set; }

        [EntityColumn("DOC_STATUS", DbType.Int16)]
        public DocumentStatus Status { get; set; }

        [Display(Name = "添加人ID", AutoGenerateField = false, Order = 7)]
        [EntityColumn("CREATED_BY", DbType.Int32)]
        public int CreatedBy { get; set; }

        [Display(Name = "添加日期", AutoGenerateField = false, Order = 8)]
        [EntityColumn("CREATE_TIME", DbType.DateTime)]
        public DateTime CreateTime { get; set; }

        [Display(Name = "最后更新人ID", AutoGenerateField = false, Order = 9)]
        [EntityColumn("LAST_UPDATED_BY", DbType.Int32)]
        public int LastUpdatedBy { get; set; }

        [Display(Name = "最后更新时间", AutoGenerateField = false, Order = 10)]
        [EntityColumn("LAST_UPDATE_TIME", DbType.DateTime)]
        public DateTime LastUpdateTime { get; set; }

        public byte[] Content { get; set; }

        public object Clone()
        {
            return new Document
                {
                    DocumentId = DocumentId,
                    FolderId = FolderId,
                    OrganizationId = OrganizationId,
                    FileType = FileType,
                    UniqeName = UniqeName,
                    FileName = FileName,
                    FilePath = FilePath,
                    OrignalName = OrignalName,
                    Revision = Revision,
                    Status = Status,
                    CreatedBy = CreatedBy,
                    CreateTime = CreateTime,
                    LastUpdateTime = LastUpdateTime,
                    LastUpdatedBy = LastUpdatedBy
                };
        }
    }
}
