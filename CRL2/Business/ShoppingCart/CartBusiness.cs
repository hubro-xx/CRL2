using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

namespace CRL.ShoppingCart
{
    /// <summary>
    /// 购物车维护
    /// </summary>
    public class CartBusiness<TType, TModel> : BaseProvider<TModel> where TType : class where TModel:CartItem,new()
    {
        protected override DBExtend dbHelper
        {
            get { return GetDbHelper<TType>(); }
        }
        /// <summary>
        /// 购物车数量
        /// </summary>
        public int GetCartCount(int type,int userId=0)
        {
            DBExtend helper = dbHelper;
            if (userId>0)
            {
                string _u = userId.ToString();
                int a = Sum<int>(b => b.UserId == _u && b.CartType == type, b => b.Num, true);
                return a;
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// 获取购物车总额
        /// </summary>
        /// <param name="cartType"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public decimal GetTotalAmount(int cartType,string userId )
        {
            DBExtend helper = dbHelper;
            string sql = "select sum(num*price) from $ICartItem where userid=" + userId + " and selected=1 and cartType=" + cartType;
            object obj = helper.ExecScalar(sql, typeof(ICartItem));
            if (obj is DBNull)
            {
                obj = 0;
            }
            return Convert.ToDecimal(obj);
        }

        /// <summary>
        /// 添加到购物车
        /// </summary>
        /// <param name="item"></param>
        public int Add(TModel item,bool groupByPrice=false)
        {
            DBExtend helper = dbHelper;
            TModel item2;
            if (groupByPrice)
            {
                item2 = helper.QueryItem<TModel>(b => b.UserId == item.UserId && b.ProductId == item.ProductId && b.StyleId == item.StyleId && b.CartType == item.CartType && b.Price == item.Price);
            }
            else
            {
                item2 = helper.QueryItem<TModel>(b => b.UserId == item.UserId && b.ProductId == item.ProductId && b.StyleId == item.StyleId && b.CartType == item.CartType);
            }
            if (item2==null)
            {
                //int n = GetCartCount(item.CartType) + 1;
                //SetCartCount(item.CartType,n);
                base.Add(item);
            }
            else
            {
                AddNum(item);
            }
            return GetCartCount(item.CartType);
        }
        /// <summary>
        /// 更改数量
        /// </summary>
        /// <param name="item"></param>
        public void AddNum(TModel item)
        {
            ParameCollection c = new ParameCollection();
            c["$Num"] = "Num+" + item.Num;
            //int n = GetCartCount(item.CartType) + item.Num;
            //SetCartCount(item.CartType, n);
            Update(b => b.UserId == item.UserId && b.ProductId == item.ProductId && b.StyleId == item.StyleId && b.CartType == item.CartType, c);
        }
        /// <summary>
        /// 更改数量
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="num"></param>
        /// <param name="id"></param>
        public void ChangeNum(string userId,int num, int id)
        {
            var cartItem = QueryItem(b => b.UserId == userId && b.Id == id);
            if (cartItem != null)
            {
                //int n = GetCartCount(cartItem.CartType) + num - cartItem.Num;
                //SetCartCount(cartItem.CartType, n);
            }
            ParameCollection c = new ParameCollection();
            c["Num"] = num;
            Update(b => b.UserId == userId && b.Id == id, c);
        }
        public void UpdatePrice(string userId, int id, decimal price)
        {
            ParameCollection c = new ParameCollection();
            c["price"] = price;
            Update(b => b.UserId == userId && b.Id == id, c);
        }
        public CartItem GetCartItem(string userId, int id)
        {
            return QueryItem(b => b.UserId == userId && b.Id == id);
        }
        /// <summary>
        /// 设置是否选中
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cartType"></param>
        /// <param name="id"></param>
        /// <param name="selected"></param>
        public void SetSelected(string userId,int cartType, int id, bool selected)
        {
            ParameCollection c = new ParameCollection();
            c["Selected"] = selected;
            Update(b => b.UserId == userId && b.Id == id, c);
            //int n = GetCartCount(cartType);
            //SetCartCount(cartType, n);
        }
        /// <summary>
        /// 移除多项,不移除没选中的
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cartType"></param>
        public void RemoveAll(string userId, int cartType)
        {
            Delete(b => b.UserId == userId && b.CartType == cartType && b.Selected == true);
            //int n = GetCartCount(cartType);
            //SetCartCount(cartType, n);
        }
        /// <summary>
        /// 移除一项
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="id"></param>
        public bool Remove(string userId, int id)
        {
            return Delete(b => b.UserId == userId && b.Id == id) > 0;
        }
    }
}
