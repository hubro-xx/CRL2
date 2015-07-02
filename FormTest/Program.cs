using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace FormTest
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //配置数据连接
            CRL.SettingConfig.GetDbAccess = (type) =>
            {
                //可按type区分数据库
                return Code.LocalSqlHelper.TestConnection;
            };
            CRL.CacheServerSetting.AddClientProxy("127.0.0.1", 1129);
            CRL.CacheServerSetting.Init();

            Application.Run(new ThreadForm());
            CRL.CacheServerSetting.Dispose();
        }
    }
}
