<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="SqlTransaction.aspx.cs" Inherits="WebTest.SqlTransaction" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h4>以下演示了如何使用事务</h4>
    <blockquote>注此事务是由ADO.NET控制,在回滚和提交时,如果遇上网络异常或数据库服务器异常,将导致事务出错<br />
    要保证绝对稳定,建议业务写成存储过程,在存储过程里进行控制</blockquote>
     
    <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="测试事务" />
    <pre>
    message = "";
    var helper = dbHelper;
    helper.BeginTran();
    try
    {
        helper.Delete<ProductData>(b => b.Id == 1);
        var item = new Code.ProductData() { InterFaceUser = "2222", ProductName = "product2", BarCode = "" };
        helper.InsertFromObj(item);//不符合数据校验规则,将会抛出异常
        helper.CommitTran();
        message = "事务已提交";
        return true;
    }
    catch(Exception ero)
    {
        message = ero.Message + " 事务已回滚";
        helper.RollbackTran();
    }
    return false;
    </pre>
    <asp:Button ID="Button2" runat="server"  Text="跨业务实现事务" 
        onclick="Button2_Click" />
    <pre>
    var helper = dbHelper;//当前访问对象
    helper.BeginTran();//开启事物
    try
    {

        var product = new ProductData() { ProductName="test", BarCode="1212" };

        //使用当前会话创建ProductDataManage实例
        //ProductDataManage对象使用的数据访问和当前是一个
        ProductDataManage.ContextInstance(this).Add(product);
        var c = new CRL.ParameCollection();
        c["userId"] = 1;
        helper.Update<Order>(b => b.Id == 100, c);
        helper.CommitTran();//提交
    }
    catch (Exception ex)
    {
        helper.RollbackTran();//如果出错回滚
    }
            </pre>
</asp:Content>
