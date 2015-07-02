using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Business.OnlinePay.Company.ChinaPay.UserBalanceQuery
{
    public class Response:ResponseBase
    {
        public override string ResponseName
        {
            get { return "A101AcctBalInqRs"; }
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
        /// 用户状态
        /// </summary>
        public string Status
        {
            get;
            set;
        }
        /// <summary>
        /// 账面余额
        /// </summary>
        public string Bal
        {
            get;
            set;
        }
        /// <summary>
        /// 冻结金额
        /// </summary>
        public string FrzBal
        {
            get;
            set;
        }
        /// <summary>
        /// 可用余额
        /// </summary>
        public string AvailableBal
        {
            get;
            set;
        }
        /// <summary>
        /// 可提现余额
        /// </summary>
        public string DrawBal
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
        /// 扩展域
        /// </summary>
        public string Reserved
        {
            get;
            set;
        }

    }
}
