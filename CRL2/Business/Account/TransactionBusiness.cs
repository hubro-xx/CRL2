using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRL.Account
{
    /// <summary>
    /// 帐户交易
    /// 锁定=>提交流水=>解锁=>确认流水
    /// 此逻辑会生成详细的帐户变动情况
    /// </summary>
    public class TransactionBusiness<TType> : BaseProvider<ITransaction> where TType : class
    {
        public static TransactionBusiness<TType> Instance
        {
            get { return new TransactionBusiness<TType>(); }
        }
        protected override DBExtend dbHelper
        {
            get { return GetDbHelper<TType>(); }
        }


        public new string CreateTable()
        {
            DBExtend helper = dbHelper;
            ITransaction obj1 = new ITransaction();
            //IAccountRecord obj2 = new IAccountRecord();
            string msg = obj1.CreateTable(helper);
            //msg += obj2.CreateTable(helper);
            ILockRecord rec = new ILockRecord();
            msg += rec.CreateTable(helper);
            return msg;
        }
        #region 内部属性
        long serialNumber = 0;
        #endregion
        /// <summary>
        /// 根据交易类型生成当前时间唯一流水号
        /// </summary>
        /// <returns></returns>
        public string GetSerialNumber(int transactionType, object tradeType, int operateType)
        {
            //I/X(收入/支出)01(流水类型)0021(交易类型)
            string pat = (operateType == 1 ? "I" : "X") + ((transactionType + "").PadLeft(2, '0'));
            pat += (tradeType + "").PadLeft(4, '0');
            string no;
            lock (lockObj)
            {
                serialNumber += 1;
                if (serialNumber > 10000)
                    serialNumber = 1;
                no = DateTime.Now.ToString("yyMMddhhmmssff") + serialNumber.ToString().PadLeft(5, '0');
            }
            return pat + no;
        }

        /// <summary>
        /// 获取一个流水记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ITransaction GetTransaction(int id)
        {
            ITransaction item = QueryItem(b => b.Id == id);
            return item;
        }

        #region 提交
        /// <summary>
        /// 判断流水是否提交过
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool CheckTransactionSubmited(DBExtend helper, ITransaction item)
        {
            return helper.QueryItem<ITransaction>(b => b.AccountId == item.AccountId && b.TradeType == item.TradeType && b.Hash == item.Hash, true, true) != null;
        }

        /// <summary>
        /// 提交流水
        /// </summary>
        /// <param name="error"></param>
        /// <param name="items">多个流水,请根据实际情况处理</param>
        /// <returns></returns>
        public bool SubmitTransaction(out string error, params ITransaction[] items)
        {
            error = "";
            if (items.Length == 0)
            {
                error = "流水为空 items";
                return false;
            }
            DBExtend helper = dbHelper;
            foreach (var item in items)
            {
                if (item.Amount == 0)
                {
                    error = "金额不能为0";
                    return false;
                }
                if (string.IsNullOrEmpty(item.Remark))
                {
                    error = "备注必须填写";
                    return false;
                }
                if (string.IsNullOrEmpty(item.OutOrderId))
                {
                    error = "外部订单号必须填写";
                    //return false;
                }
                //if (CheckTransactionSubmited(helper, item))
                //{
                //    error = "该流水已经提交过" + item.ToString();
                //    return false;
                //}
            }
            helper.BeginTran();
            foreach (var item in items)
            {
                bool a = SubmitTransaction(helper, item, out error);
                if (!a)
                {
                    helper.RollbackTran();
                    return false;
                }
            }
            helper.CommitTran();
            return true;
        }


        /// <summary>
        /// 提交流水不带事务
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="item"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool SubmitTransaction(DBExtend helper, ITransaction item, out string error)
        {
            //if (item.TradeType is System.Enum)
            //{
            //    item.TradeType = (int)item.TradeType;
            //}
            error = "";
            var account = AccountBusiness<TType>.Instance.GetAccountFromCache(item.AccountId);
            item.TransactionType = account.TransactionType;
            item.Amount = Math.Abs(item.Amount);
            if (item.OperateType == OperateType.支出)
            {
                item.Amount = 0 - item.Amount;
            }
            if (string.IsNullOrEmpty(item.TransactionNo))
            {
                item.TransactionNo = GetSerialNumber(1, item.TradeType, (int)item.OperateType);
            }

            int transactionId = 0;
            try
            {
                //检测余额
                if (item.OperateType == OperateType.支出 && item.CheckBalance)
                {
                    string sql1 = "select CurrentBalance-LockedAmount from $IAccountDetail with (nolock) where id=@AccountId";
                    //sql1 = FormatTable(sql1);
                    helper.AddParam("AccountId", item.AccountId);
                    var balance = helper.AutoExecuteScalar<decimal>(sql1, typeof(IAccountDetail));
                    //var balance = helper.ExecScalar<decimal>(sql1, typeof(IAccountDetail));
                    if (balance + item.Amount < 0)
                    {
                        error = "对应帐户余额不足";
                        return false;
                    }
                }
                transactionId = helper.InsertFromObj(item);
                string sql = @"
update $ITransaction set CurrentBalance=b.CurrentBalance+Amount,LastBalance=b.CurrentBalance from $IAccountDetail b where b.id=@AccountId and $ITransaction.id=@id
update $IAccountDetail set CurrentBalance=CurrentBalance+@amount where id=@AccountId";
                //helper.Clear();
                helper.AddParam("id", item.Id);
                helper.AddParam("amount", item.Amount);
                helper.AddParam("AccountId", item.AccountId);
                //sql = FormatTable(sql);
                helper.AutoSpUpdate(sql, typeof(ITransaction), typeof(IAccountDetail));
                //helper.Execute(sql, typeof(ITransaction), typeof(IAccountDetail));
            }
            catch (Exception ero)
            {
                error = ero.Message;
                CoreHelper.EventLog.Log("SubmitTransaction 发生错误" + ero, true);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 解锁锁定金额并确认流水
        /// </summary>
        /// <param name="item"></param>
        /// <param name="lockId"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool SubmitTransactionAndUnlock(ITransaction item, int lockId, out string error)
        {
            error = "";
            if (lockId <= 0)
            {
                throw new Exception("lockId值不符合");
            }
            DBExtend helper = dbHelper;
            helper.BeginTran();
            bool a = UnlockAmount(helper, lockId, out error);
            if (!a)
            {
                helper.RollbackTran();
                return false;
            }
            bool b = SubmitTransaction(helper, item, out error);
            if (!b)
            {
                helper.RollbackTran();
                return false;
            }
            helper.CommitTran();
            return true;
        }

        #endregion

        #region 锁定

        /// <summary>
        /// 锁定一定金额
        /// </summary>
        public bool LockAmount(ILockRecord record, out int id, out string message)
        {
            message = "";
            if (record.Amount <= 0)
            {
                id = 0;
                message = "amount格式不正确";
                return false;
            }
            string key = string.Format("LockAmount_{0}_{1}_{2}_{3}", record.AccountId, 0, record.Remark, 0);
            if (!CoreHelper.ConcurrentControl.Check(key, 3))
            {
                throw new Exception("同时提交了多次相同的参数" + key);
            }
            DBExtend helper = dbHelper;
            string sql = "update $IAccountDetail set LockedAmount=LockedAmount+@LockedAmount where id=@AccountId and CurrentBalance-(abs(LockedAmount)+@LockedAmount)>=0";
            //sql = FormatTable(sql);
            helper.AddParam("LockedAmount", Math.Abs(record.Amount));
            helper.AddParam("AccountId", record.AccountId);
            helper.BeginTran();
            try
            {
                int n = helper.Execute(sql, typeof(IAccountDetail));
                if (n == 0)
                {
                    message = "余额不足";
                    id = 0;
                    helper.RollbackTran();
                    return false;
                }
                //helper.Clear();
                id = helper.InsertFromObj(record);
                helper.CommitTran();
            }
            catch (Exception ero)
            {
                message = "提交事物时发生错误:" + ero.Message;
                helper.RollbackTran();
                CoreHelper.ConcurrentControl.Remove(key);
                CoreHelper.EventLog.Log("LockAmount 执行出错" + ero, true);
                throw ero;
            }
            bool ok = id > 0;
            if (!ok)
            {
                CoreHelper.ConcurrentControl.Remove(key);
            }
            return ok;
        }
        /// <summary>
        /// 解锁
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool UnlockAmount(int id, out string message)
        {
            DBExtend helper = dbHelper;
            helper.BeginTran();
            bool a = UnlockAmount(helper, id, out message);
            if (!a)
            {
                helper.RollbackTran();
                return false;
            }
            helper.CommitTran();
            return a;
        }
        /// <summary>
        /// 解锁金额,没有事务
        /// </summary>
        public bool UnlockAmount(DBExtend helper, int lockedId, out string message)
        {
            //helper.Clear();
            var lockRecord = helper.QueryItem<ILockRecord>(b => b.Id == lockedId);
            message = "";
            if (lockRecord == null)
            {
                message = "找不到锁ID:" + lockedId;
                return false;
            }
            if (lockRecord.Checked)
            {
                message = "该锁已经解过ID:" + lockedId;
                return false;
            }
            string key = string.Format("UnlockAmount_{0}", lockedId);
            if (!CoreHelper.ConcurrentControl.Check(key))
            {
                message = "同时提交了多次相同的参数" + key;
                return false;
            }
            string sql = "update $IAccountDetail set LockedAmount=LockedAmount-b.Amount from $ILockRecord b where  $IAccountDetail.id=b.AccountId and b.id=@id ";
            //sql += "update $ILockRecord set checked=1 where id=@id";
            sql += "delete from $ILockRecord where id=@id";
            //sql = FormatTable(sql);
            helper.AddParam("id", lockedId);
            int count;
            try
            {
                count = helper.Execute(sql, typeof(IAccountDetail), typeof(ILockRecord));
            }
            catch (Exception ero)
            {
                CoreHelper.ConcurrentControl.Remove(key);
                CoreHelper.EventLog.Log("UnlockAmount 执行出错" + ero, true);
                message = ero.Message;
                return false;
            }
            CoreHelper.ConcurrentControl.Remove(key);
            return true;
        }
        #endregion

        /// <summary>
        /// 查询用户帐户历史
        /// </summary>
        /// <param name="account"></param>
        /// <param name="transactionType"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<ITransaction> QueryAccountHistory(string account, int accountType, int transactionType, int pageIndex, int pageSize, out int count, DateTime? startTime, DateTime? endTime)
        {
            count = 0;
            string condition = " 1 = 1 ";

            if (startTime != null)
                condition += " and AddTime > '" + startTime + "' ";

            if (endTime != null)
                condition += " and AddTime < '" + endTime + "' ";

            List<ITransaction> list = new List<ITransaction>();
            int accountId = AccountBusiness<TType>.Instance.GetAccountId(account, accountType, transactionType);
            if (accountId == 0)
                return list;
            DBExtend helper = dbHelper;
            ParameCollection c = new ParameCollection();
            c.SetQueryCondition(condition);
            c.SetQueryPageIndex(pageIndex);
            c.SetQueryPageSize(pageSize);
            c["AccountId"] = accountId;
            list = helper.QueryListByPage<ITransaction>(c, out count);
            return list;
        }
    }
}
