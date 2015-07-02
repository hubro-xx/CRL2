using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Freight
{
    /// <summary>
    /// 运费维护/查询
    /// </summary>
    public class FreightBusiness<TType> : BaseProvider<Freight> where TType : class
    {
        public static FreightBusiness<TType> Instance
        {
            get { return new FreightBusiness<TType>(); }
        }
        protected override DBExtend dbHelper
        {
            get { return GetDbHelper<TType>(); }
        }
        /// <summary>
        /// 检测是否存在,并添加
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        public new int Add(Freight p)
        {
            var item = QueryItem(b => b.AreaId == p.AreaId && b.DeliverType == p.DeliverType);
            if (item != null)
            {
                return 0;
            }
            //helper.Clear();
            int id = Add( p);
            return id;
        }
        /// <summary>
        /// 运费计算
        /// 查询不到返回0
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <param name="areaId"></param>
        /// <param name="supplier"></param>
        /// <param name="freight1">物流运费</param>
        /// <param name="freight2">快递运费</param>
        public void CalculateFreight(List<CRL.Order.ProductOrder.OrderDetail> orderDetail, string areaId, Person.ShopSupplier supplier, out double freight1, out double freight2)
        {
            //总重
            double totalHeavy = 0;
            //免运费金额
            decimal minFreePostAmount = 0;

            int supplierId = supplier.Id;
            minFreePostAmount = supplier.MinFreePostAmount;
            decimal freePostAmount = 0;
            //计算总额
            foreach (var item in orderDetail)
            {
                if (item.IncludedFreePost)
                {
                    freePostAmount += item.Price * item.Num;
                }
                totalHeavy += item.TotalWeight;
            }
            //小于免运费金额才计算运费
            if (freePostAmount > minFreePostAmount)
            {
                freight1 = 0;
                freight2 = 0;
                return;
            }
            //freight1 = 100;
            //freight2 = 100;
            //return;
            //实际运费＝首重运费＋((总重－首重)/续重)*续重运费
            freight1 = 0;
            freight2 = 0;
            List<Freight> freight = null;
            while(true)
            {
                //helper.Clear();
                freight = QueryList(b => b.SupplierId == supplierId && b.AreaId == areaId && b.Disable == false);
                if (freight.Count == 0)//找上一级
                {
                    areaId = Area.AreaAction.Get(areaId).ParentCode;
                    if (areaId == "1")
                        return;
                }
                else
                {
                    break;
                }
            }
            //同时返回物流和快递,没有则只返回一个
            var f1 = freight[0];
            double continuedMoney = 0;
            if (totalHeavy > 0)
            {
                continuedMoney = ((totalHeavy - f1.Heavy) / f1.ContinuedHeavy) * f1.ContinuedHeavyMoney;
                if (continuedMoney < 0)
                    continuedMoney = 0;
            }
            double a = f1.HeavyMoney + continuedMoney;
            if (f1.DeliverType == DeliverType.物流)
            {
                freight1 = a;
            }
            else
            {
                freight2 = a;
            }
            if (freight.Count > 1)
            {
                var f2 = freight[1];
                double continuedMoney2 = 0;
                if (totalHeavy > 0)
                {
                    continuedMoney2 = ((totalHeavy - f2.Heavy) / f2.ContinuedHeavy) * f2.ContinuedHeavyMoney;
                    if (continuedMoney2 < 0)
                        continuedMoney2 = 0;
                }
                double b = f2.HeavyMoney + continuedMoney2;
                if (f2.DeliverType == DeliverType.物流)
                {
                    freight1 = b;
                }
                else
                {
                    freight2 = b;
                }
            }
            return ;
        }
    }
}
