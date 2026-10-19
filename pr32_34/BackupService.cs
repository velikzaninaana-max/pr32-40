using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace pr32_34
{
    public static class BackupService
    {
        public static string CreateBackup(string connectionString, string backupFolder)
        {
            config config = new config();
            string dbName = "pr32";

            Directory.CreateDirectory(backupFolder);

            // Метка времени плюс GUID гарантируют уникальность папки.
            string backupPath = Path.Combine(
                backupFolder,
                DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + "_" +
                Guid.NewGuid().ToString("N").Substring(0, 8));

            Directory.CreateDirectory(backupPath);

            using (var connection = new MySqlConnection(new MySqlConnectionStringBuilder(config.con()) { Database = dbName }.ConnectionString))
            {
                connection.Open();

                var tables = new System.Collections.Generic.List<string>();

                using (var command = new MySqlCommand("SHOW FULL TABLES WHERE Table_type = 'BASE TABLE'", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        tables.Add(reader.GetString(0));
                }

                // Индекс связывает CSV и SQL-файл с именем таблицы.
                using (var manifest = new StreamWriter(
                    Path.Combine(backupPath, "manifest.csv"), false, new UTF8Encoding(false)))
                {
                    for (int i = 0; i < tables.Count; i++)
                    {
                        string table = tables[i];
                        string id = i.ToString("D4", CultureInfo.InvariantCulture);

                        // Сохраняем структуру таблицы.
                        using (var command = new MySqlCommand(
                            "SHOW CREATE TABLE " + QuoteIdentifier(table), connection))
                        using (var reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                                throw new InvalidOperationException(
                                    "Не удалось получить структуру таблицы " + table);

                            File.WriteAllText(
                                Path.Combine(backupPath, id + ".sql"),
                                reader.GetString(1) + ";",
                                new UTF8Encoding(false));
                        }

                        // Имя таблицы записано в Base64, чтобы корректно сохранить
                        // даже необычные символы в имени.
                        manifest.WriteLine(
                            id + "," + Convert.ToBase64String(Encoding.UTF8.GetBytes(table)));

                        // Сохраняем данные в CSV без заголовка. Порядок колонок
                        // соответствует порядку колонок в таблице.
                        using (var csv = new StreamWriter(
                            Path.Combine(backupPath, id + ".csv"), false, new UTF8Encoding(false)))
                        using (var command = new MySqlCommand(
                            "SELECT * FROM " + QuoteIdentifier(table), connection))
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var fields = new string[reader.FieldCount];

                                for (int column = 0; column < reader.FieldCount; column++)
                                    fields[column] = EncodeValue(reader.GetValue(column));

                                csv.WriteLine(string.Join(",", fields));
                            }
                        }
                    }
                }
            }

            return backupPath;
        }

        private static string EncodeValue(object value)
        {
            if (value == DBNull.Value)
                return "N";

            if (value is byte[] bytes)
                return "B" + Convert.ToBase64String(bytes);

            string text;

            if (value is DateTime dateTime)
                text = dateTime.ToString("yyyy-MM-dd HH:mm:ss.fffffff",
                                         CultureInfo.InvariantCulture);
            else if (value is TimeSpan timeSpan)
                text = timeSpan.ToString("c", CultureInfo.InvariantCulture);
            else
                text = Convert.ToString(value, CultureInfo.InvariantCulture);

            return "V" + Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        }

        private static string QuoteIdentifier(string identifier)
        {
            return "`" + identifier.Replace("`", "``") + "`";
        }
    }
}
