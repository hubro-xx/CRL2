using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CRL;
namespace WebTest.Code
{
    /// <summary>
    /// ProductData业务处理类
    /// 这里实现处理逻辑
    /// </summary>
    public class ProductDataManage : CRL.BaseProvider<ProductData>
    {
        protected override bool QueryCacheFromRemote
        {
            get
            {
                return true ;
            }
        }
        /// <summary>
        /// 实现会话实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="baseProvider"></param>
        /// <returns></returns>
        public static ProductDataManage ContextInstance<T>(CRL.BaseProvider<T> baseProvider) where T : CRL.IModel, new()
        {
            var instance = Instance;
            instance.SetContext(baseProvider);
            return instance;
        }
       
        /// <summary>
        /// 实例访问入口
        /// </summary>
        public static ProductDataManage Instance
        {
            get { return new ProductDataManage(); }
        }
        /// <summary>
        /// 数据查询实现
        /// </summary>
        protected override CRL.DBExtend dbHelper
        {
            get { return GetDbHelper(this.GetType()); }
        }
        protected override LambdaQuery<ProductData> CacheQuery()
        {
            return base.CacheQuery().Where(b => b.Id < 1000);
        }
        public List<ProductData> QueryDayProduct(DateTime date)
        {
            var helper = dbHelper;
            string sql = "select * from ProductData where datediff(d,addtime,@date)=0";
            helper.AddParam("date", date);
            return helper.AutoSpQuery<ProductData>(sql);
            //其它数据结果参见Auto开头的其它方法
        }
        public List<ProductData> SpPageQuery(DateTime date,string user)
        {
            var query = GetLamadaQuery();
            //DateDiff为CRL里的扩展方法
            query = query.Where(b => b.AddTime.DateDiff(CRL.DatePart.dd, date) == 0);
            if (!string.IsNullOrEmpty(user))
            {
                query = query.Where(b => b.InterFaceUser == user);
            }
            int pageSize = 15;
            int pageIndex = 1;
            int count;
            query = query.Page(pageSize, pageIndex);//设定分页参数
            query = query.OrderBy(b => b.Number, true);
            var helper = dbHelper;
            return helper.AutoSpPage<ProductData>(query, out count);
        }
        public bool TransactionTest(out string message)
        {
            //注此事务是由ADO.NET控制,在回滚和提交时,如果遇上网络异常或数据库服务器异常,将导致事务出错
            //要保证绝对稳定,建议业务写成存储过程,在存储过程里进行控制
            message = "";
            var helper = dbHelper;
            helper.BeginTran();
            try
            {
                helper.Delete<ProductData>(b => b.Id == 1);
                var item = new Code.ProductData() { InterFaceUser = "2222", ProductName = "product2", BarCode = "" };
                helper.InsertFromObj(item);//不符合数据校验规则,将会抛出异常
                helper.CommitTran();
                message = "事务已提交";
                return true;
            }
            catch(Exception ero)
            {
                message = ero.Message + " 事务已回滚";
                helper.RollbackTran();
            }
            return false;
        }
        public void DynamicQueryTest()
        {
            //此方法演示根据结果集返回动态对象
            string sql = "select top 10 Id,ProductId,ProductName from ProductData";
            var helper = dbHelper;
            var list = helper.ExecDynamicList(sql);
            //添加引用 Miscorsoft.CSharp程序集
            foreach(dynamic item in list)
            {
                var id = item.Id;
                var productId = item.ProductId;
            }
        }
    }
}