using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VIPER2.Forms
{
    public partial class Debug : Form
    {
        System.Timers.Timer tmrUpdate = new System.Timers.Timer();
        public Debug()
        {
            InitializeComponent();
            tmrUpdate.Interval = 30;
            tmrUpdate.Elapsed += new System.Timers.ElapsedEventHandler(tmrUpdate_Elapsed);
            tmrUpdate.Start();
            int cnt = 0, x = 20, y = 0;
            foreach (cRIO.PARAMETER_NAME p in Enum.GetValues(typeof(cRIO.PARAMETER_NAME)))
            {
                cnt++;
                if (y > gbDebug.Height - 50)
                {
                    x += 350;
                    y = 0;
                }
                y += 25;
                Label l = new Label();
                l.Text = p.ToString();
                l.Location = new Point(x, y);
                l.AutoSize = true;
                Label l1 = new Label();
                l1.Name = "lbl" + p.ToString();
                l1.Text = "-1";
                l1.Location = new Point(x+150, y);
                l1.AutoSize = true;
                gbDebug.Controls.Add(l);
                gbDebug.Controls.Add(l1);
            }
        }

        void tmrUpdate_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            foreach (cRIO.PARAMETER_NAME p in Enum.GetValues(typeof(cRIO.PARAMETER_NAME)))
            {
                Control l = this.Controls.Find("lbl" + p.ToString(), true).First();
                if (l.InvokeRequired)
                    l.Invoke(new MethodInvoker(delegate {l.Text = cRIO.parameters[p].dvalue.ToString();}));
                else
                    l.Text = cRIO.parameters[p].dvalue.ToString();
            }
        }
    }
}
