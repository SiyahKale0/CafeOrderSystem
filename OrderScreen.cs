using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public class OrderScreen : UserControl
{
    private SQLiteConnection connection;
    private FlowLayoutPanel productPanel, categoryPanel;
    private Panel cartPanel;
    private DataGridView orderDataGridView;
    private TextBox txtSearch;
    private Label lblTotal;
    private Button btnCompleteOrder;
    private int? currentCategoryId = null;

    public event EventHandler OrderCompleted; // Event tanımı

    public OrderScreen(SQLiteConnection conn)
    {
        connection = conn;
        InitializeComponent();
        EnsureConnectionOpen();
        LoadData(); // Initial data loading
    }

    private void EnsureConnectionOpen()
    {
        if (connection.State != ConnectionState.Open)
        {
            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database connection error: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void ExecuteQuery(Action<SQLiteCommand> queryAction)
    {
        EnsureConnectionOpen();
        try
        {
            using (SQLiteCommand cmd = new SQLiteCommand(connection))
            {
                queryAction(cmd);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Database query error: {ex.Message}", "Query Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    Panel rightPanel = new Panel { Dock = DockStyle.Fill };

    private void InitializeComponent()
    {
        // Main Layout
        TableLayoutPanel mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30)); // Left (Cart)
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); // Center (Products)
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20)); // Right (Categories and Search)

        // Left Panel (Cart)
        cartPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
        cartPanel.Controls.Add(InitializeCartPanel());
        mainLayout.Controls.Add(cartPanel, 0, 0);

        // Center Panel (Products)
        productPanel = new FlowLayoutPanel
        {
            AutoScroll = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };
        mainLayout.Controls.Add(productPanel, 1, 0);

        // Right Panel (Categories and Search)

        // Search Box
        txtSearch = new TextBox
        {
            PlaceholderText = "Ürün Ara...",
            Dock = DockStyle.Top,
            Height = 30,
            Margin = new Padding(0, 0, 0, 10)
        };
        txtSearch.TextChanged += TxtSearch_TextChanged;

        // "All Products" Button
        Button btnAllProducts = new Button
        {
            Text = "Tüm Ürünler",
            Dock = DockStyle.Top,
            Height = 40,
            BackColor = Color.LightBlue,
            FlatStyle = FlatStyle.Flat
        };
        btnAllProducts.Click += (s, e) =>
        {
            currentCategoryId = null;
            txtSearch.Clear();
            LoadProducts();
        };

        // Categories Panel
        categoryPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(10)
        };

        // Adding controls to rightPanel - IMPORTANT ORDER
        rightPanel.Controls.Add(txtSearch);
        rightPanel.Controls.Add(btnAllProducts);
        rightPanel.Controls.Add(categoryPanel);

        mainLayout.Controls.Add(rightPanel, 2, 0);
        Controls.Add(mainLayout);
    }


    private Control InitializeCartPanel()
    {
        Panel cartContent = new Panel { Dock = DockStyle.Fill };

        orderDataGridView = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        };
        orderDataGridView.Columns.Add("ProductName", "Ürün");
        orderDataGridView.Columns.Add("ProductPrice", "Fiyat");
        orderDataGridView.Columns.Add("Quantity", "Adet");
        orderDataGridView.Columns.Add("Decrease", "");
        orderDataGridView.Columns.Add("Increase", "");
        orderDataGridView.CellClick += OrderDataGridView_CellClick;
        orderDataGridView.Columns["Decrease"].Width = 30;
        orderDataGridView.Columns["Increase"].Width = 30;

        lblTotal = new Label
        {
            Text = "Toplam: 0 TL",
            Dock = DockStyle.Bottom,
            Height = 30,
            TextAlign = ContentAlignment.MiddleCenter
        };

        btnCompleteOrder = new Button
        {
            Text = "Siparişi Tamamla",
            Dock = DockStyle.Bottom,
            Height = 40
        };
        btnCompleteOrder.Click += BtnCompleteOrder_Click;

        cartContent.Controls.Add(orderDataGridView);
        cartContent.Controls.Add(lblTotal);
        cartContent.Controls.Add(btnCompleteOrder);

        return cartContent;
    }

    private void AddQuantityButtons(DataGridViewRow row)
    {
        DataGridViewButtonCell decreaseButtonCell = new DataGridViewButtonCell();
        decreaseButtonCell.Value = "-";
        decreaseButtonCell.Tag = row;
        orderDataGridView.Rows[row.Index].Cells["Decrease"] = decreaseButtonCell;

        DataGridViewButtonCell increaseButtonCell = new DataGridViewButtonCell();
        increaseButtonCell.Value = "+";
        increaseButtonCell.Tag = row;
        orderDataGridView.Rows[row.Index].Cells["Increase"] = increaseButtonCell;
    }

    private void OrderDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0)
        {
            DataGridViewRow clickedRow = orderDataGridView.Rows[e.RowIndex];
            if (e.ColumnIndex == orderDataGridView.Columns["Decrease"].Index)
            {
                DecreaseItem(clickedRow);
            }
            else if (e.ColumnIndex == orderDataGridView.Columns["Increase"].Index)
            {
                IncreaseItem(clickedRow);
            }
        }
    }

    private void LoadData()
    {
        LoadCategories();
        LoadProducts();
    }
    private void LoadProducts(int? categoryId = null, string searchTerm = null)
    {
        productPanel.Controls.Clear();
        ExecuteQuery(cmd =>
        {
            string sql = "SELECT * FROM Products ";

            if (categoryId.HasValue)
            {
                sql += "WHERE CategoryId = @CategoryId ";
                cmd.Parameters.AddWithValue("@CategoryId", categoryId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += "WHERE Name LIKE @SearchTerm ";
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            }
            cmd.CommandText = sql;

            using (SQLiteDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Button productButton = new Button
                    {
                        Text = $"{reader["Name"]}\n{reader["Price"]} TL",
                        Width = 150,
                        Height = 150,
                        BackColor = Color.WhiteSmoke,
                        FlatStyle = FlatStyle.Flat,
                        Tag = new { Id = reader["Id"], Price = reader["Price"], Name = reader["Name"] }
                    };
                    productButton.Click += ProductButton_Click;
                    productPanel.Controls.Add(productButton);
                }
            }
        });
    }
    private void LoadCategories()
    {
        categoryPanel.Controls.Clear();
        ExecuteQuery(cmd =>
        {
            cmd.CommandText = "SELECT * FROM Categories";
            using (SQLiteDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int categoryId = Convert.ToInt32(reader["Id"]);
                    string categoryName = reader["Name"].ToString();

                    Button categoryButton = new Button
                    {
                        Text = categoryName,
                        Dock = DockStyle.Top,
                        Height = 40,
                        BackColor = Color.LightGray,
                        FlatStyle = FlatStyle.Flat,
                        Tag = categoryId,
                        Margin = new Padding(5)
                    };
                    categoryButton.Click += CategoryButton_Click;
                    rightPanel.Controls.Add(categoryButton); // Note: Added to category panel here
                }
            }
        });
    }
    private void CategoryButton_Click(object sender, EventArgs e)
    {
        Button clickedButton = sender as Button;
        if (clickedButton?.Tag is int categoryId)
        {
            currentCategoryId = categoryId;
            txtSearch.Clear(); // Clear search when category is selected
            LoadProducts(categoryId);
        }
    }
    private void TxtSearch_TextChanged(object sender, EventArgs e)
    {
        string searchTerm = txtSearch.Text;

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            LoadProducts(currentCategoryId);
        }
        else
        {
            currentCategoryId = null;
            LoadProducts(searchTerm: searchTerm);
        }
    }
    private void ProductButton_Click(object sender, EventArgs e)
    {
        Button clickedButton = sender as Button;
        var productData = clickedButton?.Tag as dynamic;
        if (productData != null)
        {
            string productName = productData.Name;
            double productPrice = Convert.ToDouble(productData.Price);

            // Check if the product already exists in the cart
            var existingRow = orderDataGridView.Rows.Cast<DataGridViewRow>()
                .FirstOrDefault(row => row.Cells["ProductName"].Value?.ToString() == productName);

            if (existingRow != null)
            {
                // Increment the quantity
                int currentQuantity = Convert.ToInt32(existingRow.Cells["Quantity"].Value);
                existingRow.Cells["Quantity"].Value = currentQuantity + 1;
            }
            else
            {
                // Add a new item to the cart
                int rowIndex = orderDataGridView.Rows.Add();
                DataGridViewRow newRow = orderDataGridView.Rows[rowIndex];
                newRow.Cells["ProductName"].Value = productName;
                newRow.Cells["ProductPrice"].Value = productPrice.ToString("0.00");
                newRow.Cells["Quantity"].Value = 1;
                AddQuantityButtons(newRow);
            }
            UpdateTotal();
        }
    }

    private void BtnCompleteOrder_Click(object sender, EventArgs e)
    {
        if (orderDataGridView.Rows.Count == 0)
        {
            MessageBox.Show("Sepetinizde ürün bulunmamaktadır!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        SaveOrder();
        MessageBox.Show("Sipariş başarıyla tamamlandı!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        orderDataGridView.Rows.Clear();
        UpdateTotal();
        OnOrderCompleted(); // Sipariş tamamlandı eventini tetikle
    }
    protected virtual void OnOrderCompleted()
    {
        OrderCompleted?.Invoke(this, EventArgs.Empty);
    }
    private void SaveOrder()
    {
        double totalAmount = CalculateTotal();
        ExecuteQuery(cmd =>
        {
            cmd.CommandText = "INSERT INTO Orders (DateTime, TotalAmount) VALUES (@DateTime, @TotalAmount); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@DateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);

            long orderId = (long)cmd.ExecuteScalar();

            foreach (DataGridViewRow row in orderDataGridView.Rows)
            {
                string productName = row.Cells["ProductName"].Value.ToString();
                int quantity = int.Parse(row.Cells["Quantity"].Value.ToString());
                int productId = GetProductId(productName);

                cmd.CommandText = "INSERT INTO OrderItems (OrderId, ProductId, Quantity) VALUES (@OrderId, @ProductId, @Quantity)";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@OrderId", orderId);
                cmd.Parameters.AddWithValue("@ProductId", productId);
                cmd.Parameters.AddWithValue("@Quantity", quantity);
                cmd.ExecuteNonQuery();
            }
        });
    }
    private int GetProductId(string productName)
    {
        int productId = -1;
        ExecuteQuery(cmd =>
        {
            cmd.CommandText = "SELECT Id FROM Products WHERE Name = @ProductName";
            cmd.Parameters.AddWithValue("@ProductName", productName);

            object result = cmd.ExecuteScalar();
            if (result != null && result != DBNull.Value)
                productId = Convert.ToInt32(result);
        });
        return productId;
    }

    private void DecreaseItem(DataGridViewRow selectedRow)
    {
        if (selectedRow != null)
        {
            int currentQuantity = Convert.ToInt32(selectedRow.Cells["Quantity"].Value);
            if (currentQuantity > 1)
            {
                selectedRow.Cells["Quantity"].Value = currentQuantity - 1;
            }
            else
            {
                orderDataGridView.Rows.Remove(selectedRow);
            }
            UpdateTotal();
        }
    }
    private void IncreaseItem(DataGridViewRow selectedRow)
    {
        if (selectedRow != null)
        {
            int currentQuantity = Convert.ToInt32(selectedRow.Cells["Quantity"].Value);
            selectedRow.Cells["Quantity"].Value = currentQuantity + 1;
            UpdateTotal();
        }
    }
    private double CalculateTotal()
    {
        double total = 0;
        foreach (DataGridViewRow row in orderDataGridView.Rows)
        {
            total += Convert.ToDouble(row.Cells["ProductPrice"].Value) * Convert.ToInt32(row.Cells["Quantity"].Value);
        }
        return total;
    }

    private void UpdateTotal()
    {
        double total = CalculateTotal();
        lblTotal.Text = $"Toplam: {total:0.00} TL";
    }
}