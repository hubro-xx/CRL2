using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
//using CRL;
namespace WebTest
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }


        protected void Button1_Click(object sender, EventArgs e)
        {
            //创建表
            Code.ProductDataManage.Instance.CreateTable();
            Code.AccountManage.Instance.CreateTable();
            Code.TransactionManage.Instance.CreateTable();
            Code.OrderManage.Instance.CreateTable();
            Button3_Click(null, null);
            Response.Write("初始表结构完成" );
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            string file = Server.MapPath("data.xml");
            Code.ProductDataManage.Instance.ExportToFile(file, b => b.Id > 0);
            Response.Write("导出文件为:" + file);
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            string file = Server.MapPath("data.xml");
            Code.ProductDataManage.Instance.ImportFromFile(file, b => b.Id > 0);
            Response.Write("成功导入文件:" + file);
        }
    }
}