using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using DocumentManage.Entities;
using DocumentManage.Utility;

namespace DocumentManageService.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel.DomainServices.Hosting;
    using System.ServiceModel.DomainServices.Server;

    // TODO: 创建包含应用程序逻辑的方法。
    [EnableClientAccess()]
    public class DocumentDomainService : DomainService        //文件服务
    {
        //通过组织ID，用户ID获取文件列表
        public List<FileSystemEntity> GetFileSystemEntityByOrg(string orgId, int userId)
        {
            var list = GetUserFolders(orgId, 0, userId);
            list.AddRange(SearchFile(string.Empty, orgId, 0, userId));
            return list;
        }
        //通过组织ID，用户ID获取文件夹列表
        public List<FileSystemEntity> GetFileSystemEntityByFolder(int folderId, int userId)
        {
            var list = GetUserFolders(string.Empty, folderId, userId);
            list.AddRange(SearchFile(string.Empty, string.Empty, folderId, userId).Where(o => o.FolderId == folderId));
            return list;
        }
        //返回上级菜单
        public List<FileSystemEntity> BackToParentFolder(int folderId, int userId)
        {
            IBaseEntity entity = new DocumentFolder();
            SqlHelper.GetSingleEntity(folderId, ref entity);
            var folder = entity as DocumentFolder;
            if (folder != null && folder.FolderId == folderId && !string.IsNullOrEmpty(folder.Name))
            {
                return GetFileSystemEntityByFolder(folder.ParentId, userId).Where(o => o.OrgId == folder.OrganizationId).ToList();
            }
            return null;
        }
        //查找文件
        public List<FileSystemEntity> SearchFile(string searchKey, string orgId, int folderId, int userId)
        {
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@SearchKey", SqlDbType.NVarChar, 128) {Value = searchKey},
                    new SqlParameter("@OrgId", SqlDbType.VarChar, 32) {Value = orgId},
                    new SqlParameter("@FolderId", SqlDbType.Int) {Value = folderId},
                    new SqlParameter("@UserId", SqlDbType.Int) {Value = userId}
                };
            var docs = SqlHelper.ExecuteStoredProcedure<Document>("PROC_SEARCHDOCUMENT", parameters);
            return docs.Select(doc => new FileSystemEntity
                                          {
                                              DocumentInfo = doc,
                                              FileId = doc.UniqeName,
                                              FileRevision = doc.Revision,
                                              FileType = doc.FileType,
                                              FolderId = doc.FolderId,
                                              Name = doc.FileName,
                                              OrgId = doc.OrganizationId,
                                              ParentFolder = 0,
                                              Type = FileSystemEntityType.File
                                          }).ToList();
        }
        //获取最新文档
        public Document GetLatestDocument(Guid fileId)
        {
            IList docList = new List<Document>();
            SqlHelper.SearchByField("UniqeName", fileId, ref docList, false, true);
            if (docList.Count > 0)
                return docList.Cast<Document>().OrderBy(o => o.Revision).Last();
            return null;
        }
        //获取用户文件夹列表
        public List<FileSystemEntity> GetUserFolders(string orgId, int folderId, int userId)
        {
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@OrgId", SqlDbType.VarChar, 32) {Value = orgId},
                    new SqlParameter("@FolderId", SqlDbType.Int) {Value = folderId},
                    new SqlParameter("@UserId", SqlDbType.Int) {Value = userId}
                };
            var docs = SqlHelper.ExecuteStoredProcedure<DocumentFolder>("PROC_GETUSERDOCFOLDER", parameters);
            return docs.Select(doc => new FileSystemEntity
            {
                FolderId = doc.FolderId,
                Name = doc.Name,
                OrgId = doc.OrganizationId,
                ParentFolder = doc.ParentId,
                Type = FileSystemEntityType.Folder
            }).ToList();
        }
        //上传文件
        public Document UploadFile(Document doc)
        {
            CheckAndCreateFolder(doc);
            if (doc.Content != null && doc.Content.Length > 0)
            {
                var config = new SystemConfigDomainService().GetSystemConfig();
                string saveFolder = string.Format("{0}\\{1}\\", config.FileSaveUrl, DateTime.Now.ToString("yyyy-MM"));
                string saveFullName = string.Format("{0}{1}_{2}", saveFolder, doc.UniqeName, doc.Revision);
                doc.FilePath = saveFullName;
                if (!Directory.Exists(saveFolder))
                    Directory.CreateDirectory(saveFolder);
                using (var stream = new FileStream(saveFullName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    stream.Write(doc.Content, 0, doc.Content.Length);
                }
                doc.Content = null;
            }
            int n = 0;
            if (doc.DocumentId > 0)
                n = SqlHelper.Update(doc);
            else
                n = SqlHelper.Insert(doc);

            if (n > 0 && doc.Descriptions != null)
            {
                foreach (var desc in doc.Descriptions)
                {
                    desc.DocumentId = doc.DocumentId;
                    if (desc.Id > 0)
                        SqlHelper.Update(desc);
                    else
                        SqlHelper.Insert(desc);
                }
            }
            return doc;
        }
        //创建文件夹
        public void CheckAndCreateFolder(Document docInfo)
        {
            if (string.IsNullOrEmpty(docInfo.OrignalName))
                return;
            string filePath = docInfo.OrignalName;
            var arrFolder = filePath.Split('\\');
            int curFolderId = docInfo.FolderId;
            var rootFolder = GetFolder(curFolderId);
            var sbPath = new StringBuilder();
            sbPath.Append(rootFolder.FullName);

            for (int i = 0; i < arrFolder.Length; i++)
            {
                if (string.IsNullOrEmpty(arrFolder[i]))
                    continue;

                if (i == arrFolder.Length - 1 && arrFolder[i].IndexOf('.') > -1)
                    break;

                sbPath.AppendFormat("\\{0}", arrFolder[i]);
                int tmpChildId = GetChildFolderId(curFolderId, arrFolder[i]);
                if (tmpChildId > 0)
                {
                    curFolderId = tmpChildId;
                    docInfo.FolderId = curFolderId;
                    continue;
                }

                curFolderId = CreateFolder(new DocumentFolder
                    {
                        Identity = -1,
                        ParentId = curFolderId,
                        CreatedBy = docInfo.CreatedBy,
                        CreateTime = docInfo.CreateTime,
                        FullName = sbPath.ToString(),
                        Name = arrFolder[i],
                        OrganizationId = docInfo.OrganizationId,
                        Status = ActiveStatus.Active
                    });
            }
            docInfo.FolderId = curFolderId;
        }
        //获取子文件夹ID
        private int GetChildFolderId(int folderId, string folderName)
        {
            var filters = new EntityFilters { And = true };
            filters.Add(new EntityFilter { Operator = FilterOperator.Equal, PropertyName = "ParentId", Value = folderId });
            filters.Add(new EntityFilter { Operator = FilterOperator.Equal, PropertyName = "Name", Value = folderName });
            var children = SqlHelper.Filter<DocumentFolder>(filters);
            if (children == null || children.Count < 1)
                return -1;
            return children[0].FolderId;
        }
        //重命名文件夹
        public int RenameFolder(int folderId, string newName)
        {
            var folder = GetFolder(folderId);
            if (folder.FolderId == folderId && !string.IsNullOrEmpty(folder.Name) &&
                !folder.Name.Equals(newName, StringComparison.CurrentCultureIgnoreCase))
            {
                folder.Name = newName;
                return SqlHelper.Update(folder);
            }
            return -2;
        }
        //删除文件夹
        public int DeleteFolder(int folderId, int userId)
        {
            var list = GetUserFolders(string.Empty, folderId, userId);
            foreach (var folder in list)
            {
                DeleteFolder(folder.FolderId, userId);
            }
            SqlHelper.UpdateColumnByField<Document>("Status", (Int16)(DocumentStatus.Deleted), "FolderId", folderId);
            return SqlHelper.DeleteByField<DocumentFolder>("FolderId", folderId);
        }
        //下载文件
        public Document DownloadFile(Guid fileId, int revision, int userId)
        {
            IList files = new List<Document>();
            SqlHelper.SearchByField("UniqeName", fileId, ref files);
            var doc = files.Cast<Document>().FirstOrDefault(o => o.Revision == revision);
            if (doc != null && !string.IsNullOrEmpty(doc.FilePath) && File.Exists(doc.FilePath))
            {
                var log = new SystemLog
                    {
                        Identity = 0,
                        UserId = userId,
                        LogType = LogType.Document | LogType.View,
                        LogTime = DateTime.Now,
                        LogInfo = string.Format("下载文件，ID:{0}; 文件名：{1};版本：{2}", doc.Identity, doc.FileName, doc.Revision)
                    };
                SqlHelper.Insert(log);
                using (var stream = new FileStream(doc.FilePath, FileMode.Open, FileAccess.Read))
                {
                    doc.Content = new byte[stream.Length];
                    stream.Read(doc.Content, 0, doc.Content.Length);
                }
            }
            return doc;
        }
        //下载最新文件
        public Document DownloadLatestFile(Guid fileId)
        {
            var doc = GetLatestDocument(fileId);
            if (doc != null && !string.IsNullOrEmpty(doc.FilePath) && File.Exists(doc.FilePath))
            {
                using (var stream = new FileStream(doc.FilePath, FileMode.Open, FileAccess.Read))
                {
                    doc.Content = new byte[stream.Length];
                    stream.Read(doc.Content, 0, doc.Content.Length);
                }
            }
            return doc;
        }
        //删除文件
        public int DeleteFile(Guid fileId, int revision)
        {
            IList files = new List<Document>();
            SqlHelper.SearchByField("UniqeName", fileId, ref files);
            var doc = files.Cast<Document>().FirstOrDefault(o => o.Revision == revision);
            if (doc == null || doc.UniqeName != fileId)
                return -9;

            if (doc.Status == DocumentStatus.Draft)
            {
                File.Delete(doc.FilePath);
                SqlHelper.DeleteByField<DocumentDesc>("DocumentId", doc.DocumentId);
                SqlHelper.DeleteByField<WorkflowFileInfo>("DocumentId", doc.DocumentId);
                SqlHelper.Delete(doc);
            }
            else
            {
                doc.Status = DocumentStatus.Deleted;
                SqlHelper.Update(doc);
            }

            return 1;
        }
        //文件描述
        public int ReleaseDocument(DocumentRelease dr)
        {
            if (dr == null)
                return -2;
            var filters = new EntityFilters { And = true };
            filters.Add(new EntityFilter { Operator = FilterOperator.Equal, PropertyName = "ReleaseType", Value = dr.ReleaseType });
            filters.Add(new EntityFilter { Operator = FilterOperator.Equal, PropertyName = "DocumentId", Value = dr.DocumentId });
            filters.Add(new EntityFilter { Operator = FilterOperator.Equal, PropertyName = "FolderId", Value = dr.FolderId });
            filters.Add(new EntityFilter { Operator = FilterOperator.Equal, PropertyName = "ReviewUserId", Value = dr.ReviewUserId });
            filters.Add(new EntityFilter { Operator = FilterOperator.Equal, PropertyName = "ReviewOrgId", Value = dr.ReviewOrgId });
            if (SqlHelper.Filter<DocumentRelease>(filters).Count > 0)
                return 0;
            dr.ReleaseTime = DateTime.Now;
            return SqlHelper.Insert(dr);
        }
        //删除文件描述
        public int RemoveDocumentRelease(DocumentRelease dr)
        {
            if (dr == null || dr.Identity < 1)
                return -2;
            return SqlHelper.Delete(dr);
        }
        //获取文件描述历史
        public List<DocumentRelease> GetReleaseHistory(FileSystemEntity fse)
        {
            var filters = new EntityFilters { And = true };
            filters.Add(fse.Type == FileSystemEntityType.File
                            ? new EntityFilter
                                {
                                    Operator = FilterOperator.Equal,
                                    PropertyName = "DocumentId",
                                    Value = fse.FileId
                                }
                            : new EntityFilter
                                {
                                    Operator = FilterOperator.Equal,
                                    PropertyName = "FolderId",
                                    Value = fse.FolderId
                                });
            return SqlHelper.Filter<DocumentRelease>(filters, true, false);
        }
        //获取文件夹
        public DocumentFolder GetFolder(int folderId)
        {
            IBaseEntity folder = new DocumentFolder();
            SqlHelper.GetSingleEntity(folderId, ref folder, true, false);
            return folder as DocumentFolder;
        }
        //创建文件夹
        public int CreateFolder(DocumentFolder folder)
        {
            if (folder == null || folder.ParentId < 0 || string.IsNullOrEmpty(folder.Name))
                return -1;
            folder.CreateTime = DateTime.Now;
            SqlHelper.Insert(folder);
            return folder.Identity;
        }
    }
}


