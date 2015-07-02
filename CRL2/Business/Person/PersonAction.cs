using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace CRL.Person
{
    /// <summary>
    /// 会员/人维护
    /// </summary>
    public class PersonAction<TType> : BaseAction<TType, IPerson> where TType : class
    {
        /// <summary>
        /// 检测帐号是否存在
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="accountNo"></param>
        /// <returns></returns>
        public static bool CheckAccountExists<T>(string accountNo) where T : IPerson, new()
        {
            T item = QueryItem<T>(b => b.AccountNo == accountNo);
            return item != null;
        }
        /// <summary>
        /// 验证密码
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="accountNo"></param>
        /// <param name="passWord"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool CheckPass<T>(string accountNo, string passWord, out string message) where T : IPerson, new()
        {
            message = "";
            T item = QueryItem<T>(b => b.AccountNo == accountNo);
            if (item == null)
            {
                message = "帐号不存在";
                return false;
            }
            bool a = item.PassWord.ToUpper() == CoreHelper.StringHelper.EncryptMD5(passWord);
            if (!a)
            {
                message = "密码不正确";
            }
            return a;
        }
        /// <summary>
        /// 更改密码
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="accountNo"></param>
        /// <param name="passWord"></param>
        public static void UpdatePass<T>(string accountNo, string passWord) where T : IPerson, new()
        {
            ParameCollection c2 = new ParameCollection();
            c2["PassWord"] = CoreHelper.StringHelper.EncryptMD5(passWord);
            Update<T>(b => b.AccountNo == accountNo, c2);
        }
        #region 登录验证
        /// <summary>
        /// 使用当前用户写入登录票据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="user"></param>
        /// <param name="rules">用户组</param>
        /// <param name="expires">是否自动过期</param>
        public static void Login<T>(T user,string rules,bool expires)where T : IPerson
        {
            user.RuleName = rules;
            CoreHelper.FormAuthentication.AuthenticationSecurity.SetTicket(user, rules, expires);
        }
        /// <summary>
        /// 登出
        /// </summary>
        public static void LoginOut()
        {
            CoreHelper.FormAuthentication.AuthenticationSecurity.LoginOut();
        }
        /// <summary>
        /// 检查登录票据
        /// Application_AuthenticateRequest使用
        /// </summary>
        public static void CheckTicket()
        {
            CoreHelper.FormAuthentication.AuthenticationSecurity.CheckTicket();
        }
        /// <summary>
        /// 获取当前登录的用户
        /// </summary>
        public static IPerson CurrentUser
        {
            get
            {
                string userTicket = System.Web.HttpContext.Current.User.Identity.Name;
                if (string.IsNullOrEmpty(userTicket))
                    return null;
                //数据不对会造成空
                CoreHelper.FormAuthentication.IUser user = new IPerson().ConverFromArry(userTicket);
                if (user == null)
                {
                    LoginOut();
                }
                return user as IPerson;
            }
        }
        #endregion
    }
}
