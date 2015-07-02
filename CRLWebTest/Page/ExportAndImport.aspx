<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="ExportAndImport.aspx.cs" Inherits="WebTest.ExportAndImport" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h4>以下演示了如何导出/导入数据</h4>
    <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="导出数据测试" />
    <pre>
    string file = Server.MapPath("/data.xml");
    Code.ProductDataManage.Instance.ExportToFile(file, b => b.Id > 0);//导出到文件
    Response.Write("成功导出到文件:" + file);
    var xml = Code.ProductDataManage.Instance.ExportToXml(b => b.Id > 0);//导出为字符串
    </pre>
    <asp:Button ID="Button2" runat="server" OnClick="Button2_Click" Text="导入数据测试" />
    <pre>
    string file = Server.MapPath("/data.xml");
    Code.ProductDataManage.Instance.ImportFromFile(file, b => b.Id > 0);//从文件中导入
    Response.Write("成功从文件导入:" + file);
    //Code.ProductDataManage.Instance.ImportFromXml("", b => b.Id > 0);//从XML序列化字符串导入
    </pre>
</asp:Content>
