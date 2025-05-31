using iText.Kernel.Pdf.Collection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using ClosedXML.Excel;


namespace BrewNosh
{
    public partial class AdminDashboard : Form
    {
        public AdminDashboard()
        {
            InitializeComponent();
        }

        SqlConnection conn = DatabaseHelper.GetConnection();
        SqlCommand cmd;

        private void AdminDashboard_Load(object sender, EventArgs e)
        {
            deactivate();
            dashboard();
            LoadChartFromDatabase();
            LoadPieChartKategori();
            chart1.Visible = true;
            log.Visible = false;
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
            cmd = new SqlCommand("SELECT * FROM table_log ORDER BY id_log DESC", conn);
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
            public void LoadPieChartKategori()
            {
                string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Kasir;Integrated Security=True;";
                string query = @"
            SELECT p.kategori, SUM(dt.jumlah) AS total_jumlah
            FROM detail_transaksi dt
            JOIN produk p ON dt.id_produk = p.id_produk
            GROUP BY p.kategori";

                DataTable dt = new DataTable();

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(dt);
                }

                chart2.Series.Clear();

                Series series = new Series("PenjualanKategori");
                series.ChartType = SeriesChartType.Pie;

                foreach (DataRow row in dt.Rows)
                {
                    string kategori = row["kategori"].ToString();
                    int total = Convert.ToInt32(row["total_jumlah"]);

                    series.Points.AddXY(kategori, total);
                }

                chart2.Series.Add(series);

            // Optional styling
            chart2.ChartAreas[0].Position = new ElementPosition(10, 10, 80, 80);
            // x=10%, y=10%, width=80%, height=80%
            series["PieLabelStyle"] = "Outside";  // label di luar lingkaran
            series["PieLineColor"] = "Black";    // warna garis penghubung label

            chart2.Legends[0].Docking = Docking.Right;
                chart2.Legends[0].Font = new Font("Segoe UI", 10);
                series["PieLabelStyle"] = "Outside";
                series.BorderWidth = 1;
                series.BorderColor = Color.Black;
            }
        // Sidebar btn
        private void dashboard_btn_Click(object sender, EventArgs e)
        {
            deactivate();
            dashboard();
            tb_log();


        }

        private void ShowPanel(Panel targetPanel)
        {
            // Sembunyikan semua panel
            produk_panel.Visible = false;

            // Tampilkan panel yang sesuai
            targetPanel.Visible = true;
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

            object result = cmd.ExecuteScalar();
            conn.Close();

            if (result != DBNull.Value && result != null)
            {
                int penghasilan = Convert.ToInt32(result);
                penghasilan_label.Text = penghasilan.ToString("C", new CultureInfo("id-ID"));
            }
            else
            {
                penghasilan_label.Text = "Rp 0,00";
            }
        }
        public void terlaris()
        {
            cmd = new SqlCommand("SELECT MAX(jumlah) FROM detail_transaksi", conn);
            conn.Open();
            object result = cmd.ExecuteScalar();
            conn.Close();
            if (result != DBNull.Value && result != null)
            {
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
            else
            {
                product_label.Text = "-";
            }
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
            dashboard_panel.Visible = true;
            LoadChartFromDatabase();
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
            dashboard_panel.Visible = false;
            //produkstok
            produk_panel.Visible = false;
            //transaksi
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
            pictureBox.Image = null;
            clear();
        }

        private void btn_detail_Click(object sender, EventArgs e)
        {
            tb_detail();
            label_1.Text = "Id Detail";
            label_2.Text = "Id Transaksi";
            label_3.Text = "Id Produk";
            label_4.Text = "Jumlah";
            pictureBox.Image = null;
            clear();

        }

        private void btn_transaksi_Click(object sender, EventArgs e)
        {

            tb_transaksi();
            label_1.Text = "Id Transaksi";
            label_2.Text = "Id Tanggal";
            label_3.Text = "Harga Total";
            label_4.Text = "";
            t_stok.Text = "";
            pictureBox.Image = null;
            clear();

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
            cb_kategori.Text = produkstok_tbl.CurrentRow.Cells[4].Value.ToString();
            MemoryStream ms = new MemoryStream((byte[])produkstok_tbl.CurrentRow.Cells[5].Value);
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
            cb_kategori.SelectedIndex = -1; // Reset combo box selection
            pictureBox.Image = null;
        }

        private void insert_btn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(t_id.Text) && !string.IsNullOrEmpty(t_nama.Text) && !string.IsNullOrEmpty(t_hrg.Text) && !string.IsNullOrEmpty(t_stok.Text) && pictureBox.Image != null)
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
                    string kategoriProduk = cb_kategori.Text;
                    MemoryStream baos = new MemoryStream();
                    pictureBox.Image.Save(baos, pictureBox.Image.RawFormat);
                    cmd = new SqlCommand($"INSERT INTO Produk (id_produk, nama_barang, harga_barang, stok, kategori, foto) VALUES ('{idProduk}', '{namaProduk}', '{hargaProduk}', '{stokProduk}','{kategoriProduk}', @image)", conn);
                    cmd.Parameters.AddWithValue("Image", baos.ToArray());
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    tb_produk();
                    MessageBox.Show("Produk berhasil ditambahkan!");
                    clear();

                }
            }
            else
            {
                MessageBox.Show("Data belum lengkap!");
            }
        }

        private void update_btn_Click(object sender, EventArgs e)
        {

            int idProduk = Convert.ToInt32(t_id.Text);
            string namaProduk = t_nama.Text;
            int hargaProduk = Convert.ToInt32(t_hrg.Text);
            int stokProduk = Convert.ToInt32(t_stok.Text);
            string kategoriProduk = cb_kategori.Text;
            MemoryStream baos = new MemoryStream();
            pictureBox.Image.Save(baos, pictureBox.Image.RawFormat);
            cmd = new SqlCommand($"UPDATE Produk SET nama_barang = '{namaProduk}', harga_barang = '{hargaProduk}', stok = '{stokProduk}', kategori = '{kategoriProduk}', foto = @image WHERE id_produk = {idProduk}", conn);
            cmd.Parameters.AddWithValue("Image", baos.ToArray());
            conn.Open();
            int rowsAffected = cmd.ExecuteNonQuery();
            if (rowsAffected == 0)
            {
                MessageBox.Show("Produk tidak ditemukan. Pastikan ID benar.", "Update Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show("Produk berhasil diupdate!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            conn.Close();
            tb_produk();
            clear();

        }

        private void delete_btn_Click(object sender, EventArgs e)
        {
           
                int idProduk = Convert.ToInt32(t_id.Text);
                cmd = new SqlCommand($"DELETE FROM Produk WHERE id_produk = {idProduk}", conn);
                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    MessageBox.Show("Produk tidak ditemukan. Pastikan ID benar.", "Hapus Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("Produk berhasil dihapus!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                conn.Close();
                tb_produk();
                clear();
            
        }
        public void doomsday_procedure()
        {
            cmd = new SqlCommand("TRUNCATE TABLE transaksi", conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
            cmd = new SqlCommand("TRUNCATE TABLE detail_transaksi", conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
            cmd = new SqlCommand("TRUNCATE TABLE produk", conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            doomsday_procedure();
        }

        private void grafik_btn_Click(object sender, EventArgs e)
        {
            LoadChartFromDatabase();
            chart1.Visible = true;
            log.Visible = false;
        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            tb_log();
            chart1.Visible = false;
            log.Visible = true;
        }

        private void users_btn_Click(object sender, EventArgs e)
        {

        }

        private void ExportLogToExcel()
        {
            if (log.Rows.Count == 0)
            {
                MessageBox.Show("Data kosong, tidak bisa diexport.");
                return;
            }

            DataTable dt = new DataTable();

            // Buat kolom dari header DataGridView
            foreach (DataGridViewColumn col in log.Columns)
            {
                dt.Columns.Add(col.HeaderText);
            }

            // Isi data baris per baris
            foreach (DataGridViewRow row in log.Rows)
            {
                if (!row.IsNewRow)
                {
                    DataRow dRow = dt.NewRow();
                    for (int i = 0; i < log.Columns.Count; i++)
                    {
                        dRow[i] = row.Cells[i].Value ?? DBNull.Value;
                    }
                    dt.Rows.Add(dRow);
                }
            }

            // SaveFileDialog untuk pilih lokasi simpan
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel Workbook|*.xlsx";
                sfd.FileName = "log_export.xlsx";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (XLWorkbook wb = new XLWorkbook())
                        {
                            wb.Worksheets.Add(dt, "LogData");
                            wb.SaveAs(sfd.FileName);
                        }
                        MessageBox.Show("Data berhasil diexport ke Excel:\n" + sfd.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error saat export Excel:\n" + ex.Message);
                    }
                }
            }
        }

        private void ExportChartWithImageToExcel()
        {
            MemoryStream ms = new MemoryStream();
            chart1.SaveImage(ms, ChartImageFormat.Png);
            ms.Position = 0;

            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Chart Data");

            ws.Cell(1, 1).Value = "Tanggal";
            ws.Cell(1, 2).Value = "Total Penghasilan";

            int row = 2;
            foreach (var point in chart1.Series[0].Points)
            {
                ws.Cell(row, 1).Value = point.AxisLabel ?? point.XValue.ToString();
                ws.Cell(row, 2).Value = point.YValues[0];
                row++;
            }

            var image = ws.AddPicture(ms)
                          .MoveTo(ws.Cell("D2"))
                          .Scale(0.75);

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel Workbook|*.xlsx";
                sfd.FileName = "chart_with_data.xlsx";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    wb.SaveAs(sfd.FileName);
                    MessageBox.Show("Export sukses ke Excel: " + sfd.FileName);
                }
            }
        }

        private void guna2Button6_Click(object sender, EventArgs e)
        {
            if (log.Visible == true)
            {
                ExportLogToExcel();
            }
            else if (chart1.Visible == true)
            {
                ExportChartWithImageToExcel();
            }
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            deactivate();
            transaksi();
        }
    }
}