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
            Dashboard();
            LoadChartFromDatabase();
            LoadPieChartKategori();
            chart1.Visible = true;
            log.Visible = false;
        }
        private void t_logout_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }
        public void deactivate()
        {
            dashboard_panel.Visible = false;
            //produkstok
            produk_panel.Visible = false;
            //transaksi
            transaksi_panel.Visible = false;
            //users
            users_panel.Visible = false;
        }
        // TABLES LOAD
        public void Tb_transaksi()
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
        public void Tb_produk()
        {
            cmd = new SqlCommand("SELECT * FROM Produk", conn);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.SelectCommand = cmd;
            DataTable dt = new DataTable();
            dt.Clear();
            sda.Fill(dt);
            produkstok_tbl.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            produkstok_tbl.DataSource = dt;

        }
        public void Tb_detail()
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
        public void Tb_user()
        {
            cmd = new SqlCommand("SELECT * FROM Cashier", conn);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.SelectCommand = cmd;
            DataTable dt = new DataTable();
            dt.Clear();
            sda.Fill(dt);
            tbl_on_users.DataSource = dt;
            tbl_on_users.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // supaya bisa atur manual
            tbl_on_users.Columns[3].Width = 100;  // atur lebar kolom gambar
            tbl_on_users.RowTemplate.Height = 150; // atur tinggi baris supaya gambar tidak terpotong
            DataGridViewImageColumn pic = (DataGridViewImageColumn)tbl_on_users.Columns[3];
            pic.ImageLayout = DataGridViewImageCellLayout.Stretch;
        }
        public void Tb_admin()
        {
            cmd = new SqlCommand("SELECT * FROM Admin", conn);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.SelectCommand = cmd;
            DataTable dt = new DataTable();
            dt.Clear();
            sda.Fill(dt);
            tbl_on_users.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            tbl_on_users.DataSource = dt;

        }
        public void Tb_log()
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
        // ========== DASHBOARD ==========
        private void dashboard_btn_Click(object sender, EventArgs e)
        {
            LoadPieChartKategori();
            deactivate();
            Dashboard();
            Tb_log();
        }
        public void Dashboard()
        {
            dashboard_panel.Visible = true;
            LoadChartFromDatabase();
            Penghasilan();
            Terlaris();
            Transaction();
        }
        // PANELS
        public void Penghasilan()
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
        public void Terlaris()
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
        public void Transaction()
        {
            cmd = new SqlCommand("SELECT COUNT(*) FROM Transaksi WHERE CAST(tanggal AS DATE) = CAST(GETDATE() AS DATE)", conn);
            conn.Open();
            int jumlah = Convert.ToInt32(cmd.ExecuteScalar());
            conn.Close();
            transaction_label.Text = (jumlah.ToString() + " Transaksi");
        }
        // CHARTS
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
        private void grafik_btn_Click(object sender, EventArgs e)
        {
            LoadChartFromDatabase();
            chart1.Visible = true;
            log.Visible = false;
        }
        private void guna2Button5_Click(object sender, EventArgs e)
        {
            Tb_log();
            chart1.Visible = false;
            log.Visible = true;
        }
        // ========== DASHBOARD ========== // 


        // ========== PRODUK STOK ==========
        private void produk_btn_Click(object sender, EventArgs e)
        {
            deactivate();
            ProdukStok();
        }
        public void ProdukStok()
        {
            produk_panel.Visible = true;
            Tb_produk();
            label_1.Text = "Id Produk";
            label_2.Text = "Nama Produk";
            label_3.Text = "Harga Produk";
            label_4.Text = "Stok Produk";
        }
        private void btn_clear_Click(object sender, EventArgs e)
        {
            Clear();
        }
        public void Clear()
        {
            t_id.Text = string.Empty;
            t_nama.Text = string.Empty;
            t_hrg.Text = string.Empty;
            t_stok.Text = string.Empty;
            cb_kategori.SelectedIndex = -1; // Reset combo box selection
        }
        private void produkstok_tbl_Click(object sender, EventArgs e)
        {
            t_id.Text = produkstok_tbl.CurrentRow.Cells[0].Value.ToString();
            t_nama.Text = produkstok_tbl.CurrentRow.Cells[1].Value.ToString();
            t_hrg.Text = produkstok_tbl.CurrentRow.Cells[2].Value.ToString();
            t_stok.Text = produkstok_tbl.CurrentRow.Cells[3].Value.ToString();
            cb_kategori.Text = produkstok_tbl.CurrentRow.Cells[4].Value.ToString();
        }
        // CRUD
        private void insert_btn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(t_id.Text) && !string.IsNullOrEmpty(t_nama.Text) && !string.IsNullOrEmpty(t_hrg.Text) && !string.IsNullOrEmpty(t_stok.Text))
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
                    using (SqlCommand cmd = new SqlCommand("sp_InsertProduk", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id_produk", idProduk);
                        cmd.Parameters.AddWithValue("@nama_barang", namaProduk);
                        cmd.Parameters.AddWithValue("@harga_barang", hargaProduk);
                        cmd.Parameters.AddWithValue("@stok", stokProduk);
                        cmd.Parameters.AddWithValue("@kategori", kategoriProduk);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }

                    Tb_produk();
                    MessageBox.Show("Produk berhasil ditambahkan!");
                    Clear();
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
            using (SqlCommand cmd = new SqlCommand("sp_UpdateProduk", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_produk", idProduk);
                cmd.Parameters.AddWithValue("@nama_barang", namaProduk);
                cmd.Parameters.AddWithValue("@harga_barang", hargaProduk);
                cmd.Parameters.AddWithValue("@stok", stokProduk);
                cmd.Parameters.AddWithValue("@kategori", kategoriProduk);

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
            }
            Tb_produk();
            Clear();

        }
        private void delete_btn_Click(object sender, EventArgs e)
        {

            int idProduk = Convert.ToInt32(t_id.Text);
            using (SqlCommand cmd = new SqlCommand("sp_DeleteProduk", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_produk", idProduk);

                conn.Open();
                cmd.ExecuteNonQuery();
                MessageBox.Show("Produk berhasil dihapus!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                conn.Close();
            }
            Tb_produk();
            Clear();

        }
        // ========== PRODUK STOK ========== //


        // ========== TRANSAKSI ==========
        int idTransaksiBaru;
        private void guna2Button2_Click(object sender, EventArgs e)
        {
            deactivate();
            transaksi();
            transaksi_produk();
            tbl_on_transaksi.Enabled = true;
            btn_tmbh.Enabled = false;
            btn_upd.Enabled = false;
            btn_hps.Enabled = false;
        }
        public void transaksi()
        {
            transaksi_panel.Visible = true;
        }
        public void transaksi_produk()
        {
            cmd = new SqlCommand("SELECT * FROM Produk", conn);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.SelectCommand = cmd;
            DataTable dt = new DataTable();
            dt.Clear();
            sda.Fill(dt);
            tbl_on_transaksi.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            tbl_on_transaksi.DataSource = dt;
            t_idd.Enabled = false;
            t_idt.Enabled = false;
            t_idp.Enabled = false;
            t_jml.Enabled = false;
            t_sub.Enabled = false;
            labelid.Text = "";
            labelidt.Text = "";
            labelidp.Text = "";
            labeljml.Text = "";
            labelsb.Text = "";
            nama_produk.Text = "";
        }
        public void transaksi_detail()
        {
            cmd = new SqlCommand("SELECT * FROM detail_transaksi", conn);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.SelectCommand = cmd;
            DataTable dt = new DataTable();
            dt.Clear();
            sda.Fill(dt);
            tbl_on_transaksi.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            tbl_on_transaksi.DataSource = dt;
            t_idd.Enabled = false;
            t_idt.Enabled = true;
            t_idp.Enabled = true;
            t_jml.Enabled = true;
            t_sub.Enabled = true;
            labelid.Text = "Id Detail";
            labelidt.Text = "Id Transaksi";
            labelidp.Text = "Id Produk";
            labeljml.Text = "Jumlah";
            labelsb.Text = "Subtotal";
        }
        public void transaksi_transaksi()
        {
            cmd = new SqlCommand("SELECT * FROM transaksi", conn);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.SelectCommand = cmd;
            DataTable dt = new DataTable();
            dt.Clear();
            sda.Fill(dt);
            tbl_on_transaksi.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            tbl_on_transaksi.DataSource = dt;
            t_idd.Enabled = false;
            t_idt.Enabled = true;
            t_idp.Enabled = true;
            t_jml.Enabled = false;
            t_sub.Enabled = false;
            labelid.Text = "Id Transaksi";
            labelidt.Text = "Tanggal";
            labelidp.Text = "Total";
            labeljml.Text = "";
            labelsb.Text = "";
        }
        private void btn_tbl_produk_Click(object sender, EventArgs e)
        {
            transaksi_produk();
            tbl_on_transaksi.Enabled = true;
            btn_tmbh.Enabled = false;
            btn_upd.Enabled = false;
            btn_hps.Enabled = false;
            t_idd.Text = "";
            t_idt.Text = "";
            t_idp.Text = "";
            t_jml.Text = "";
            t_sub.Text = "";
        }
        private void btn_tbl_detail_Click(object sender, EventArgs e)
        {
            transaksi_detail();
            btn_tmbh.Enabled = true;
            btn_upd.Enabled = true;
            btn_hps.Enabled = true;
            t_idd.Text = "";
            t_idt.Text = "";
            t_idp.Text = "";
            t_jml.Text = "";
            t_sub.Text = "";
        }
        private void btn_tbl_transaksi_Click(object sender, EventArgs e)
        {
            transaksi_transaksi();
            btn_tmbh.Enabled = true;
            btn_upd.Enabled = true;
            btn_hps.Enabled = true;
            t_idd.Text = "";
            t_idt.Text = "";
            t_idp.Text = "";
            t_jml.Text = "";
            t_sub.Text = "";
        }
        public void add_transaksi()
        {
            cmd = new SqlCommand("INSERT INTO Transaksi (tanggal, harga_total) VALUES (GETDATE(), 0);" +
                "SELECT SCOPE_IDENTITY()", conn);
            conn.Open();
            idTransaksiBaru = Convert.ToInt32(cmd.ExecuteScalar());
            conn.Close();
        }
        private void guna2Button3_Click(object sender, EventArgs e)
        {
            cmd = new SqlCommand($"SELECT harga_total FROM Transaksi WHERE id_transaksi = {idTransaksiBaru}", conn);
            conn.Open();
            int harga = Convert.ToInt32(cmd.ExecuteScalar());
            conn.Close();
            if (harga == 0 && idTransaksiBaru != 0)
            {
                MessageBox.Show("Anda belum menyelesaikan pesanan sebelumnya", "Tambah Pesanan", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                DialogResult result = MessageBox.Show("Tambah pesanan baru?", "Tambah pesanan", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    add_transaksi();
                }
            }
        }
        private void tbl_on_transaksi_Click(object sender, EventArgs e)
        {
            if (labelid.Text == "")
            {
                nama_produk.Text = tbl_on_transaksi.CurrentRow.Cells[1].Value.ToString();
            }
            else if (labelid.Text == "Id Detail")
            {
                t_idd.Text = tbl_on_transaksi.CurrentRow.Cells[0].Value.ToString();
                t_idt.Text = tbl_on_transaksi.CurrentRow.Cells[1].Value.ToString();
                t_idp.Text = tbl_on_transaksi.CurrentRow.Cells[2].Value.ToString();
                t_jml.Text = tbl_on_transaksi.CurrentRow.Cells[3].Value.ToString();
                t_sub.Text = tbl_on_transaksi.CurrentRow.Cells[4].Value.ToString();
            }
            else if (labelid.Text == "Id Transaksi")
            {
                t_idd.Text = tbl_on_transaksi.CurrentRow.Cells[0].Value.ToString();
                t_idt.Text = tbl_on_transaksi.CurrentRow.Cells[1].Value.ToString();
                t_idp.Text = tbl_on_transaksi.CurrentRow.Cells[2].Value.ToString();
            }
        }
        public void add_detail()
        {
            string id = tbl_on_transaksi.CurrentRow.Cells[0].Value.ToString();
            int jumlah = Convert.ToInt32(jml_barang.Text);
            int harga = (Convert.ToInt32(tbl_on_transaksi.CurrentRow.Cells[2].Value) * jumlah);
            cmd = new SqlCommand($"INSERT INTO detail_transaksi (id_transaksi, id_produk, jumlah, subtotal) VALUES ({idTransaksiBaru}, {id}, {jumlah}, {harga});", conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
        }
        public void updateStock()
        {
            // Ambil ID produk dari baris yang sedang dipilih di master_table
            int idProduk = Convert.ToInt32(tbl_on_transaksi.CurrentRow.Cells[0].Value);

            // Ambil jumlah stok produk yang ada di tabel Produk
            cmd = new SqlCommand($"SELECT stok FROM Produk WHERE id_produk = {idProduk};", conn);
            conn.Open();
            int stokProduk = Convert.ToInt32(cmd.ExecuteScalar());
            conn.Close();

            // Kurangi stok dengan jumlah yang dipesan
            int jumlahPesan = Convert.ToInt32(jml_barang.Text);
            int stokTerbaru = stokProduk - jumlahPesan;

            // Update stok di tabel Produk
            cmd = new SqlCommand($"UPDATE Produk SET stok = {stokTerbaru} WHERE id_produk = {idProduk};", conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
            transaksi_produk();
        }
        private void guna2Button2_Click_1(object sender, EventArgs e)
        {

            cmd = new SqlCommand($"SELECT harga_total FROM Transaksi WHERE id_transaksi = {idTransaksiBaru}", conn);
            conn.Open();
            int harga = Convert.ToInt32(cmd.ExecuteScalar());
            conn.Close();
            if (harga != 0)
            {
                MessageBox.Show("Anda sudah menyelesaikan pesanan! Tambah pesanan baru untuk melanjutkan");
            }
            else if (idTransaksiBaru == 0)
            {

                MessageBox.Show("Anda belum membuat pesanan! Tambah pesanan baru untuk melanjutkan");
            }
            else
            {
                if (nama_produk.Text == "")
                {
                    MessageBox.Show("Anda belum memiliih pesanan!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {

                    int idProduk = Convert.ToInt32(tbl_on_transaksi.CurrentRow.Cells[0].Value);
                    cmd = new SqlCommand($"SELECT stok FROM Produk WHERE id_produk = {idProduk};", conn);
                    conn.Open();
                    int stokSekarang = Convert.ToInt32(cmd.ExecuteScalar());
                    conn.Close();

                    int jumlahPesan = Convert.ToInt32(jml_barang.Text);

                    // Cek apakah stok cukup
                    if (stokSekarang < jumlahPesan)
                    {
                        MessageBox.Show("Stok produk tidak mencukupi!", "Stok Habis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return; // Stop proses
                    }
                    else
                    {

                        add_detail();
                        cmd = new SqlCommand($"SELECT SUM(subtotal) FROM detail_transaksi WHERE id_transaksi = {idTransaksiBaru};", conn);
                        conn.Open();
                        int hargaDisplay = Convert.ToInt32(cmd.ExecuteScalar());
                        tampil_harga.Text = hargaDisplay.ToString("C", new CultureInfo("id-ID"));
                        conn.Close();
                        int jumlah = Convert.ToInt32(jml_barang.Text);
                        int strk_hrg = (Convert.ToInt32(tbl_on_transaksi.CurrentRow.Cells[2].Value) * jumlah);
                        string strng_hrg = string.Format(new CultureInfo("id-ID"), "Rp{0:N0}", strk_hrg);
                        updateStock();
                    }


                }
            }
        }
        private void guna2Button7_Click(object sender, EventArgs e)
        {
            if (t_bayar.Text != "")
            {
                int uang = Convert.ToInt32(t_bayar.Text);
                cmd = new SqlCommand($"SELECT SUM(subtotal) FROM detail_transaksi WHERE id_transaksi = {idTransaksiBaru};", conn);
                conn.Open();
                int harga = Convert.ToInt32(cmd.ExecuteScalar());
                conn.Close();
                if (uang >= harga)
                {
                    DialogResult result = MessageBox.Show("Konfirmasi pembayaran?", "Bayar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        cmd = new SqlCommand($"UPDATE Transaksi SET harga_total = (SELECT SUM(subtotal) FROM detail_transaksi WHERE id_transaksi = {idTransaksiBaru}) WHERE id_transaksi = {idTransaksiBaru};", conn);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        tampil_harga.Text = "";
                        t_bayar.Text = "";
                        MessageBox.Show("Transaksi selesai!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Uang tidak cukup!", "Bayar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Isi kolom bayar dengan benar!");
            }
        }
        // CRUD
        private void btn_tmbh_Click(object sender, EventArgs e)
        {
            string t_1 = t_idd.Text;
            string t_2 = t_idt.Text;
            string t_3 = t_idp.Text;
            string t_4 = t_jml.Text;
            string t_5 = t_sub.Text;
            if (labelid.Text == "Id Detail")
            {
                using (SqlCommand cmd = new SqlCommand("sp_InsertDetail", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_transaksi", t_2);
                    cmd.Parameters.AddWithValue("@id_produk", t_3);
                    cmd.Parameters.AddWithValue("@jumlah", t_4);
                    cmd.Parameters.AddWithValue("@subtotal", t_5);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    MessageBox.Show("Data terdaftar!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    transaksi_detail();
                }
            }
            else if (labelid.Text == "Id Transaksi")
            {
                using (SqlCommand cmd = new SqlCommand("sp_InsertTransaksi", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@tanggal", t_2);
                    cmd.Parameters.AddWithValue("@harga_total", t_3);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    MessageBox.Show("Data terdaftar!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    transaksi_detail();
                }
            }
        }
        private void btn_upd_Click(object sender, EventArgs e)
        {
            string t_1 = t_idd.Text;
            string t_2 = t_idt.Text;
            string t_3 = t_idp.Text;
            string t_4 = t_jml.Text;
            string t_5 = t_sub.Text;
            if (labelid.Text == "Id Detail")
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateDetail", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_detail", t_1);
                    cmd.Parameters.AddWithValue("@id_transaksi", t_2);
                    cmd.Parameters.AddWithValue("@id_produk", t_3);
                    cmd.Parameters.AddWithValue("@jumlah", t_4);
                    cmd.Parameters.AddWithValue("@subtotal", t_5);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    MessageBox.Show("Data terupdate!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    transaksi_detail();

                }
            }
            else if (labelid.Text == "Id Transaksi")
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateTransaksi", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_transaksi", t_1);
                    cmd.Parameters.AddWithValue("@tanggal", t_2);
                    cmd.Parameters.AddWithValue("@harga_total", t_3);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    MessageBox.Show("Data terupdate!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    transaksi_detail();

                }
            }
        }
        private void guna2Button8_Click(object sender, EventArgs e)
        {
            string t_1 = t_idd.Text;
            if (t_idd.Text == "Id Detail")
            {

                using (SqlCommand cmd = new SqlCommand("sp_DeleteDetail", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_detail", t_1);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    MessageBox.Show("Data terhapus!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    transaksi_detail();
                }
            }
            else if (t_idd.Text == "Id Transaksi")
            {
                using (SqlCommand cmd = new SqlCommand("sp_DeleteTransaksi", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_transaksi", t_1);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    MessageBox.Show("Data terhapus!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    transaksi_transaksi();
                }
            }
        }
        private void guna2Button1_Click_1(object sender, EventArgs e)
        {
            t_idd.Text = "";
            t_idt.Text = "";
            t_idp.Text = "";
            t_jml.Text = "";
            t_sub.Text = "";
        }
        // ========== TRANSAKSI ========== //


        // ========== USERS ==========
        private void users_btn_Click(object sender, EventArgs e)
        {
            users();
            Tb_user();
            Tb_user();
        }
        public void users_clear()
        {
            user_id.Text = "";
            user_name.Text = "";
            user_pass.Text = "";
            user_role.SelectedIndex = -1;
            user_picture.Image = null;
        }
        private void guna2Button4_Click(object sender, EventArgs e)
        {
            users_clear();
        }
        private void btn_cashier_Click(object sender, EventArgs e)
        {
            Tb_user();
            users_clear();

        }
        private void btn_admin_Click(object sender, EventArgs e)
        {
            Tb_admin();
            users_clear();
        }
        private void tbl_on_users_Click(object sender, EventArgs e)
        {
            if (tbl_on_users.Columns.Count == 4)
            {
                user_id.Text = tbl_on_users.CurrentRow.Cells[0].Value.ToString();
                user_name.Text = tbl_on_users.CurrentRow.Cells[1].Value.ToString();
                user_pass.Text = tbl_on_users.CurrentRow.Cells[2].Value.ToString();
                user_role.Text = "Cashier";
                MemoryStream ms = new MemoryStream((byte[])tbl_on_users.CurrentRow.Cells[3].Value);
                user_picture.Image = Image.FromStream(ms);
            }
            else
            {
                user_id.Text = tbl_on_users.CurrentRow.Cells[0].Value.ToString();
                user_name.Text = tbl_on_users.CurrentRow.Cells[1].Value.ToString();
                user_pass.Text = tbl_on_users.CurrentRow.Cells[2].Value.ToString();
                user_role.Text = "Admin";
                user_picture.Image = null;

            }
        }
        // CRUD
        public void users()
        {
            users_panel.Visible = true;
        }
        private void btn_upload_Click_1(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Select image(*.JpG; *.png; *.jpeg;)|*.JpG; *.png; *.jpeg";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                user_picture.Image = Image.FromFile(openFileDialog1.FileName);
            }
        }
        private void btn_ins_Click(object sender, EventArgs e)
        {
            if (user_id.Text == "" || user_name.Text == "" || user_pass.Text == "" || user_role.Text == "")
            {
                MessageBox.Show("Data belum lengkap!");
            }
            else
            {

                int id = Convert.ToInt32(user_id.Text);
                string nama = user_name.Text;
                string password = user_pass.Text;
                if (user_role.Text == "Cashier")
                {
                    if (user_picture.Image == null)
                    {
                        MessageBox.Show("Data belum lengkap!");
                    }
                    else
                    {
                        MemoryStream baos = new MemoryStream();
                        user_picture.Image.Save(baos, user_picture.Image.RawFormat);

                        using (SqlCommand cmd = new SqlCommand("sp_InsertCashier", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@id_cashier", id);
                            cmd.Parameters.AddWithValue("@name", nama);
                            cmd.Parameters.AddWithValue("@password", password);
                            cmd.Parameters.AddWithValue("@picture", baos.ToArray());
                            conn.Open();
                            cmd.ExecuteNonQuery();
                            conn.Close();
                            MessageBox.Show("Data terdaftar!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Tb_user();
                            users_clear();
                        }
                    }
                }
                else if (user_role.Text == "Admin")
                {
                    using (SqlCommand cmd = new SqlCommand("sp_InsertAdmin", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id_admin", id);
                        cmd.Parameters.AddWithValue("@name", nama);
                        cmd.Parameters.AddWithValue("@password", password);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        MessageBox.Show("Data terdaftar!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Tb_admin();
                        users_clear();
                    }
                }
            }
        }
        private void btn_edt_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(user_id.Text);
            string nama = user_name.Text;
            string password = user_pass.Text;
            MemoryStream baos = new MemoryStream();
            user_picture.Image.Save(baos, user_picture.Image.RawFormat);
            if (user_role.Text == "Cashier")
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateCashier", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_cashier", id);
                    cmd.Parameters.AddWithValue("@name", nama);
                    cmd.Parameters.AddWithValue("@password", password);
                    cmd.Parameters.AddWithValue("@foto", baos.ToArray());
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    MessageBox.Show("Data terupdate!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Tb_user();
                    users_clear();
                }
            }
            else if (user_role.Text == "Admin")
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateAdmin", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_admin", id);
                    cmd.Parameters.AddWithValue("@name", nama);
                    cmd.Parameters.AddWithValue("@password", password);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    MessageBox.Show("Data terupdate!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Tb_admin();
                    users_clear();
                }
            }
        }
        private void btn_del_Click(object sender, EventArgs e)
        {
            int id = Convert.ToInt32(user_id.Text);
            if (user_role.Text == "Cashier")
            {
                using (SqlCommand cmd = new SqlCommand("sp_DeleteCashier", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_cashier", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    MessageBox.Show("Data terhapus!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Tb_user();
                    users_clear();
                }
            }
            else if (user_role.Text == "Admin")
            {
                using (SqlCommand cmd = new SqlCommand("sp_DeleteAdmin", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_admin", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    MessageBox.Show("Data terhapus!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Tb_admin();
                    users_clear();
                }
            }
        }
        // ========== USERS ========== //
    }
}