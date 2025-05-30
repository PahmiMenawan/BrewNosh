using iText.Kernel.Pdf.Collection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace BrewNosh
{
    public partial class AdminDashboard : Form
    {
        public AdminDashboard()
        {
            InitializeComponent();
        }

        SqlConnection conn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Kasir;Integrated Security=True;");
        SqlCommand cmd;

        private void AdminDashboard_Load(object sender, EventArgs e)
        {
            deactivate();
            dashboard();
            tb_log();

        }


        // Database load
        public void tb_transaksi()
        {
            cmd = new SqlCommand("SELECT * FROM Transaksi", conn);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.SelectCommand = cmd;
            DataTable dt = new DataTable();
            dt.Clear();
            sda.Fill(dt);
            produkstok_tbl.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            produkstok_tbl.DataSource = dt;
        }

        public void tb_produk()
        {
            cmd = new SqlCommand("SELECT * FROM Produk", conn);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.SelectCommand = cmd;
            DataTable dt = new DataTable();
            dt.Clear();
            sda.Fill(dt);
            produkstok_tbl.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            produkstok_tbl.DataSource = dt;
            DataGridViewImageColumn imgCol = (DataGridViewImageColumn)produkstok_tbl.Columns["foto"];
            imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;

        }
        public void tb_detail()
        {
            cmd = new SqlCommand("SELECT * FROM detail_transaksi", conn);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.SelectCommand = cmd;
            DataTable dt = new DataTable();
            dt.Clear();
            sda.Fill(dt);
            produkstok_tbl.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            produkstok_tbl.DataSource = dt;
        }
        public void tb_user()
        {
            cmd = new SqlCommand("SELECT * FROM Cashier", conn);
        }
        public void tb_admin()
        {
            cmd = new SqlCommand("SELECT * FROM Admin", conn);
        }
        public void tb_log()
        {
            cmd = new SqlCommand("SELECT * FROM table_log", conn);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            log.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            log.DataSource = dt;

            foreach (DataGridViewRow row in log.Rows)
            {
                if (!row.IsNewRow && row.Cells["waktu"].Value != null && row.Cells["waktu"].Value is byte[] bytes)
                {
                    row.Cells["waktu"].Value = BitConverter.ToString(bytes).Replace("-", "");
                }
            }
        }

        public void load_data()
        {

        }

        // functions
        private void LoadChartFromDatabase()
        {
            try
            {
                conn.Open();

                // Query contoh ambil total penjualan per tanggal (7 hari terakhir)
                string query = @"
                SELECT CONVERT(date, tanggal) as tanggal, SUM(harga_total) as total_penjualan
                FROM Transaksi
                WHERE tanggal >= DATEADD(day, -7, CAST(GETDATE() AS date))
                GROUP BY CONVERT(date, tanggal)
                ORDER BY tanggal";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                // Siapkan series chart
                Series series = new Series("Penjualan");
                series.ChartType = SeriesChartType.Line;
                series.XValueType = ChartValueType.Date;

                // Loop data dari database
                while (reader.Read())
                {
                    DateTime tanggal = reader.GetDateTime(0);
                    int total = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);

                    // Tambah data ke series
                    series.Points.AddXY(tanggal.ToString("dd-MM"), total);
                }

                reader.Close();

                // Bersihkan series lama dan tambah series baru
                chart1.Series.Clear();
                chart1.Series.Add(series);

                // Atur sumbu X dan Y
                chart1.ChartAreas[0].AxisX.Title = "Tanggal";
                chart1.ChartAreas[0].AxisY.Title = "Total Penjualan";
                chart1.ChartAreas[0].AxisX.Interval = 1;

                // Refresh chart
                chart1.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error load chart: " + ex.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        // Sidebar btn
        private void dashboard_btn_Click(object sender, EventArgs e)
        {
            deactivate();
            dashboard();
            tb_log();


        }
        private void produk_btn_Click(object sender, EventArgs e)
        {
            deactivate();
            produkStok();
        }

        // Sidebar btn //
        // Panels
        public void penghasilan()
        {
            cmd = new SqlCommand("SELECT SUM(harga_total) FROM Transaksi", conn);
            conn.Open();
            int penghasilan = Convert.ToInt32(cmd.ExecuteScalar());
            conn.Close();
            penghasilan_label.Text = penghasilan.ToString("C", new CultureInfo("id-ID"));
        }
        public void terlaris()
        {
            cmd = new SqlCommand("SELECT MAX(jumlah) FROM detail_transaksi", conn);
            conn.Open();
            int jumlah = Convert.ToInt32(cmd.ExecuteScalar());
            conn.Close();
            cmd = new SqlCommand($"SELECT id_produk FROM detail_transaksi WHERE jumlah = {jumlah}", conn);
            conn.Open();
            string id = Convert.ToString(cmd.ExecuteScalar());
            conn.Close();
            cmd = new SqlCommand($"SELECT nama_barang FROM produk WHERE id_produk = {id}", conn);
            conn.Open();
            string produk = Convert.ToString(cmd.ExecuteScalar());
            conn.Close();
            product_label.Text = produk;
        }
        public void transaction()
        {
            cmd = new SqlCommand("SELECT count(*) FROM Transaksi", conn);
            conn.Open();
            int jumlah = Convert.ToInt32(cmd.ExecuteScalar());
            conn.Close();
            transaction_label.Text = (jumlah.ToString() + " Transaksi");
        }
        // Panels // 
        public void dashboard()
        {
            LoadChartFromDatabase();
            panel_penghasilan.Visible = true;
            panel_produk.Visible = true;
            panel_transaksi.Visible = true;
            chart1.Visible = true;
            log.Visible = true;
            // Panels
            penghasilan();
            terlaris();
            transaction();
        }

        public void produkStok()
        {
            produk_panel.Visible = true;
            tb_produk();
            label_1.Text = "Id Produk";
            label_2.Text = "Nama Produk";
            label_3.Text = "Harga Produk";
            label_4.Text = "Stok Produk";

        }
        public void transaksi()
        {

        }
        public void users()
        {

        }
        public void customize()
        {

        }
        public void deactivate()
        {
            panel_penghasilan.Visible = false;
            panel_produk.Visible = false;
            panel_transaksi.Visible = false;
            chart1.Visible = false;
            log.Visible = false;
            produk_panel.Visible = false;
        }

        // Panels //
        private void t_logout_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }

        private void btn_produk_Click(object sender, EventArgs e)
        {
            tb_produk();
            label_1.Text = "Id Produk";
            label_2.Text = "Nama Produk";
            label_3.Text = "Harga Produk";
            label_4.Text = "Stok Produk";
        }

        private void btn_detail_Click(object sender, EventArgs e)
        {
            tb_detail();

        }

        private void btn_transaksi_Click(object sender, EventArgs e)
        {
            tb_transaksi();

        }

        private void btn_upload_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Selet image(*.JpG; *.png; *.jpeg;)|*.JpG; *.png; *.jpeg";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox.Image = Image.FromFile(openFileDialog1.FileName);
            }
        }

        private void produkstok_tbl_Click(object sender, EventArgs e)
        {

            t_id.Text = produkstok_tbl.CurrentRow.Cells[0].Value.ToString();
            t_nama.Text = produkstok_tbl.CurrentRow.Cells[1].Value.ToString();
            t_hrg.Text = produkstok_tbl.CurrentRow.Cells[2].Value.ToString();
            t_stok.Text = produkstok_tbl.CurrentRow.Cells[3].Value.ToString();
            MemoryStream ms = new MemoryStream((byte[])produkstok_tbl.CurrentRow.Cells[4].Value);
            pictureBox.Image = Image.FromStream(ms);
        }

        private void t_stok_TextChanged(object sender, EventArgs e)
        {

        }

        private void t_hrg_TextChanged(object sender, EventArgs e)
        {

        }

        private void t_nama_TextChanged(object sender, EventArgs e)
        {

        }

        private void t_id_TextChanged(object sender, EventArgs e)
        {

        }

        private void btn_clear_Click(object sender, EventArgs e)
        {
            clear();
        }
        public void clear()
        {
            t_id.Text = string.Empty;
            t_nama.Text = string.Empty;
            t_hrg.Text = string.Empty;
            t_stok.Text = string.Empty;
            pictureBox.Image = null;
        }

        private void insert_btn_Click(object sender, EventArgs e)
        {


            if (!string.IsNullOrEmpty(t_id.Text) && !string.IsNullOrEmpty(t_nama.Text) && !string.IsNullOrEmpty(t_hrg.Text) && !string.IsNullOrEmpty(t_stok.Text) && pictureBox.Image != null)
            {
                
                if (label_1.Text == "Id Produk")
                {
                    int id_produk = Convert.ToInt32(t_id.Text);
                    cmd = new SqlCommand($"SELECT id_produk FROM Produk WHERE id_produk = {id_produk}", conn);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sda.Fill(dt);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();

                    if (dt.Rows.Count > 0)
                    {
                        MessageBox.Show("ID Produk sudah ada!");
                    }
                    else
                    {
                        int idProduk = Convert.ToInt32(t_id.Text);
                        string namaProduk = t_nama.Text;
                        int hargaProduk = Convert.ToInt32(t_hrg.Text);
                        int stokProduk = Convert.ToInt32(t_stok.Text);
                        MemoryStream baos = new MemoryStream();
                        pictureBox.Image.Save(baos, pictureBox.Image.RawFormat);
                        cmd = new SqlCommand($"INSERT INTO Produk (id_produk, nama_barang, harga_barang, stok, foto) VALUES ('{idProduk}', '{namaProduk}', '{hargaProduk}', '{stokProduk}', @image)", conn);
                        cmd.Parameters.AddWithValue("Image", baos.ToArray());
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        tb_produk();
                    }
                }
                
            }
            else
            {
                MessageBox.Show("Data belum lengkap!");
            }
        }
    }
}