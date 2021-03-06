﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL
{
    public class CacheServerSetting
    {
        #region 缓存服务器
        internal static List<string> ServerTypeSetting = new List<string>();
        internal static Dictionary<string, ExpressionDealDataHandler> CacheServerDealDataRules = new Dictionary<string, ExpressionDealDataHandler>();
        /// <summary>
        /// 清加数据处理规则
        /// </summary>
        /// <param name="type"></param>
        /// <param name="handler"></param>
        public static void AddCacheServerDealDataRule(Type type, ExpressionDealDataHandler handler)
        {
            CacheServerDealDataRules.Add(type.FullName, handler);
            ServerTypeSetting.Add(type.FullName);
        }
        #endregion
        /// <summary>
        /// 缓存客户端代理
        /// </summary>
        internal static List<CacheServer.CacheClientProxy> CacheClientProxies = new List<CacheServer.CacheClientProxy>();
        internal static Dictionary<string, CacheServer.CacheClientProxy> ServerTypeSettings = new Dictionary<string, CacheServer.CacheClientProxy>();
        /// <summary>
        /// 添加TCP客户端代理
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public static void AddClientProxy(string host, int port)
        {
            var client = new CRL.CacheServer.TcpPoolClient(host, port);
            CacheClientProxies.Add(client);
        }
        /// <summary>
        /// 初始服务端设置,会访问所有服务端获取设置
        /// </summary>
        public static void Init()
        {
            foreach (var p in CacheClientProxies)
            {
                p.GetServerTypeSetting();
            }
        }
        public static void Dispose()
        {
            foreach (var p in CacheClientProxies)
            {
                p.Dispose();
            }
        }
        internal static CacheServer.CacheClientProxy GetCurrentClient(Type type)
        {
            string typeName = type.FullName;
            if (ServerTypeSettings.ContainsKey(typeName))
            {
                return ServerTypeSettings[typeName];
            }
            return null;
            //throw new Exception("未在服务器上找到对应的数据处理类型;" + typeName);
        }
    }
}
