using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebTest.Code
{
    /// <summary>
    /// 帐户管理
    /// </summary>
    public class AccountManage : CRL.Account.AccountBusiness<AccountManage>
    {
        public static AccountManage Instance
        {
            get { return new AccountManage(); }
        }
        protected override CRL.DBExtend dbHelper
        {
            get { return GetDbHelper(this.GetType()); }
        }
       
    }
}