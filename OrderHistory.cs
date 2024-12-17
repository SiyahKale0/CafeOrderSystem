using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace CafeOrderSystem
{
    public class OrderHistory : UserControl
    {
        private SQLiteConnection connection;
        private DataGridView orderHistoryGridView;
        private Button deleteButton;
        private Label totalAmountLabel;
        private Button detailsButton;

        public OrderHistory(SQLiteConnection conn)
        {
            connection = conn;
            InitializeComponent();
            // Load event'ini burada tetikle
            this.Load += OrderHistory_Load;
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;

            orderHistoryGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            // Details button
            detailsButton = new Button
            {
                Text = "Detaylar",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.LightGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            detailsButton.Click += DetailsButton_Click;

            // Delete button
            deleteButton = new Button
            {
                Text = "Sil",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            deleteButton.Click += DeleteButton_Click;

            // Total amount label
            totalAmountLabel = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 12, FontStyle.Bold),
                BackColor = Color.LightGray
            };

            Controls.Add(orderHistoryGridView);
            Controls.Add(detailsButton);
            Controls.Add(deleteButton);
            Controls.Add(totalAmountLabel);
        }

        private void OrderHistory_Load(object sender, EventArgs e)
        {
            // UserControl her yüklendiğinde verileri yenile
            LoadOrderHistory();
        }


        private void LoadOrderHistory()
        {
            string sql = @"
                 SELECT
                     o.Id AS 'Sipariş ID',
                     o.DateTime AS 'Tarih ve Saat',
                     o.TotalAmount AS 'Toplam Tutar (TL)',
                     strftime('%Y-%m-%d', o.DateTime) AS 'Tarih'
                 FROM Orders o
                 ORDER BY o.DateTime DESC";

            try
            {
                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, connection))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    orderHistoryGridView.DataSource = table;
                }

                // After loading the order history, calculate today's total amount
                DisplayTotalAmountForToday();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message, "Veri Yükleme Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayTotalAmountForToday()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd"); // Get today's date in the correct format
            string sql = @"
                SELECT SUM(o.TotalAmount)
                 FROM Orders o
                 WHERE strftime('%Y-%m-%d', o.DateTime) = @Today";

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Today", today);

                    // Open the connection if it's not already open
                    if (connection.State != ConnectionState.Open)
                        connection.Open();

                    object result = command.ExecuteScalar();

                    // Display the result on the label
                    if (result != DBNull.Value)
                    {
                        totalAmountLabel.Text = "Bugün Yapılan Toplam Tutar: " + result.ToString() + " TL";
                    }
                    else
                    {
                        totalAmountLabel.Text = "Bugün Yapılan Toplam Tutar: 0 TL";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message, "Toplam Tutar Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (orderHistoryGridView.SelectedRows.Count > 0)
                {
                    int orderId = Convert.ToInt32(orderHistoryGridView.SelectedRows[0].Cells["Sipariş ID"].Value);

                    // 1. OrderItems tablosundan ilgili kayıtları sil
                    string deleteOrderItemsSql = "DELETE FROM OrderItems WHERE OrderId = @OrderId";
                    using (SQLiteCommand deleteOrderItemsCmd = new SQLiteCommand(deleteOrderItemsSql, connection))
                    {
                        deleteOrderItemsCmd.Parameters.AddWithValue("@OrderId", orderId);

                        if (connection.State != ConnectionState.Open)
                            connection.Open();

                        deleteOrderItemsCmd.ExecuteNonQuery();
                    }

                    // 2. Orders tablosundan siparişi sil
                    string deleteOrderSql = "DELETE FROM Orders WHERE Id = @Id";
                    using (SQLiteCommand deleteOrderCmd = new SQLiteCommand(deleteOrderSql, connection))
                    {
                        deleteOrderCmd.Parameters.AddWithValue("@Id", orderId);

                        if (connection.State != ConnectionState.Open)
                            connection.Open();
                        deleteOrderCmd.ExecuteNonQuery();
                    }

                    // Sipariş geçmişini güncelle
                    LoadOrderHistory();  // Veri değişikliğinde verileri yenile
                    MessageBox.Show("Sipariş ve ilişkili detayları silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Lütfen silmek için bir sipariş seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message, "Silme Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
        private void DetailsButton_Click(object sender, EventArgs e)
        {
            if (orderHistoryGridView.SelectedRows.Count > 0)
            {
                int orderId = Convert.ToInt32(orderHistoryGridView.SelectedRows[0].Cells["Sipariş ID"].Value);
                ShowOrderDetails(orderId);
            }
            else
            {
                MessageBox.Show("Lütfen detaylarını görmek için bir sipariş seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ShowOrderDetails(int orderId)
        {
            Form orderDetailsForm = new Form
            {
                Text = "Sipariş Detayları",
                Width = 400,
                Height = 300,
                StartPosition = FormStartPosition.CenterScreen
            };
            ListView detailsListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true
            };
            detailsListView.Columns.Add("Ürün Adı", 150);
            detailsListView.Columns.Add("Adet", 50);

            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = @"
                        SELECT p.Name, oi.Quantity
                        FROM OrderItems oi
                        JOIN Products p ON oi.ProductId = p.Id
                        WHERE oi.OrderId = @OrderId";
                    cmd.Parameters.AddWithValue("@OrderId", orderId);
                    if (connection.State != ConnectionState.Open)
                        connection.Open();

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string productName = reader["Name"].ToString();
                            int quantity = Convert.ToInt32(reader["Quantity"]);

                            ListViewItem item = new ListViewItem(productName);
                            item.SubItems.Add(quantity.ToString());

                            detailsListView.Items.Add(item);
                        }
                    }
                }
                orderDetailsForm.Controls.Add(detailsListView);
                orderDetailsForm.ShowDialog();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message, "Detay Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
        public void RefreshOrderHistory()
        {
            LoadOrderHistory();
            DisplayTotalAmountForToday();
        }
    }
}