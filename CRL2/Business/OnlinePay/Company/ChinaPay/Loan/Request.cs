using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Business.OnlinePay.Company.ChinaPay.Loan
{
    /// <summary>
    /// 放款
    /// </summary>
    public class Request:RequestBase
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
            get { return "201833"; }
        }

        public override string RequestName
        {
            get { return "C101LoanRq"; }
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
        /// 投资金额
        /// </summary>
        public string PyrAmt
        {
            get;
            set;
        }
        /// <summary>
        /// 服务费
        /// </summary>
        public string Fee
        {
            get;
            set;
        }
        /// <summary>
        /// 服务费分账明细
        /// </summary>
        public string DivDetail
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
