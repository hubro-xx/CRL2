using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreHelper;
namespace CRL.Account
{
    /// <summary>
    /// 操作类型
    /// </summary>
    public enum OperateType
    {
        /// <summary>
        /// 收入
        /// </summary>
        [ItemDisc("收入")]
        收入 = 1,
        /// <summary>
        /// 支出
        /// </summary>
        [ItemDisc("支出")]
        支出 = 2
    }
}
