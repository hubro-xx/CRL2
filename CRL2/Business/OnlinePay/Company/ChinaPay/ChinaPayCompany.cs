
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;

namespace CRL.Business.OnlinePay.Company.ChinaPay
{
    public class ChinaPayCompany : CompanyBase
    {
        public override CompanyType ThisCompanyType
        {
            get { return CompanyType.银联托管; }
        }
        #region 支付
        public override void Submit(IPayHistory order)
        {
            BaseSubmit(order);
            var request = CreateHead<Charge.Request>();
            request.UserId = order.UserId;
            request.Amt = (Convert.ToInt64(order.Amount) * 100).ToString();
            request.SrcReqId = order.OrderId;//更改为订单ID
            request.CurCode = "156";
            string notifyUrl = ChargeConfig.GetConfigKey(CompanyType.银联托管, ChargeConfig.DataType.NotifyUrl);
            string returnUrl = ChargeConfig.GetConfigKey(CompanyType.银联托管, ChargeConfig.DataType.ReturnUrl);
            request.NotifyUrl = ConvertUrl(notifyUrl);
            request.ReturnUrl = ConvertUrl(returnUrl);
            Submit(request);
        }
        public static string ConvertUrl(string path)
        {
            return CoreHelper.RequestHelper.GetCurrentHost() + path;
        }
        protected override string OnNotify(HttpContext context)
        {
            string error;
            var result = ChargeNotify(context, out error);
            if (result == null)
            {
                return error;
            }
            var a = DealChargeNotify(result, out error);
            return error;
        }
        /// <summary>
        /// 处理通知
        /// </summary>
        /// <param name="result"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool DealChargeNotify(Charge.Response result,out string error)
        {
            error = "";
            var orderId = result.SrcReqId;
            IPayHistory order = OnlinePayBusiness.Instance.GetOrder(orderId, ThisCompanyType);
            order.spBillno = result.SrcReqId;
            Confirm(order, GetType(), order.Amount);
            return true;
        }
        #endregion
        /// <summary>
        /// 生成请求头
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CreateHead<T>() where T : RequestBase, new()
        {
            string merId = ChargeConfig.GetConfigKey(CompanyType.银联托管, ChargeConfig.DataType.User);
            string channelId = ChargeConfig.GetConfigKey(CompanyType.银联托管, ChargeConfig.DataType.Data);
            var obj = new T();
            obj.SetCode();
            obj.Ver = "100";
            obj.MerId = merId;
            obj.SrcReqDate = DateTime.Now.ToString("yyyyMMdd");
            obj.SrcReqId = CreateOrderId();
            obj.ChannelId = channelId;
            return obj;
        }
        /// <summary>
        /// 跳转提交
        /// </summary>
        /// <param name="send"></param>
        public static void Submit(RequestBase send)
        {
            var html = ChinaPayUtil.GetSubmit(send);
            CoreHelper.EventLog.Log(html, "ChinaPaySubmit", false);
            System.Web.HttpContext.Current.Response.Write(html);
            System.Web.HttpContext.Current.Response.End();
        }
        /// <summary>
        /// 后台请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="send"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static T Request<T>(RequestBase send,out string error) where T : ResponseBase, new()
        {
            var xml = ChinaPayUtil.GetRequest(send);
            var response = CoreHelper.HttpRequest.HttpPost(send.InterFaceUrl, xml, Encoding.UTF8, "application/xml");
            var obj = ChinaPayUtil.Deserialize<T>(response, out error);
            if (obj == null)
            {
                CoreHelper.EventLog.Log("Request:" + xml, "ChinaPay");
            }
            return obj;
        }
        public Charge.Response ChargeNotify(System.Web.HttpContext context, out string error)
        {
            error = "";
            var result = ChinaPayUtil.Deserialize<Charge.Response>(context, out error);
            return result;
        }
        /// <summary>
        /// 创建账号通知
        /// </summary>
        /// <param name="context"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public CreateAccount.Response CreateAccountNotify(System.Web.HttpContext context, out string error)
        {
            error = "";
            var result = ChinaPayUtil.Deserialize<CreateAccount.Response>(context, out error);
            return result;
        }
        /// <summary>
        /// 投标通知
        /// </summary>
        /// <param name="context"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public Tender.Response TenderNotify(System.Web.HttpContext context, out string error)
        {
            error = "";
            var result = ChinaPayUtil.Deserialize<Tender.Response>(context, out error);
            return result;
        }
        /// <summary>
        /// 公司创建账号通知
        /// </summary>
        /// <param name="context"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public CreateCompanyAccount.Response CreateCompanyAccountNotify(System.Web.HttpContext context, out string error)
        {
            error = "";
            var result = ChinaPayUtil.Deserialize<CreateCompanyAccount.Response>(context, out error);
            return result;
        }

        public override bool CheckOrder(IPayHistory order, out string message)
        {
            throw new NotImplementedException();
        }

        public override bool RefundOrder(IPayHistory order, out string message)
        {
            throw new NotImplementedException();
        }

    }
}
