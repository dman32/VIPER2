using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Timers;

namespace VIPER2
{
    public partial class TCPDebug : Form
    {
        DataTable dt = new DataTable();
        public TCPDebug()
        {
            InitializeComponent();
            System.Timers.Timer refresh = new System.Timers.Timer();
            refresh.Interval = 80;
            refresh.Elapsed += new ElapsedEventHandler(refresh_Elapsed);
            refresh.Start();
            button1_Click(null, null);
            dt.Columns.Add("Name");
            dt.Columns.Add("Value");
            dt.Columns["Value"].ReadOnly = false;
            foreach (RIO.PARAMETER_NAME name in Enum.GetValues(typeof(RIO.PARAMETER_NAME)))
            {
                DataRow row = dt.NewRow();
                row["Name"] = name;
                row["Value"] = "n/a";
                dt.Rows.Add(row);
            }
            dataGridView1.DataSource = dt;
        }

        void refresh_Elapsed(object sender, ElapsedEventArgs e)
        {
            textBox2.Text = RIO.currentState.ToString();
            checkBox1.Checked = TCP.dataTCP.Connected;
            checkBox2.Checked = TCP.heartTCP.Connected;
            updateValues();
        }
        
        private void updateValues()
        {
            try
            {
                int i = 0;
                foreach (RIO.PARAMETER_NAME name in Enum.GetValues(typeof(RIO.PARAMETER_NAME)))
                {
                    if (name.ToString().StartsWith("DT") || name.ToString().StartsWith("SWITCH") || name.ToString().StartsWith("ERROR"))
                        dt.Rows[i]["Value"] = RIO.getParameter(name).uint64.ToString();
                    else
                        dt.Rows[i]["Value"] = RIO.getParameter(name).processed.ToString();
                    i++;
                }
            }
            catch (Exception Exception) { textBox1.Text += Exception.Message; }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Log.Write("Logging initialized");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            writeText("Initializing configuration file");
            Configuration.init();
        }
        public void writeText(String text)
        {
            textBox1.Text += text + Environment.NewLine;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (TCP.isConnected())
                TCP.disconnect();
            else
                TCP.connect();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            RIO.sendConfig();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            RIO.downloadFile();
        }
    }
}
