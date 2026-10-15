using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pr32_34
{
    public partial class Client : Form
    {
        public Client(string ClientFullName)
        {
            InitializeComponent();
            label1.Text = $"{ClientFullName}";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            products p = new products();
            p.ShowDialog();
        }
    }
}
