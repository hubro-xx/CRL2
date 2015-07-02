using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Business.OnlinePay.Company.ChinaPay.Charge
{
    public class Request : RequestBase
    {
        public override string RequestName
        {
            get { return ""; }
        }
        public override string MessageCode
        {
            get { return "201804"; }
        }
        protected override string InterfacePath
        {
            get { return "/recharge/recharge.htm"; }
        }
        /// <summary>
        /// 用户号
        /// </summary>
        public string UserId
        {
            get;
            set;
        }
        /// <summary>
        /// 金额
        /// </summary>
        public string Amt
        {
            get;
            set;
        }
        /// <summary>
        /// 币种
        /// </summary>
        public string CurCode
        {
            get;
            set;
        }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark
        {
            get;
            set;
        }
        /// <summary>
        /// 商户私有域
        /// </summary>
        public string MerPriv
        {
            get;
            set;
        }
        /// <summary>
        /// 商户后台通知地址
        /// </summary>
        public string NotifyUrl
        {
            get;
            set;
        }
        /// <summary>
        /// 商户前台返回地址
        /// </summary>
        public string ReturnUrl
        {
            get;
            set;
        }

        /// <summary>
        /// 扩展域
        /// </summary>
        public string Reserved
        {
            get;
            set;
        }
    }
}
