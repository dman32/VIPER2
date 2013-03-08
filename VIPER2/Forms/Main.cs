using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace VIPER2.Forms
{
    public partial class Main : Form
    {
        private System.Timers.Timer tmrUpdate = new System.Timers.Timer();

        public Main()
        {
            InitializeComponent();
            SEALib.Configuration.Init("viper2.xml");
            SEALib.Database.OLEDB.Init("viper2.accdb");
            SEALib.Logging.Init("log.txt");
            cRIO.init();
            reindexWeights();
            reindexFLRestraints();
            reindexFRRestraints();
            reindexRLRestraints();
            reindexRRRestraints();
            cbPlatform.SelectedIndex = 0;
            tmrUpdate.Interval = 1;
            tmrUpdate.Elapsed += new System.Timers.ElapsedEventHandler(tmrUpdate_Elapsed);
            tmrUpdate.Start();
        }

        void tmrUpdate_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            toolStripStatusLabel1.Text = cRIO.currentState.ToString() + cRIO.getBufferedSends();
            //updateControlText(txtDescription, cRIO.missedBeats.ToString(), true);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
        }

        private void dgvAxleWeights_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            reindexWeights();
        }

        private void dgvAxleWeights_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            validateWeights();
            recalculateWeights();
        }
        
        private void cbPlatform_SelectedIndexChanged(object sender, EventArgs e)
        {
            label15.Visible = cbPlatform.SelectedIndex == 2;
            numDeckPosition.Visible = cbPlatform.SelectedIndex == 2;

        }

        private void dgvAxleWeights_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            reindexWeights();
        }

        private void reindexWeights()
        {
            for (int i = 0; i < dgvAxleWeights.Rows.Count; i++)
                dgvAxleWeights.Rows[i].Cells["number"].Value = "Axle " + (i + 1).ToString();
        }
        public void recalculateWeights()
        {
            int totalLWeight = 0, totalRWeight = 0;
            foreach (DataGridViewRow dr in dgvAxleWeights.Rows)
            {
                try
                {
                    totalLWeight += int.Parse(dr.Cells["lweight"].Value.ToString());
                }
                catch { }
                try
                {
                    totalRWeight += int.Parse(dr.Cells["rweight"].Value.ToString());
                }
                catch { }
            }
            txtTotalLWeight.Text = totalLWeight.ToString();
            txtTotalRWeight.Text = totalRWeight.ToString();
            txtTotalWeight.Text = (totalLWeight + totalRWeight).ToString();
        }
        public void validateWeights()
        {
            foreach (DataGridViewRow row in dgvAxleWeights.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    switch (cell.ColumnIndex)
                    {
                        case 1://track
                        case 2://lweight
                        case 3://rweight
                        case 4://distance
                            try
                            {
                                if (cell.Value != null)
                                    int.Parse(cell.Value.ToString());
                                cell.ErrorText = "";
                            }
                            catch{ cell.ErrorText = "Not an Integer"; }
                            break;
                    }
                }
            }
        }


        private void reindexFLRestraints()
        {
            for (int i = 0; i < dgvFLRestraints.Rows.Count; i++)
                dgvFLRestraints.Rows[i].Cells["FLComp"].Value = (i + 1).ToString();
        }
        private void reindexFRRestraints()
        {
            for (int i = 0; i < dgvFRRestraints.Rows.Count; i++)
                dgvFRRestraints.Rows[i].Cells["FRComp"].Value = (i + 1).ToString();
        }
        private void reindexRLRestraints()
        {
            for (int i = 0; i < dgvRLRestraints.Rows.Count; i++)
                dgvRLRestraints.Rows[i].Cells["RLComp"].Value = (i + 1).ToString();
        }
        private void reindexRRRestraints()
        {
            for (int i = 0; i < dgvRRRestraints.Rows.Count; i++)
                dgvRRRestraints.Rows[i].Cells["RRComp"].Value = (i + 1).ToString();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            OpenProject op = new OpenProject();
            op.Show();
        }

        private void updateControlText(Control c, string text, bool concatenate)
        {
            if (c.InvokeRequired)
            {
                if (concatenate)
                    c.Invoke(new MethodInvoker(delegate { c.Text += text; }));
                else
                    c.Invoke(new MethodInvoker(delegate { c.Text = text; }));
            }
            else
            {
                if (concatenate)
                    c.Text += text;
                else
                    c.Text = text;
            }
        }


    }
}
