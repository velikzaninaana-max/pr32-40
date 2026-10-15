using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace pr32_34
{
    public partial class Form1 : Form
    {
        private string[] img = { @"captcha\&f2W.jpeg", @"captcha\dQh4.jpeg", @"captcha\MN9S.jpeg", @"captcha\P6S@.jpeg" };
        Random random = new Random();
        private string currentCaptcha;
        int loginAttempts = 0;
        config config = new config();
        static string dbName = "pr32";

        public Form1()
        {
            InitializeComponent();
            pictureBox1.Cursor = Cursors.Hand;
            textBox2.PasswordChar = '*';
            textBox3.Visible = false;
            pictureBox3.Visible = false;
            // 1. Подписываемся на событие таймаута
            ActivityMonitor.Instance.OnTimeout += HandleTimeout;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                MySqlConnection con = new MySqlConnection(config.con());
                con.Open();
                label4.Text = "Подключение успешно!";
            }
            catch (Exception)
            {
                label4.Text = "Не удалось подключиться";
                MessageBox.Show("Измените подключение в настройках.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                nastroiki nastroiki = new nastroiki();
                this.Visible = false;
                nastroiki.ShowDialog();
                this.Visible = true;
            }
        }

        private void GenerateCaptcha()
        {
            try
            {
                int index = random.Next(img.Length);
                string selectedFile = img[index];
                string fullPath = Path.Combine(selectedFile);
                using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                {
                    pictureBox3.Image = Image.FromStream(stream);
                }
                currentCaptcha = Path.GetFileNameWithoutExtension(selectedFile);
                textBox3.Enabled = true;
                textBox3.Text = "";
            }catch(Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке капчи: " + ex.Message);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (textBox3.Visible)
            {
                if (textBox3.Text != currentCaptcha)
                {
                    MessageBox.Show("Неверная CAPTCHA!");
                    await LockSystem();
                    GenerateCaptcha();
                    return;
                }
            }

            string login = textBox1.Text;
            string password = textBox2.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль.");
                return;
            }

            string sql = $@"SELECT u.UserID, u.UserRole,
                            r.RoleID,
                            r.RoleName,
                            CONCAT_WS(' ', u.UserSurname, u.UserName, u.UserPatronymic) AS full_name FROM user u
                            INNER JOIN role r ON r.RoleID = u.UserRole
                            WHERE UserLogin = @log AND UserPassword = @pass";

            using (MySqlConnection con = new MySqlConnection(new MySqlConnectionStringBuilder(config.con()) { Database = dbName }.ConnectionString))
            {
                try
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@log", login);
                        cmd.Parameters.AddWithValue("@pass", password);

                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                string userID = rdr["UserID"].ToString();
                                string fullName = rdr["full_name"].ToString();
                                string roleId = rdr["UserRole"].ToString();
                                string roleName = rdr["RoleName"].ToString();

                                MyClass.id = userID;
                                MyClass.fio = fullName;
                                MyClass.idrole = roleId;
                                MyClass.role = roleName;

                                this.Hide();
                                MessageBox.Show($"Добро пожаловать, {MyClass.role} {MyClass.fio}","Успешный вход", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                switch (roleId)
                                {
                                    case "1":
                                        Admin Admin = new Admin(fullName);
                                        Admin.ShowDialog();
                                        break;
                                    case "2":
                                        Manager Manager = new Manager(fullName);
                                        Manager.ShowDialog();
                                        break;
                                    case "3":
                                        Client Client = new Client(fullName);
                                        Client.ShowDialog();
                                        break;
                                    default:
                                        break;
                                }
                                this.Show();
                            }
                            else
                            {
                                // Неверный логин или пароль
                                loginAttempts++;
                                if (!textBox3.Visible)
                                {
                                    // Первая неудачная попытка —  CAPTCHA
                                    pictureBox3.Visible = true;
                                    textBox3.Visible = true;
                                    GenerateCaptcha();
                                    textBox3.Clear();

                                    MessageBox.Show("Неверный логин или пароль. Введите CAPTCHA для следующей попытки.");
                                }
                                textBox1.Clear();
                                textBox2.Clear();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка, восстановите бд или импортируйте данные в таблицы: " + ex.Message);
                }
            }
        }
        private async Task LockSystem()
        {
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            textBox3.Enabled = false;
            button1.Enabled = false;
            button3.Enabled = false;
            pictureBox1.Enabled = false;

            MessageBox.Show("Система заблокирована на 10 секунд за превышение попыток.");
            await Task.Delay(10000);

            textBox1.Enabled = true;
            textBox2.Enabled = true;
            button3.Enabled = true;
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox3.Enabled = true;
            button1.Enabled = true;
            pictureBox1.Enabled = true;
            loginAttempts = 0;

            GenerateCaptcha();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            nastroiki nastroiki = new nastroiki();
            this.Visible = false;
            nastroiki.ShowDialog();
            this.Visible = true;
        }

        //вход гостя
        private void button3_Click(object sender, EventArgs e)
        {
            products p = new products();
            p.ShowDialog();
        }

        // 2. Этот метод сработает сам, когда пройдет 30 секунд бездействи
        private void HandleTimeout()
        {
            // Закрываем все открытые формы (Админа, Менеджера и т.д.)
            // Мы идем по списку всех открытых окон и закрываем их
            var openForms = Application.OpenForms.Cast<Form>().ToList();
            foreach (var form in openForms)
            {
                if (form != this) // Не закрываем саму форму логина
                {
                    form.Close();
                }
            }

            // Если форма логина была скрыта — показываем её
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();

            // Очищаем поля (необязательно, но полезно для безопасности)
            textBox1.Clear();
            textBox2.Clear();
            MessageBox.Show("Сессия истекла из-за бездействия. Пожалуйста, войдите снова");
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            textBox1.Clear(); 
            textBox2.Clear(); 
            textBox3.Clear();
            textBox1.Focus();
        }
    }
}
