<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="WebTest.About" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <p class="lead">
    当前版本 CRL<%=WebTest.Code.Setting.GetVersion() %> <br />
    暂不发布源码,想深入理解请反编译了解,或留言讨论<br />
    版本更新请关注:<a href="http://www.cnblogs.com/hubro">http://www.cnblogs.com/hubro</a>
    <br />
    QQ群:1582632 
        <br />
        有好的意见或建议请邮箱:hubro@163.com
        <br />
        最后编译时间 2015-03-03
    </p>
</asp:Content>
