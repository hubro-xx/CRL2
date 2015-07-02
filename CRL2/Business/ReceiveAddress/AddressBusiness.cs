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
    public class AddressBusiness<TType, TModel> : BaseProvider<TModel>
        where TType : class
        where TModel : Address,new()
    {
        public static AddressBusiness<TType, TModel> Instance
        {
            get { return new AddressBusiness<TType, TModel>(); }
        }
        protected override DBExtend dbHelper
        {
            get { return GetDbHelper<TType>(); }
        }
        /// <summary>
        /// 获取用户收货地址
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<TModel> QueryUserAddress(string userId)
        {
            LambdaQuery<TModel> query = GetLamadaQuery();
            query = query.Where(b => b.UserId == userId).OrderBy(b => b.DefaultAddress, true);
            return QueryList(query);
        }
        /// <summary>
        /// 设为默认
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="id"></param>
        public void SetDefault(string userId,int id)
        {
            ParameCollection setValue = new ParameCollection();
            setValue["DefaultAddress"] = 0;
            Update((b=>b.UserId==userId), setValue);//去掉默认

            setValue["DefaultAddress"] = 1;
            Update(b => b.Id == id, setValue);//设为默认
        }
    }
}
