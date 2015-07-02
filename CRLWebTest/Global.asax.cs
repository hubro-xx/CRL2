using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Xml.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
namespace WebTest
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            //配置数据连接
            CRL.SettingConfig.GetDbAccess = (type) =>
            {
                //可按type区分数据库
                return WebTest.Code.LocalSqlHelper.TestConnection;
            };
            #region 缓存服务端实现
            //增加处理规则
            CRL.CacheServerSetting.AddCacheServerDealDataRule(typeof(Code.ProductData), Code.ProductDataManage.Instance.DeaCacheCommand);
            //启动服务端
            var cacheServer = new CRL.CacheServer.TcpServer(1129);
            cacheServer.Start();
            #endregion

            //实现缓存客户端调用
            //有多个服务器添加多个
            CRL.CacheServerSetting.AddClientProxy("127.0.0.1", 1129);
            CRL.CacheServerSetting.Init();
        }
        class HttpCacheClient : CRL.CacheServer.CacheClientProxy
        {
            public override string Host
            {
                get { return "http://localhost:56640/page/CacheServer.ashx"; }
            }
            public override string SendQuery(string data)
            {
                return CoreHelper.HttpRequest.HttpPost(Host, "q=" + data, System.Text.Encoding.UTF8);
            }
            public override void Dispose()
            {
                throw new NotImplementedException();
            }
        }
        
    }
}