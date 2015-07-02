using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Business.OnlinePay.Company.ChinaPay.CreateAccount
{
    public class Request:RequestBase
    {
        public override string RequestName
        {
            get { return ""; }
        }
        public override string MessageCode
        {
            get { return "201801"; }
        }
        protected override string InterfacePath
        {
            get { return "/account/reg/person.htm"; }
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
        /// 审核标识
        /// </summary>
        public string IsChecked
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
