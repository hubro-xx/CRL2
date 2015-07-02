using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CRL;
namespace WebTest.Page
{
    public partial class Pager : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            //创建分页存储过程sp_page.sql
            int page = 1;
            int pageSize = 15;
            int count;
            CRL.ParameCollection c = new CRL.ParameCollection();
            string where = " InterFaceUser='test'";//按标准 SQL 进行拼接
            c.SetQueryCondition(where);
            c.SetQueryPageIndex((int)page);
            c.SetQueryPageSize(pageSize);

            var list = Code.ProductDataManage.Instance.QueryListByPage(c, out count);
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            int pageSize = 15;
            int page = 1;
            int count;
            var query = Code.ProductDataManage.Instance.GetLamadaQuery();
            query.Where(b => b.InterFaceUser == "test");
            query.Page(pageSize, page);//设置分页参数,会自动编译存储过程分页
            query.OrderBy(b => b.Id, true);
            var list = Code.ProductDataManage.Instance.QueryList(query);
            count = query.RowCount;
        }

        protected void Button4_Click(object sender, EventArgs e)
        {
            //using CRL以获取扩展方法
            var query = Code.ProductDataManage.Instance.GetLamadaQuery();
            query.Page(15, 1);
            query.Where(b => b.Id>0);
            int count;
            //选择GROUP字段
            query.Select(b => new
            {
                b.BarCode,
                b.ProductName,
                total = b.BarCode.COUNT(),//等效为count(BarCode) as total
                sum1 = b.Number.SUM()//等效为sum(Number) as sum1
            });
            //GROUP条件
            query.GroupBy(b => new { b.BarCode, b.ProductName });
            //having
            query.GroupHaving(b => b.Number.SUM() >= 0);
            //设置排序
            query.OrderBy(b => b.BarCode.Count(), true);//等效为 order by count(BarCode) desc
            var list = Code.ProductDataManage.Instance.AutoSpGroupPage(query, out count);
            foreach (dynamic item in list)
            {
                var str = string.Format("{0}______{1} {2} {3}<br>", item.BarCode, item.ProductName, item.total, item.sum1);//动态对象
                Response.Write(str);
            }
        }
    }
}