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
using System.Security.Permissions;

namespace BrewNosh
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        SqlConnection conn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Kasir;Integrated Security=True;");
        SqlCommand cmd;

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void btn_lgn_Click(object sender, EventArgs e)
        {
            string id = t_id.Text;
            string pw = t_pw.Text;

            cmd = new SqlCommand($"SELECT * FROM Cashier WHERE id_cashier = '{id}' AND password = '{pw}'", conn);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();


            if (dt.Rows.Count > 0)
            {
                MessageBox.Show("Berhasil login!");
                Dashboard kl = new Dashboard();
                kl.Show();
                this.Hide();

            }
            else
            {
                MessageBox.Show("Password atau id salah!");
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Registrasi kl = new Registrasi();
            kl.Show();
            this.Hide();
        }
    }
}
