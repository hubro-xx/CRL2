using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Business.OnlinePay.Company.ChinaPay.Tender
{
    public class Request:RequestBase
    {
        public override string RequestName
        {
            get { return ""; }
        }
        public override string MessageCode
        {
            get
            {
                return "201831";
            }
        }
        protected override string InterfacePath
        {
            get { return "/tender/tender.htm"; }
        }
        /// <summary>
        /// 标的号
        /// </summary>
        public string SubjectNo
        {
            get;
            set;
        }
        /// <summary>
        /// 商户订单号
        /// </summary>
        public string OrderNoMer
        {
            get;
            set;
        }
        /// <summary>
        /// 投资人用户号
        /// </summary>
        public string PyrUserId
        {
            get;
            set;
        }
        /// <summary>
        /// 借款人用户号
        /// </summary>
        public string PyeUserId
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
        /// 投资金额
        /// </summary>
        public string PyrAmt
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
