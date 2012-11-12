using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MongoDBDemo.Code;

namespace Analyzer
{
    public partial class frmAnalyzer : Form
    {
        public frmAnalyzer()
        {
            InitializeComponent();
        }

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            //Declarations
            MongoDBUtility objMongoDBUtility = new MongoDBUtility();
            DataTable dtNetworkData;
            String strFilter = txtAnalyze.Text.Trim();
            Int32 intMatches = 0;

            pbAnalyze.Value = 0;

            //Get the network data            
            if (strFilter != String.Empty)
            {

                pbAnalyze.Value = 70;

                //Get the matching network data
                dtNetworkData = objMongoDBUtility.GetData("NetworkData", "NetworkData", strFilter, out intMatches);

                //Bind the data to the grid view
                gvViewer.DataSource = dtNetworkData;

                lblCount.Text = intMatches.ToString() + " matches";

                pbAnalyze.Value = 100;
            }

        }

    }
}
