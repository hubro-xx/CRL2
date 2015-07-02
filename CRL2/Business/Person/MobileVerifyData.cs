using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace CRL.Person
{
    /// <summary>
    /// 读写短信验证数据
    /// </summary>
    public class MobileVerifyData
    {
        /// <summary>
        /// 写入手机信息
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="code"></param>
        /// <param name="receiveMobile"></param>
        public static void SetCode(string moduleName,string code,string receiveMobile)
        {
            CoreHelper.ServerDataCache c = new CoreHelper.ServerDataCache(moduleName);
            c["C"] = code;
            c["M"] = DateTime.Now.ToString();
            c["R"] = receiveMobile;
            if (c["N"] == null)
            {
                c["N"] = 1;
            }
            else
            {
                c["N"] = Convert.ToInt32(c["N"]) + 1;
            }
            c["T"] = 1;
        }
        /// <summary>
        /// 通过模块名称获取验证码
        /// </summary>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        public static string GetCode(string moduleName)
        {
            CoreHelper.ServerDataCache c = new CoreHelper.ServerDataCache(moduleName);
            if (c["C"] == null)
            {
                return null;
            }
            c["T"] = Convert.ToInt32(c["T"]) + 1;
            return c["C"].ToString();
        }
        /// <summary>
        /// 获取验证码读取次数,用来作次数限制
        /// </summary>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        public static int GetTimes(string moduleName)
        {
            CoreHelper.ServerDataCache c = new CoreHelper.ServerDataCache(moduleName);
            if (c["T"] == null)
            {
                return 0;
            }
            return Convert.ToInt32(c["T"]);
        }
        /// <summary>
        /// 通过模块名称获取手机
        /// </summary>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        public static string GetReceiveMobile(string moduleName)
        {
            CoreHelper.ServerDataCache c = new CoreHelper.ServerDataCache(moduleName);
            if (c["R"] == null)
            {
                return null;
            }
            return c["R"].ToString();
        }
        /// <summary>
        /// 通过模块名称获取发送间隔
        /// </summary>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        public static int GetSendTimeDiff(string moduleName)
        {
            CoreHelper.ServerDataCache c = new CoreHelper.ServerDataCache(moduleName);
            if (c["M"]==null)
            {
                return 9999;
            }
            else
            {
                DateTime t = Convert.ToDateTime(c["M"]);
                TimeSpan ts = DateTime.Now - t;
                return (int)ts.TotalSeconds;
            }
        }
        /// <summary>
        /// 获取本次验证码发送次数
        /// </summary>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        public static int GetTotalSendTimes(string moduleName)
        {
            CoreHelper.ServerDataCache c = new CoreHelper.ServerDataCache(moduleName);
            if (c["N"] == null)
            {
                return 1;
            }
            return Convert.ToInt32(c["N"]);
        }
        /// <summary>
        /// 清除本次数据
        /// </summary>
        /// <param name="moduleName"></param>
        public static void Clear(string moduleName)
        {
            CoreHelper.ServerDataCache c = new CoreHelper.ServerDataCache(moduleName);
            c.Clear();
        }
    }
}
