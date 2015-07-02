using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebTest
{
    public partial class UserTransactionTest : System.Web.UI.Page
    {
        public List<CRL.Account.ITransaction> data;
        int accountType = 0;//可扩展,帐号类型 ,会员?商家
        int transactionType = 0;//可扩展,账户类型 钱?积分?优惠券
        protected void Page_Load(object sender, EventArgs e)
        {
            
            Bind();
        }
        void Bind()
        {
            var account = Code.AccountManage.Instance.GetAccount(TextBox1.Text, accountType, transactionType);
            data = Code.TransactionManage.Instance.QueryList(b => b.AccountId == account.Id && b.TransactionType == 0);
        }
        protected void Button1_Click(object sender, EventArgs e)
        {
            var account = Code.AccountManage.Instance.GetAccount(TextBox1.Text, accountType, transactionType);
            Response.Write("帐户余额为:" + account.CurrentBalance);
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            var account = Code.AccountManage.Instance.GetAccount(TextBox1.Text, accountType, transactionType);
            decimal amount=Convert.ToInt32(TextBox2.Text);
            int op = Convert.ToInt32(drpOperate.SelectedValue);
            //创建交易流水
            var ts = new CRL.Account.ITransaction() { AccountId = account.Id, Amount = amount, OperateType = (CRL.Account.OperateType)op, TradeType = op, Remark = "业务交易:" + amount };
            ts.OutOrderId = DateTime.Now.ToShortTimeString();//外部订单号,会用来判断有没有重复提交
            string error;
            //提交交易
            bool a = Code.TransactionManage.Instance.SubmitTransaction(out error, ts);
            Response.Write("操作" + a + " " + error);
            Bind();
        }
    }
}