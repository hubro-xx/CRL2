using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebTest
{
    public partial class ExportAndImport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {

            string file = Server.MapPath("/data.xml");
            Code.ProductDataManage.Instance.ExportToFile(file, b => b.Id > 0);//导出到文件
            Response.Write("成功导出到文件:" + file);
            var xml = Code.ProductDataManage.Instance.ExportToXml(b => b.Id > 0);//导出为字符串
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            string file = Server.MapPath("/data.xml");
            Code.ProductDataManage.Instance.ImportFromFile(file, b => b.Id > 0);//从文件中导入
            Response.Write("成功从文件导入:" + file);
            //Code.ProductDataManage.Instance.ImportFromXml("", b => b.Id > 0);//从XML序列化字符串导入
        }
    }
}