using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq.Expressions;

namespace CRL
{
    /// <summary>
    /// 请实现dbHelper,和单例对象Instance
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public abstract class BaseProvider<TModel> 
        where TModel : IModel, new()
    {
        /// <summary>
        /// 对象被更新时,是否通知缓存服务器
        /// 在业务类中进行控制
        /// </summary>
        protected virtual bool OnUpdateNotifyCacheServer
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// 是否从远程查询缓存
        /// </summary>
        protected virtual bool QueryCacheFromRemote
        {
            get
            {
                return false;
            }
        }
        DBExtend _dbHelper;
        /// <summary>
        /// 数据访部对象
        /// 当前实例内只会创建一个,查询除外
        /// </summary>
        protected virtual DBExtend dbHelper
        {
            get
            {
                if (_dbHelper == null)
                {
                    _dbHelper = GetDbHelper(GetType());
                }
                return _dbHelper;
            }
            set
            {
                _dbHelper = value;
            }
        }
        internal DBExtend GetCurrentDBExtend()
        {
            return dbHelper;
        }
        /// <summary>
        /// 设置当前数据访问上下文,不能跨库
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="baseProvider"></param>
        protected void SetContext<T>(BaseProvider<T> baseProvider) where T : IModel, new()
        {
            var source = baseProvider.GetCurrentDBExtend();
            if (source.DatabaseName != dbHelper.DatabaseName)
            {
                throw new Exception(string.Format("不能跨库设置数据上下文访问,当前:{0} 设置为:{1}", this, baseProvider));
            }
            dbHelper = source;
        }
        /// <summary>
        /// lockObj
        /// </summary>
        protected static object lockObj = new object();
        /// <summary>
        /// 数据访问对象[基本方法]
        /// 按传入的类型
        /// </summary>
        protected DBExtend GetDbHelper<TType>() where TType : class
        {
            return GetDbHelper(typeof(TType));
        }
        DBExtend GetDbHelper() 
        {
            return GetDbHelper(GetType());
        }
        /// <summary>
        /// 数据访问对象[基本方法]
        /// 按指定的类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected DBExtend GetDbHelper(Type type)
        {
            if (SettingConfig.GetDbAccess == null)
            {
                throw new Exception("请配置CRL数据访问对象,实现CRL.SettingConfig.GetDbAccess");
            }
            CoreHelper.DBHelper helper = SettingConfig.GetDbAccess(type);
            var db= new DBExtend(helper);
            db.OnUpdateNotifyCacheServer = OnUpdateNotifyCacheServer;
            TypeCache.SetDBAdapterCache(typeof(TModel), db._DBAdapter);
            return db;
        }
        
        public LambdaQuery<TModel> GetLamadaQuery()
        {
            return GetLambdaQuery();
        }
        /// <summary>
        /// 创建当前类型查询表达式实列
        /// </summary>
        /// <returns></returns>
        public LambdaQuery<TModel> GetLambdaQuery()
        {
            var helper = GetDbHelper(GetType());//避开事务控制,使用新的连接
            var query = new LambdaQuery<TModel>(helper);
            return query;
        }
        #region 创建缓存
        /// <summary>
        /// 按类型清除当前所有缓存
        /// </summary>
        public void ClearCache() 
        {
            Type type = typeof(TModel);
            if (TypeCache.ModelKeyCache.ContainsKey(type))
            {
                CRL.MemoryDataCache.RemoveCache(TypeCache.ModelKeyCache[type]);
                TypeCache.ModelKeyCache.Remove(type);
            }
        }
        /// <summary>
        /// 缓存默认查询
        /// </summary>
        /// <returns></returns>
        protected virtual LambdaQuery<TModel> CacheQuery()
        {
            return GetLamadaQuery();
        }
        /// <summary>
        /// 获取当前对象缓存,不指定条件
        /// 可重写
        /// </summary>
        public IEnumerable<TModel> AllCache
        {
            get
            {
                var query = CacheQuery();
                return GetCache(query).Values;
            }
        }
        #region 查询分布式缓存
        #region 客户端
        List<TModel> QueryFromCacheServer(Expression<Func<TModel, bool>> expression, out int total, int pageIndex = 0, int pageSize = 0)
        {
            var proxy = CacheServerSetting.GetCurrentClient(typeof(TModel));
            if (proxy == null)
            {
                throw new Exception("未在服务器上找到对应的数据处理类型:" + typeof(TModel).FullName);
            }
            var data = proxy.Query(expression, out total, pageIndex, pageSize);
            return data;
        }
        #endregion

        #region 服务端
        /// <summary>
        /// 查询命令处理
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public CacheServer.ResultData DeaCacheCommand(CacheServer.Command command)
        {
            if (command.CommandType == CacheServer.CommandType.查询)
            {
                var expression = LambdaQuery.CRLQueryExpression.FromJson(command.Data);
                return QueryFromAllCache(expression);
            }
            else
            {
                //更新缓存
                var item = (TModel)CoreHelper.SerializeHelper.SerializerFromJSON(command.Data, typeof(TModel), Encoding.UTF8);
                var updateModel = MemoryDataCache.GetCacheTypeKey(typeof(TModel));
                foreach (var key in updateModel)
                {
                    MemoryDataCache.UpdateCacheItem(key, item, null);
                }
                return new CacheServer.ResultData();
            }
        }
        /// <summary>
        /// 使用CRLExpression从缓存中查询
        /// 仅在缓存接口部署
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        CacheServer.ResultData QueryFromAllCache(LambdaQuery.CRLQueryExpression expression)
        {
            var _CRLExpression = new CRL.LambdaQuery.CRLExpressionVisitor<TModel>().CreateLambda(expression.Expression);
            int total;
            var data = QueryFromAllCacheBase(_CRLExpression, out total, expression.PageIndex, expression.PageSize);
            return new CacheServer.ResultData() { Total = total, JsonData = CoreHelper.StringHelper.SerializerToJson(data) };
        }
        #endregion
        #endregion
        /// <summary>
        /// 从对象缓存中进行查询
        /// 如果QueryCacheFromRemote为true,则从远端查询
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public List<TModel> QueryFromAllCache(Expression<Func<TModel, bool>> expression)
        {
            int total;
            return QueryFromAllCache(expression, out total, 0, 0);
        }
        
        /// <summary>
        /// 从对象缓存中进行查询
        /// 如果QueryCacheFromRemote为true,则从远端查询
        /// 返回一项
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public TModel QueryItemFromAllCache(Expression<Func<TModel, bool>> expression)
        {
            int total;
            int pageIndex = 0;
            int pageSize = 0;
            if (QueryCacheFromRemote)
            {
                pageIndex = 1;
                pageSize = 1;
            }
            var list = QueryFromAllCache(expression, out total, pageIndex, pageSize);
            if (list.Count == 0)
                return null;
            return list[0];
        }
        /// <summary>
        /// 从对象缓存中进行查询
        /// 如果QueryCacheFromRemote为true,则从远端查询
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="total"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<TModel> QueryFromAllCache(Expression<Func<TModel, bool>> expression, out int total, int pageIndex = 0, int pageSize = 0)
        {
            total = 0;
            if (QueryCacheFromRemote)
            {
                return QueryFromCacheServer(expression, out total, pageIndex, pageSize);
            }
            return QueryFromAllCacheBase(expression, out total, pageIndex, pageSize);
        }
        List<TModel> QueryFromAllCacheBase(Expression<Func<TModel, bool>> expression, out int total, int pageIndex = 0, int pageSize = 0)
        {
            total = 0;
            #region 按KEY查找
            if (expression.Body is BinaryExpression)
            {
                var binary = expression.Body as BinaryExpression;
                if (binary.NodeType == ExpressionType.Equal)
                {
                    if (binary.Left is MemberExpression)
                    {
                        var member = binary.Left as MemberExpression;
                        var primaryKey = TypeCache.GetTable(typeof(TModel)).PrimaryKey.Name;
                        if (member.Member.Name.ToUpper() == primaryKey.ToUpper())
                        {
                            var value = (int)Expression.Lambda(binary.Right).Compile().DynamicInvoke();
                            var all = GetCache(CacheQuery());
                            if (all.ContainsKey(value))
                            {
                                total = 1;
                                return new List<TModel>() { all[value] };
                            }
                            return new List<TModel>();
                        }
                    }
                }
            }
            #endregion
            var predicate = expression.Compile();
            var data = AllCache.Where(predicate);
            total = data.Count();
            if (pageIndex > 0)
            {
                var data2 = Base.CutList(data, pageIndex, pageSize);
                return data2;
            }
            return data.ToList();
        }
        /// <summary>
        /// 按类型获取缓存,只能在继承类实现,只能同时有一个类型
        /// 不建议直接调用,请调用AllCache或重写调用
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected Dictionary<int, TModel> GetCache(LambdaQuery<TModel> query)
        {
            Type type = typeof(TModel);
            int expMinute = query.ExpireMinute;
            if (expMinute == 0)
                expMinute = 5;
            string dataCacheKey;
            var list = new Dictionary<int, TModel>();
            if (!TypeCache.ModelKeyCache.ContainsKey(type))
            {
                var helper = GetDbHelper(GetType());//避开事务控制,使用新的连接
                var list2 = helper.QueryList<TModel>(query, expMinute, out dataCacheKey);
                list = MemoryDataCache.ConvertToDictionary<TModel>(list2);
                lock (lockObj)
                {
                    if (!TypeCache.ModelKeyCache.ContainsKey(type))
                    {
                        TypeCache.ModelKeyCache.Add(type, dataCacheKey);
                    }
                }
            }
            else
            {
                dataCacheKey = TypeCache.ModelKeyCache[type];
                list = MemoryDataCache.GetCacheItem<TModel>(dataCacheKey);
            }
            return list;
        }
        #endregion

        /// <summary>
        /// 创建TABLE[基本方法]
        /// </summary>
        /// <returns></returns>
        public string CreateTable() 
        {
            DBExtend helper = dbHelper;
            TModel obj1 = new TModel();
            return obj1.CreateTable(helper);
        }
        /// <summary>
        /// 创建表索引
        /// </summary>
        public void CreateTableIndex()
        {
            DBExtend helper = dbHelper;
            TModel obj1 = new TModel();
            obj1.CheckIndexExists(helper);
        }
        /// <summary>
        /// 写日志[基本方法]
        /// </summary>
        /// <param name="message"></param>
        public void Log(string message)
        {
            CoreHelper.EventLog.Log(message, "CRL", false);
        }
        

        /// <summary>
        /// 添加一条记录[基本方法]
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public int Add(TModel p)
        {
            DBExtend helper = dbHelper;
            int id = helper.InsertFromObj(p);
            var field = TypeCache.GetTable(p.GetType()).PrimaryKey;
            field.SetValue(p, id);
            return id;
        }
        /// <summary>
        /// 按条件取单个记录[基本方法]
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="idDest">是否按主键倒序</param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public TModel QueryItem(Expression<Func<TModel, bool>> expression, bool idDest = true, bool compileSp = false)
        {
            DBExtend helper = dbHelper;
            return helper.QueryItem<TModel>(expression, idDest, compileSp);
        }
        /// <summary>
        /// 按主键删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int DeleteById(int id)
        {
            return Delete(id);
        }
        /// <summary>
        /// 按主键删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int Delete(int id)
        {
            var helper = dbHelper;
            var filed = TypeCache.GetTable(typeof(TModel)).PrimaryKey;
            string where = string.Format("{0}=@{0}", filed.Name);
            helper.AddParam(filed.Name, id);
            return helper.Delete<TModel>(where);
        }

        /// <summary>
        /// 按条件删除[基本方法]
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public int Delete(Expression<Func<TModel, bool>> expression)
        {
            DBExtend helper = dbHelper;
            int n = helper.Delete<TModel>(expression);
            return n;
        }
        /// <summary>
        /// 返回全部结果[基本方法]
        /// </summary>
        /// <returns></returns>
        public List<TModel> QueryList() 
        {
            DBExtend helper = GetDbHelper(GetType());//避开事务控制,使用新的连接
            return helper.QueryList<TModel>();
        }
        /// <summary>
        /// 指定条件查询[基本方法]
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public List<TModel> QueryList(Expression<Func<TModel, bool>> expression, bool compileSp = false)
        {
            DBExtend helper = GetDbHelper(GetType());//避开事务控制,使用新的连接
            return helper.QueryList<TModel>(expression, compileSp);
        }
        /// <summary>
        /// 指定条件查询[基本方法]
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cacheTime"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public List<TModel> QueryList(LambdaQuery<TModel> query, int cacheTime = 0, bool compileSp = false)
        {
            DBExtend helper = GetDbHelper(GetType());//避开事务控制,使用新的连接
            return helper.QueryList<TModel>(query, cacheTime, compileSp);
        }
        /// <summary>
        /// 自带存储过程分页查询[基本方法]
        /// </summary>
        /// <param name="parame"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<TModel> QueryListByPage(ParameCollection parame, out int count) 
        {
            DBExtend helper = dbHelper;
            return helper.QueryListByPage<TModel>(parame, out count);
        }
        /// <summary>
        /// 动态存储过程分页
        /// </summary>
        /// <param name="query"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<TModel> AutoSpPage(CRL.LambdaQuery<TModel> query, out int count)
        {
            DBExtend helper = dbHelper;
            return helper.AutoSpPage(query, out count);
        }
        /// <summary>
        /// 按ID整体更新[基本方法]
        /// </summary>
        /// <param name="item"></param>
        public void UpdateById(TModel item)
        {
            DBExtend helper = dbHelper;
            helper.UpdateById<TModel>(item);
        }
        /// <summary>
        /// 按对象差异更新,对象需由查询创建
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int Update(TModel item)
        {
            DBExtend helper = dbHelper;
            return helper.Update(item);
        }

        /// <summary>
        /// 指定条件并按对象差异更新
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update(Expression<Func<TModel, bool>> expression, TModel model)
        {
            DBExtend helper = dbHelper;
            int n = helper.Update<TModel>(expression, model);
            return n;
        }
        /// <summary>
        /// 指定条件和参数进行更新[基本方法]
        /// </summary>
        /// <param name="expression">条件</param>
        /// <param name="setValue">值</param>
        /// <returns></returns>
        public int Update(Expression<Func<TModel, bool>> expression, ParameCollection setValue)
        {
            DBExtend helper = dbHelper;
            int n = helper.Update<TModel>(expression, setValue);
            return n;
        }
       /// <summary>
        /// 批量插入[基本方法]
       /// </summary>
       /// <param name="list"></param>
       /// <param name="keepIdentity">是否保持自增主键</param>
        public void BatchInsert(List<TModel> list, bool keepIdentity = false)
        {
            DBExtend helper = dbHelper;
            helper.BatchInsert(list, keepIdentity);
        }
        #region 格式化命令查询

        /// <summary>
        /// 指定格式化查询列表[基本方法]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parame"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        protected List<T> ExecListWithFormat<T>(string sql, ParameCollection parame, params Type[] types) where T : class, new()
        {
            DBExtend helper = dbHelper;
            foreach (var p in parame)
            {
                helper.AddParam(p.Key, p.Value);
            }
            return helper.ExecList<T>(sql, types);
        }
        /// <summary>
        /// 指定格式化更新[基本方法]
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parame"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        protected int ExecuteWithFormat(string sql, ParameCollection parame, params Type[] types)
        {
            DBExtend helper = dbHelper;
            foreach (var p in parame)
            {
                helper.AddParam(p.Key, p.Value);
            }
            return helper.Execute(sql, types);
        }
        /// <summary>
        /// 指定格式化返回单个结果[基本方法]
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parame"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        protected T ExecScalarWithFormat<T>(string sql, ParameCollection parame, params Type[] types)
        {
            DBExtend helper = dbHelper;
            foreach (var p in parame)
            {
                helper.AddParam(p.Key, p.Value);
            }
            return helper.ExecScalar<T>(sql, types);
        }

        #endregion

        #region 存储过程
        /// <summary>
        /// 执行存储过程返回结果集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sp"></param>
        /// <returns></returns>
        protected List<T> RunList<T>(string sp) where T : class, new()
        {
            DBExtend helper = dbHelper;
            return helper.RunList<T>(sp);
        }
        /// <summary>
        /// 对GROUP进行分页
        /// </summary>
        /// <param name="query">包含了group的查询</param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<dynamic> AutoSpGroupPage(LambdaQuery<TModel> query, out int count)
        {
            count = 0;
            DBExtend helper = dbHelper;
            return helper.AutoSpGroupPage(query,out count);
        }
        #endregion

        #region 导入导出
        /// <summary>
        /// 导出为XML
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public string ExportToXml(Expression<Func<TModel, bool>> expression)
        {
            var list = QueryList(expression);
            var xml = CoreHelper.SerializeHelper.XmlSerialize(list, Encoding.UTF8);
            return xml;
        }
        /// <summary>
        /// 导出到文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public int ExportToFile(string file, Expression<Func<TModel, bool>> expression)
        {
            var list = QueryList(expression);
            System.IO.File.Delete(file);
            CoreHelper.SerializeHelper.XmlSerialize(list, file);
            return list.Count;
        }
        /// <summary>
        /// 从XML导入
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="delExpression">要删除的数据</param>
        /// <param name="keepIdentity">是否保留自增主键</param>
        /// <returns></returns>
        public int ImportFromXml(string xml, Expression<Func<TModel, bool>> delExpression, bool keepIdentity = false)
        {
            var obj = CoreHelper.SerializeHelper.XmlDeserialize<List<TModel>>(xml, Encoding.UTF8);
            Delete(delExpression);
            BatchInsert(obj, keepIdentity);
            return obj.Count;
        }
        /// <summary>
        /// 从XML文件导入
        /// </summary>
        /// <param name="file"></param>
        /// <param name="delExpression">要删除的数据</param>
        /// <param name="keepIdentity">是否保留自增主键</param>
        /// <returns></returns>
        public int ImportFromFile(string file, Expression<Func<TModel, bool>> delExpression, bool keepIdentity = false)
        {
            var obj = CoreHelper.SerializeHelper.XmlDeserialize<List<TModel>>(file);
            if (obj.Count == 0)
                return 0;
            Delete(delExpression);
            BatchInsert(obj, keepIdentity);
            return obj.Count;
        }
        #endregion

        #region 统计
        /// <summary>
        /// 统计
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public int Count(Expression<Func<TModel, bool>> expression, bool compileSp = false) 
        {
            DBExtend helper = GetDbHelper(GetType());//避开事务控制,使用新的连接
            return helper.Count<TModel>(expression, compileSp);
        }
        /// <summary>
        /// sum 按表达式指定字段
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="expression"></param>
        /// <param name="field"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public TType Sum<TType>(Expression<Func<TModel, bool>> expression, Expression<Func<TModel, TType>> field, bool compileSp = false)
        {
            DBExtend helper = GetDbHelper(GetType());//避开事务控制,使用新的连接
            return helper.Sum<TType, TModel>(expression, field, compileSp);
        }
        /// <summary>
        /// 取最大值
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="expression"></param>
        /// <param name="field"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public TType Max<TType>(Expression<Func<TModel, bool>> expression, Expression<Func<TModel, TType>> field, bool compileSp = false)
        {
            DBExtend helper = GetDbHelper(GetType());//避开事务控制,使用新的连接
            return helper.Max<TType, TModel>(expression, field, compileSp);
        }
        /// <summary>
        /// 取最小值
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="expression"></param>
        /// <param name="field"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public TType Min<TType>(Expression<Func<TModel, bool>> expression, Expression<Func<TModel, TType>> field, bool compileSp = false)
        {
            DBExtend helper = GetDbHelper(GetType());//避开事务控制,使用新的连接
            return helper.Min<TType, TModel>(expression, field, compileSp);
        }
        #endregion

        /// <summary>
        /// 返回动态对象的查询
        /// 已支持关联,见LeftJoin方法
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cacheTime"></param>
        /// <param name="compileSp"></param>
        /// <returns></returns>
        public List<dynamic> QueryDynamic(LambdaQuery<TModel> query, int cacheTime=0, bool compileSp = false)
        {
            DBExtend helper = dbHelper;
            return helper.QueryDynamic<TModel>(query, cacheTime, compileSp);
        }
    }
}
