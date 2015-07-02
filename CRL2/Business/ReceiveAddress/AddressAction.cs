using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace CRL.ReceiveAddress
{
    /// <summary>
    /// 收货地址维护
    /// </summary>
    public class AddressAction<TType> : BaseAction<TType, IAddress> where TType : class
    {
        /// <summary>
        /// 获取用户收货地址
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<T> QueryUserAddress<T>(string userId)where T:IAddress,new()
        {
            DBExtend helper = dbHelper;
            LamadaQuery<T> query = new LamadaQuery<T>();
            query = query.Select().Where(b => b.UserId == userId).OrderBy(b => b.DefaultAddress, true);
            string key;
            return helper.QueryList<T>(query, 0, out key);
        }
        /// <summary>
        /// 设为默认
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="id"></param>
        public static void SetDefault<T>(string userId,int id)where T:IAddress,new()
        {
            ParameCollection setValue = new ParameCollection();
            setValue["DefaultAddress"] = 0;
            Update<T>((b=>b.UserId==userId), setValue);//去掉默认

            setValue["DefaultAddress"] = 1;
            Update<T>(b => b.Id == id, setValue);//设为默认
        }
    }
}
