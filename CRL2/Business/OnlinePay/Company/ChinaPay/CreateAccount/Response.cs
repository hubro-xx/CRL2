using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Business.OnlinePay.Company.ChinaPay.CreateAccount
{
    public class Response : ResponseBase
    {
        public override string ResponseName
        {
            get { return "C101PerAccOpenCbRq"; }
        }
        /// <summary>
        /// 商户端用户号
        /// </summary>
        public string UserIdMer
        {
            get;
            set;
        }
        /// <summary>
        /// 证件类型
        /// </summary>
        public string IdType
        {
            get;
            set;
        }
        /// <summary>
        /// 证件号码
        /// </summary>
        public string IdNo
        {
            get;
            set;
        }
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name
        {
            get;
            set;
        }
        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile
        {
            get;
            set;
        }
        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email
        {
            get;
            set;
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
        /// 商户私有域
        /// </summary>
        public string MerPriv
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
