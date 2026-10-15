using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pr32_34
{
    public partial class Manager : Form
    {
        public Manager(string ManagerFullName)
        {
            InitializeComponent();
            label1.Text = $"{ManagerFullName}";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            products p = new products();
            p.ShowDialog();
        }
    }
}
