using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Business.OnlinePay.Company.ChinaPay.Loan
{
    public class Response : ResponseBase
    {
        public override string ResponseName
        {
            get { return "C101LoanRs"; }
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
        /// 扩展域
        /// </summary>
        public string Reserved
        {
            get;
            set;
        }
    }
}
