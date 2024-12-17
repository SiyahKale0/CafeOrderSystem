using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace CafeOrderSystem
{
    public class CategoryManagement : UserControl
    {
        private SQLiteConnection connection;
        private DataGridView categoryGridView;
        private TextBox txtCategoryName;
        private Button btnAddCategory, btnDeleteCategory, btnUpdateCategory;
        private int selectedCategoryId = -1; // Seçilen kategorinin ID'sini saklar

        public CategoryManagement(SQLiteConnection conn)
        {
            connection = conn;
            InitializeComponent();
            LoadCategories();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;

            Label lblCategory = new Label
            {
                Text = "Kategori Adı:",
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Roboto", 10),
                Padding = new Padding(10, 0, 0, 0)
            };

            txtCategoryName = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Roboto", 10)
            };

            btnAddCategory = new Button
            {
                Text = "Kategori Ekle",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = ColorTranslator.FromHtml("#3D008A"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAddCategory.Click += BtnAddCategory_Click;

            btnUpdateCategory = new Button
            {
                Text = "Kategori Güncelle",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnUpdateCategory.Click += BtnUpdateCategory_Click;


            btnDeleteCategory = new Button
            {
                Text = "Seçili Kategoriyi Sil",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDeleteCategory.Click += BtnDeleteCategory_Click;

            categoryGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            categoryGridView.SelectionChanged += CategoryGridView_SelectionChanged;


            Controls.Add(categoryGridView);
            Controls.Add(btnDeleteCategory);
            Controls.Add(btnUpdateCategory);
            Controls.Add(btnAddCategory);
            Controls.Add(txtCategoryName);
            Controls.Add(lblCategory);
        }

        private void LoadCategories()
        {
            if (connection == null)
            {
                MessageBox.Show("Veritabanı bağlantısı bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter("SELECT * FROM Categories", connection))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    categoryGridView.DataSource = table;
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show($"Veritabanı hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Beklenmeyen hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void BtnAddCategory_Click(object sender, EventArgs e)
        {
            if (connection == null)
            {
                MessageBox.Show("Veritabanı bağlantısı bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string categoryName = txtCategoryName.Text.Trim();
            if (string.IsNullOrEmpty(categoryName))
            {
                MessageBox.Show("Lütfen kategori adı girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand("INSERT INTO Categories (Name) VALUES (@Name)", connection))
                {
                    cmd.Parameters.AddWithValue("@Name", categoryName);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Kategori başarıyla eklendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtCategoryName.Clear();
                LoadCategories();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show($"Veritabanı hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Beklenmeyen hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }


        private void BtnUpdateCategory_Click(object sender, EventArgs e)
        {
            if (connection == null)
            {
                MessageBox.Show("Veritabanı bağlantısı bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string categoryName = txtCategoryName.Text.Trim();
            if (string.IsNullOrEmpty(categoryName))
            {
                MessageBox.Show("Lütfen kategori adı girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (selectedCategoryId == -1)
            {
                MessageBox.Show("Lütfen güncellemek için bir kategori seçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand("UPDATE Categories SET Name = @Name WHERE Id = @Id", connection))
                {
                    cmd.Parameters.AddWithValue("@Name", categoryName);
                    cmd.Parameters.AddWithValue("@Id", selectedCategoryId);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Kategori başarıyla güncellendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtCategoryName.Clear();
                selectedCategoryId = -1;
                LoadCategories();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show($"Veritabanı hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Beklenmeyen hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDeleteCategory_Click(object sender, EventArgs e)
        {
            if (connection == null)
            {
                MessageBox.Show("Veritabanı bağlantısı bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (categoryGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen silmek için bir kategori seçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int categoryId = Convert.ToInt32(categoryGridView.SelectedRows[0].Cells["Id"].Value);


            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM Categories WHERE Id = @Id", connection))
                {
                    cmd.Parameters.AddWithValue("@Id", categoryId);
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Kategori başarıyla silindi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                selectedCategoryId = -1;
                LoadCategories();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show($"Veritabanı hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Beklenmeyen hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void CategoryGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (categoryGridView.SelectedRows.Count > 0)
            {
                selectedCategoryId = Convert.ToInt32(categoryGridView.SelectedRows[0].Cells["Id"].Value);
                txtCategoryName.Text = categoryGridView.SelectedRows[0].Cells["Name"].Value.ToString();
            }
            else
            {
                selectedCategoryId = -1;
                txtCategoryName.Text = "";
            }
        }
    }
}