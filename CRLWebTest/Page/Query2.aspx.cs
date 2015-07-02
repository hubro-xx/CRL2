using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CRL;
namespace WebTest
{
    public partial class Query2 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        class classA
        {
            public string Name;
            public string Method()
            {
                return "ffffff";
            }
        }
        public enum Status
        {
            正常,
            不正常
        }
        protected void Button1_Click(object sender, EventArgs e)
        {
            var query = Code.ProductDataManage.Instance.GetLamadaQuery();
            query = query.Select(b => new { b.InterFaceUser, b.ProductName, b.PurchasePrice });//选择查询的字段
            int? n2 = 10;
            query = query.Top(10);//取多少条
            query = query.Where(b => b.Id < 700 && b.InterFaceUser == "USER1");//查询条件
            query.Where(b => b.ProductName.Contains("w2") || b.ProductName.Contains("sss"));
            string s="ssss";
            int n = 10;
            classA a = new classA() { Name = "ffffff" };
            query.Where(b => b.ProductName == s && b.Id > n || b.ProductName.Contains("sss"));
            query.Where(b => b.Id == n2.Value);
            query.Where(b => b.ProductName == a.Name);
            query.Where(b => b.ProductName == a.Method());
            query.Where(b=>b.ProductName.Contains("sss"));
            query = query.OrderBy(b => b.Id, true);//排序条件
            var list = Code.ProductDataManage.Instance.QueryList(query);
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            //创建分页存储过程sp_TablesPageNew.sql
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


        protected void Button5_Click(object sender, EventArgs e)
        {
            Code.ProductDataManage.Instance.DynamicQueryTest();
        }

        protected void Button6_Click(object sender, EventArgs e)
        {
            //SUM
            //按条件id>0,合计Number列
            var sum = Code.ProductDataManage.Instance.Sum(b => b.Id > 0, b => b.Number);
            //按条件id>0,进行总计
            var count = Code.ProductDataManage.Instance.Count(b => b.Id > 0);
            var max = Code.ProductDataManage.Instance.Max(b => b.Id > 0, b => b.Id);
            var min = Code.ProductDataManage.Instance.Min(b => b.Id > 0, b => b.Id);
        }

        protected void Button7_Click(object sender, EventArgs e)
        {
            //using CRL以获取扩展方法
            var query = Code.ProductDataManage.Instance.GetLamadaQuery();
            query.Where(b => b.Id > 0);
            query.Top(10);
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
            var list = Code.ProductDataManage.Instance.QueryDynamic(query);
            foreach (dynamic item in list)
            {
                var str = string.Format("{0}______{1} {2} {3}<br>", item.BarCode, item.ProductName, item.total, item.sum1);//动态对象
                Response.Write(str);
            }
        }

        protected void Button8_Click(object sender, EventArgs e)
        {
            var query = Code.ProductDataManage.Instance.GetLamadaQuery();
            query.Where(b => b.Id > 0);
            query.DistinctBy(b => new { b.ProductName, b.BarCode });
            //query.DistinctCount();//表示count Distinct 结果名为Total
            var list = Code.ProductDataManage.Instance.QueryDynamic(query);
            foreach (dynamic item in list)
            {
                var str = string.Format("{0}______{1}<br>", item.ProductName, item.BarCode);//动态对象
                Response.Write(str);
            }
        }
    }
}