using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace CafeOrderSystem
{
    public partial class MainForm : Form
    {
        private SQLiteConnection connection;
        private CategoryManagement categoryManagement;
        private ProductManagement productManagement;
        private OrderScreen orderScreen;
        private OrderHistory orderHistory;

        public MainForm()
        {
            InitializeComponent();
            InitializeDatabase();
            InitializeUserControls();
            AttachEventHandlers();
        }

        private void InitializeDatabase()
        {
            string dbPath = "cafe.db";
            bool createTables = !System.IO.File.Exists(dbPath);

            connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            connection.Open();

            if (createTables)
            {
                string sql = @"
                    CREATE TABLE Categories (Id INTEGER PRIMARY KEY, Name TEXT);
                    CREATE TABLE Products (Id INTEGER PRIMARY KEY, Name TEXT, Price REAL, CategoryId INTEGER, ImagePath TEXT);
                    CREATE TABLE Orders (Id INTEGER PRIMARY KEY, DateTime TEXT, TotalAmount REAL);
                    CREATE TABLE OrderItems (Id INTEGER PRIMARY KEY, OrderId INTEGER, ProductId INTEGER, Quantity INTEGER);
                ";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private void InitializeUserControls()
        {
            categoryManagement = new CategoryManagement(connection);
            productManagement = new ProductManagement(connection);
            orderScreen = new OrderScreen(connection);
            orderHistory = new OrderHistory(connection);

            categoryManagement.Dock = DockStyle.Fill;
            productManagement.Dock = DockStyle.Fill;
            orderScreen.Dock = DockStyle.Fill;
            orderHistory.Dock = DockStyle.Fill;

            // OrderScreen'in sipariþ tamamlandý eventini yakala
            orderScreen.OrderCompleted += OrderScreen_OrderCompleted; // BURASI EKLENDI
        }

        private void OrderScreen_OrderCompleted(object sender, EventArgs e)
        {
            // OrderHistory'i güncelle
            orderHistory.RefreshOrderHistory();
        }


        private void AttachEventHandlers()
        {
            btnOrders.Click += (s, e) => ShowUserControl(orderScreen);
            btnHistory.Click += (s, e) => ShowUserControl(orderHistory);
            btnCategories.Click += (s, e) => ShowUserControl(categoryManagement);
            btnProducts.Click += (s, e) => ShowUserControl(productManagement);
            ShowUserControl(orderScreen);
        }

        private void ShowUserControl(UserControl control)
        {
            mainContent.Controls.Clear();
            mainContent.Controls.Add(control);
            control.BringToFront();
        }
    }
}