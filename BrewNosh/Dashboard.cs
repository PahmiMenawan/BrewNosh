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

namespace BrewNosh
{


    public partial class Dashboard : Form
    {
        private Timer timer;
        public Dashboard()
        {
            InitializeComponent();

        }

        SqlConnection conn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Kasir;Integrated Security=True;");
        SqlCommand cmd;


        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        // Table load START
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
            load_data();
        }

        public void load_detail()
        {
            cmd = new SqlCommand($"SELECT * FROM detail_transaksi WHERE id_transaksi = {idTransaksiBaru}", conn);
            load_data();

        }
        public void load_transaksi()
        {
            cmd = new SqlCommand("SELECT * FROM transaksi", conn);
            load_data();
        }
        // Table load END

        private void Dashboard_Load(object sender, EventArgs e)
        {
            Design();
            timer = new Timer();
            timer.Interval = 1000; // Update setiap 1 detik
            timer.Tick += new EventHandler(UpdateClock);
            timer.Start();
            load_product();
            getDate();
        }

        public void getDate()
        {
            DateTime tanggalSekarang = DateTime.Now;
            label3.Text = tanggalSekarang.ToString("dd-MM-yyyy");
        }

        private void UpdateClock(object sender, EventArgs e)
        {
            t_jam.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void Design()
        {
            // Search Button

            // Pay Button

            // Pay button

            // Add order

        }



        private void label2_Click_1(object sender, EventArgs e)
        {
            load_product();
        }

        private void t_logout_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();

        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_Click(object sender, EventArgs e)
        {
            label_product_name.Text = master_table.CurrentRow.Cells[1].Value.ToString();
            jml_barang.Value = 0;
        }

        private void label12_Click(object sender, EventArgs e)
        {
            load_detail();
        }


        int idTransaksiBaru;

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


        private void t_transaksi_Click(object sender, EventArgs e)
        {
            load_transaksi();
        }

        // Debug
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

        public void letThereBeLight()
        {
            cmd = new SqlCommand("INSERT INTO produk (Id_produk, nama_barang, harga_barang, stok) VALUES " +
                "('1', 'cappucino', '5000', '50'), " +
                "('2', 'americano', '5000', '50'), " +
                "('3', 'ice tea', '2500', '50'), " +
                "('4', 'sandwich', '10000', '50'), " +
                "('5', 'croissant', '15000', '50'), " +
                "('6', 'indomie', '10000', '50'), " +
                "('7', 'beef lasagna', '35000', '50'), " +
                "('8', 'spaghetti', '50000', '50');", conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
            load_product();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            letThereBeLight();
            load_data();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            doomsday_procedure();

            load_data();
        }





        private void master_table_Click(object sender, EventArgs e)
        {
            label_product_name.Text = master_table.CurrentRow.Cells[1].Value.ToString();
            jml_barang.Value = 1;
        }
       

        private void pay_btn_Click(object sender, EventArgs e)
        {
            if(t_bayar.Text != "")
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

        private void jml_barang_ValueChanged(object sender, EventArgs e)
        {

        }
        int pesanan = 0;
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

        private void t_jam_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void accept_btn_Click(object sender, EventArgs e)
        {
            pay_btn.Enabled = true;
            t_bayar.Enabled = true;
            l_kembalian.Text = t_harga.Text;
        }


        private void txt_search_TextChanged(object sender, EventArgs e)
        {
            string keyword = txt_search.Text.Trim();

            cmd = new SqlCommand("SELECT * FROM Produk WHERE nama_barang LIKE @keyword", conn);
            cmd.Parameters.AddWithValue("@keyword", "%" + keyword + "%");

            conn.Open();
            // langsung panggil load_data setelah cmd sudah siap
            load_data();
            conn.Close();
        }

        private void t_bayar_TextChanged(object sender, EventArgs e)
        {
            if(t_bayar.Text != "")
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



        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click_2(object sender, EventArgs e)
        {

        }

        private void jml_barang_ValueChanged_1(object sender, EventArgs e)
        {

        }

        private void label_product_name_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {

        }

        private void panel11_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void l_hrg_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void strk_name_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel10_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel9_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void panel7_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel8_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void panel6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void panel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
