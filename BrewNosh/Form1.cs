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

        SqlConnection conn = DatabaseHelper.GetConnection();
        SqlCommand cmd;
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        // ========== LOGIN ==========
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            string id = t_id.Text;
            string pw = t_pw.Text;

            cmd = new SqlCommand($"SELECT * FROM Admin WHERE id_admin = '{id}' AND password = '{pw}'", conn);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
            cmd = new SqlCommand($"SELECT * FROM Cashier WHERE id_cashier = '{id}' AND password = '{pw}'", conn);
            SqlDataAdapter sdaa = new SqlDataAdapter(cmd);
            DataTable dtt = new DataTable();
            sdaa.Fill(dtt);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            if (id == "" || pw == "")
            {
                MessageBox.Show("Id atau password tidak boleh kosong!");
                return;
            }

            if (dt.Rows.Count > 0)
            {
                DialogResult result = MessageBox.Show("Kamu login sebagai admin, apakah kamu ingin mengunjungi dashboard admin? (Pilih no untuk ke dashboard kasir)", "Login", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                switch (result)
                {
                    case DialogResult.Yes:
                        AdminDashboard adminDashboard = new AdminDashboard();
                        adminDashboard.Show();
                        this.Hide();
                        break;
                    case DialogResult.No:
                        Dashboard dashboard = new Dashboard();
                        dashboard.Show();
                        this.Hide();
                        break;
                    case DialogResult.Cancel:
                        break;
                }
                clear();
            }
            else if (dtt.Rows.Count > 0)
            {
                MessageBox.Show("Kamu login sebagai kasir, silahkan ke dashboard kasir!", "Login", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Dashboard dashboard = new Dashboard();
                dashboard.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Password atau id salah!");
            }
        }
        private void guna2Button2_Click(object sender, EventArgs e)
        {
            clear();
        }
        public void clear()
        {
            t_id.Text = "";
            t_pw.Text = "";
        }
        private void close_btn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        // ========= LOGIN ========== //
    }
}
