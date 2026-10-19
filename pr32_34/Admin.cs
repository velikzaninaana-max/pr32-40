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
    public partial class Admin : Form
    {
        config config = new config();
        public Admin(string AdminFullName)
        {
            InitializeComponent();
            label1.Text = $"{AdminFullName}";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            products products = new products();
            this.Visible = false;
            products.ShowDialog();
            this.Visible = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            order_placement order_placement = new order_placement();
            this.Visible = false;
            order_placement.ShowDialog();
            this.Visible = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            order_processing order_processing = new order_processing();
            this.Visible = false;
            order_processing.ShowDialog();
            this.Visible = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Выберите папку для резервной копии";

                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    string backupPath = BackupService.CreateBackup(
                        config.con(), // замените на вашу строку подключения
                        dialog.SelectedPath);

                    MessageBox.Show("Резервная копия создана:\n" + backupPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удалось создать резервную копию: " + ex.Message);
                }
            }
        }
    }
}
