using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FormTest.Code
{
    public class MultiThreadTest
    {
        ITest currentTest;
        int progress;
        int total;
        public MultiThreadTest(ITest test,int _progress,int _total)
        {
            currentTest = test;
            progress = _progress;
            total = _total;
           
        }
        public void Run()
        {
            thread = new System.Threading.Thread(new System.Threading.ThreadStart(ThreadStart));
            thread.Start();
        }
        System.Threading.Thread thread;
        public void ThreadStart()
        {
            var working = true;
            var list = new List<ITest>();
            for (int i = 0; i < total; i++)
            {
                var item = currentTest.Clone() as ITest;
                item.Data = i;
                list.Add(item);
            }
            var threadSplit = new CoreHelper.ThreadSplit<ITest>(list, progress);
            threadSplit.OnWork = (sender) =>
            {
                foreach (var item in sender)
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    item.Do(item.Data);
                    sw.Stop();
                    item.TotalTime = sw.ElapsedMilliseconds;
                }
            };
            //任务执行完成
            threadSplit.OnFinish += (sender, e) =>
            {
                working = false;
                var n = list.Sum(b => b.TotalTime);
                var avg = n / list.Count;
                System.Windows.Forms.MessageBox.Show(string.Format("总用时{0},平均{1}", n, avg));
            };
            threadSplit.Start();
        }
    }
}
