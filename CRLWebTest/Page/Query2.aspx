<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Query2.aspx.cs" Inherits="WebTest.Query2" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h4>以下演示了复杂查询</h4>
    <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" class="btn" Text="使表完整表达式查询" />
    <pre>
    var query = Code.ProductDataManage.Instance.GetLamadaQuery();
    //添加引用CRL
    query = query.Select(b =&gt; new { b.InterFaceUser, b.ProductName, b.PurchasePrice });//选择查询的字段
    query = query.Top(10);//取多少条
    query = query.Where(b => b.Id < 700 && b.InterFaceUser == "USER1");//查询条件
    query = query.OrderBy(b => b.Id, true);//排序条件
    var list = Code.ProductDataManage.Instance.QueryList(query);
    </pre>
    <asp:Button ID="Button5" runat="server" OnClick="Button5_Click" class="btn" Text="返回动态对象" />
    <pre>
    //此方法演示根据结果集返回动态对象
    string sql = "select top 10 Id,ProductId,ProductName from ProductData";
    var helper = dbHelper;
    var list = helper.ExecDynamicList(sql);
    //添加引用 Miscorsoft.CSharp程序集
    foreach(dynamic item in list)
    {
        var id = item.Id;
        var productId = item.ProductId;
    }
    </pre>
    <asp:Button ID="Button6" runat="server" OnClick="Button6_Click" class="btn" Text="SUM/COUNT" />
<pre>
    //按条件id>0,合计Number列
    var sum = Code.ProductDataManage.Instance.Sum(b => b.Id > 0, b => b.Number);
    //按条件id>0,进行总计
    var count = Code.ProductDataManage.Instance.Count(b => b.Id > 0);
    var max = Code.ProductDataManage.Instance.Max(b => b.Id > 0, b => b.Id);
    var min = Code.ProductDataManage.Instance.Min(b => b.Id > 0, b => b.Id);
</pre><asp:Button ID="Button7" runat="server" Text="Group查询" OnClick="Button7_Click" />
    <pre>
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
    </pre>
    <asp:Button ID="Button8" runat="server" Text="Distinct" OnClick="Button8_Click" />
    <pre>
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
    </pre>
</asp:Content>
