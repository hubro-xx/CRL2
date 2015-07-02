using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace CRL.Business.OnlinePay.Company.ChinaPay
{
    public class ChinaPayUtil
    {
        /// <summary>
        /// 平台公开KEY
        /// </summary>
        static string platformPublicKey
        {
            get
            {
                string file = CoreHelper.RequestHelper.GetFilePath("/config/chinaPayPublicKey.config");
                var data = System.IO.File.ReadAllText(file);
                return data;
            }
        }
        /// <summary>
        /// 本地私有KEY
        /// </summary>
        static string privateKey
        {
            get
            {
                string file = CoreHelper.RequestHelper.GetFilePath("/config/chinaPayPrivateKey.config");
                var data = System.IO.File.ReadAllText(file);
                return data;
            }
        }
        static SortedDictionary<int, PropertyInfo> GetProperties(Type type)
        {
            SortedDictionary<int, PropertyInfo> dic = new SortedDictionary<int, PropertyInfo>();
            var property = type.GetProperties().ToList();
            property.RemoveAll(b => b.GetSetMethod() == null);
            int index=20;
            foreach (var item in property)
            {
                var f = new FieldIndexAttribute(999);
                object[] attrs = item.GetCustomAttributes(typeof(FieldIndexAttribute), true);
                if (attrs != null && attrs.Length > 0)
                {
                    f = attrs[0] as FieldIndexAttribute;
                    index = f.Index;
                }
                else
                {
                    index += 1;
                }
                dic.Add(index,item);
            }
            return dic;
        }
        public static string GetSubmit(RequestBase msg)
        {
            var property = GetProperties(msg.GetType());
            SetSgin(msg);
            string html = "<form id='form1' name='form1' action='" + msg.InterFaceUrl + "' method='post'>\r\n";
            foreach (var item in property.Values)
            {
                html += string.Format("<input type='hidden' Name='{0}' value='{1}' />\r\n", item.Name, item.GetValue(msg, null));
            }
            html += "</form>\r\n";
            html += "<script>form1.submit()</script>";
            return html;
        }
        public static string GetRequest(RequestBase msg)
        {
            var property = GetProperties(msg.GetType());
            SetSgin(msg);
            string xml = @"<?xml version='1.0' encoding='UTF-8'?>
<UMSFX xmlns='http://www.chinaums.com/UMSFX/1.0'>
<RequestName>"+msg.RequestName+@"</RequestName>
<" + msg.RequestName + @">
<MsgHeader>
<Ver>100</Ver>
<TransCode>" + msg.TransCode + @"</TransCode>
<MerId>" + msg.MerId + @"</MerId>
<SrcReqDate>" + msg.SrcReqDate + @"</SrcReqDate>
<SrcReqId>" + msg.SrcReqId + @"</SrcReqId>
<ChannelId>" + msg.ChannelId + @"</ChannelId>
<RespCode/>
<RespMsg/>
<Signature>" + msg.Signature + @"</Signature>
</MsgHeader>";
           foreach(var item in property)
           {
               if (item.Key < 10)
                   continue;
               var value = item.Value.GetValue(msg, null);
               xml += string.Format("<{0}>{1}</{0}>", item.Value.Name, value);
           }
           xml += @"</" + msg.RequestName + @">
</UMSFX>";
           return xml;
        }
        /// <summary>
        /// 生成签名
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static void SetSgin(RequestBase msg)
        {
            var property = GetProperties(msg.GetType());
            string value = "";
            foreach (var item in property.Values)
            {
                if (item.Name != "Signature")
                {
                    value += item.GetValue(msg, null);
                }
            }
            //CoreHelper.EventLog.Log("签名内容:" + msg.MessageCode + "\r\n" + value, "ChinaPay");
            msg.Signature = MakeSign(value);
        }
        public static string MakeSign(string value)
        {
            var hash = getHash(value);
            var sign = CoreHelper.Encrypt.RSA.SignatureFormatter(privateKey, hash);
            return CoreHelper.MAC.ByteToHex(sign).ToLower();
        }
        public static bool CheckSgin(ResponseBase msg)
        {
            var property = GetProperties(msg.GetType());
            string value = "";
            foreach (var item in property.Values)
            {
                if (item.Name != "Signature")
                {
                    value += item.GetValue(msg, null);
                }
            }
            var hash = getHash(value);
            var sign = CoreHelper.MAC.HexToByte(msg.Signature);
            var a = CoreHelper.Encrypt.RSA.SignatureDeformatter(platformPublicKey, hash, sign);
            if (!a)
            {
                CoreHelper.EventLog.Log("验证签名时不正确," + msg.ResponseName + "\r\n" + value, "ChinaPay");
            }
            return a;
        }
        static byte[] getHash(string str_source)
        {
            HashAlgorithm ha = HashAlgorithm.Create("SHA1");
            byte[] bytes = Encoding.UTF8.GetBytes(str_source);
            byte[] str_hash = ha.ComputeHash(bytes);
            return str_hash;
        }
        public static T Deserialize<T>(System.Web.HttpContext context, out string error) where T : ResponseBase, new()
        {
            if (!string.IsNullOrEmpty(context.Request["Ver"]))
            {
                return Deserialize<T>(context.Request.Form, out error);
            }
            StreamReader stream = new StreamReader(context.Request.InputStream);
            string xml = stream.ReadToEnd();
            return Deserialize<T>(xml, out error);
        }
        /// <summary>
        /// 转换POST值为对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        static T Deserialize<T>(System.Collections.Specialized.NameValueCollection collection, out string error) where T : ResponseBase, new()
        {
            error = "";
            var obj = new T();
            var property = typeof(T).GetProperties().ToList();
            foreach(var item in property)
            {
                var value = collection[item.Name];
                if (value != null)
                {
                    item.SetValue(obj, value, null);
                }
            }
            //检查签名
            if (!CheckSgin(obj))
            {
                Log("签名验证错误:" + CoreHelper.SerializeHelper.XmlSerialize(obj, Encoding.UTF8));
                //throw new Exception("签名验证错误");
                error = "签名验证错误";
                return null;
            }
            if (obj.RespCode != "99999999")
            {
                Log("返回失败:" + CoreHelper.SerializeHelper.XmlSerialize(obj, Encoding.UTF8));
                error = obj.RespMsg;
                return null;
            }
            return obj;
        }
        /// <summary>
        /// 转换消息为对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string xml, out string error) where T : ResponseBase, new()
        {
            error = "";
            var obj = new T();
            var property = typeof(T).GetProperties().ToList();
            //property.RemoveAll(b => b.GetSetMethod() == null);
            var buffer = System.Text.Encoding.UTF8.GetBytes(xml);
            var stream = new System.IO.MemoryStream(buffer);
            XElement rootE;
            try
            {
                rootE = System.Xml.Linq.XElement.Load(stream);
                //head
                IEnumerable<XElement> query =
                                                from ele in rootE.Elements().ToList()[1].Elements().FirstOrDefault().Elements()
                                                select ele;
                foreach (XElement e in query)
                {
                    var p = property.Find(b => b.Name == e.Name.LocalName);
                    if (p != null)
                    {
                        p.SetValue(obj, e.Value, null);
                    }
                }
                IEnumerable<XElement> query2 =
                                                from ele in rootE.Elements().ToList()[1].Elements()
                                                select ele;
                foreach (XElement e in query2)
                {
                    var p = property.Find(b => b.Name == e.Name.LocalName);
                    if (p != null)
                    {
                        p.SetValue(obj, e.Value, null);
                    }
                }
            }
            catch (Exception ero)
            {
                Log("解析XML时出错:" + ero.Message + "\r\n" + xml);
                throw ero;
            }
            //检查签名
            if (!CheckSgin(obj))
            {
                Log("签名验证错误:" + xml);
                throw new Exception("签名验证错误");
                error = "签名验证错误";
                return null;
            }
            if (obj.RespCode != "99999999")
            {
                Log("返回失败:" + xml);
                error = obj.RespMsg;
                return null;
            }
            return obj;
        }
        static void Log(string message)
        {
            CoreHelper.EventLog.Log(message,"ChinaPay");
        }
    }
}
