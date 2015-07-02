<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Pager.aspx.cs" Inherits="WebTest.Page.Pager" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h4>以下演示了如何进行分页查询</h4>
    <blockquote>
        分页有以下两种形式
        <ul>
            <li>使用内置通用分页存储过程分页,需要拼接条件参数</li>
            <li>使用动态存储过程分页,按表达式编写条件</li>
        </ul>
    </blockquote>
    <p>
        <asp:Button ID="Button3" runat="server" OnClick="Button3_Click" Text="使用动态存储过程分页(推荐)" />
    <pre>
    int pageSize = 15;
    int page = 1;
    int count;
    var query = Code.ProductDataManage.Instance.GetLamadaQuery();
    query.Where(b => b.InterFaceUser == "test");
    query.Page(pageSize, page);//设置分页参数,会自动编译存储过程分页
    query.OrderBy(b => b.Id, true);
    var list = Code.ProductDataManage.Instance.QueryList(query);
    count = query.RowCount;
    </pre>
<asp:Button ID="Button2" runat="server" OnClick="Button2_Click" class="btn" Text="使用自带存储过程分页查询" />
            (在数据库中先创建分页存储过程sp_page.sql)
    <pre>
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
    </pre>
    
    <asp:Button ID="Button4" runat="server" OnClick="Button4_Click" Text="使用动态存储过程GROUP分页" />
        <pre>
    //using CRL以获取扩展方法
    var query = Code.ProductDataManage.Instance.GetLamadaQuery();
    query.Page(15, 1);
    query.Where(b => b.Id > 0);
    int count;
    //选择GROUP字段
    query.Select(b => new
    {
        b.BarCode,
        b.ProductName,
        total = b.BarCode.Count(),//等效为count(BarCode) as total
        sum1 = b.Number.Sum()//等效为sum(Number) as sum1
    });
    //GROUP条件
    query.GroupBy(b => new { b.BarCode, b.ProductName });
    //having
    query.GroupHaving(b => b.Number.SUM() > 1);
    //设置排序
    query.OrderBy(b => b.BarCode.Count(), true);//等效为 order by count(BarCode) desc
    var list = Code.ProductDataManage.Instance.AutoSpGroupPage(query, out count);
    foreach (dynamic item in list)
    {
        var str = string.Format("{0}______{1} {2} {3}<br>", item.BarCode, item.ProductName, item.total,item.sum1);//动态对象
        Response.Write(str);
    }
    </pre>
</asp:Content>
