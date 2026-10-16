using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;
using System.Xml.Linq;
using System.IO;
using System.Data.SqlClient;

namespace pr32_34
{
    public partial class order_review : Form
    {
        config config = new config();
        static string dbName = "pr32";
        DateTime today;
        DateTime delivery;
        int currentRowIndex = -1;
        string fileName = Directory.GetCurrentDirectory() + @"\temp\rep.docx";
        string OrderDetailsDate, OrderDetailsDeliveryDate, PickUpPointName, OrderDetailsCode;
        int OrderDetailsID;
        public order_review()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
        void FillData()
        {
            string sql = $@"SELECT
                                p.ProductArticleNumber,
                                p.ProductPhoto,
                                p.ProductCost,
                                p.ProductDiscountAmount,
                                CONCAT(
                                    c.CategoryProductrName, ' | ', p.ProductName,
                                    '\nОписание товара: ', p.ProductDescription,
                                    '\nПроизводитель: ', p.ProductManufacturer,
                                    '\nПоставщик: ', s.SuplierName,
                                    '\nЕдиница измерения: ', p.ProductUnitMeasurement,
                                    '\nКоличество на складе: ', p.ProductQuantityInStock,
                                    '\nМаксимальная скидка: ', p.ProductDiscountMax
                                ) AS `Информация о товаре`
                            FROM product AS p
                            INNER JOIN categoryproduct AS c
                                ON p.ProductCategory = c.CategoryProductID
                            INNER JOIN suplier AS s
                                ON p.ProductSuplier = s.SuplierID
                            WHERE ProductArticleNumber IN ('{string.Join("', '", MyClass.Mypr37.Keys.ToArray())}') ";

            MySqlConnection con = new MySqlConnection(new MySqlConnectionStringBuilder(config.con()) { Database = dbName }.ConnectionString);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(sql, con);
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            dataGridView1.DataSource = dt;
            dataGridView1.Columns["ProductArticleNumber"].Visible = false;
            dataGridView1.Columns["ProductPhoto"].Visible = false;
            dataGridView1.Columns["ProductCost"].HeaderText = "Цена";
            dataGridView1.Columns["ProductDiscountAmount"].HeaderText = "Действующая скидка";

            var infoColumn = dataGridView1.Columns["Информация о товаре"];
            if (infoColumn != null)
            {
                infoColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                infoColumn.Width = 300;
                infoColumn.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            }

            if (!dataGridView1.Columns.Contains("ImageColumn"))
            {
                var imageColumn = new DataGridViewImageColumn
                {
                    Name = "ImageColumn",
                    HeaderText = "Фото",
                    ImageLayout = DataGridViewImageCellLayout.Zoom,
                    Width = 200
                };
                dataGridView1.Columns.Insert(0, imageColumn);
            }
            string photo = "";
            // наполнение
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                var value = row.Cells["ProductPhoto"].Value;
                photo = value == null ? "" : value.ToString();
                // ФОТО для товара
                if (photo == "")
                {
                    photo = "picture.png";
                }
                row.Cells["ImageColumn"].Value = MyClass.OpenImage(MyClass.folderResources + photo);
            }
            
            if (!dataGridView1.Columns.Contains("count"))
            {
                dataGridView1.Columns.Add("count", "Количество");
            }
            dataGridView1.AllowUserToAddRows = false;

            foreach (DataGridViewRow r in dataGridView1.Rows)
            {
                string ProductArticleNumber = r.Cells["ProductArticleNumber"].Value.ToString();
                r.Cells["count"].Value = MyClass.Mypr37[ProductArticleNumber];
            }
            con.Close();
        }
        void FillPickupPoint()
        {
            try
            {
                MySqlConnection con = new MySqlConnection(new MySqlConnectionStringBuilder(config.con()) { Database = dbName }.ConnectionString);
                con.Open();
                MySqlCommand cmd = new MySqlCommand(@"SELECT PickUpPointID, PickUpPointName FROM pickuppoint;", con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dtPickuppoint = new DataTable();

                da.Fill(dtPickuppoint);

                comboBox1.DataSource = dtPickuppoint;
                comboBox1.DisplayMember = "PickUpPointName";
                comboBox1.ValueMember = "PickUpPointID";
                comboBox1.SelectedIndex = -1;

                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void order_review_Load(object sender, EventArgs e)
        {
            button2.Enabled = false;
            label4.Text = "ФИО заказчика: " + MyClass.fio;

            today = DateTime.Today;
            delivery = today.AddDays(3);

            label1.Text = "Дата заказа: " + today.ToString("dd.MM.yyyy");
            label2.Text = "Дата доставки: " + delivery.ToShortDateString();

            FillData();
            FillPickupPoint();
            UpdateOrderSummary();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button2.Enabled = comboBox1.SelectedIndex != -1;
        }
        void UpdateOrderSummary()
        {
            decimal sumNoDiscount = 0;
            decimal totalDiscount = 0;
            decimal sumYesDiscount = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow ||
                    row.Cells["ProductCost"].Value == null ||
                    row.Cells["ProductCost"].Value == DBNull.Value ||
                    row.Cells["count"].Value == null ||
                    row.Cells["count"].Value == DBNull.Value)
                {
                    continue;
                }

                decimal price = Convert.ToDecimal(row.Cells["ProductCost"].Value);
                int quantity = Convert.ToInt32(row.Cells["count"].Value);

                object discountValue = row.Cells["ProductDiscountAmount"].Value;
                decimal discountPercent =
                    discountValue == null || discountValue == DBNull.Value
                        ? 0m
                        : Convert.ToDecimal(discountValue);

                decimal productSum = price * quantity;
                decimal productDiscount = productSum * discountPercent / 100m;

                sumNoDiscount += productSum;
                totalDiscount += productDiscount;
                sumYesDiscount += productSum - productDiscount;
            }
            sumNoDis.Text = $"Сумма заказа без скидки: {sumNoDiscount:F2} руб.";
            Dis.Text = $"Скидка: {totalDiscount:F2} руб.";
            sumYesDis.Text = $"Итоговая сумма\nзаказа со скидкой: {sumYesDiscount:F2} руб.";
        }

        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ContextMenu m = new ContextMenu();
                m.MenuItems.Add(new MenuItem("Удалить из заказа", contextmenu_click));
                currentRowIndex = dataGridView1.HitTest(e.X, e.Y).RowIndex;
                if (currentRowIndex >= 0)
                {
                    dataGridView1.Rows[currentRowIndex].Selected = true;
                    m.Show(dataGridView1, new Point(e.X, e.Y));
                }
            }
        }
        void contextmenu_click(object sender, EventArgs e)
        {
            string ProductArticleNumber = dataGridView1.Rows[currentRowIndex].Cells["ProductArticleNumber"].Value.ToString();
            if (MyClass.Mypr37.ContainsKey(ProductArticleNumber))
            {
                MyClass.Mypr37.Remove(ProductArticleNumber);
            }
            FillData();
            UpdateOrderSummary();
            if (MyClass.Mypr37.Count == 0)
            {
                button2.Enabled = false;
                button2.Text = "Корзина пуста";

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MySqlConnection con = new MySqlConnection(new MySqlConnectionStringBuilder(config.con()) { Database = dbName }.ConnectionString);
            con.Open();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;
            string PickupPointID = comboBox1.SelectedValue.ToString();
            string cmdOrder = string.Format(@"INSERT INTO orderdetails(
                                                            OrderDetailsDate, 
                                                            OrderDetailsDeliveryDate, 
                                                            OrderDetailsPickupPoint, 
                                                            OrderDetailsUser, 
                                                            OrderDetailsCode, 
                                                            OrderDetailsStatus
                                                        ) 
                                              VALUES('{0}', '{1}', {2}, {3}, {4}, 'Новый');",
                                                            today.ToString("yyyy-MM-dd"),
                                                            delivery.ToString("yyyy-MM-dd"),
                                                            PickupPointID,
                                                            MyClass.id,
                                                            (new Random()).Next(100, 999)
            );
            string cmdLastId = "SELECT last_insert_id();";
            cmd.CommandText = cmdOrder + cmdLastId;
            string OrderID = cmd.ExecuteScalar().ToString();

            string cmdOrderProduct = @"INSERT INTO orderr VALUES ";
            string OrderProductArticleNumber, OrderQuantity;

            foreach (var item in MyClass.Mypr37)
            {
                OrderProductArticleNumber = item.Key.ToString();
                OrderQuantity = item.Value.ToString();
                cmdOrderProduct += string.Format("({0},'{1}',{2}),", OrderID, OrderProductArticleNumber, OrderQuantity);
            }

            cmd.CommandText = cmdOrderProduct.Substring(0, cmdOrderProduct.Length - 1);
            int res = cmd.ExecuteNonQuery();
            MessageBox.Show("Заказ сформирован", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

            //для талона word
            cmd.CommandText = cmdOrder;
            cmd.ExecuteNonQuery();
            cmd.CommandText = "SELECT LAST_INSERT_ID();";
            int newOrderDetailsID = Convert.ToInt32(cmd.ExecuteScalar());
            LoadOrderDetails(newOrderDetailsID);
            talon();

            con.Close();
            MyClass.Mypr37.Clear();
            this.Close();
        }
        void LoadOrderDetails(int OrderDetailsID)
        {
            string productCmd = @"SELECT OrderDetailsDate,
                                         OrderDetailsDeliveryDate,
                                         PickUpPointName,
                                         OrderDetailsCode
                                  FROM orderdetails
                                  INNER JOIN pickuppoint ON OrderDetailsPickupPoint = PickUpPointID
                                  WHERE OrderDetailsID = @OrderDetailsID;";
            MySqlConnection con = new MySqlConnection(new MySqlConnectionStringBuilder(config.con()) { Database = dbName }.ConnectionString);
            MySqlCommand cmd = new MySqlCommand(productCmd, con);
            cmd.Parameters.AddWithValue("@OrderDetailsID", OrderDetailsID);
            con.Open();
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                if (!reader.Read())
                {
                    MessageBox.Show("Заказ не найден.");
                    return;
                }
                OrderDetailsDate = Convert.ToString(reader["OrderDetailsDate"]);
                OrderDetailsDeliveryDate = Convert.ToString(reader["OrderDetailsDeliveryDate"]);
                PickUpPointName = Convert.ToString(reader["PickUpPointName"]);
                OrderDetailsCode = Convert.ToString(reader["OrderDetailsCode"]);
            }
        }
        void talon()
        {
            try
            {
                Word.Application wordApp = new Word.Application();
                wordApp.Visible = false;

                if (!File.Exists(fileName))
                {
                    MessageBox.Show("Файл шаблона rep.docx не найден в папке temp!");
                    return;
                }

                Word.Document wordDocument = wordApp.Documents.Open(fileName);

                ReplaceWord("{OrderDetailsDate}", OrderDetailsDate, wordDocument);
                ReplaceWord("{OrderDetailsDeliveryDate}", OrderDetailsDeliveryDate, wordDocument);
                ReplaceWord("{PickUpPointName}", PickUpPointName, wordDocument);
                ReplaceWord("{OrderDetailsCode}", OrderDetailsCode, wordDocument);

                wordApp.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании отчета: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        void ReplaceWord(string src, string dest, Word.Document doc)
        {
            Word.Range range = doc.Content;
            range.Find.ClearFormatting();
            range.Find.Execute(FindText: src, ReplaceWith: dest, Replace: Word.WdReplace.wdReplaceAll);
        }
    }
}
