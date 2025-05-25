using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;

namespace BrewNosh
{
    public partial class Registrasi : Form
    {
        public Registrasi()
        {
            InitializeComponent();
        }

        SqlConnection conn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Kasir;Integrated Security=True;");
        SqlCommand cmd;

        private void button1_Click(object sender, EventArgs e)
        {
            string id = t_id.Text;
            string nama = t_nama.Text;
            string pw = t_pw.Text;
            string kpw = t_kpw.Text;
            MemoryStream baos = new MemoryStream();
            pictureBox1.Image.Save(baos, pictureBox1.Image.RawFormat);
            if (id != "" && nama != "" && pw != "" && kpw != "" && pictureBox1.Image != defaultImage)
            {
                if(pw == kpw)
                {
                    cmd = new SqlCommand($"INSERT INTO [Cashier] (id_cashier, name, password, picture) values ('{id}', '{nama}', '{pw}', @image)", conn);
                    cmd.Parameters.AddWithValue("Image", baos.ToArray());
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    DialogResult result = MessageBox.Show("Data terdaftar, kembali ke halaman login?", "Registrasi berhasil", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if(result == DialogResult.Yes)
                    {
                        Form1 form1 = new Form1();
                        form1.Show();
                        this.Hide();
                    }
                    else
                    {
                        clear();
                    }
                }
                else
                {
                    MessageBox.Show("PASSWORD SALAH!");
                }
                clear();
            }
            else
            {
                MessageBox.Show("Data belum lengkap!");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            clear();
        }

        Image defaultImage;

        public void clear()
        {
            t_id.Text = string.Empty;
            t_nama.Text = string.Empty;
            t_pw.Text = string.Empty;
            t_kpw.Text = string.Empty;
            pictureBox1.Image = defaultImage;
            

        }

        private void Registrasi_Load(object sender, EventArgs e)
        {
            defaultImage = pictureBox1.Image;
        }

        

        private void button3_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Selet image(*.JpG; *.png; *.jpeg;)|*.JpG; *.png; *.jpeg";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = Image.FromFile(openFileDialog1.FileName);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }
    }
}
