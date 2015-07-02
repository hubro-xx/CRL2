<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="ExtensionMethod.aspx.cs" Inherits="WebTest.Page.Extension" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h4>以下演示了如何使用扩展方法</h4>
    <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" class="btn" Text="查询测试" />
    <blockquote>
    using CRL 以获取扩展方法
    <br />一些系统方法已经解析,不支持的方法将会抛出异常<br />
    目前只实现了MSSQL数据库
    </blockquote>
    <pre>
    var query = Code.ProductDataManage.Instance.GetLamadaQuery();
    query.Where(b => b.ProductName.Contains("122"));//包含字符串
    query.Where(b => b.ProductName.In("111", "222"));//string in
    query.Where(b => b.AddTime.Between(DateTime.Now, DateTime.Now));//在时间段内
    query.Where(b => b.AddTime.DateDiff(DatePart.dd, DateTime.Now) > 1);//时间比较
    query.Where(b => b.ProductName.Substring(0, 3) == "222");//截取字符串
    query.Where(b => b.Id.In(1, 2, 3));//in
    query.Where(b => b.Id.NotIn(1, 2, 3));//not in
    var list = Code.ProductDataManage.Instance.QueryList(query);
    </pre>
</asp:Content>
