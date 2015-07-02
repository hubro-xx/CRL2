<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Query3.aspx.cs" Inherits="WebTest.Page.Query3" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h4>以下演示了关联查询</h4>
    <blockquote>
        关联查询有以下两种形式
        <ul>
            <li>返回Select结果,结果为动态对象</li>
            <li>将结果附加给当前对象索引值</li>
        </ul>
        关联查询有累加效果,可关联多个表<br />
        可通过匿名对象指定返回的别名,如 BarCode1 = a.BarCode 返回 BarCode1<br />
        可按参数指定关联方式,Left,Inner,Right,默认为Left
    </blockquote>
    <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="返回筛选查询" />
    <pre>
        //返回筛选值
        var query = Code.ProductDataManage.Instance.GetLamadaQuery();
        query.Join&lt;Code.Member&gt;((a, b) => a.UserId == b.Id && b.Id >0,
            (a, b) => new { BarCode1 = a.BarCode, Name1 = b.Name }, CRL.LambdaQuery.JoinType.Left
            );
        var list = Code.ProductDataManage.Instance.QueryDynamic(query);
        foreach (dynamic item in list)
        {
            var str = string.Format("{0}______{1}", item.BarCode1, item.Name1);
            Response.Write(str);
        }
    </pre>
    <asp:Button ID="Button2" runat="server" OnClick="Button2_Click" Text="关联值附加查询" />
    <pre>
        //把关联值存入对象内部索引
        var query = Code.ProductDataManage.Instance.GetLamadaQuery();
        query.AppendJoinValue&lt;Code.Member&gt;((a, b) => a.UserId == b.Id && b.Id > 0,
            (b) => new { Name1 = b.Name, b.AccountNo }, CRL.LambdaQuery.JoinType.Left
            );
        var list = Code.ProductDataManage.Instance.QueryList(query);
        foreach (var item in list)
        {
            var str = string.Format("{0}______{1}", item.BarCode, item["Name1"]);//取名称为Name1的索引值
            Response.Write(str);
        }
    </pre>
    <asp:Button ID="Button3" runat="server" Text="返回匿名类" OnClick="Button3_Click" />
</asp:Content>
