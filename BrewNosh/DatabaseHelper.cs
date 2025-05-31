using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrewNosh
{
    public static class DatabaseHelper
    {
        // Update with your actual connection string
        private static readonly string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Kasir;Integrated Security=True;";

        // Reusable method to get a new SqlConnection
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

    }
}
