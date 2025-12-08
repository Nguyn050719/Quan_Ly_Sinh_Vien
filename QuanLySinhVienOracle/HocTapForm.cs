using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;

namespace QuanLySinhVienOracle
{
    public partial class HocTapForm: Form
    {
        private string _username;
        private AuthManager _auth;
        public HocTapForm(string username)
        {
            InitializeComponent();
            _username = username;
            _auth = new AuthManager();
        }

        private void HocTapForm_Load(object sender, EventArgs e)
        {
            LoadDiem();
        }
        private void LoadDiem()
        {
            // QUAN TRỌNG: Câu lệnh SELECT KHÔNG CẦN WHERE
            // VPD trong Oracle sẽ tự động thêm WHERE ngầm bên dưới
            string sql = @"SELECT d.MaDiem, mh.TenMH, d.DiemSo
                       FROM Diem d
                       JOIN Diem_MonHoc dmh ON d.MaDiem = dmh.FK_Diem_DMH
                       JOIN MonHoc mh ON dmh.FK_MH_DMH = mh.MaMH";

            if (_auth.dbHelper.OpenConnection())
            {
                // 1. GỌI CONTEXT (Bắt buộc để VPD hoạt động)
                _auth.SetUserContext(_username);

                // 2. Chạy câu lệnh Select "trần trụi"
                using (OracleCommand cmd = new OracleCommand(sql, _auth.dbHelper.GetConnection()))
                {
                    DataTable dt = new DataTable();
                    using (OracleDataAdapter da = new OracleDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                    dgvDiem.DataSource = dt;
                }
                _auth.dbHelper.CloseConnection();
            }
        }

    }
}
