using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using PdfSharp.Pdf;
using PdfSharp.Fonts;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

namespace BrewNosh
{
    public partial class Dashboard : Form
    {
        private Timer timer;
        public Dashboard()
        {
            InitializeComponent();
        }
        SqlConnection conn = DatabaseHelper.GetConnection();
        SqlCommand cmd;
        private void Dashboard_Load(object sender, EventArgs e)
        {
            timer = new Timer();
            timer.Interval = 1000; // Update setiap 1 detik
            timer.Tick += new EventHandler(UpdateClock);
            timer.Start();
            load_product();
            getDate();
        }
        // ========== TABLE LOAD ==========
        public void load_data()
        {
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.SelectCommand = cmd;
            DataTable dt = new DataTable();
            dt.Clear();
            sda.Fill(dt);
            master_table.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            master_table.DataSource = dt;
        }
        public void load_product()
        {
            cmd = new SqlCommand("SELECT Id_produk AS 'Product Id', nama_barang AS 'Nama Barang', harga_barang AS 'Harga Barang', stok FROM Produk", conn);
            master_table.Enabled = true;
            load_data();
        }
        public void load_detail()
        {
            cmd = new SqlCommand($"SELECT * FROM detail_transaksi WHERE id_transaksi = {idTransaksiBaru}", conn);
            master_table.Enabled = false;
            load_data();
        }
        public void load_transaksi()
        {
            cmd = new SqlCommand("SELECT * FROM transaksi", conn);
            master_table.Enabled = false;
            load_data();
        }
        // NavBar
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            load_product();
        }
        private void label2_Click_1(object sender, EventArgs e)
        {
            load_product();
        }
        private void label12_Click(object sender, EventArgs e)
        {
            load_detail();
        }
        private void pictureBox5_Click(object sender, EventArgs e)
        {
            load_detail();
        }
        private void t_transaksi_Click(object sender, EventArgs e)
        {
            load_transaksi();
        }
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            load_transaksi();
        }
        // ========== TABLE LOAD ========== //

        // Global Variables 
        int idTransaksiBaru;
        int pesanan = 0;
        // Global Variables //

        // ========== GLOBAL FUNCTIONS ==========
        public void getDate()
        {
            DateTime tanggalSekarang = DateTime.Now;
            label3.Text = tanggalSekarang.ToString("dd-MM-yyyy");
        }
        private void UpdateClock(object sender, EventArgs e)
        {
            t_jam.Text = DateTime.Now.ToString("HH:mm:ss");
        }
        public void add_transaksi()
        {
            cmd = new SqlCommand("INSERT INTO Transaksi (tanggal, harga_total) VALUES (GETDATE(), 0);" +
                "SELECT SCOPE_IDENTITY()", conn);
            conn.Open();
            idTransaksiBaru = Convert.ToInt32(cmd.ExecuteScalar());
            conn.Close();
        }
        public void add_detail()
        {
            string id = master_table.CurrentRow.Cells[0].Value.ToString();
            int jumlah = Convert.ToInt32(jml_barang.Text);
            int harga = (Convert.ToInt32(master_table.CurrentRow.Cells[2].Value) * jumlah);
            cmd = new SqlCommand($"INSERT INTO detail_transaksi (id_transaksi, id_produk, jumlah, subtotal) VALUES ({idTransaksiBaru}, {id}, {jumlah}, {harga});", conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
        }
        public void updateStock()
        {
            // Ambil ID produk dari baris yang sedang dipilih di master_table
            int idProduk = Convert.ToInt32(master_table.CurrentRow.Cells[0].Value);

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
            load_product();
        }
        private void ExportTextToPdf()
        {
            // ——————————————  
            // Aktifkan Windows Fonts agar PlatformFontResolver otomatis
            // mengenali Times New Roman, Arial, Courier New, dsb.
            GlobalFontSettings.UseWindowsFontsUnderWindows = true;
            // :contentReference[oaicite:0]{index=0}
            // ——————————————  
            string pesanan = strk_name.Text;
            string harga = t_harga.Text;

            // 1. Buat dokumen MigraDoc
            var document = new Document();
            // 2. Atur style default 'Normal' ke Times New Roman
            var normal = document.Styles["Normal"];
            normal.Font.Name = "Times New Roman";
            normal.Font.Size = 12;
            normal.ParagraphFormat.Alignment = ParagraphAlignment.Left;

            // 3. Tambahkan section + paragraf
            var section = document.AddSection();
            // 1. Buat satu paragraph saja (satu baris)
            var paragraph = section.AddParagraph();

            // 2. Tambahkan tab stop di posisi X (misal 10 cm dari margin kiri)
            paragraph.Format.TabStops.AddTabStop(Unit.FromCentimeter(10));
            section.PageSetup.TopMargin = Unit.FromCentimeter(2);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(2);
            section.AddParagraph("TERIMAKASIH TELAH BERBELANJA DI BREWNOSH");
            section.AddParagraph("========================================");
            section.AddParagraph(pesanan);
            section.AddParagraph("========================================");
            section.AddParagraph(harga);

            // 4. Render PDF (gunakan ctor tanpa bool, karena Unicode selalu aktif)
            var pdfRenderer = new PdfDocumentRenderer() { Document = document };
            pdfRenderer.RenderDocument();

            // 5. SaveFileDialog & simpan
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "PDF File|*.pdf";
                sfd.FileName = "percobaan.pdf";
                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    pdfRenderer.PdfDocument.Save(sfd.FileName);
                    MessageBox.Show("PDF berhasil disimpan di:\n" + sfd.FileName,
                                    "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal menyimpan PDF:\n" + ex.Message,
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ========== GLOBAL FUNCTIONS ========== //

        // Logout
        private void t_logout_Click(object sender, EventArgs e)
        {
            Form form1 = new Form1();
            form1.Show();
            this.Hide();
        }
        private void pictureBox6_Click(object sender, EventArgs e)
        {
            Form form1 = new Form1();
            form1.Show();
            this.Hide();

        }
        // Logout //

        // ========== SCENARIO ==========
        // Scenario - Search
        private void txt_search_TextChanged(object sender, EventArgs e)
        {
            string keyword = txt_search.Text.Trim();

            cmd = new SqlCommand("SELECT * FROM Produk WHERE nama_barang LIKE @keyword", conn);
            cmd.Parameters.AddWithValue("@keyword", "%" + keyword + "%");

            conn.Open();
            load_data();
            conn.Close();
        }
        // Scenario - Add Order
        private void order_btn_Click(object sender, EventArgs e)
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
        // Scenario - Select & Add Product
        private void master_table_Click(object sender, EventArgs e)
        {
            label_product_name.Text = master_table.CurrentRow.Cells[1].Value.ToString();
            jml_barang.Value = 1;
        }
        private void add_btn_Click(object sender, EventArgs e)
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
                if (label_product_name.Text == "")
                {
                    MessageBox.Show("Anda belum memiliih pesanan!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    if (pesanan < 16)
                    {
                        int idProduk = Convert.ToInt32(master_table.CurrentRow.Cells[0].Value);
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

                            pesanan += 1;
                            add_detail();
                            cmd = new SqlCommand($"SELECT SUM(subtotal) FROM detail_transaksi WHERE id_transaksi = {idTransaksiBaru};", conn);
                            conn.Open();
                            int hargaDisplay = Convert.ToInt32(cmd.ExecuteScalar());
                            t_harga.Text = hargaDisplay.ToString("C", new CultureInfo("id-ID"));
                            conn.Close();
                            int jumlah = Convert.ToInt32(jml_barang.Text);
                            int strk_hrg = (Convert.ToInt32(master_table.CurrentRow.Cells[2].Value) * jumlah);
                            string strng_hrg = string.Format(new CultureInfo("id-ID"), "Rp{0:N0}", strk_hrg);

                            strk_name.Text += label_product_name.Text + " x" + jumlah + "\n";
                            l_hrg.Text += strng_hrg + "\n";
                            updateStock();
                            label_product_name.Text = "";
                        }
                    }
                    else
                    {
                        MessageBox.Show("Pesanan mu melebihi batas", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                }
            }
        }
        // Scenario - Pay Order
        private void t_bayar_TextChanged(object sender, EventArgs e)
        {
            if (t_bayar.Text != "")
            {
                cmd = new SqlCommand($"SELECT SUM(subtotal) FROM detail_transaksi WHERE id_transaksi = {idTransaksiBaru};", conn);
                conn.Open();
                int harga = Convert.ToInt32(cmd.ExecuteScalar());
                conn.Close();
                int bayar = Convert.ToInt32(t_bayar.Text);

                int kembalian = bayar - harga;
                l_kembalian.Text = kembalian.ToString("C", new CultureInfo("id-ID"));
                if (kembalian < 0)
                {
                    l_kembalian.Text = "";
                }
            }
        }
        private void t_bayar_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // batalkan input karakter selain angka
            }
        }
        private void pay_btn_Click(object sender, EventArgs e)
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
                        ExportTextToPdf();
                        cmd = new SqlCommand($"UPDATE Transaksi SET harga_total = (SELECT SUM(subtotal) FROM detail_transaksi WHERE id_transaksi = {idTransaksiBaru}) WHERE id_transaksi = {idTransaksiBaru};", conn);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        t_harga.Text = "Rp.0";
                        strk_name.Text = "";
                        l_hrg.Text = "";
                        t_bayar.Text = "";
                        l_kembalian.Text = "";
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
        // ========== SCENARIO ========== //
    }
}