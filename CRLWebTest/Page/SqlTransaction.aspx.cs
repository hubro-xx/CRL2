using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebTest
{
    public partial class SqlTransaction : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            string message;
            bool a = Code.ProductDataManage.Instance.TransactionTest(out message);
            Response.Write("操作" + a + message);
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            Code.OrderManage.Instance.TransactionTest2();
        }
    }
}