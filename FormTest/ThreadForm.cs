using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FormTest
{
    public partial class ThreadForm : Form
    {
        public ThreadForm()
        {
            InitializeComponent();

        }
        private void button1_Click(object sender, EventArgs e)
        {
            var test = new Code.CacheTest();
            int n = Convert.ToInt32(txtProgress.Text);
            int n2 = Convert.ToInt32(txtNum.Text) * n;
            var thread = new Code.MultiThreadTest(test, n, n2);
            thread.Run();
        }
    }
}
