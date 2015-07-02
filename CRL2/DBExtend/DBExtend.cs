using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Data.Common;

namespace CRL
{
    /// <summary>
    /// 对象数据访问
    /// </summary>
    public partial class DBExtend
    {
        internal bool OnUpdateNotifyCacheServer = false;
        internal Guid GUID;
        /// <summary>
        /// 构造DBExtend
        /// </summary>
        /// <param name="_helper"></param>
        public DBExtend(CoreHelper.DBHelper _helper)
        {
            if (_helper == null)
            {
                throw new Exception("数据访问对象未实例化,请实现CRL.SettingConfig.GetDbAccess");
            }
            GUID = Guid.NewGuid();
            helper = _helper;
        }

        CoreHelper.DBHelper helper;
        internal string DatabaseName
        {
            get
            {
                return helper.DatabaseName;
            }
        }
        DBAdapter.DBAdapterBase __DBAdapter;
        /// <summary>
        /// 当前数据库适配器
        /// </summary>
        internal DBAdapter.DBAdapterBase _DBAdapter
        {
            get
            {
                //return Base.CurrentDBAdapter;
                if (__DBAdapter == null)
                {
                    __DBAdapter = DBAdapter.DBAdapterBase.GetDBAdapterBase(helper.CurrentDBType);
                }
                return __DBAdapter;
            }
        }

        static object lockObj = new object();
        public void ClearParams()
        {
            helper.ClearParams();
        }
        /// <summary>
        /// 增加参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddParam(string name,object value)
        {
            value = ObjectConvert.SetNullValue(value);
            helper.AddParam(name,value);
        }
        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetParam(string name, object value)
        {
            value = ObjectConvert.SetNullValue(value);
            helper.SetParam(name, value);
        }
        /// <summary>
        /// 增加输出参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value">对应类型任意值</param>
        public void AddOutParam(string name, object value = null)
        {
            helper.AddOutParam(name, value);
        }
        /// <summary>
        /// 获取存储过程return的值
        /// </summary>
        /// <returns></returns>
        public int GetReturnValue()
        {
            return helper.GetReturnValue();
        }
        /// <summary>
        /// 获取OUTPUT的值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object GetOutParam(string name)
        {
            return helper.GetOutParam(name);
        }
        public T GetOutParam<T>(string name)
        {
            var obj = helper.GetOutParam(name);
            return ObjectConvert.ConvertObject<T>(obj);
        }
        void ClearParame()
        {
            helper.ClearParams();
        }
        Dictionary<string, object> ParamsBackup;
        Dictionary<string, object> OutParamsBackup;
        /// <summary>
        /// 备份参数
        /// </summary>
        void BackupParams()
        {
            ParamsBackup = new Dictionary<string, object>(helper.Params);
            OutParamsBackup = new Dictionary<string, object>(helper.OutParams);
            helper.ClearParams();
        }
        /// <summary>
        /// 还原参数
        /// </summary>
        void RecoveryParams()
        {
            helper.Params = new Dictionary<string, object>(ParamsBackup);
            helper.OutParams = new Dictionary<string, object>(OutParamsBackup);
            ParamsBackup = null;
            OutParamsBackup = null;
        }
        void CheckData(IModel obj)
        {
            var types = CRL.TypeCache.GetProperties(obj.GetType(), true).Values;
            string msg;
            //检测数据约束
            foreach (Attribute.FieldAttribute p in types)
            {
                if (p.PropertyType == typeof(System.String))
                {
                    string value = p.GetValue(obj) + "";
                    if (p.NotNull && string.IsNullOrEmpty(value))
                    {
                        msg = string.Format("对象{0}属性{1}值不能为空", obj.GetType(), p.Name);
                        throw new Exception(msg);
                    }
                    if (value.Length > p.Length && p.Length < 3000)
                    {
                        msg = string.Format("对象{0}属性{1}长度超过了设定值{2}", obj.GetType(), p.Name, p.Length);
                        throw new Exception(msg);
                    }
                }
            }
            //校验数据
            msg = obj.CheckData();
            if (!string.IsNullOrEmpty(msg))
            {
                msg = string.Format("数据校验证失败,在类型{0} {1} 请核对校验规则", obj.GetType(), msg);
                throw new Exception(msg);
            }
        }

        #region 更新缓存中的一项

        /// <summary>
        /// 按表达式更新缓存中项
        /// 当前类型有缓存时才会进行查询
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="expression"></param>
        void UpdateCacheItem<TItem>(Expression<Func<TItem, bool>> expression, ParameCollection c) where TItem : IModel, new()
        {
            //事务开启不执行查询
            if (currentTranStatus == TranStatus.已开始)
            {
                return;
            }
            Type type = typeof(TItem);
            var updateModel = MemoryDataCache.GetCacheTypeKey(typeof(TItem));
            foreach (var key in updateModel)
            {
                var list = QueryList<TItem>(expression);
                foreach (var item in list)
                {
                    MemoryDataCache.UpdateCacheItem(key, item, c);
                    NotifyCacheServer(item);
                }
            }
        }
        /// <summary>
        /// 更新缓存中的一项
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="newObj"></param>
        /// <param name="c"></param>
        internal void UpdateCacheItem<TItem>(TItem newObj, ParameCollection c) where TItem : IModel
        {
            var updateModel = MemoryDataCache.GetCacheTypeKey(typeof(TItem));
            foreach (var key in updateModel)
            {
                MemoryDataCache.UpdateCacheItem(key, newObj, c);
            }
            NotifyCacheServer(newObj);
            //Type type = typeof(TItem);
            //if (TypeCache.ModelKeyCache.ContainsKey(type))
            //{
            //    string key = TypeCache.ModelKeyCache[type];
            //    MemoryDataCache.UpdateCacheItem(key,newObj);
            //}
        }
        void NotifyCacheServer<TItem>(TItem newObj) where TItem : IModel
        {
            if (!OnUpdateNotifyCacheServer)
                return;
            var client = CacheServerSetting.GetCurrentClient(typeof(TItem));
            if (client != null)
            {
                client.Update(newObj);
            }
        }
        #endregion
        /// <summary>
        /// 格式化为更新值查询
        /// </summary>
        /// <param name="setValue"></param>
        /// <returns></returns>
        string ForamtSetValue<T>(ParameCollection setValue) where T : IModel
        {
            string tableName = TypeCache.GetTableName(typeof(T));
            string setString = "";
            foreach (var pair in setValue)
            {
                string name = pair.Key;
                object value = pair.Value;
                value = ObjectConvert.SetNullValue(value);
                if (name.StartsWith("$"))//直接按值拼接 c2["$SoldCount"] = "SoldCount+" + num;
                {
                    name = name.Substring(1, name.Length - 1);
                    setString += string.Format(" {0}={1},", name, value);
                }
                else
                {
                    setString += string.Format(" {0}=@{0},", name);
                    helper.AddParam(name, value);
                }
            }
            setString = setString.Substring(0, setString.Length - 1);
            return setString;
        }
        /// <summary>
        /// 通过关键类型,格式化SQL语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="args">格式化SQL语句的关键类型</param>
        /// <returns></returns>
        string AutoFormat(string sql, params Type[] args)
        {
            if (args == null)
            {
                return sql;
            }
            if (args.Length == 0)
            {
                return sql;
            }
            //System.Object
            Regex r = new Regex(@"\$(\w+)", RegexOptions.IgnoreCase);//like $table
            Match m;
            List<string> pars = new List<string>();
            for (m = r.Match(sql); m.Success; m = m.NextMatch())
            {
                string par = m.Groups[1].ToString();
                if (!pars.Contains(par))
                {
                    pars.Add(par);
                }
            }
            foreach (string par in pars)
            {
                foreach (Type type in args)
                {
                    string tableName = TypeCache.GetTableName(type);
                    //string fullTypeName = type.FullName.Replace("+", ".") + ".";//like classA+classB
                    string fullTypeName = GetTypeFullName(type);
                    if (fullTypeName.IndexOf("." + par + ".") > -1)
                    {
                        sql = sql.Replace("$" + par, tableName);
                    }
                }
            }
            if (sql.IndexOf("$") > -1)
            {
                throw new Exception("格式化SQL语句时发生错误,表名未被替换:" + sql);
            }
            return sql;
        }
        static string GetTypeFullName(Type type)
        {
            string str = "";
            while (type != typeof(IModel))
            {
                str += "." + type.FullName + ".;";
                type = type.BaseType;
            }
            return str;
        }

        #region 指定对象类型
        /// <summary>
        /// 指定替换对象查询,并返回对象列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public List<T> ExecList<T>(string sql, params Type[] types) where T : class, new()
        {
            sql = _DBAdapter.SqlFormat(sql);
            var reader = GetDataReader(sql, types);
            return ObjectConvert.DataReaderToList<T>(reader);
        }
        
        DbDataReader GetDataReader(string sql, params Type[] types)
        {
            sql = AutoFormat(sql, types);
            sql = _DBAdapter.SqlFormat(sql);
            var  reader = helper.ExecDataReader(sql);
            ClearParame();
            return reader;
        }
        public Dictionary<TKey, TValue> ExecDictionary<TKey, TValue>(string sql, params Type[] types)
        {
            var reader = GetDataReader(sql, types);
            return ObjectConvert.DataReadToDictionary<TKey, TValue>(reader);
        }

        /// <summary>
        /// 指定替换对象更新
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public int Execute(string sql, params Type[] types)
        {
            sql = AutoFormat(sql, types);
            sql = _DBAdapter.SqlFormat(sql);
            int count = helper.Execute(sql);
            ClearParame();
            return count;
        }
        /// <summary>
        /// 指定替换对象返回单个结果
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="types">格式化SQL语句的关键类型</param>
        /// <returns></returns>
        public object ExecScalar(string sql, params Type[] types)
        {
            sql = AutoFormat(sql, types);
            sql = _DBAdapter.SqlFormat(sql);
            object obj = helper.ExecScalar(sql);
            ClearParame();
            return obj;
        }
        /// <summary>
        /// 指定替换对象返回单个结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public T ExecScalar<T>(string sql, params Type[] types)
        {
            sql = _DBAdapter.SqlFormat(sql);
            var obj = ExecScalar(sql, types);
            return ObjectConvert.ConvertObject<T>(obj);
        }
        /// <summary>
        /// 返回首个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public T ExecObject<T>(string sql, params Type[] types) where T : class, new()
        {
            var list = ExecList<T>(sql, types);
            if (list.Count == 0)
                return null;
            return list[0];
        }
        #endregion

        #region 存储过程
        /// <summary>
        /// 执行存储过程返回结果集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sp"></param>
        /// <returns></returns>
        public List<T> RunList<T>(string sp) where T : class, new()
        {
            var reader = helper.RunDataReader(sp);
            ClearParame();
            return ObjectConvert.DataReaderToList<T>(reader);
        }
        /// <summary>
        /// 执行一个存储过程
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        public int Run(string sp)
        {
            int count = helper.Run(sp);
            ClearParame();
            return count;
        }
        /// <summary>
        /// 返回首个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sp"></param>
        /// <returns></returns>
        public T RunObject<T>(string sp) where T : class, new()
        {
            var list = RunList<T>(sp);
            if (list.Count == 0)
                return null;
            return list[0];
        }
        /// <summary>
        /// 执行存储过程并返回结果
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        public object RunScalar(string sp)
        {
            object obj = helper.RunScalar(sp);
            ClearParame();
            return obj;
        }

        #endregion

        #region 事务控制
        enum TranStatus
        {
            未开始,
            已开始
        }
        TranStatus currentTranStatus = TranStatus.未开始;
        /// <summary>
        /// 开始物务
        /// </summary>
        public void BeginTran()
        {
            if (currentTranStatus != TranStatus.未开始)
            {
                throw new Exception("事务开始失败,已有未完成的事务");
            }
            helper.BeginTran();
            currentTranStatus = TranStatus.已开始;
        }
        /// <summary>
        /// 回滚事务
        /// </summary>
        public void RollbackTran()
        {
            if (currentTranStatus != TranStatus.已开始)
            {
                throw new Exception("事务回滚失败,没有需要回滚的事务");
            }
            helper.RollbackTran();
            currentTranStatus = TranStatus.未开始;
        }
        /// <summary>
        /// 提交事务
        /// </summary>
        public void CommitTran()
        {
            if (currentTranStatus != TranStatus.已开始)
            {
                throw new Exception("事务提交失败,没有需要提交的事务");
            }
            helper.CommitTran();
            currentTranStatus = TranStatus.未开始;
        }
        #endregion

        #region 检查表是否被创建
        internal void CheckTableCreated<T>() where T : IModel, new()
        {
            CheckTableCreated(typeof(T));
        }
        //static Dictionary<string, List<string>> tableCache = new Dictionary<string, List<string>>();
        /// <summary>
        /// 检查表是否被创建
        /// </summary>
       internal void CheckTableCreated(Type type)
        {
            TypeCache.SetDBAdapterCache(type, _DBAdapter);
            var dbName = helper.DatabaseName;
            var cacheInstance = ExistsTableCache.Instance;
            var table = TypeCache.GetTable(type);
            if (!cacheInstance.Tables.ContainsKey(dbName))
            {
                #region 初始表
                lock (lockObj)
                {
                    BackupParams();
                    string sql = _DBAdapter.GetAllTablesSql(helper);
                    var dic = ExecDictionary<string, int>(sql);
                    RecoveryParams();
                    cacheInstance.InitTable(dbName, dic.Keys.ToList());
                }
                #endregion
            }
            var tb = cacheInstance.GetTable(dbName, table);
            if (tb == null)//没有创建表
            {
                #region 创建表
                BackupParams();
                var obj = System.Activator.CreateInstance(type) as IModel;
                string msg;
                var a = obj.CreateTable(this, out msg);
                RecoveryParams();
                if (!a)
                {
                    return;
                    throw new Exception(msg);
                }
                cacheInstance.SaveTable(dbName, table);
                return;
                #endregion
            }
            if (tb.ColumnChecked)
            {
                return;
            }
            //判断字段是否一致
            var needCreates = ExistsTableCache.Instance.CheckFieldExists(dbName, table);
            if (needCreates.Count > 0)
            {
                #region 创建列
                BackupParams();
                var mapping = _DBAdapter.GetFieldMapping();
                foreach (var item in needCreates)
                {
                    IModel.SetColumnDbType(_DBAdapter, item, mapping);
                    string str = IModel.CreateColumn(this, item);
                }
                RecoveryParams();
                #endregion
            }
            
        }
        #endregion
    }
}
