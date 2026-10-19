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
                            od.OrderDetailsID,
                            od.OrderDetailsCode,
                            CONCAT(' ',
                            CONCAT(LEFT(u.UserSurname, FLOOR(CHAR_LENGTH(u.UserSurname) / 2)),
                            REPEAT('*', CEIL(CHAR_LENGTH(u.UserSurname) / 2))), 
                            CONCAT(LEFT(u.UserName, 1), '.'),
                            CONCAT(LEFT(u.UserPatronymic, 1), '.')) AS fio,
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
                            u.UserPatronymic";
        string whereDiscount = "";
        string orderBy = ""; // ASC / DESC
        private bool _resettingFilters;
        public order_processing()
        {
            InitializeComponent();
        }
        void FillData()
        {
            string sql = orderSql + whereDiscount + orderBy + ";";
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

                dataGridView1.Columns["OrderDetailsID"].Visible = false;
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

            if (dataGridView1.CurrentRow == null)
            {
                comboBox3.Enabled = false;
                dateTimePicker1.Enabled = false;
                button4.Enabled = false;
                return;
            }
            string status = Convert.ToString(dataGridView1.CurrentRow.Cells["Статус заказа"].Value);
            bool isCompleted = status == "Завершен";
            comboBox3.Enabled = !isCompleted;
            dateTimePicker1.Enabled = !isCompleted;
            button4.Enabled = !isCompleted;
        }
        //сортировка заказа по убыванию и возрастанию стоимости
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_resettingFilters)
                return;

            if (comboBox1.SelectedIndex == 0)
                orderBy = " ORDER BY totalSum ASC";
            else if (comboBox1.SelectedIndex == 1)
                orderBy = " ORDER BY totalSum DESC";
            else
                orderBy = "";

            FillData();
        }
        //сброс фильтрации и сортировки
        private void button1_Click(object sender, EventArgs e)
        {
            _resettingFilters = true;

            orderBy = "";
            whereDiscount = "";

            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;

            if (comboBox1.DropDownStyle != ComboBoxStyle.DropDownList)
                comboBox1.Text = "Сортировка по стоимости";

            if (comboBox2.DropDownStyle != ComboBoxStyle.DropDownList)
                comboBox2.Text = "Все диапазоны";

            _resettingFilters = false;

            FillData();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Close();
        }
        //фильтрация по диапазонам скидки
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_resettingFilters)
                return;

            if (comboBox2.SelectedIndex == 0)
                whereDiscount = " HAVING ROUND(discount, 0) BETWEEN 0 AND 10 ";
            else if (comboBox2.SelectedIndex == 1)
                whereDiscount = " HAVING ROUND(discount, 0) BETWEEN 11 AND 14 ";
            else if (comboBox2.SelectedIndex == 2)
                whereDiscount = " HAVING ROUND(discount, 0) >= 15 ";
            else
                whereDiscount = "";

            FillData();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                int orderDetailsId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["OrderDetailsID"].Value);
                MySqlConnection con = new MySqlConnection(new MySqlConnectionStringBuilder(config.con()) { Database = dbName }.ConnectionString);
                con.Open();
                string productCmd = $@"UPDATE orderdetails
                                                        SET OrderDetailsStatus = @status,
                                                            OrderDetailsDeliveryDate = @deliveryDate
                                                        WHERE OrderDetailsID = @id;";

                MySqlCommand cmd = new MySqlCommand(productCmd, con);
                cmd.Parameters.AddWithValue("@status", comboBox3.SelectedItem);
                cmd.Parameters.AddWithValue("@deliveryDate", dateTimePicker1.Value.Date);
                cmd.Parameters.AddWithValue("@id", orderDetailsId);
                int res = cmd.ExecuteNonQuery();
                if (res > 0)
                {
                    FillData();
                }
                MessageBox.Show($"Заказ изменён!", "УСПЕХ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ОШИБКА", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Сначала выберите заказ.");
                return;
            }
            int orderDetailsId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["OrderDetailsID"].Value);
            order_composition order_composition = new order_composition(orderDetailsId);
            order_composition.ShowDialog();
            FillData();
        }
    }
}