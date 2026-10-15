using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Xml.Linq;

namespace pr32_34
{
    public partial class import : Form
    {
        static string dbName = "pr32";
        string pathToDumpDb = Directory.GetCurrentDirectory() + $"\\script\\mysql_script_trade.sql";
        config config = new config();
        public import()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            using (MySqlConnection con = new MySqlConnection(config.con()))
            {
                try
                {
                    string sqlScript = File.ReadAllText(pathToDumpDb);
                    con.Open();
                    MySqlScript script = new MySqlScript(con, sqlScript);
                    int res = script.Execute();
                    MessageBox.Show("База даных успешно восстановлена!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string selectedTable = comboBox1.SelectedItem.ToString();

            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            openFileDialog1.Filter = "CSV файлы (*.csv)|*.csv|Все файлы (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string pathToFile = openFileDialog1.FileName;
                DialogResult confirm = MessageBox.Show($"Вы действительно хотите импортировать данные из файла '{openFileDialog1.SafeFileName}' в таблицу '{selectedTable}'?","Подтверждение импорта",MessageBoxButtons.YesNo,MessageBoxIcon.Question);

                if (confirm == DialogResult.Yes)
                {
                    try
                    {
                        //Читаем данные из CSV файла
                        DataTable dtFromFile = new DataTable();
                        using (StreamReader sr = new StreamReader(pathToFile, Encoding.Default))
                        {
                            //Читаем первую строку 
                            string[] headers = sr.ReadLine().Split(';'); 
                            foreach (string header in headers)
                            {
                                dtFromFile.Columns.Add(header.Trim());
                            }

                            //Читаем остальные строки с данными
                            while (!sr.EndOfStream)
                            {
                                string[] rows = sr.ReadLine().Split(';');
                                dtFromFile.Rows.Add(rows);
                            }
                        }

                        using (MySqlConnection con = new MySqlConnection(new MySqlConnectionStringBuilder(config.con()) { Database = dbName }.ConnectionString))
                        {
                            con.Open();

                            // Получаем список колонок из БД для выбранной таблицы
                            List<string> dbColumns = new List<string>();
                            MySqlCommand getColumnsCmd = new MySqlCommand($"SHOW COLUMNS FROM {selectedTable}", con);

                            using (var reader = getColumnsCmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string colName = reader.GetString(0);
                                    string extra = reader.GetString(5);

                                    // Добавляем все колонки, кроме тех, что заполняются сами (ID)
                                    if (!extra.Contains("auto_increment"))
                                    {
                                        dbColumns.Add(colName);
                                    }
                                }
                            }

                            string columnsPart = string.Join(", ", dbColumns);
                            string parametersPart = string.Join(", ", dbColumns.Select(c => "@" + c));
                            string sqlInsert = $"INSERT INTO {selectedTable} ({columnsPart}) VALUES ({parametersPart})";

                            int rowsImported = 0;
                            foreach (DataRow row in dtFromFile.Rows)
                            {
                                using (MySqlCommand cmd = new MySqlCommand(sqlInsert, con))
                                {
                                    foreach (string dbCol in dbColumns)
                                    {
                                        // Если в файле есть колонка с таким именем - берем значение, иначе NULL
                                        if (dtFromFile.Columns.Contains(dbCol))
                                            cmd.Parameters.AddWithValue("@" + dbCol, row[dbCol]);
                                        else
                                            cmd.Parameters.AddWithValue("@" + dbCol, DBNull.Value);
                                    }
                                    cmd.ExecuteNonQuery();
                                    rowsImported++;
                                }
                            }
                            MessageBox.Show($"Успешно импортировано строк: {rowsImported}", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка! Сначала импортируйте вспомогательные таблицы: " + ex.Message);
                    }
                }
            }
        }
                    

        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(config.con()))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand($"SHOW TABLES FROM {dbName}", con);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Сохраняем текущее выбранное значение, чтобы оно не сбросилось
                        string currentSelected = comboBox1.SelectedItem?.ToString();
                        comboBox1.Items.Clear();

                        while (reader.Read())
                        {
                            comboBox1.Items.Add(reader.GetString(0));
                        }

                        // Возвращаем выделение, если этот элемент всё еще есть в списке
                        if (currentSelected != null && comboBox1.Items.Contains(currentSelected))
                        {
                            comboBox1.SelectedItem = currentSelected;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Восстановите базу данных для выбора таблицы: " + ex.Message);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button2.Enabled = comboBox1.SelectedIndex != -1;
        }
    }
}

