using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace QuanLySinhVienOracle
{
    public class OracleHelper
    {
        private string connectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)" +
            "(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVER=DEDICATED)" +
            "(SERVICE_NAME=orclsv)));User Id=QuanLySinhVien;Password=123;";
        private OracleConnection conn;

        public OracleHelper()
        {
            conn = new OracleConnection(connectionString);
        }

        public bool OpenConnection()
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                    return true;
                }
                return false;
            }
            catch (OracleException ex)
            {
                // Xử lý lỗi kết nối
                Console.WriteLine("Lỗi kết nối đến Oracle: " + ex.Message);
                return false;
            }
        }

        public void CloseConnection()
        {
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }

        public OracleConnection GetConnection()
        {
            return conn;
        }
    }
}
