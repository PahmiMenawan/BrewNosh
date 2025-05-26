using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace BrewNosh
{
    public partial class AdminDashboard : Form
    {
        public AdminDashboard()
        {
            InitializeComponent();
        }

        private void AdminDashboard_Load(object sender, EventArgs e)
        {
            dashboard();
            SetupLineChart();
        }

        private void SetupLineChart()
        {
            // Bersihkan series lama jika ada
            chart1.Series.Clear();

            // Tambah series baru dengan nama "Penjualan"
            Series series = new Series("Penjualan");

            // Set tipe chart jadi Line
            series.ChartType = SeriesChartType.Line;

            // Tambahkan data dummy (misal data penjualan per hari)
            series.Points.AddXY("Senin", 50);
            series.Points.AddXY("Selasa", 75);
            series.Points.AddXY("Rabu", 60);
            series.Points.AddXY("Kamis", 80);
            series.Points.AddXY("Jumat", 90);
            series.Points.AddXY("Sabtu", 70);
            series.Points.AddXY("Minggu", 100);

            // Tambahkan series ke chart
            chart1.Series.Add(series);

            // Opsi tambahan supaya label sumbu X tampil dengan baik
            chart1.ChartAreas[0].AxisX.Interval = 1;
            chart1.ChartAreas[0].AxisX.Title = "Hari";
            chart1.ChartAreas[0].AxisY.Title = "Jumlah Penjualan";

            // Refresh chart supaya langsung tampil
            chart1.Invalidate();
        }
    

    // Sidebar btn
    private void dashboard_btn_Click(object sender, EventArgs e)
        {
            deactivate();
            dashboard();
        }
        // Sidebar btn //
        // Panels
        public void dashboard()
        {
            panel_penghasilan.Visible = true;
            panel_produk.Visible = true;
            panel_transaksi.Visible = true;
        }
        public void produkStok()
        {

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
        }
        // Panels //
    }
}
