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
using System.IO;
using System.Xml.Linq;

namespace pr32_34
{
    public partial class work_products : Form
    {
        bool isAdded = true; // признак открытия на добавление/редактирования
        string art; // генерируемый артикул при добавлении
        string photoPath = ""; // полный путь выбраного фото
        string photoName = ""; // только имя.расширение
        string photoDB = ""; // из БД
        config config = new config();
        static string dbName = "pr32";
        public work_products()
        {
            InitializeComponent();
            // определение режима открытия формы
            isAdded = MyClass.selectedProduct == "" ? true : false;
            FillComboBox();
        }

        private void work_products_Load(object sender, EventArgs e)
        {
            if (isAdded)
            {
                button3.Hide();
                button4.Hide();

                GenerateArticle();
            }
            else
            {
                label10.Text = MyClass.selectedProduct;
                button1.Hide();
                FillData();
            }
        }
        //ЗАПОЛНЕНИЕ СПИСКОВ
        void FillComboBox()
        {
            try
            {
                MySqlConnection con = new MySqlConnection(new MySqlConnectionStringBuilder(config.con()) { Database = dbName }.ConnectionString);
                con.Open();

                // КАТЕГОРИИ
                MySqlCommand cmd = new MySqlCommand(@"SELECT CategoryProductID, CategoryProductrName FROM categoryproduct;", con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dtCategory = new DataTable();

                da.Fill(dtCategory);

                comboBox1.DataSource = dtCategory;
                comboBox1.DisplayMember = "CategoryProductrName";
                comboBox1.ValueMember = "CategoryProductID";

                // ПОСТАВЩИК
                cmd.CommandText = @"SELECT SuplierID, SuplierName FROM Suplier;";
                da = new MySqlDataAdapter(cmd);
                DataTable dtSupplier = new DataTable();

                da.Fill(dtSupplier);

                comboBox3.DataSource = dtSupplier;
                comboBox3.DisplayMember = "SuplierName";
                comboBox3.ValueMember = "SuplierID";

                // ПРОИЗВОДИТЕЛЬ
                cmd.CommandText = @"SELECT DISTINCT ProductManufacturer FROM product;";
                da = new MySqlDataAdapter(cmd);
                DataTable dtManufacturer = new DataTable();

                da.Fill(dtManufacturer);

                comboBox2.DataSource = dtManufacturer;
                comboBox2.DisplayMember = "ProductManufacturer";
                comboBox2.ValueMember = "ProductManufacturer";

                // ЕДИНИЦА ИЗМЕРЕНИЯ
                cmd.CommandText = @"SELECT DISTINCT ProductUnitMeasurement FROM product;";
                da = new MySqlDataAdapter(cmd);
                DataTable dtUnitMeasurement = new DataTable();

                da.Fill(dtUnitMeasurement);

                comboBox4.DataSource = dtUnitMeasurement;
                comboBox4.DisplayMember = "ProductUnitMeasurement";
                comboBox4.ValueMember = "ProductUnitMeasurement";

                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;
            comboBox3.SelectedIndex = -1;
            comboBox4.SelectedIndex = -1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                MySqlConnection con = new MySqlConnection(new MySqlConnectionStringBuilder(config.con()) { Database = dbName }.ConnectionString);
                con.Open();

                if (photoName != "")
                {
                    File.Copy(photoPath, MyClass.folderResources + photoName, true);
                }

                string productCmd = @"INSERT INTO product (ProductArticleNumber, ProductName, ProductUnitMeasurement, ProductCost, ProductDiscountMax, ProductManufacturer, ProductSuplier, ProductCategory, ProductDiscountAmount, ProductQuantityInStock, ProductDescription, ProductPhoto)
                                   VALUES (@art, @name, @unit, @cost, @maxDiscount, @manufacturer, @supplier, @category, @discountAmount, @quantity, @description, @photo);";

                using (MySqlCommand cmd = new MySqlCommand(productCmd, con))
                {
                    // Безопасное добавление параметров с их значениями
                    cmd.Parameters.AddWithValue("@art", art);
                    cmd.Parameters.AddWithValue("@name", textBox1.Text);
                    cmd.Parameters.AddWithValue("@unit", comboBox4.SelectedValue ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@cost", textBox3.Text);
                    cmd.Parameters.AddWithValue("@maxDiscount", textBox6.Text);
                    cmd.Parameters.AddWithValue("@manufacturer", comboBox2.SelectedValue ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@supplier", comboBox3.SelectedValue ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@category", comboBox1.SelectedValue ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@discountAmount", textBox5.Text);
                    cmd.Parameters.AddWithValue("@quantity", textBox4.Text);
                    cmd.Parameters.AddWithValue("@description", textBox2.Text);
                    cmd.Parameters.AddWithValue("@photo", string.IsNullOrEmpty(photoName) ? DBNull.Value : (object)photoName);

                    int res = cmd.ExecuteNonQuery();

                    MessageBox.Show($"Добавлен {res} записей", "УСПЕХ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ОШИБКА", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //ГЕНЕРАЦИЯ АРТИКУЛА
        void GenerateArticle()
        {
            Random rand = new Random();
            string alf = "QWERTYUIOPASDFGHJKLZXCVBNM";
            art = alf[rand.Next(0, 26)].ToString() + rand.Next(100, 999).ToString() + alf[rand.Next(0, 26)].ToString() + rand.Next(0, 9).ToString();

            label10.Text = art;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";

            openFileDialog1.ShowDialog();

            photoPath = openFileDialog1.FileName;
            photoName = openFileDialog1.SafeFileName;

            if (photoName != "")
            {
                Image img = Image.FromFile(photoPath);
                int w = img.Width;
                int h = img.Height;
                pictureBox2.Image = img;


                //if (w <= 1000 && h <= 2000)
                //{
                //    pictureBox2.Image = img;
                //}
                //else
                //{
                //    MessageBox.Show($"Фото {photoPath} слишком большое", "ОШИБКА", MessageBoxButtons.OK, MessageBoxIcon.Error);

                //    photoPath = "";
                //    photoName = "";
                //}
            }
        }

        //ЗАПОЛНЕНИЕ ПОЛЕЙ
        void FillData()
        {
            try
            {
                MySqlConnection con = new MySqlConnection(new MySqlConnectionStringBuilder(config.con()) { Database = dbName }.ConnectionString);
                con.Open();

                MySqlCommand cmd = new MySqlCommand($@"SELECT * FROM product WHERE ProductArticleNumber = '{MyClass.selectedProduct}';", con);

                MySqlDataReader rdr = cmd.ExecuteReader();
                rdr.Read();

                textBox1.Text = rdr["ProductName"].ToString();
                textBox2.Text = rdr["ProductDescription"].ToString();
                textBox3.Text = rdr["ProductCost"].ToString();
                textBox4.Text = rdr["ProductQuantityInStock"].ToString();
                textBox5.Text = rdr["ProductDiscountAmount"].ToString();
                textBox6.Text = rdr["ProductDiscountMax"].ToString();

                photoDB = rdr["ProductPhoto"].ToString() == "" ? "picture.png" : rdr["ProductPhoto"].ToString();
                pictureBox2.Image = MyClass.OpenImage(MyClass.folderResources + photoDB);

                comboBox1.SelectedValue = Convert.ToInt32(rdr["ProductCategory"].ToString());
                comboBox3.SelectedValue = Convert.ToInt32(rdr["ProductSuplier"].ToString());
                comboBox2.SelectedValue = rdr["ProductManufacturer"].ToString();
                comboBox4.SelectedValue = rdr["ProductUnitMeasurement"].ToString();

                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"Вы точно хотите удалить товар {MyClass.selectedProduct}", "ВНИМАНИЕ", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    MySqlConnection con = new MySqlConnection(new MySqlConnectionStringBuilder(config.con()) { Database = dbName }.ConnectionString);
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand($@"DELETE FROM product WHERE ProductArticleNumber = '{MyClass.selectedProduct}';", con);

                    int res = cmd.ExecuteNonQuery();
                    MessageBox.Show($"Удалено {res} записей", "ИНФОРМАЦИЯ", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    con.Close();

                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Товар {MyClass.selectedProduct} нельзя удалить" , "ВНИМАНИЕ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                MySqlConnection con = new MySqlConnection(new MySqlConnectionStringBuilder(config.con()) { Database = dbName }.ConnectionString);
                con.Open();


                if (photoName != "")
                {
                    File.Copy(photoPath, MyClass.folderResources + photoName, true);
                    MyClass.deletedPhoto = photoDB;
                }
                else
                {
                    photoName = photoDB;
                }

                string productCmd = $@"UPDATE Product SET  

                                            ProductName = '{textBox1.Text}', 
                                            ProductCost = {textBox3.Text}, 
                                            ProductSuplier = {comboBox3.SelectedValue}, 
                                            ProductCategory = {comboBox1.SelectedValue}, 
                                            ProductDiscountAmount = {textBox5.Text}, 
                                            ProductQuantityInStock = {textBox4.Text}, 
                                            ProductDescription = '{textBox2.Text}',
                                            ProductPhoto = '{photoName}',
                                            ProductDiscountMax = {textBox6.Text},
                                            ProductManufacturer = '{comboBox2.SelectedValue}',
                                            ProductUnitMeasurement = '{comboBox4.SelectedValue}'

                                      WHERE ProductArticleNumber = '{MyClass.selectedProduct}';";

                MySqlCommand cmd = new MySqlCommand(productCmd, con);
                int res = cmd.ExecuteNonQuery();

                MessageBox.Show($"Изменено {res} записей", "УСПЕХ", MessageBoxButtons.OK, MessageBoxIcon.Information);

                con.Close();

                // ЗАКРЫВАЕМ после редактирования
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ОШИБКА", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
