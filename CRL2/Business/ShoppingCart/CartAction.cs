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
    public class CartAction<TType> : BaseAction<TType, ICartItem> where TType : class
    {
        
        static string cookieName = "cartCount";

        //static void SetCartCount(int type, int num)
        //{
        //    CoreHelper.LocalCookie c = new CoreHelper.LocalCookie(cookieName);
        //    if (num < 0)
        //        num = 0;
        //    c[cookieName + type] = num.ToString();
        //}

        /// <summary>
        /// 购物车数量,用COOKIE存
        /// </summary>
        public static int GetCartCount(int type)
        {
            DBExtend helper = dbHelper;
            var user = Person.PersonAction<TType>.CurrentUser;
            if (user != null)
            {
                string sql = "select sum(num) from $ICartItem where userid=" + user.Id + " and cartType=" + type;
                object obj = helper.ExecScalar(sql, typeof(ICartItem));
                if (obj is DBNull)
                {
                    obj = 0;
                }
                return (int)obj;
            }
            else
            {
                return 0;
            }
            //CoreHelper.LocalCookie c = new CoreHelper.LocalCookie(cookieName);
            //if (string.IsNullOrEmpty(c[cookieName + type]))
            //{
            //    DBExtend helper = dbHelper;
            //    var user = Person.PersonAction<TType>.CurrentUser;
            //    if (user != null)
            //    {
            //        string sql = "select sum(num) from $ICartItem where userid=" + user.Id + " and cartType=" + type;
            //        object obj = helper.ExecScalar(sql, typeof(ICartItem));
            //        if (obj is DBNull)
            //        {
            //            obj = 0;
            //        }
            //        c[cookieName + type] = obj.ToString();
            //        return (int)obj;
            //    }
            //    else
            //    {
            //        return 0;
            //    }
            //}
            //return Convert.ToInt32(c[cookieName + type]);
        }
        /// <summary>
        /// 获取购物车总额
        /// </summary>
        /// <param name="cartType"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static decimal GetTotalAmount(int cartType,string userId )
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
        public static int Add(ICartItem item,bool groupByPrice=false)
        {
            DBExtend helper = dbHelper;
            ICartItem item2;
            if (groupByPrice)
            {
                item2 = helper.QueryItem<ICartItem>(b => b.UserId == item.UserId && b.ProductId == item.ProductId && b.StyleId == item.StyleId && b.CartType == item.CartType && b.Price == item.Price);
            }
            else
            {
                item2 = helper.QueryItem<ICartItem>(b => b.UserId == item.UserId && b.ProductId == item.ProductId && b.StyleId == item.StyleId && b.CartType == item.CartType);
            }
            if (item2==null)
            {
                //int n = GetCartCount(item.CartType) + 1;
                //SetCartCount(item.CartType,n);
                Add<ICartItem>(item);
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
        public static void AddNum(ICartItem item)
        {
            DBExtend helper = dbHelper;
            ParameCollection c = new ParameCollection();
            c["$Num"] = "Num+" + item.Num;
            //int n = GetCartCount(item.CartType) + item.Num;
            //SetCartCount(item.CartType, n);
            helper.Update<ICartItem>(b => b.UserId == item.UserId && b.ProductId == item.ProductId && b.StyleId == item.StyleId && b.CartType == item.CartType, c);
        }
        /// <summary>
        /// 更改数量
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="num"></param>
        /// <param name="id"></param>
        public static void ChangeNum(string userId,int num, int id)
        {
            DBExtend helper = dbHelper;
            var cartItem = helper.QueryItem<ICartItem>(b => b.UserId == userId && b.Id == id);
            if (cartItem != null)
            {
                //int n = GetCartCount(cartItem.CartType) + num - cartItem.Num;
                //SetCartCount(cartItem.CartType, n);
            }
            ParameCollection c = new ParameCollection();
            c["Num"] = num;
            helper.Update<ICartItem>(b => b.UserId == userId && b.Id == id, c);
        }
        public static void UpdatePrice(string userId, int id, decimal price)
        {
            ParameCollection c = new ParameCollection();
            c["price)"] = price;
            Update<ICartItem>(b => b.UserId == userId && b.Id == id, c);
        }
        public static ICartItem GetCartItem(string userId, int id)
        {
            return QueryItem<ICartItem>(b => b.UserId == userId && b.Id == id);
        }
        /// <summary>
        /// 通过产品详细信息加入购物车
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="product"></param>
        /// <param name="num"></param>
        /// <param name="styleId"></param>
        public static bool Add(string userId, Product.IProduct product, int num, int styleId, string styleName, string spreadInfo)
        {
            DBExtend helper = dbHelper;
            ICartItem c = new ICartItem();
            c.Num = num;
            c.ProductId = product.Id;
            c.Price = product.SoldPrice;
            c.ProductName = product.ProductName;
            c.StyleId = styleId;
            c.StyleName = styleName;
            c.Integral = product.Integral;
            c.SupplierId = product.SupplierId;
            c.UserId = userId;
            c.SpreadInfo = spreadInfo;
            c.TagData = product.TagData;
            c.IncludedFreePost = product.IncludedFreePost;
            double totalWeight = 0;
            totalWeight = product.Weight * num;
            if (product.ExemptFreightCount > 0 && product.ExemptFreightCount <= num)//如果达到多少个,则总重量为0
            {
                totalWeight = 0;
            }
            c.TotalWeight = totalWeight;
            Add(c);
            return true;
        }
        /// <summary>
        /// 查询购物车,并指定是否按商家分组
        /// </summary>
        /// <param name="cartType"></param>
        /// <param name="userId"></param>
        /// <param name="groupBySupplier"></param>
        /// <returns></returns>
        public static Dictionary<int, List<ICartItem>> Query(int cartType, string userId, bool groupBySupplier) 
        {
            DBExtend helper = dbHelper;
            Dictionary<int, List<ICartItem>> list = new Dictionary<int, List<ICartItem>>();
            List<ICartItem> all = QueryList<ICartItem>(b => b.UserId == userId && b.CartType == cartType);
            if (all.Count == 0)
                return list;
            foreach (ICartItem item in all)
            {
                int groupId = item.SupplierId;
                if (!groupBySupplier)
                {
                    groupId = 1;
                }
                if (!list.ContainsKey(groupId))
                {
                    list.Add(groupId, new List<ICartItem>());
                }
                list[groupId].Add(item);
            }
            return list;
        }
        /// <summary>
        /// 设置是否选中
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cartType"></param>
        /// <param name="id"></param>
        /// <param name="selected"></param>
        public static void SetSelected(string userId,int cartType, int id, bool selected)
        {
            ParameCollection c = new ParameCollection();
            c["Selected"] = selected;
            Update<ICartItem>(b => b.UserId == userId && b.Id == id, c);
            //int n = GetCartCount(cartType);
            //SetCartCount(cartType, n);
        }
        /// <summary>
        /// 移除多项,不移除没选中的
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cartType"></param>
        public static void RemoveAll(string userId, int cartType)
        {
            Delete<ICartItem>(b => b.UserId == userId && b.CartType == cartType && b.Selected == true);
            //int n = GetCartCount(cartType);
            //SetCartCount(cartType, n);
        }
        /// <summary>
        /// 移除一项
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="id"></param>
        public static bool Remove(string userId, int id)
        {
            DBExtend helper = dbHelper;
            var cartItem = helper.QueryItem<ICartItem>(b => b.UserId == userId && b.Id == id);
            if (cartItem != null)
            {
                //int n = GetCartCount(cartItem.CartType) - cartItem.Num;
                //SetCartCount(cartItem.CartType, n);
            }
            return Delete<ICartItem>(b => b.UserId == userId && b.Id == id) > 0;
        }
    }
}
