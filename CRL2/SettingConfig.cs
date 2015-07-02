using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CRL
{
    /// <summary>
    /// 获取数据连接
    /// </summary>
    /// <param name="type">区分名称</param>
    /// <returns></returns>
    public delegate CoreHelper.DBHelper GetDbHandler(Type type);

    /// <summary>
    /// 权限验证加密方法
    /// </summary>
    /// <param name="pass"></param>
    /// <returns></returns>
    public delegate string RoleAuthorizeEncryptPassHandler(string pass);
    /// <summary>
    /// 暂用来格式化虚拟字段
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public delegate string StringFormatHandler(string str);

    public delegate CacheServer.ResultData ExpressionDealDataHandler(CacheServer.Command command);
    /// <summary>
    /// 框架部署,请实现委托
    /// </summary>
    public class SettingConfig
    {
        #region 委托
        /// <summary>
        /// 获取数据连接
        /// </summary>
        public static GetDbHandler GetDbAccess;
        /// <summary>
        /// 订单退款时
        /// 需判断订单类型
        /// </summary>
        public static CRL.Business.OnlinePay.OrderDealHandler OnlinePayOrderRefund;
        /// <summary>
        /// 订单确认成功时
        /// 需判断订单类型
        /// </summary>
        public static CRL.Business.OnlinePay.OrderDealHandler OnlinePayConfirmOrder;

        /// <summary>
        /// 权限控制密码加密
        /// </summary>
        public static RoleAuthorizeEncryptPassHandler RoleAuthorizeEncryptPass;
        /// <summary>
        /// 暂存虚拟字段格式化方法
        /// </summary>
        public static StringFormatHandler StringFormat;

        
        #endregion

        /// <summary>
        /// 清除所有内置缓存
        /// </summary>
        public static void ClearCache()
        {
            MemoryDataCache.Clear();
        }
        /// <summary>
        /// 是否使用属性更改通知
        /// 如果使用了,在查询时就不设置源对象克隆
        /// 在实现了属性构造后,可设为true
        /// </summary>
        public static bool UsePropertyChange = false;

    }


}
