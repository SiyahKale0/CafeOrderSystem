using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace CafeOrderSystem
{
    public class ProductManagement : UserControl
    {
        private SQLiteConnection connection;
        private DataGridView productGridView;
        private TextBox txtProductName, txtProductPrice;
        private ComboBox cmbCategories;
        private Button btnAddProduct, btnDeleteProduct, btnUpdateProduct;
        private int _selectedProductId = -1;


        public ProductManagement(SQLiteConnection conn)
        {
            connection = conn;
            InitializeComponent();
            LoadProducts();
            LoadCategories();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;

            Label lblProduct = new Label
            {
                Text = "Ürün Adı:",
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Roboto", 10),
                Padding = new Padding(10, 0, 0, 0)
            };

            txtProductName = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Roboto", 10)
            };

            Label lblPrice = new Label
            {
                Text = "Fiyat:",
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Roboto", 10),
                Padding = new Padding(10, 0, 0, 0)
            };

            txtProductPrice = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Roboto", 10)
            };

            Label lblCategory = new Label
            {
                Text = "Kategori:",
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Roboto", 10),
                Padding = new Padding(10, 0, 0, 0)
            };

            cmbCategories = new ComboBox
            {
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Roboto", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            btnAddProduct = new Button
            {
                Text = "Ürün Ekle",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = ColorTranslator.FromHtml("#3D008A"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAddProduct.Click += BtnAddProduct_Click;


            btnUpdateProduct = new Button
            {
                Text = "Seçili Ürünü Güncelle",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
            };
            btnUpdateProduct.Click += BtnUpdateProduct_Click;

            btnDeleteProduct = new Button
            {
                Text = "Seçili Ürünü Sil",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDeleteProduct.Click += BtnDeleteProduct_Click;

            productGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            productGridView.SelectionChanged += ProductGridView_SelectionChanged;

            Controls.Add(productGridView);
            Controls.Add(btnUpdateProduct);
            Controls.Add(btnDeleteProduct);
            Controls.Add(btnAddProduct);
            Controls.Add(cmbCategories);
            Controls.Add(lblCategory);
            Controls.Add(txtProductPrice);
            Controls.Add(lblPrice);
            Controls.Add(txtProductName);
            Controls.Add(lblProduct);
        }

        private void LoadProducts()
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            string sql = @"SELECT 
                            p.Id, p.Name AS 'Ürün Adı', p.Price AS 'Fiyat', c.Name AS 'Kategori' 
                          FROM Products p 
                          JOIN Categories c ON p.CategoryId = c.Id";
            using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(sql, connection))
            {
                DataTable table = new DataTable();
                adapter.Fill(table);
                productGridView.DataSource = table;
            }

            connection.Close();
        }

        private void LoadCategories()
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            string sql = "SELECT * FROM Categories";
            using (SQLiteCommand cmd = new SQLiteCommand(sql, connection))
            {
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    cmbCategories.Items.Clear(); // Var olanları temizle
                    while (reader.Read())
                    {
                        cmbCategories.Items.Add(new
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString()
                        });
                    }
                }
            }

            connection.Close();
        }

        private void BtnAddProduct_Click(object sender, EventArgs e)
        {
            string productName = txtProductName.Text.Trim();
            string productPrice = txtProductPrice.Text.Trim();
            var selectedCategory = cmbCategories.SelectedItem;

            if (string.IsNullOrEmpty(productName) || string.IsNullOrEmpty(productPrice) || selectedCategory == null)
            {
                MessageBox.Show("Lütfen tüm bilgileri doldurun.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int categoryId = (int)((dynamic)selectedCategory).Id;
            string sql = "INSERT INTO Products (Name, Price, CategoryId) VALUES (@Name, @Price, @CategoryId)";

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            using (SQLiteCommand cmd = new SQLiteCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@Name", productName);
                cmd.Parameters.AddWithValue("@Price", productPrice);
                cmd.Parameters.AddWithValue("@CategoryId", categoryId);
                cmd.ExecuteNonQuery();
            }

            connection.Close();

            MessageBox.Show("Ürün başarıyla eklendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ClearInputFields();
            LoadProducts();
        }

        private void BtnDeleteProduct_Click(object sender, EventArgs e)
        {
            if (productGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen silmek için bir ürün seçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int productId = Convert.ToInt32(productGridView.SelectedRows[0].Cells["Id"].Value);
            string sql = "DELETE FROM Products WHERE Id = @Id";

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            using (SQLiteCommand cmd = new SQLiteCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@Id", productId);
                cmd.ExecuteNonQuery();
            }

            connection.Close();

            MessageBox.Show("Ürün başarıyla silindi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ClearInputFields();
            LoadProducts();
        }

        private void BtnUpdateProduct_Click(object sender, EventArgs e)
        {
            if (_selectedProductId == -1)
            {
                MessageBox.Show("Lütfen güncellemek için bir ürün seçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string productName = txtProductName.Text.Trim();
            string productPrice = txtProductPrice.Text.Trim();
            var selectedCategory = cmbCategories.SelectedItem;

            if (string.IsNullOrEmpty(productName) || string.IsNullOrEmpty(productPrice) || selectedCategory == null)
            {
                MessageBox.Show("Lütfen tüm bilgileri doldurun.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int categoryId = (int)((dynamic)selectedCategory).Id;
            string sql = "UPDATE Products SET Name = @Name, Price = @Price, CategoryId = @CategoryId WHERE Id = @Id";


            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            using (SQLiteCommand cmd = new SQLiteCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@Id", _selectedProductId);
                cmd.Parameters.AddWithValue("@Name", productName);
                cmd.Parameters.AddWithValue("@Price", productPrice);
                cmd.Parameters.AddWithValue("@CategoryId", categoryId);
                cmd.ExecuteNonQuery();
            }
            connection.Close();

            MessageBox.Show("Ürün başarıyla güncellendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ClearInputFields();
            LoadProducts();
            _selectedProductId = -1;
        }


        private void ProductGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (productGridView.SelectedRows.Count > 0)
            {
                DataGridViewRow row = productGridView.SelectedRows[0];
                _selectedProductId = Convert.ToInt32(row.Cells["Id"].Value);
                txtProductName.Text = row.Cells["Ürün Adı"].Value.ToString();
                txtProductPrice.Text = row.Cells["Fiyat"].Value.ToString();
                // Kategoriyi combobox'ta seçili hale getir
                string selectedCategoryName = row.Cells["Kategori"].Value.ToString();

                foreach (var item in cmbCategories.Items)
                {
                    if (((dynamic)item).Name == selectedCategoryName)
                    {
                        cmbCategories.SelectedItem = item;
                        break;
                    }
                }

            }
            else
            {
                _selectedProductId = -1;
            }
        }
        private void ClearInputFields()
        {
            txtProductName.Clear();
            txtProductPrice.Clear();
            cmbCategories.SelectedIndex = -1;
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadProducts();
            LoadCategories();
        }
    }
}