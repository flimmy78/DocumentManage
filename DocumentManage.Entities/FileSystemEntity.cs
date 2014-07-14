using System;

namespace DocumentManage.Entities
{
    public class FileSystemEntity
    {
        public FileSystemEntityType Type { get; set; }
        public DocumentType FileType { get; set; }
        public string OrgId { get; set; }
        public int FolderId { get; set; }
        public int ParentFolder { get; set; }
        public Guid FileId { get; set; }
        public int FileRevision { get; set; }
        public Document DocumentInfo { get; set; }
        public string Name { get; set; }
    }
}
