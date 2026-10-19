using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pr32_34
{
    public partial class order_composition : Form
    {
        private readonly int _orderDetailsId;
        config config = new config();
        static string dbName = "pr32";
        string sqlOr = @"SELECT
                        od.OrderDetailsID,
                        od.OrderDetailsCode AS OrderNumber,
                        p.ProductArticleNumber AS ArticleNumber,
                        p.ProductName AS ProductName,
                        o.OrderQuantity AS OrderQuantity,
                        p.ProductQuantityInStock AS StockQuantity,
                        CONCAT_WS(' ', u.UserSurname, u.UserName, u.UserPatronymic) AS FIO
                    FROM OrderDetails AS od
                    JOIN orderr AS o
                        ON o.OrderID = od.OrderDetailsID
                    JOIN Product AS p
                        ON p.ProductArticleNumber = o.OrderProductArticleNumber
                    JOIN User AS u
                    ON u.UserID = od.OrderDetailsUser
                    WHERE od.OrderDetailsID = @OrderDetailsID
                    ORDER BY p.ProductName;";
        public order_composition(int orderDetailsId)
        {
            InitializeComponent();
            _orderDetailsId = orderDetailsId;
            Load += order_composition_Load;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
        void FillData()
        {
            string sql = sqlOr;
            MySqlConnection con = new MySqlConnection(new MySqlConnectionStringBuilder(config.con()) { Database = dbName }.ConnectionString);
            dataGridView1.DataSource = null;
            dataGridView1.Columns.Clear();
            try
            {
                con.Open();

                MySqlCommand cmd = new MySqlCommand(sql, con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                cmd.Parameters.Add("@OrderDetailsID", (MySqlDbType)SqlDbType.Int).Value = _orderDetailsId;

                DataTable t = new DataTable();
                da.Fill(t);
                dataGridView1.DataSource = t;

                label1.Text = t.Rows.Count > 0 ? Convert.ToString(t.Rows[0]["FIO"]): string.Empty;

                dataGridView1.Columns["StockQuantity"].Visible = false;
                dataGridView1.Columns["OrderNumber"].Visible = false;
                dataGridView1.Columns["OrderDetailsID"].Visible = false;
                dataGridView1.Columns["FIO"].Visible = false;
                dataGridView1.Columns["ArticleNumber"].HeaderText = "Артикул";
                dataGridView1.Columns["ProductName"].HeaderText = "Наименование товара";
                dataGridView1.Columns["OrderQuantity"].HeaderText = "Количество в заказе";

                
                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void order_composition_Load(object sender, EventArgs e)
        {
            FillData();
        }
    }
}
