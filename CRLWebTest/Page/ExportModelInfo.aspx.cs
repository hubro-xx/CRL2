﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebTest
{
    public partial class ExportModelInfo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Type[] types = new Type[] { typeof(Code.ProductData) };
            string[] xmlFiles = new string[] { Server.MapPath(TextBox1.Text) };
            CRL.SummaryAnalysis.ExportToFile(types, xmlFiles);
        }
    }
}