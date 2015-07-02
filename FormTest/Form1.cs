using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FormTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var item = WebTest.Code.ProductDataManage.Instance.QueryItemFromAllCache(b => b.Id > 0 && b.ProductName.Contains("product"));
            sw.Stop();

            var str = string.Format("Compile：{0} value:{1}", sw.ElapsedMilliseconds, item.Number);
            MessageBox.Show(str);
        }
    }
}
