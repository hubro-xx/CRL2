using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using CoreHelper;
using System.Collections.Generic;

namespace CRL.Business.OnlinePay
{
	/// <summary>
	/// 提供充值方法
	/// </summary>
	public class ChargeService
	{
		private static Company.CompanyBase GetCompany(CompanyType companyType)
        {
            #region 实例化
            Company.CompanyBase company = null;
            switch (companyType)
            {
                case CompanyType.支付宝:
                    company = new Company.Alipay.AlipayCompany();
                    break;
                case CompanyType.财付通:
                    company = new Company.Tenpay.TenpayCompany();
                    break;
                case CompanyType.银联托管:
                    company = new Business.OnlinePay.Company.ChinaPay.ChinaPayCompany();
                    break;
                case CompanyType.快钱:
                    company = new Business.OnlinePay.Company.Bill99.Bill99Company();
                    break;
                case CompanyType.连连:
                    company = new Business.OnlinePay.Company.Lianlian.LianlianCompany();
                    break;
                case CompanyType.汇付天下:
                    company = new Business.OnlinePay.Company.Chinapnr.ChinapnrCompany();
                    break;
                //case CompanyType.UMPayBank:
                //    company = new Company.UMpay.UMCompanyBank();
                //    break;
                    throw new Exception("错误的CompanyType");
            }
			
			if (company == null)
				throw new Exception("实例化company错误");
			return company;

			#endregion
		}
        static object lockObj = new object();
        /// <summary>
        /// 生成订单
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="user"></param>
        /// <param name="companyType"></param>
        /// <returns></returns>
		public static IPayHistory CreateOrder(decimal amount, string user,CompanyType companyType)
		{
			Company.CompanyBase company = GetCompany(companyType);
            IPayHistory order = null;
            lock (lockObj)
            {
                order = company.CreateOrder(amount, user);
            }
            return order;
		}
		/// <summary>
		/// 提交支付
		/// </summary>
		/// <param name="order"></param>
		public static void Submit(IPayHistory order)
        {
            if (order.OrderType == OrderType.支付)
            {
                if (string.IsNullOrEmpty(order.ProductOrderId))
                {
                    throw new Exception("支付类型订单必须传ProductOrderId");
                }
            }

			Company.CompanyBase company = GetCompany(order.CompanyType);
            try
            {
                company.Submit(order);
            }
            catch(Exception ero)
            {
                if (ero is System.Threading.ThreadAbortException)
                {
                    return;
                }
                CoreHelper.EventLog.Log("提交支付订单时出错:" + ero, true);
                throw ero;
            }
		}
        /// <summary>
        /// 通过参数直接提交
        /// 会产生跳转
        /// </summary>
        /// <param name="user"></param>
        /// <param name="amount"></param>
        /// <param name="bankType"></param>
        /// <param name="orderType"></param>
        /// <param name="companyType"></param>
        /// <param name="byProductOrder"></param>
        /// <param name="redirectUrl"></param>
        public static void Submit(string user, decimal amount, string bankType, OrderType orderType, CompanyType companyType, string byProductOrder, string redirectUrl)
        {
            //todo 订单需要判断没有有付过款,并存不存在
            if (amount <= 0)
            {
                throw new Exception("找不到订单,或订单金额为0");
            }

            IPayHistory order = ChargeService.CreateOrder(amount, user, companyType);
            order.RedirectUrl = redirectUrl;
            //在这里传入银行代码
            order.BankType = bankType;
            //传入商城订单编号
            order.ProductOrderId = byProductOrder;
            //在这里传入订单类型,默认为充值

            order.OrderType = orderType;
  
            ChargeService.Submit(order);
        }
		/// <summary>
		/// 接口回调
		/// </summary>
		/// <param name="companyType"></param>
		/// <param name="context"></param>
		public static string GetNotify(CompanyType companyType, HttpContext context)
		{
			Company.CompanyBase company = GetCompany(companyType);

            return company.GetNotify(context);
		}

		/// <summary>
		/// 查询订单
		/// 如果订单未确认,会自动确认
		/// </summary>
		/// <param name="order"></param>
		public static bool CheckOrder(IPayHistory order,out string message)
		{
            if (order.Status == OrderStatus.已确认 || order.Status == OrderStatus.已退款)
            {
                message = "此订单状态为" + order.Status.Discription();
                return false;
            }
			Company.CompanyBase company = GetCompany(order.CompanyType);
			return company.CheckOrder(order,out message);
		}
		/// <summary>
		/// 接口转向页执行此方法
		/// </summary>
		/// <param name="order"></param>
		public static void Redirect(IPayHistory order)
		{
			Company.CompanyBase company = GetCompany(order.CompanyType);
			company.Redirect(order);
		}

		
        /// <summary>
        /// 订单取消,退款
        /// 只要提交成功就一定能成功,特殊情况除外
        /// </summary>
        /// <param name="order"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool RefundOrder(IPayHistory order, out string message)
        {
            if (order.Status != OrderStatus.已确认)
            {
                message = "此订单状态为" + order.Status.Discription();
                return false;
            }
            Company.CompanyBase company = GetCompany(order.CompanyType);
            return company.RefundOrder(order, out message);
        }
        /// <summary>
        /// 提交支付转帐
        /// </summary>
        /// <param name="payDetail"></param>
        /// <param name="batch_no"></param>
        public static void BatchTransfers(List<Company.Alipay.AlipayCompany.BatchPayItem> payDetail, string batch_no)
        {
            new Company.Alipay.AlipayCompany().BatchTransfers(payDetail, batch_no);
        }
	}
}
