using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CRL;
namespace WebTest.Page
{
    public partial class Query3 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var count = Code.MemberManage.Instance.Count(b => b.Id > 0);
            if (count == 0)
            {
                var m = new Code.Member() { Name = "hubro" };
                int n = Code.MemberManage.Instance.Add(m);
                var c = new CRL.ParameCollection();
                c["UserId"] = n;
                Code.ProductDataManage.Instance.Update(b => b.Id > 0, c);
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            //返回筛选值
            var query = Code.ProductDataManage.Instance.GetLamadaQuery();
            query.Top(10);
            query.Join<Code.Member>((a, b) => a.UserId == b.Id && b.Id > 0,
                (a, b) => new { BarCode1 = a.BarCode, Name1 = b.Name }, CRL.LambdaQuery.JoinType.Left
                );
            var list = Code.ProductDataManage.Instance.QueryDynamic(query);
            foreach (dynamic item in list)
            {
                var str = string.Format("{0}______{1}<br>", item.BarCode1, item.Name1);//动态对象
                Response.Write(str);
            }
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            //把关联值存入对象内部索引
            //关联对象值都以索引方式存取
            var query = Code.ProductDataManage.Instance.GetLamadaQuery();
            query.Top(10);
            query.AppendJoinValue<Code.Member>((a, b) => a.UserId == b.Id && b.Id > 0,
                (b) => new { Name1 = b.Name, b.AccountNo }, CRL.LambdaQuery.JoinType.Left
                );
            var list = Code.ProductDataManage.Instance.QueryList(query);
            foreach (var item in list)
            {
                var str = string.Format("{0}______{1}<br>", item.BarCode, item["Name1"]);//取名称为Name的索引值
                Response.Write(str);
            }
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            var query = Code.ProductDataManage.Instance.GetLamadaQuery();
            query.Top(10);
            query.Where(b => b.Id > 0);
            query.Join<Code.Member>((a, b) => a.UserId == b.Id && b.Id > 0,
                (a, b) => new { a.BarCode,b.Name }, CRL.LambdaQuery.JoinType.Left
                );
            var list = query.ReturnSelect(b => new { b.BarCode, Name = b["Name"] });//关联值以索引的方式返回
            foreach (var item in list)
            {
                var str = string.Format("{0}______{1}<br>", item.BarCode, item.Name);//动态对象
                Response.Write(str);
            }
        }
    }
}