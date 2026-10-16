using Microsoft.Office.Interop.Word;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace pr32_34
{
    public partial class order_processing : Form
    {
        config config = new config();
        static string dbName = "pr32";
        string orderSql = @"SELECT
                            od.OrderDetailsCode,
                            CONCAT(u.UserSurname, ' ', u.UserName, ' ', u.UserPatronymic) AS fio,
                            od.OrderDetailsStatus AS 'Статус заказа',
                            od.OrderDetailsDate AS 'Дата заказа',
                            od.OrderDetailsDeliveryDate AS 'Дата доставки',
                            ROUND(SUM(p.ProductCost * o.OrderQuantity * (1 - COALESCE(p.ProductDiscountAmount, 0) / 100)), 2) AS totalSum,
                            ROUND(SUM(p.ProductCost * o.OrderQuantity * COALESCE(p.ProductDiscountAmount, 0)) / NULLIF(SUM(p.ProductCost * o.OrderQuantity), 0),2) AS discount
                        FROM OrderDetails AS od
                        JOIN orderr AS o
                            ON o.OrderID = od.OrderDetailsID
                        JOIN Product AS p
                            ON p.ProductArticleNumber = o.OrderProductArticleNumber
                        JOIN User AS u
                            ON u.UserID = od.OrderDetailsUser
                        GROUP BY
                            od.OrderDetailsID,
                            od.OrderDetailsCode,
                            od.OrderDetailsDate,
                            od.OrderDetailsDeliveryDate,
                            u.UserSurname,
                            u.UserName,
                            u.UserPatronymic;";
        string whereDiscount = "";
        string orderBy = ""; // ASC / DESC
        public order_processing()
        {
            InitializeComponent();
        }
        void FillData()
        {
            string sql = orderSql + whereDiscount + orderBy;
            MySqlConnection con = new MySqlConnection(new MySqlConnectionStringBuilder(config.con()) { Database = dbName }.ConnectionString);
            dataGridView1.DataSource = null;
            dataGridView1.Columns.Clear();
            try
            {
                con.Open();

                MySqlCommand cmd = new MySqlCommand(sql, con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                System.Data.DataTable t = new System.Data.DataTable();

                da.Fill(t);
                dataGridView1.DataSource = t;

                dataGridView1.Columns["OrderDetailsCode"].HeaderText = "Номер заказа";
                dataGridView1.Columns["fio"].HeaderText = "ФИО клиента";
                dataGridView1.Columns["totalSum"].HeaderText = "Общая сумма заказа";
                dataGridView1.Columns["discount"].HeaderText = "Общий размер скидки заказа";
                string[] visibleColumns = {"OrderDetailsCode", "fio", "totalSum", "discount"};
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    column.Visible = visibleColumns.Contains(column.Name);
                }
                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void order_processing_Load(object sender, EventArgs e)
        {
            FillData();
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null || dataGridView1.CurrentRow.IsNewRow) return;

            DataGridViewRow row = dataGridView1.CurrentRow;

            comboBox3.Text = Convert.ToString(row.Cells["Статус заказа"].Value);
            var value = row.Cells["Дата заказа"].Value;
            label6.Text = value == null || value == DBNull.Value
                ? ""
                : Convert.ToDateTime(value).ToString("dd.MM.yyyy");
            dateTimePicker1.Value = Convert.ToDateTime(row.Cells["Дата доставки"].Value);
        }
        //сортировка заказа по убыванию и возрастанию стоимости
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                orderBy = " ORDER BY totalSum ASC";
                FillData();
            }
            else
            {
                orderBy = " ORDER BY totalSum DESC";
                FillData();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            orderBy = "";
            comboBox1.SelectedIndex = -1;
            comboBox1.Text = "Сортировка по стоимости";

            //comboBox2.SelectedIndex = -1;
            //comboBox2.Text = "Все поставщики";   
            //comboBox2.SelectedIndex = 0;
        }
    }
}
