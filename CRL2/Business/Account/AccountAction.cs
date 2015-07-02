using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreHelper;
using System.Data;
using System.Collections;
namespace CRL.Account
{

    /// <summary>
    /// 帐号维护,区分不同的帐号类型和流水类型
    /// </summary>
    public class AccountAction<TType> : BaseAction<TType, IAccountDetail> where TType : class
    {
        /// <summary>
        /// 创建帐号
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static bool CreateAccount(IAccountDetail info)
        {
            Add(info);
            return true;
        }

        /// <summary>
        /// 获取账号类型,传入枚举
        /// </summary>
        /// <param name="account"></param>
        /// <param name="accountType"></param>
        /// <param name="transactionType"></param>
        /// <returns></returns>
        public static IAccountDetail GetAccount(int account, Enum accountType, Enum transactionType)
        {
            return GetAccount(account.ToString(), accountType.ToInt(), transactionType.ToInt());
        }

        //public static IAccountDetail GetAccount(string account, int transactionType)
        //{
        //    return GetAccount(account, 0, transactionType);
        //}
        /// <summary>
        /// 取得帐号信息,没有则创建(实时)
        /// </summary>
        /// <param name="account"></param>
        /// <param name="accountType"></param>
        /// <param name="transactionType"></param>
        /// <returns></returns>
        public static IAccountDetail GetAccount(string account, int accountType, int transactionType)
        {
            DBExtend helper = dbHelper;
            IAccountDetail info = helper.QueryItem<IAccountDetail>(b => b.Account == account && b.TransactionType == transactionType && b.AccountType == accountType);
            if (info == null)
            {
                info = new IAccountDetail();
                info.Account = account;
                info.AccountType = accountType;
                info.TransactionType = transactionType;
                CreateAccount(info);
            }
            return info;
        }
        static Dictionary<int, IAccountDetail> detailInfoCache = new Dictionary<int, IAccountDetail>();
        /// <summary>
        /// 获取帐户详细信息,按帐户ID
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public static IAccountDetail GetAccountFromCache(int accountId)
        {
            if (detailInfoCache.ContainsKey(accountId))
            {
                return detailInfoCache[accountId];
            }
            IAccountDetail info = QueryItem<IAccountDetail>(b => b.Id == accountId);
            if (info == null)
                return null;

            lock (lockObj)
            {
                if (!detailInfoCache.ContainsKey(accountId))
                {
                    detailInfoCache.Add(accountId, info);
                }
            }
            return info;
        }

        /// <summary>
        /// 根据帐户ID取得对应的帐号
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public static string GetAccountNoFromCache(int accountId)
        {
            var info = GetAccountFromCache(accountId);
            if (info == null)
                return null;
            return info.Account;
            //string key;
            //if (accountCache.ContainsValue(accountId))
            //{
            //    key = accountCache.First(b => b.Value == accountId).Key;
            //    return key.Split('_')[0];
            //}
            //IAccountDetail item = QueryItem<IAccountDetail>(b => b.Id == accountId);
            //if (item == null)
            //    throw new Exception("找不到对应的帐户");
            //key = string.Format("{0}_{1}_{2}", item.Account, item.AccountType, item.TransactionType);
            //if (!accountCache.ContainsKey(key))
            //{
            //    lock (lockObj)
            //    {
            //        accountCache.Add(key, item.Id);
            //    }
            //}
            //return item.Account;
        }
        //static Dictionary<string, int> accountCache = new Dictionary<string, int>();

        /// <summary>
        /// 使用默认用户类型取得帐户ID
        /// </summary>
        /// <param name="account"></param>
        /// <param name="transactionType"></param>
        /// <returns></returns>
        //public static int GetAccountId(string account, int transactionType)
        //{
        //    return GetAccountId(account, 0, transactionType);
        //}
        /// <summary>
        /// 取得帐户ID(从缓存)
        /// </summary>
        /// <param name="account"></param>
        /// <param name="accountType">帐号类型,用以区分不同渠道用户</param>
        /// <param name="transactionType"></param>
        /// <returns></returns>
        public static int GetAccountId(string account,int accountType, int transactionType)
        {
            int id = 0;
            foreach (var item in detailInfoCache.Values)
            {
                if (item.Account == account && item.AccountType == accountType&& item.TransactionType== transactionType)
                {
                    return item.Id;
                }
            }
            IAccountDetail detail = GetAccount(account, accountType, transactionType);
            lock (lockObj)
            {
                if (!detailInfoCache.ContainsKey(detail.Id))
                {

                    detailInfoCache.Add(detail.Id, detail);
                }
            }
            return detail.Id;
            //string key = string.Format("{0}_{1}_{2}", account, accountType, transactionType);
            ////加入缓存机制
            //if (accountCache.ContainsKey(key))
            //{
            //    return accountCache[key];
            //}
            //IAccountDetail item = GetAccount(account,accountType, transactionType);
            //if (!accountCache.ContainsKey(key))
            //{
            //    lock (lockObj)
            //    {
            //        accountCache.Add(key, item.Id);
            //    }
            //}
            //return item.Id;
        }
    }
}
