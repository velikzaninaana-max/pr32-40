using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace pr32_34
{
    public partial class order_placement : Form
    {
        config config = new config();
        static string dbName = "pr32";
        int currentRowIndex;
        Dictionary<string, int> order_review = new Dictionary<string, int>();
        public order_placement()
        {
            InitializeComponent();
            Fill();
            button1.Visible = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
        void Fill()
        {
            string sql = @"SELECT ProductArticleNumber, ProductName, ProductCost, ProductDiscountAmount FROM product;";
            MySqlConnection con = new MySqlConnection(new MySqlConnectionStringBuilder(config.con()) { Database = dbName }.ConnectionString);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(sql, con);
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            dataGridView1.DataSource = dt;

            dataGridView1.Columns["ProductArticleNumber"].HeaderText = "Артикул";
            dataGridView1.Columns["ProductName"].HeaderText = "Наименование товара";
            dataGridView1.Columns["ProductCost"].HeaderText = "Стоимость";
            dataGridView1.Columns["ProductDiscountAmount"].HeaderText = "Действующая сикдка";

            con.Close();
        }

        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                ContextMenu m = new ContextMenu();
                m.MenuItems.Add(new MenuItem("Добавить к заказу", contextmenu_click));
                this.currentRowIndex = dataGridView1.HitTest(e.X, e.Y).RowIndex;
                dataGridView1.Rows[currentRowIndex].Selected = true;
                m.Show(dataGridView1, new Point(e.X, e.Y));
            }
        }
        void contextmenu_click(object sender, EventArgs e)
        {
            string ProductArticleNumber = dataGridView1.Rows[currentRowIndex].Cells["ProductArticleNumber"].Value.ToString();
            dataGridView1.Rows[currentRowIndex].Selected = false;
            try
            {
                order_review.Add(ProductArticleNumber, 1);
            }
            catch (ArgumentException)
            {
                order_review[ProductArticleNumber] = order_review[ProductArticleNumber] + 1;
            }
            button1.Visible = true;

            UpdateOrderSummary();
        }
        void UpdateOrderSummary()
        {
            decimal sumNoDiscount = 0;
            decimal Discount = 0;
            decimal sumYesDiscount = 0;
            foreach (var item in order_review)
            {
                string article = item.Key;
                int quantity = item.Value;
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow || row.Cells["ProductArticleNumber"].Value?.ToString() != article)
                    {
                        continue;
                    }

                    decimal price = Convert.ToDecimal(row.Cells["ProductCost"].Value);
                    decimal discountPercent = Convert.ToDecimal(row.Cells["ProductDiscountAmount"].Value);

                    decimal itemSum = price * quantity;
                    decimal itemDiscount = itemSum * discountPercent / 100m;

                    sumNoDiscount += itemSum;
                    Discount += itemDiscount;
                    sumYesDiscount += itemSum - itemDiscount;

                    break;
                }
            }
            sumNoDis.Text = "Сумма заказа без скидки: " + sumNoDiscount.ToString("0.00");
            Dis.Text = "Скидка: " + Discount.ToString("0.00");
            sumYesDis.Text = "Итоговая сумма заказа со скидкой: " + sumYesDiscount.ToString("0.00");
        }

        private void order_placement_Load(object sender, EventArgs e)
        {
            sumNoDis.Text = "";
            Dis.Text = "";
            sumYesDis.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MyClass.Mypr37 = order_review;
            //MyClass.id = id;
            //MyClass.fio = fio;
            //MyClass.login = login;
            //MyClass.role = role;

            order_review bs = new order_review();
            this.Hide();
            bs.FormClosed += (s, args) => {
                this.Show();
                order_placement_Load(sender, e); // Вызываем метод для очистки значений
            };

            bs.ShowDialog();

            order_review = MyClass.Mypr37;
            button1.Visible = MyClass.Mypr37.Keys.Count == 0 ? false : true;
        }
    }
}
