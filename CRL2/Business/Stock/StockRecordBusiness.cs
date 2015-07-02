using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

namespace CRL.Stock
{
    /// <summary>
    /// 库存管理维护
    /// </summary>
    public class StockRecordBusiness<TType, TModel> : BaseProvider<TModel>
        where TType : class
        where TModel : IStockRecord, new()
    {
        public static StockRecordBusiness<TType, TModel> Instance
        {
            get { return new StockRecordBusiness<TType, TModel>(); }
        }
        protected override DBExtend dbHelper
        {
            get { return GetDbHelper<TType>(); }
        }
        public string CreateTable<TStock>()
            where TStock : Style, new()
        {
            DBExtend helper = dbHelper;
            TStock obj1 = new TStock();
            TModel obj2 = new TModel();
            string msg = obj1.CreateTable(helper);
            msg += obj2.CreateTable(helper);
            return msg;
        }
        /// <summary>
        /// 提交流水
        /// </summary>
        /// <param name="stocks"></param>
        /// <param name="batchNo"></param>
        /// <param name="fromType"></param>
        public void SubmitRecord(List<TModel> stocks, string batchNo, FromType fromType) 
        {
            foreach (TModel c in stocks)
            {
                c.BatchNo = batchNo;
                c.OperateTypeFrom = fromType;
                c.Num = Math.Abs(c.Num);
            }
            BatchInsert(stocks);
        }
        /// <summary>
        /// 确认流水,并更改库存
        /// </summary>
        /// <param name="batchNo"></param>
        /// <param name="operateType"></param>
        public bool ConfirmSubmit<TStock>(string batchNo, StockOperateType operateType)where TStock:Style,new()
        {
            DBExtend helper = dbHelper;
            helper.BeginTran();

            string op = operateType == StockOperateType.出 ? "-" : "+";
            string sql = "update $Style set Num=$Style.num" + op + "b.num from $IStockRecord b where $Style.id=b.styleId and b.Handled=0 and b.batchNo=@batchNo";
            sql += @"
            update $IStockRecord set Handled=1,OperateType=@OperateType,UpdateTime=getdate(),$IStockRecord.num=0" + op + "$IStockRecord.num where batchNo=@batchNo";
            //sql = AutoFormat(sql, typeof(TStock), typeof(TRecord));
            helper.AddParam("batchNo", batchNo);
            helper.AddParam("OperateType", (int)operateType);
            try
            {
                helper.Execute(sql, typeof(TStock), typeof(TModel));
            }
            catch(Exception ero)
            {
                helper.RollbackTran();
                return false;
            }
            helper.CommitTran();
            return true;
        }
        /// <summary>
        /// 查询入库记录
        /// </summary>
        /// <param name="parame"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<TModel> QueryRecord<TRecord>(ParameCollection parame, out int count)
        {
            return QueryListByPage(parame, out count);
        }
    }
}
