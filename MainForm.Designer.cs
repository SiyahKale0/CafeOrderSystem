using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;

namespace CafeOrderSystem
{
    partial class MainForm
    {
        private Panel leftSidebar;
        private Panel mainContent;

        private IconButton btnOrders;
        private IconButton btnHistory;
        private IconButton btnCategories;
        private IconButton btnProducts;

        private void InitializeComponent()
        {
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Text = "Cafe Order System";
            this.MinimumSize = new System.Drawing.Size(1024, 576);
            this.BackColor = Color.White;

            // Left Sidebar
            leftSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = ColorTranslator.FromHtml("#3D008A") // Dark purple
            };

            btnOrders = CreateSidebarButton("Siparişler", IconChar.ShoppingCart);
            btnHistory = CreateSidebarButton("Geçmiş", IconChar.History);
            btnCategories = CreateSidebarButton("Kategoriler", IconChar.Tags);
            btnProducts = CreateSidebarButton("Ürünler", IconChar.Boxes);

            leftSidebar.Controls.AddRange(new Control[] { btnProducts, btnCategories, btnHistory, btnOrders });

            // Main Content Area
            mainContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            this.Controls.Add(mainContent);
            this.Controls.Add(leftSidebar);
        }

        private IconButton CreateSidebarButton(string text, IconChar icon)
        {
            return new IconButton
            {
                Text = "   " + text,
                Dock = DockStyle.Top,
                Height = 60,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                ForeColor = Color.White,
                Font = new Font("Roboto", 12, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                IconChar = icon,
                IconColor = Color.White,
                IconSize = 30,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText
            };
        }
    }
}