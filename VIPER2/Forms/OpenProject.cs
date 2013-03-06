using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace VIPER2.Forms
{
    public partial class OpenProject : Form
    {
        public OpenProject()
        {
            InitializeComponent();
            cbProjects.LoadingType = MTGCComboBox.CaricamentoCombo.ComboBoxItem;
            fillProjects();
        }
        private void fillProjects()
        {
            DataTable dt = Database.select("SELECT * FROM projects", null);
            cbProjects.Items.Clear();
            for (int i = 0; i < dt.Rows.Count; i++)
                cbProjects.Items.Add(new MTGCComboBoxItem(dt.Rows[i]["idtest"].ToString(), Convert.ToDateTime(dt.Rows[i]["creationdate"].ToString()).ToShortDateString(), dt.Rows[i]["project"].ToString(), dt.Rows[i]["vehicle"].ToString()));
        }
    }
}
