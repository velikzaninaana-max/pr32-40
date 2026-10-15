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
    public partial class products : Form
    {
        config config = new config();
        static string dbName = "pr32";
        string productCmd = @"SELECT 
									ProductArticleNumber, 
									ProductPhoto,

									CONCAT(CategoryProductrName,' | ', ProductName, 
									'\nОписание товара: ', ProductDescription, 
									'\nПроизводитель: ', ProductManufacturer,
									'\nПоставщик: ', SuplierName,
									'\nЦена: ', ProductCost, 
									'\nЕдиница измерения: ', ProductUnitMeasurement,
									'\nКоличество на склдае: ', ProductQuantityInStock) AS `Информация о товаре`,

									ProductDiscountAmount,
									ProductQuantityInStock
								FROM product
									INNER JOIN categoryproduct ON ProductCategory = CategoryProductID
									INNER JOIN suplier ON ProductSuplier = SuplierID ";
        string whereSupplier = "";
        string having = ""; // посик
        string orderBy = ""; // ASC / DESC
        public products()
        {
            InitializeComponent();
            label1.Text = MyClass.fio;
            if (MyClass.idrole == "1")
            {
                // АДМИНИСТРАТОР
                groupBox1.Show();
                groupBox2.Show();
            }
            else if (MyClass.idrole == "2")
            {
                // МЕНЕДЖЕР
                groupBox1.Show();
                groupBox2.Hide();
                dataGridView1.CellClick -= dataGridView1_CellClick;
            }
            else if (MyClass.idrole == "3")
            {
                // АТОРИЗИРОВАННЫЙ ПОЛЬЗОВАТЕЛЬ
                groupBox1.Hide();
                groupBox2.Hide();
                dataGridView1.CellClick -= dataGridView1_CellClick;
            }
            else
            {
                // ГОСТЬ
                label1.Text = $"{DateTime.Now:dd.MM.yyyy} | Гостевой режим";
                groupBox1.Hide();
                groupBox2.Hide();
                dataGridView1.CellClick -= dataGridView1_CellClick;
            }
        }

        private void products_Load(object sender, EventArgs e)
        {
            UpdateRecordCount();
            FillData();
            FillSupplier();
            Pagination();
        }
        void FillData()
        {
            string sql = productCmd + whereSupplier + having + orderBy;

            MySqlConnection con = new MySqlConnection(new MySqlConnectionStringBuilder(config.con()) { Database = dbName }.ConnectionString);

            dataGridView1.DataSource = null;
            dataGridView1.Columns.Clear();

            try
            {
                con.Open();

                MySqlCommand cmd = new MySqlCommand(sql, con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable t = new DataTable();

                da.Fill(t);
                dataGridView1.DataSource = t;

                // высота строк
                // свойство должно быть установленно AutoSizeRowsMode = None
                //dataGridView1.RowTemplate.Height = 400;
                dataGridView1.Columns["ProductArticleNumber"].Visible = false;
                dataGridView1.Columns["ProductPhoto"].Visible = false;
                dataGridView1.Columns["ProductQuantityInStock"].Visible = false;
                dataGridView1.Columns["ProductDiscountAmount"].HeaderText = "Действующая скидка";

                DataGridViewImageColumn imageColumn = new DataGridViewImageColumn();
                imageColumn.Name = "ImageColumn";
                imageColumn.HeaderText = "Фото";
                imageColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
                imageColumn.Width = 150;
                dataGridView1.Columns.Insert(0, imageColumn);

                string photo = "";
                int percent = 0;
                int count = 0;

                // наполнение
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    var value = row.Cells["ProductPhoto"].Value;
                    photo = value == null ? "" : value.ToString();

                    var value2 = row.Cells["ProductDiscountAmount"].Value;
                    percent = value2 == null || value2 == DBNull.Value
                        ? 0
                        : Convert.ToInt32(value2);

                    var value3 = row.Cells["ProductQuantityInStock"].Value;
                    count = value3 == null || value3 == DBNull.Value
                        ? 0
                        : Convert.ToInt32(value3);


                    // ФОТО для товара
                    if (photo == "")
                    {
                        photo = "picture.png";
                    }
                    //row.Cells[0].Value = Image.FromFile(MyClass.folderResources + photo); // см. класс MyClass
                    row.Cells[0].Value = MyClass.OpenImage(MyClass.folderResources + photo);

                    // СКИДКА больше 
                    if (percent > 15)
                    {
                        row.DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#7fff00");
                    }

                    // КОЛ_ВО на складе
                    if (count == 0)
                    {
                        row.DefaultCellStyle.BackColor = Color.Cyan;
                        //row.DefaultCellStyle.BackColor = Color.Blue;
                    }
                }

                con.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        // ЗАПОЛНЕНИЕ списка поставщиков
        void FillSupplier()
        {
            try
            {
                MySqlConnection con = new MySqlConnection(new MySqlConnectionStringBuilder(config.con()) { Database = dbName }.ConnectionString);
                con.Open();

                MySqlCommand cmd = new MySqlCommand(@"SELECT SuplierID, SuplierName FROM suplier;", con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                da.Fill(dt);

                // строка сброса выбора
                DataRow r = dt.NewRow(); // создаём на основе схемы текущей таблицы
                r["SuplierID"] = 0; // признак сброса
                r["SuplierName"] = "Все поставщики";

                dt.Rows.InsertAt(r, 0); // добавление вначало 

                comboBox2.DataSource = dt;
                comboBox2.DisplayMember = "SuplierName";
                comboBox2.ValueMember = "SuplierID";


                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex == 0 || comboBox2.SelectedIndex == -1)
            {
                whereSupplier = "";
            }
            else
            {
                whereSupplier = $" WHERE ProductSuplier = {comboBox2.SelectedValue.ToString()}";
            }
            FillData();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                orderBy = " ORDER BY ProductQuantityInStock ASC";
                FillData();
            }
            else
            {
                orderBy = " ORDER BY ProductQuantityInStock DESC";
                FillData();
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int i = dataGridView1.CurrentRow.Index;
            MyClass.selectedProduct = dataGridView1.Rows[i].Cells["ProductArticleNumber"].Value.ToString();

            work_products f = new work_products();
            f.ShowDialog();

            MyClass.selectedProduct = "";
            FillData();

            if (MyClass.deletedPhoto != "" && MyClass.deletedPhoto != "picture.png")
            {
                File.Delete(MyClass.folderResources + MyClass.deletedPhoto);
                MyClass.deletedPhoto = "";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 0)
            {
                FillData();
            }
            string str = textBox1.Text;
            having = $" HAVING ProductArticleNumber LIKE '%{str}%' OR  `Информация о товаре` LIKE '%{str}%'";
            FillData();
            UpdateRecordCount();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";

            orderBy = "";
            comboBox1.SelectedIndex = -1;
            comboBox1.Text = "Сортировка по количеству";

            //comboBox2.SelectedIndex = -1;
            //comboBox2.Text = "Все поставщики";   
            comboBox2.SelectedIndex = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MyClass.selectedProduct = ""; //

            work_products f = new work_products();
            f.ShowDialog();

            FillData();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        // создание пагинации        
        void Pagination()
        {
            // удаляем LinkLabel служащий для пагинации
            // каждый раз будем создавать новую пагинацию
            for (int j = 0, count = this.Controls.Count; j < count; ++j)
            {
                this.Controls.RemoveByKey("page" + j);
            }

            // узнаём сколько страниц будет
            int size = dataGridView1.Rows.Count / 20; // на каждой странице по 20 зиписей
            if (Convert.ToBoolean(dataGridView1.Rows.Count % 20)) size += 1; // ситуакиця когда при делении получаем не целое число
            LinkLabel[] ll = new LinkLabel[size]; // пагинация на основе элемента ссылка(можно использовать и другой элемент)
            int x = 14, y = 194, step = 15; // место на форме для меню пагинации и расстояние между номерами страниц

            for (int i = 0; i < size; ++i)
            {
                ll[i] = new LinkLabel();
                ll[i].Text = Convert.ToString(i + 1); // текст(номер старницы) который видет пользователь
                ll[i].Name = "page" + i;
                ll[i].AutoSize = true; //!!!
                ll[i].Location = new Point(x, y);
                ll[i].Click += new EventHandler(LinkLabel_Click); // один обработчик для всех пунктов пагинации
                this.Controls.Add(ll[i]); // добавление на форму

                x += step;
            }

            // чтобы понять на какой странице пользователь убираем подчеркивание для активной странице
            // по умолчанию первая страница активна
            ll[0].LinkBehavior = LinkBehavior.NeverUnderline;
        }

        // выбор страницы пагинации
        // те строки которые нам не нужны на выбраной странице - скрываем
        private void LinkLabel_Click(object sender, EventArgs e)
        {
            // 1. Визуальное обновление ссылок
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is LinkLabel ll)
                {
                    ll.LinkBehavior = LinkBehavior.AlwaysUnderline;
                }
            }

            LinkLabel l = sender as LinkLabel;
            if (l == null) return;
            l.LinkBehavior = LinkBehavior.NeverUnderline;

            // 2. Расчет индексов
            int numPage = Convert.ToInt32(l.Text) - 1;
            int countRows = dataGridView1.Rows.Count;
            int sizePage = 20;
            int start = numPage * sizePage;
            int stop = start + sizePage - 1;

            if (stop >= countRows) stop = countRows - 1;

            // --- УЛЬТИМАТИВНАЯ ОЧИСТКА СОСТОЯНИЯ ---

            // 1. Если ячейка в режиме редактирования — выходим из него
            if (dataGridView1.IsCurrentCellInEditMode)
            {
                dataGridView1.EndEdit();
            }

            // 2. Снимаем фокус с текущей ячейки (переносим фокус на саму таблицу или форму)
            dataGridView1.CurrentCell = null;

            // 3. Снимаем выделение
            dataGridView1.ClearSelection();

            // 4. Приостанавливаем отрисовку
            dataGridView1.SuspendLayout();

            try
            {
                // Используем BeginInvoke, чтобы выполнение скрытия строк 
                // произошло МИГНОВЕНИЕ ПОЗЖЕ, когда Windows закончит обрабатывать клик
                this.BeginInvoke(new MethodInvoker(() =>
                {
                    for (int j = 0; j < countRows; ++j)
                    {
                        bool isVisible = (j >= start && j <= stop);

                        // Дополнительная проверка: не пытаемся ли мы скрыть строку, 
                        // которая всё еще считается "текущей" в памяти
                        if (dataGridView1.Rows[j].Visible != isVisible)
                        {
                            dataGridView1.Rows[j].Visible = isVisible;
                        }
                    }
                    dataGridView1.ResumeLayout();
                    UpdateRecordCount();
                }));
            }
            catch (Exception ex)
            {
                dataGridView1.ResumeLayout();
                MessageBox.Show(ex.Message);
            }
        }
        private void UpdateRecordCount()
        {
            int totalInDatabase;
            MySqlConnection con = new MySqlConnection(new MySqlConnectionStringBuilder(config.con()) { Database = dbName }.ConnectionString);
            MySqlCommand cmd = new MySqlCommand(@"SELECT COUNT(*) FROM product;", con);
            {
                con.Open();
                totalInDatabase = Convert.ToInt32(cmd.ExecuteScalar());
            }
            con.Close();
            // Считаем только те строки, которые пользователь видит на экране
            int visibleCount = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Visible)
                {
                    visibleCount++;
                }
            }

            // Обновляем текст Label
            lblRecordCount.Text = $"Количество записей: {visibleCount} из {totalInDatabase}";
        }

    }
}
