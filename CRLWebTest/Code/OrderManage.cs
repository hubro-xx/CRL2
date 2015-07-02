using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebTest.Code
{
    /// <summary>
    /// OrderManage
    /// </summary>
    public class OrderManage : CRL.Order.OrderBusiness<OrderManage, Order>
    {
        public static OrderManage Instance
        {
            get { return new OrderManage(); }
        }
        protected override CRL.DBExtend dbHelper
        {
            get { return GetDbHelper(this.GetType()); }
        }
        public void TestUpdate()
        {
            var helper = dbHelper;//当前访问对象
            helper.BeginTran();//开启事物
            try
            {
                helper.InsertFromObj(new Order());
                var c = new CRL.ParameCollection();
                c["userId"] = 1;
                helper.Update<Order>(b => b.Id == 100, c);
                helper.CommitTran();//提交
            }
            catch(Exception ex)
            {
                helper.RollbackTran();//如果出错回滚
            }
        }
        public void TransactionTest2()
        {

            var helper = dbHelper;//当前访问对象
            helper.BeginTran();//开启事物
            try
            {

                var product = new ProductData() { ProductName="test", BarCode="1212" };

                //使用当前会话创建ProductDataManage实例
                //ProductDataManage对象使用的数据访问和当前是一个
                ProductDataManage.ContextInstance(this).Add(product);
                var c = new CRL.ParameCollection();
                c["userId"] = 1;
                helper.Update<Order>(b => b.Id == 100, c);
                helper.CommitTran();//提交
            }
            catch (Exception ex)
            {
                helper.RollbackTran();//如果出错回滚
            }
        
        }
    }
}