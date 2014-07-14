using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Collections;
using System.Diagnostics;

namespace DocumentManage.Utility
{
    //DocumentManage.Utility  空间 -- SQL帮助类，全部是静态函数，通过类名直接调用
    public sealed class SqlHelper
    {
        private static string _connStr = string.Empty;
        //SQL 连接字符串
        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connStr))
                {
                    _connStr = GetConnectionString();
                }
                return _connStr;
            }
        }
        //SQL  获取连接字符串
        private static string GetConnectionString()
        {
            return AppConfig.GetConnectionString();
        }
        //SQL命令 获取插入命令
        public static SqlCommand GetInsertCommand(IBaseEntity info)
        {
            if (info == null)
                return null;
            Type tp = info.GetType();

            string tbName;
            var attrs = tp.GetCustomAttributes(typeof(EntityTableNameAttribute), false);
            if (attrs.Length > 0)
            {
                tbName = ((EntityTableNameAttribute)attrs[0]).TableName;
            }
            else
            {
                return null;
            }

            var comm = new SqlCommand { CommandType = CommandType.Text };

            var sb = new StringBuilder();
            var sbV = new StringBuilder();
            var memberAccessor = new DynamicMethodMemberAccessor();

            sb.AppendFormat("INSERT INTO [{0}] (", tbName);
            sbV.Append(" VALUES (");

            foreach (PropertyInfo prop in tp.GetProperties())
            {
                var eca = Attribute.GetCustomAttribute(prop, typeof(EntityColumnAttribute)) as EntityColumnAttribute;
                if (eca != null && !eca.Key && !eca.Foreign && !eca.Children)
                {
                    sb.AppendFormat("[{0}],", eca.Field);
                    string paramName = string.Format("@{0}", prop.Name);
                    sbV.AppendFormat("{0},", paramName);
                    var param = new SqlParameter(paramName, eca.DataType);
                    param.Value = memberAccessor.GetValue(info, prop.Name);
                    if (param.Value == null && (eca.DataType == DbType.String || eca.DataType == DbType.AnsiString))
                        param.Value = DBNull.Value;
                    comm.Parameters.Add(param);
                }
            }
            sbV.Remove(sbV.Length - 1, 1).Append(");SELECT @ScopeIdentity = SCOPE_IDENTITY();");

            var idParam = new SqlParameter("@ScopeIdentity", SqlDbType.Int) { Direction = ParameterDirection.Output };
            comm.Parameters.Add(idParam);

            sb.Remove(sb.Length - 1, 1).Append(")").Append(sbV);
            comm.CommandText = sb.ToString();
            return comm;
        }
        //SQL命令 获取更新命令
        public static SqlCommand GetUpdateCommand(IBaseEntity info)
        {
            if (info == null)
                return null;
            Type tp = info.GetType();

            string tbName;
            var attrs = tp.GetCustomAttributes(typeof(EntityTableNameAttribute), false);
            if (attrs.Length > 0)
            {
                tbName = ((EntityTableNameAttribute)attrs[0]).TableName;
            }
            else
            {
                return null;
            }

            var comm = new SqlCommand { CommandType = CommandType.Text };

            var sb = new StringBuilder();
            var sbW = new StringBuilder();
            var memberAccessor = new DynamicMethodMemberAccessor();

            sb.AppendFormat("UPDATE [{0}] SET ", tbName);
            sbW.Append(" WHERE 1 = 2");

            foreach (PropertyInfo prop in tp.GetProperties())
            {
                var eca = Attribute.GetCustomAttribute(prop, typeof(EntityColumnAttribute)) as EntityColumnAttribute;
                if (eca != null && !eca.Foreign && !eca.Children)
                {
                    string paramName = string.Format("@{0}", prop.Name);
                    var param = new SqlParameter(paramName, eca.DataType) { Value = memberAccessor.GetValue(info, prop.Name) };
                    if (param.Value == null && (eca.DataType == DbType.String || eca.DataType == DbType.AnsiString))
                        param.Value = DBNull.Value;
                    comm.Parameters.Add(param);

                    if (eca.Key || eca.Unique)
                        sbW.AppendFormat(" OR [{0}] = {1}", eca.Field, paramName);
                    else
                        sb.AppendFormat("[{0}] = {1},", eca.Field, paramName);
                }
            }
            sb.Remove(sb.Length - 1, 1).Append(sbW);
            comm.CommandText = sb.ToString();
            return comm;
        }
        //SQL命令 获取删除命令
        public static SqlCommand GetDeleteCommand(IBaseEntity info)
        {
            if (info == null)
                return null;
            Type tp = info.GetType();

            string tbName;
            var attrs = tp.GetCustomAttributes(typeof(EntityTableNameAttribute), false);
            if (attrs.Length > 0)
            {
                tbName = ((EntityTableNameAttribute)attrs[0]).TableName;
            }
            else
            {
                return null;
            }

            var comm = new SqlCommand { CommandType = CommandType.Text };

            var sb = new StringBuilder();
            var memberAccessor = new DynamicMethodMemberAccessor();

            sb.AppendFormat("DELETE FROM [{0}] WHERE ", tbName);

            foreach (PropertyInfo prop in tp.GetProperties())
            {
                var eca = Attribute.GetCustomAttribute(prop, typeof(EntityColumnAttribute)) as EntityColumnAttribute;
                if (eca != null && eca.Key)
                {
                    string paramName = string.Format("@{0}", prop.Name);
                    var param = new SqlParameter(paramName, eca.DataType) { Value = memberAccessor.GetValue(info, prop.Name) };
                    comm.Parameters.Add(param);

                    sb.AppendFormat("[{0}] = {1}", eca.Field, paramName);
                    break;
                }
            }
            comm.CommandText = sb.ToString();
            return comm;
        }
        //SQL命令 获取Select命令
        public static SqlCommand GetSelectCommand<T>() where T : IBaseEntity
        {
            var tp = typeof(T);
            string tbName;
            var attrs = tp.GetCustomAttributes(typeof(EntityTableNameAttribute), false);
            if (attrs.Length > 0)
            {
                tbName = ((EntityTableNameAttribute)attrs[0]).TableName;
            }
            else
            {
                return null;
            }
            var comm = new SqlCommand
                           {
                               CommandText = string.Format("SELECT * FROM [{0}]", tbName)
                           };
            return comm;
        }
        //获取记录数
        public static int GetRecordCount<T>() where T : IBaseEntity
        {
            var tp = typeof(T);
            string tbName;
            var attrs = tp.GetCustomAttributes(typeof(EntityTableNameAttribute), false);
            if (attrs.Length > 0)
            {
                tbName = ((EntityTableNameAttribute)attrs[0]).TableName;
            }
            else
            {
                return 0;
            }
            using (var conn = new SqlConnection(ConnectionString))
            {
                var comm = conn.CreateCommand();
                comm.CommandText = string.Format("SELECT COUNT(1) AS R_COUNT FROM [{0}]", tbName);
                comm.CommandType = CommandType.Text;
                conn.Open();
                return Convert.ToInt32(comm.ExecuteScalar());
            }
        }
        //SQL命令 获取过滤命令
        public static SqlCommand GetFilterCommand<T>(EntityFilters filters) where T : IBaseEntity
        {
            var tp = typeof(T);
            string tbName;
            var attrs = tp.GetCustomAttributes(typeof(EntityTableNameAttribute), false);
            if (attrs.Length > 0)
            {
                tbName = ((EntityTableNameAttribute)attrs[0]).TableName;
            }
            else
            {
                return null;
            }
            var sb = new StringBuilder();
            sb.AppendFormat("SELECT * FROM [{0}] WHERE {1} ", tbName, filters.And ? "1=1" : "1=2");
            var comm = new SqlCommand();
            foreach (PropertyInfo prop in tp.GetProperties())
            {
                var filter = filters.Get(prop.Name);
                if (filter != null && filter.Value != null)
                {
                    var eca = Attribute.GetCustomAttribute(prop, typeof(EntityColumnAttribute)) as EntityColumnAttribute;
                    var paramName = string.Format("@{0}", prop.Name);
                    string op = " = " + paramName;
                    switch (filter.Operator)
                    {
                        case FilterOperator.Equal:
                            op = " = " + paramName;
                            break;
                        case FilterOperator.EqualOrLess:
                            op = " <= " + paramName;
                            break;
                        case FilterOperator.EqualOrGreater:
                            op = " >= " + paramName;
                            break;
                        case FilterOperator.Like:
                            op = string.Format("LIKE '%'+{0}+'%'", paramName);
                            break;
                        case FilterOperator.StartWith:
                            op = string.Format("LIKE {0}+'%'", paramName);
                            break;
                        case FilterOperator.EndWith:
                            op = string.Format("LIKE '%'+{0}", paramName);
                            break;
                    }
                    if (eca != null && !string.IsNullOrEmpty(eca.Field))
                    {
                        sb.AppendFormat(" {0} [{1}] {2}", filters.And ? "AND" : "OR", eca.Field, op);
                        var param = new SqlParameter(paramName, eca.DataType) { Value = filter.Value };
                        comm.Parameters.Add(param);
                    }
                }
            }
            comm.CommandText = sb.ToString();
            comm.CommandType = CommandType.Text;
            return comm;
        }
        //SQL命令 获取页命令
        public static SqlCommand GetPageCommand<T>(int pageSize, int pageIndex) where T : IBaseEntity
        {
            var tp = typeof(T);
            string tbName;
            var attrs = tp.GetCustomAttributes(typeof(EntityTableNameAttribute), false);
            if (attrs.Length > 0)
            {
                tbName = ((EntityTableNameAttribute)attrs[0]).TableName;
            }
            else
            {
                return null;
            }
            string keyName = string.Empty;

            foreach (PropertyInfo prop in tp.GetProperties())
            {
                var eca = Attribute.GetCustomAttribute(prop, typeof(EntityColumnAttribute)) as EntityColumnAttribute;
                if (eca != null && eca.Key)
                {
                    keyName = eca.Field;
                    break;
                }
            }
            var comm = new SqlCommand();
            comm.CommandType = CommandType.StoredProcedure;
            comm.CommandText = "PROC_GETRECORDBYPAGE";
            comm.Parameters.Add(new SqlParameter("@TableName", SqlDbType.NVarChar) { Value = tbName });
            comm.Parameters.Add(new SqlParameter("@FieldNames", SqlDbType.NVarChar) { Value = string.Empty });
            comm.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
            comm.Parameters.Add(new SqlParameter("@PageIndex", SqlDbType.Int) { Value = pageIndex });
            comm.Parameters.Add(new SqlParameter("@SortFields", SqlDbType.NVarChar) { Value = keyName });
            comm.Parameters.Add(new SqlParameter("@SortDir", SqlDbType.Bit) { Value = true });
            comm.Parameters.Add(new SqlParameter("@Condition", SqlDbType.NVarChar) { Value = string.Empty });
            comm.Parameters.Add(new SqlParameter("@KeyID", SqlDbType.NVarChar) { Value = keyName });
            comm.Parameters.Add(new SqlParameter("@Distinct", SqlDbType.Bit) { Value = false });
            comm.Parameters.Add(new SqlParameter("@PageCount", SqlDbType.Int) { Direction = ParameterDirection.Output });
            comm.Parameters.Add(new SqlParameter("@RecordCounts", SqlDbType.Int) { Direction = ParameterDirection.Output });
            return comm;
        }
        //获取所有实例
        public static List<T> GetAllEntities<T>() where T : IBaseEntity
        {
            return GetAllEntities<T>(false, false);
        }
        //获取所有实例
        public static List<T> GetAllEntities<T>(bool getForeigns, bool getChldren) where T : IBaseEntity
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                var comm = GetSelectCommand<T>();
                comm.Connection = conn;
                conn.Open();
                var list = new List<T>();
                using (SqlDataReader dr = comm.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(GetEntityFromDataReader<T>(dr, getForeigns, getChldren));
                    }
                }
                return list;
            }
        }
        //通过页获取所有实例
        public static List<T> GetEntityByPage<T>(int pageSize, int pageIndex, out int recordCount, out int pageCount) where T : IBaseEntity
        {
            var comm = GetPageCommand<T>(pageSize, pageIndex);
            var list = new List<T>();
            using (var conn = new SqlConnection(ConnectionString))
            {
                comm.Connection = conn;
                conn.Open();
                using (SqlDataReader dr = comm.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(GetEntityFromDataReader<T>(dr));
                    }
                }
            }

            recordCount = Convert.ToInt32(comm.Parameters["@RecordCounts"].Value);
            pageCount = Convert.ToInt32(comm.Parameters["@PageCount"].Value);
            return list;
        }
        //获取单独实例
        public static void GetSingleEntity(object ov, ref IBaseEntity entity)
        {
            GetSingleEntity(ov, ref entity, false, false);
        }
        //获取单独实例
        public static void GetSingleEntity(object ov, ref IBaseEntity entity, bool getForeigns, bool getChldren)
        {
            if (ov == null)
                return;
            var tp = entity.GetType();
            string tbName;
            var attrs = tp.GetCustomAttributes(typeof(EntityTableNameAttribute), false);
            if (attrs.Length > 0)
            {
                tbName = ((EntityTableNameAttribute)attrs[0]).TableName;
            }
            else
            {
                return;
            }

            var sb = new StringBuilder();
            sb.AppendFormat("SELECT * FROM [{0}] WHERE ", tbName);

            var comm = new SqlCommand();

            foreach (PropertyInfo prop in tp.GetProperties())
            {
                var eca = Attribute.GetCustomAttribute(prop, typeof(EntityColumnAttribute)) as EntityColumnAttribute;
                if (eca != null)
                {
                    if ((ov is Int32 && eca.Key && eca.DataType == DbType.Int32) 
                        || (ov is String && (eca.Unique || eca.Key) && (eca.DataType == DbType.String || eca.DataType == DbType.AnsiString))
                        || (ov is Guid && (eca.Unique || eca.Key) && eca.DataType == DbType.Guid))
                    {
                        string paramName = string.Format("@{0}", prop.Name);
                        sb.AppendFormat("[{0}] = {1}", eca.Field, paramName);

                        var param = new SqlParameter(paramName, eca.DataType) { Value = ov };
                        comm.Parameters.Add(param);
                    }
                }
            }
            comm.CommandType = CommandType.Text;
            comm.CommandText = sb.ToString();
            //Trace.WriteLine(ConnectionString);
            using (var conn = new SqlConnection(ConnectionString))
            {
                comm.Connection = conn;
                conn.Open();
                using (var dr = comm.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        GetEntityFromDataReader(dr, ref entity, getForeigns, getChldren);
                    }
                }
            }
        }
        //过滤器
        public static List<T> Filter<T>(EntityFilters filters, bool getForign, bool getChild) where T : IBaseEntity
        {
            if (filters.Count == 0)
            {
                return GetAllEntities<T>();
            }
            else
            {
                var comm = GetFilterCommand<T>(filters);
                if (comm == null)
                    return null;
                var list = new List<T>();
                using (var conn = new SqlConnection(ConnectionString))
                {
                    comm.Connection = conn;
                    conn.Open();
                    using (SqlDataReader dr = comm.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(GetEntityFromDataReader<T>(dr, getForign, getChild));
                        }
                    }
                }
                return list;
            }
        }
        //过滤器
        public static List<T> Filter<T>(EntityFilters filters) where T : IBaseEntity
        {
            return Filter<T>(filters, false, false);
        }
        //通过域搜索
        public static void SearchByField(string field, object key, ref IList list)
        {
            SearchByField(field, key, ref list, false, false);
        }
        //通过域搜索
        public static void SearchByField(string field, object key, ref IList list, bool getForeigns, bool getChildren)
        {
            var tp = list.GetType().GetGenericArguments()[0];
            string tbName;
            var attrs = tp.GetCustomAttributes(typeof(EntityTableNameAttribute), false);
            if (attrs.Length > 0)
            {
                tbName = ((EntityTableNameAttribute)attrs[0]).TableName;
            }
            else
            {
                return;
            }

            var sb = new StringBuilder();
            sb.AppendFormat("SELECT * FROM [{0}] WHERE ", tbName);
            var comm = new SqlCommand();

            var prop = tp.GetProperty(field);
            if (prop != null)
            {
                var eca = Attribute.GetCustomAttribute(prop, typeof(EntityColumnAttribute)) as EntityColumnAttribute;
                if (eca != null && !string.IsNullOrEmpty(eca.Field))
                {
                    var paramName = string.Format("@{0}", prop.Name);
                    sb.AppendFormat(" [{0}] = {1}", eca.Field, paramName);
                    var param = new SqlParameter(paramName, eca.DataType) { Value = key };
                    comm.Parameters.Add(param);
                    comm.CommandType = CommandType.Text;
                    comm.CommandText = sb.ToString();
                    using (var conn = new SqlConnection(ConnectionString))
                    {
                        comm.Connection = conn;
                        conn.Open();
                        using (var dr = comm.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var tmpEntity = Activator.CreateInstance(tp) as IBaseEntity;
                                GetEntityFromDataReader(dr, ref tmpEntity, getForeigns, getChildren);
                                list.Add(tmpEntity);
                            }
                        }
                    }
                }
            }
        }
        //插入
        public static int Insert(IBaseEntity info)
        {
            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    var comm = GetInsertCommand(info);
                    comm.Connection = conn;
                    conn.Open();
                    int n = comm.ExecuteNonQuery();
                    if (comm.Parameters["@ScopeIdentity"].Value != null && comm.Parameters["@ScopeIdentity"].Value != DBNull.Value)
                    {
                        int id = Convert.ToInt32(comm.Parameters["@ScopeIdentity"].Value);
                        info.Identity = id;
                    }
                    return n;
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(ex.Message, MessageBoxImage.Error);
                return -1;
            }
        }
        //更新
        public static int Update(IBaseEntity info)
        {
            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    var comm = GetUpdateCommand(info);
                    comm.Connection = conn;
                    conn.Open();
                    return comm.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(ex.Message, MessageBoxImage.Error);
                return -1;
            }
        }
        //通过域删除
        public static int DeleteByField<T>(string field, object value) where T : IBaseEntity
        {
            var tp = typeof(T);
            string tbName;
            var attrs = tp.GetCustomAttributes(typeof(EntityTableNameAttribute), false);
            if (attrs.Length > 0)
            {
                tbName = ((EntityTableNameAttribute)attrs[0]).TableName;
            }
            else
            {
                return -3;
            }

            var sb = new StringBuilder();
            sb.AppendFormat("DELETE FROM [{0}] WHERE ", tbName);
            var comm = new SqlCommand();

            var prop = tp.GetProperty(field);
            if (prop != null)
            {
                var eca = Attribute.GetCustomAttribute(prop, typeof(EntityColumnAttribute)) as EntityColumnAttribute;
                if (eca != null && !string.IsNullOrEmpty(eca.Field))
                {
                    var paramName = string.Format("@{0}", prop.Name);
                    sb.AppendFormat(" [{0}] = {1}", eca.Field, paramName);
                    var param = new SqlParameter(paramName, eca.DataType) { Value = value };
                    comm.Parameters.Add(param);
                    comm.CommandType = CommandType.Text;
                    comm.CommandText = sb.ToString();
                    using (var conn = new SqlConnection(ConnectionString))
                    {
                        comm.Connection = conn;
                        conn.Open();
                        return comm.ExecuteNonQuery();
                    }
                }
            }
            return -1;
        }
        //通过域更新列
        public static int UpdateColumnByField<T>(string colName, object oValue, string field, object oCondition) where T : IBaseEntity
        {
            var tp = typeof(T);
            string tbName;
            var attrs = tp.GetCustomAttributes(typeof(EntityTableNameAttribute), false);
            if (attrs.Length > 0)
            {
                tbName = ((EntityTableNameAttribute)attrs[0]).TableName;
            }
            else
            {
                return -3;
            }

            var sb = new StringBuilder();
            sb.AppendFormat("UPDATE [{0}] SET ", tbName);
            var comm = new SqlCommand();

            var prop = tp.GetProperty(colName);
            var propCondition = tp.GetProperty(field);
            if (prop != null && propCondition != null)
            {
                var eca = Attribute.GetCustomAttribute(prop, typeof(EntityColumnAttribute)) as EntityColumnAttribute;
                var ecaCondition = Attribute.GetCustomAttribute(propCondition, typeof(EntityColumnAttribute)) as EntityColumnAttribute;
                if (eca != null && !string.IsNullOrEmpty(eca.Field) && ecaCondition != null && !string.IsNullOrEmpty(ecaCondition.Field))
                {
                    var paramName = string.Format("@{0}", prop.Name);
                    sb.AppendFormat(" [{0}] = {1} ", eca.Field, paramName);
                    var param = new SqlParameter(paramName, eca.DataType) { Value = oValue };
                    comm.Parameters.Add(param);

                    paramName = string.Format("@{0}", propCondition.Name);
                    sb.AppendFormat(" WHERE [{0}] = {1} ", ecaCondition.Field, paramName);
                    var paramCondition = new SqlParameter(paramName, ecaCondition.DataType) { Value = oCondition };
                    comm.Parameters.Add(paramCondition);

                    comm.CommandType = CommandType.Text;
                    comm.CommandText = sb.ToString();
                    using (var conn = new SqlConnection(ConnectionString))
                    {
                        comm.Connection = conn;
                        conn.Open();
                        return comm.ExecuteNonQuery();
                    }
                }
            }
            return -1;
        }
        //删除
        public static int Delete(IBaseEntity info)
        {
            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    var comm = GetDeleteCommand(info);
                    comm.Connection = conn;
                    conn.Open();
                    return comm.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(ex.Message, MessageBoxImage.Error);
                return -1;
            }
        }
        //运行存储过程
        public static List<T> ExecuteStoredProcedure<T>(string strProc, List<SqlParameter> parameters) where T : IBaseEntity
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                var comm = conn.CreateCommand();
                comm.CommandText = strProc;
                comm.CommandType = CommandType.StoredProcedure;
                foreach (var p in parameters)
                {
                    comm.Parameters.Add(p);
                }
                conn.Open();
                var list = new List<T>();
                using (var dr = comm.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(GetEntityFromDataReader<T>(dr));
                    }
                }
                return list;
            }
        }
        //运行过程
        public static List<string> ExecuteProcedure(string strProc, SqlParameter param)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                var comm = conn.CreateCommand();
                comm.CommandText = strProc;
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.Add(param);
                conn.Open();
                var list = new List<string>();
                using (var dr = comm.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(dr[0].ToString());
                    }
                }
                return list;
            }
        }
        //从数据读取器获取实例
        public static T GetEntityFromDataReader<T>(SqlDataReader dr) where T : IBaseEntity
        {
            return GetEntityFromDataReader<T>(dr, false, false);
        }
        //从数据读取器获取实例
        public static T GetEntityFromDataReader<T>(SqlDataReader dr, bool getForeigns, bool getChldren) where T : IBaseEntity
        {
            var type = typeof(T);
            var info = Activator.CreateInstance(type);
            var keyName = "Id";
            var memberAccessor = new DynamicMethodMemberAccessor();
            foreach (PropertyInfo property in type.GetProperties())
            {
                var mde = Attribute.GetCustomAttribute(property, typeof(EntityColumnAttribute)) as EntityColumnAttribute;
                if (mde != null)
                {
                    if (mde.Key)
                        keyName = property.Name;

                    if (getForeigns && mde.Foreign && !string.IsNullOrEmpty(mde.ForeignKey))
                    {
                        var fEntity = Activator.CreateInstance(property.PropertyType) as IBaseEntity;
                        if (fEntity != null)
                        {
                            GetSingleEntity(memberAccessor.GetValue(info, mde.ForeignKey), ref fEntity);
                            memberAccessor.SetValue(info, property.Name, fEntity);
                        }
                    }
                    else if (getChldren && mde.Children && !string.IsNullOrEmpty(mde.ChildKey))
                    {
                        var children = Activator.CreateInstance(property.PropertyType) as IList;
                        SearchByField(mde.ChildKey, memberAccessor.GetValue(info, keyName), ref children);
                        memberAccessor.SetValue(info, property.Name, children);
                    }
                    else if (!string.IsNullOrEmpty(mde.Field) && !(dr[mde.Field] is DBNull))
                    {
                        memberAccessor.SetValue(info, property.Name, dr[mde.Field]);
                    }
                }
            }
            return (T)info;
        }
        //从数据读取器获取实例
        public static void GetEntityFromDataReader(SqlDataReader dr, ref IBaseEntity entity)
        {
            GetEntityFromDataReader(dr, ref entity, false, false);
        }
        //从数据读取器获取实例
        public static void GetEntityFromDataReader(SqlDataReader dr, ref IBaseEntity entity, bool getForeigns, bool getChldren)
        {
            var memberAccessor = new DynamicMethodMemberAccessor();
            var keyName = "Id";
            foreach (PropertyInfo property in entity.GetType().GetProperties())
            {
                var mde = Attribute.GetCustomAttribute(property, typeof(EntityColumnAttribute)) as EntityColumnAttribute;
                if (mde != null)
                {
                    if (mde.Key)
                        keyName = property.Name;

                    if (getForeigns && mde.Foreign && !string.IsNullOrEmpty(mde.ForeignKey))
                    {
                        var fEntity = Activator.CreateInstance(property.PropertyType) as IBaseEntity;
                        if (fEntity != null)
                        {
                            GetSingleEntity(memberAccessor.GetValue(entity, mde.ForeignKey), ref fEntity);
                            memberAccessor.SetValue(entity, property.Name, fEntity);
                        }
                    }
                    else if (getChldren && mde.Children && !string.IsNullOrEmpty(mde.ChildKey))
                    {
                        var children = Activator.CreateInstance(property.PropertyType) as IList;
                        SearchByField(mde.ChildKey, memberAccessor.GetValue(entity, keyName), ref children);
                        memberAccessor.SetValue(entity, property.Name, children);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(mde.Field) && !(dr[mde.Field] is DBNull))
                            memberAccessor.SetValue(entity, property.Name, dr[mde.Field]);
                    }
                }
            }
        }
    }
}
