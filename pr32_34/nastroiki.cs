using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pr32_34
{
    public partial class nastroiki : Form
    {
        config config = new config();
        public nastroiki()
        {
            InitializeComponent();
        }

        private void nastroiki_Load(object sender, EventArgs e)
        {
            Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            textBox1.Text = currentConfig.AppSettings.Settings["host"].Value;
            textBox3.Text = currentConfig.AppSettings.Settings["uid"].Value;
            textBox4.Text = currentConfig.AppSettings.Settings["pwd"].Value;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            currentConfig.AppSettings.Settings["host"].Value = textBox1.Text;
            currentConfig.AppSettings.Settings["uid"].Value = textBox3.Text;
            currentConfig.AppSettings.Settings["pwd"].Value = textBox4.Text;

            currentConfig.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("appSettings");

            MessageBox.Show($"Новая строка подключения сохранена\nhost={textBox1.Text};uid={textBox3.Text};pwd={textBox4.Text}", "Сохранить", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Application.Restart();
            Environment.Exit(0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string str = $"host={textBox1.Text};uid={textBox3.Text};pwd={textBox4.Text};";

            MySqlConnection con = new MySqlConnection(str);

            try
            {
                con.Open();

                MessageBox.Show($"Успешное подключение к СУБД", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);

                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к {str}\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            import import = new import();
            this.Visible = false;
            import.ShowDialog();
            this.Visible = true;
        }
    }
}
