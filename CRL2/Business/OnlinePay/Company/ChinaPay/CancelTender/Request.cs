using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Business.OnlinePay.Company.ChinaPay.CancelTender
{
    /// <summary>
    /// 取消投标
    /// </summary>
    public class Request : RequestBase
    {
        protected override string Host
        {
            get
            {
                return "http://210.22.91.77:8404/channel";
            }
        }
        protected override string InterfacePath
        {
            get { return "/Business/EasMerchant/" + RequestName; }
        }

        public override string MessageCode
        {
            get { return "201834"; }
        }

        public override string RequestName
        {
            get { return "C101TenderCancelRq"; }
        }
        /// <summary>
        /// 投标订单号
        /// </summary>
        public string OrderNo
        {
            get;
            set;
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
        /// 投标金额
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
        /// 扩展域
        /// </summary>
        public string Reserved
        {
            get;
            set;
        }
    }
}
