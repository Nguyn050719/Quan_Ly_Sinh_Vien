using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;

namespace QuanLySinhVienOracle
{
    public partial class AdminForm : Form
    {
        private string _username;
        private AuthManager _auth;

        // Các biến giao diện
        private TabControl tabControl;
        private TabPage tabDiem;
        private TabPage tabMonHoc;
        private ComboBox cboSinhVien;
        private ComboBox cboMonHoc;
        private TextBox txtDiemSo;
        private Button btnThemDiem;
        private DataGridView dgvAllDiem;
        private TextBox txtMaMH;
        private TextBox txtTenMH;
        private TextBox txtSoTinChi;
        private Button btnThemMon;

        public AdminForm(string username)
        {
            // 1. Giữ nguyên hàm này của Visual Studio (để khởi tạo Form cơ bản)
            InitializeComponent();

            // 2. Gọi hàm vẽ giao diện riêng của mình SAU ĐÓ
            VeGiaoDien();

            _username = username;
            _auth = new AuthManager();
        }

        private void AdminForm_Load(object sender, EventArgs e)
        {
            LoadComboBoxData();
            LoadAllDiem();
        }

        // --- PHẦN 1: LOGIC XỬ LÝ (Code giữ nguyên như cũ) ---
        private void LoadComboBoxData()
        {
            if (_auth.dbHelper.OpenConnection())
            {
                // Load Sinh Viên
                string sqlSV = "SELECT MaSV, HoTen FROM SinhVien";
                using (OracleCommand cmd = new OracleCommand(sqlSV, _auth.dbHelper.GetConnection()))
                {
                    OracleDataAdapter da = new OracleDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    cboSinhVien.DataSource = dt;
                    cboSinhVien.DisplayMember = "HoTen";
                    cboSinhVien.ValueMember = "MaSV";
                }

                // Load Môn Học
                string sqlMH = "SELECT MaMH, TenMH FROM MonHoc";
                using (OracleCommand cmd = new OracleCommand(sqlMH, _auth.dbHelper.GetConnection()))
                {
                    OracleDataAdapter da = new OracleDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    cboMonHoc.DataSource = dt;
                    cboMonHoc.DisplayMember = "TenMH";
                    cboMonHoc.ValueMember = "MaMH";
                }
                _auth.dbHelper.CloseConnection();
            }
        }

        private void LoadAllDiem()
        {
            string sql = @"SELECT sv.HoTen, sv.MaSV, mh.TenMH, d.DiemSo
                           FROM Diem d
                           JOIN SinhVien sv ON d.FK_SV_Diem = sv.MaSV
                           JOIN MonHoc mh ON d.FK_MH_Diem = mh.MaMH
                           ORDER BY sv.HoTen";

            if (_auth.dbHelper.OpenConnection())
            {
                _auth.SetUserContext(_username);
                using (OracleCommand cmd = new OracleCommand(sql, _auth.dbHelper.GetConnection()))
                {
                    DataTable dt = new DataTable();
                    OracleDataAdapter da = new OracleDataAdapter(cmd);
                    da.Fill(dt);
                    dgvAllDiem.DataSource = dt;
                }
                _auth.dbHelper.CloseConnection();
            }
        }

        private void btnThemDiem_Click(object sender, EventArgs e)
        {
            if (cboSinhVien.SelectedValue == null || cboMonHoc.SelectedValue == null) return;

            string maSV = cboSinhVien.SelectedValue.ToString();
            string maMH = cboMonHoc.SelectedValue.ToString();
            float diemSo;

            if (!float.TryParse(txtDiemSo.Text, out diemSo))
            {
                MessageBox.Show("Điểm số phải là số.");
                return;
            }

            string maDiem = "D" + DateTime.Now.Ticks.ToString().Substring(10);

            if (_auth.dbHelper.OpenConnection())
            {
                OracleConnection conn = _auth.dbHelper.GetConnection();
                OracleTransaction trans = conn.BeginTransaction();

                try
                {
                    string sqlDiem = "INSERT INTO Diem (MaDiem, DiemSo, FK_SV_Diem, FK_MH_Diem) VALUES (:id, :d, :sv, :mh)";
                    using (OracleCommand cmd = new OracleCommand(sqlDiem, conn))
                    {
                        cmd.Parameters.Add("id", maDiem);
                        cmd.Parameters.Add("d", diemSo);
                        cmd.Parameters.Add("sv", maSV);
                        cmd.Parameters.Add("mh", maMH);
                        cmd.ExecuteNonQuery();
                    }

                    string sqlDiemMH = "INSERT INTO Diem_MonHoc (FK_Diem_DMH, FK_MH_DMH) VALUES (:id, :mh)";
                    using (OracleCommand cmd = new OracleCommand(sqlDiemMH, conn))
                    {
                        cmd.Parameters.Add("id", maDiem);
                        cmd.Parameters.Add("mh", maMH);
                        cmd.ExecuteNonQuery();
                    }

                    string sqlDiemSV = "INSERT INTO Diem_SinhVien (FK_SV_DiemSV, FK_Diem_DiemSV) VALUES (:sv, :id)";
                    using (OracleCommand cmd = new OracleCommand(sqlDiemSV, conn))
                    {
                        cmd.Parameters.Add("sv", maSV);
                        cmd.Parameters.Add("id", maDiem);
                        cmd.ExecuteNonQuery();
                    }

                    trans.Commit();
                    MessageBox.Show("Thêm điểm thành công!");
                    LoadAllDiem();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Lỗi thêm điểm: " + ex.Message);
                }
                finally
                {
                    _auth.dbHelper.CloseConnection();
                }
            }
        }

        private void btnThemMon_Click(object sender, EventArgs e)
        {
            string maMH = txtMaMH.Text;
            string tenMH = txtTenMH.Text;
            int tinChi;

            if (string.IsNullOrEmpty(maMH) || string.IsNullOrEmpty(tenMH) || !int.TryParse(txtSoTinChi.Text, out tinChi))
            {
                MessageBox.Show("Vui lòng nhập đúng thông tin môn học.");
                return;
            }

            string sql = "INSERT INTO MonHoc (MaMH, TenMH, SoTinChi) VALUES (:ma, :ten, :tc)";

            if (_auth.dbHelper.OpenConnection())
            {
                try
                {
                    using (OracleCommand cmd = new OracleCommand(sql, _auth.dbHelper.GetConnection()))
                    {
                        cmd.Parameters.Add("ma", maMH);
                        cmd.Parameters.Add("ten", tenMH);
                        cmd.Parameters.Add("tc", tinChi);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Thêm môn học thành công!");
                        LoadComboBoxData();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
                finally { _auth.dbHelper.CloseConnection(); }
            }
        }

        // --- PHẦN 2: VẼ GIAO DIỆN (ĐÃ ĐỔI TÊN HÀM ĐỂ KHÔNG BỊ LỖI) ---

        // Đã đổi tên từ InitializeComponent -> VeGiaoDien
        private void VeGiaoDien()
        {
            this.Size = new Size(800, 500);
            this.Text = "Quản Lý Đào Tạo (Admin)";

            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;

            // --- TAB 1: QUẢN LÝ ĐIỂM ---
            tabDiem = new TabPage("Quản lý Điểm");

            Label lblSV = new Label() { Text = "Sinh Viên:", Location = new Point(20, 20), AutoSize = true };
            cboSinhVien = new ComboBox() { Location = new Point(100, 20), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblMH = new Label() { Text = "Môn Học:", Location = new Point(320, 20), AutoSize = true };
            cboMonHoc = new ComboBox() { Location = new Point(400, 20), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblDiem = new Label() { Text = "Điểm số:", Location = new Point(20, 60), AutoSize = true };
            txtDiemSo = new TextBox() { Location = new Point(100, 60), Width = 100 };

            btnThemDiem = new Button() { Text = "Nhập Điểm", Location = new Point(220, 58), Width = 100 };
            btnThemDiem.Click += btnThemDiem_Click;

            dgvAllDiem = new DataGridView() { Location = new Point(20, 100), Size = new Size(740, 320), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom };

            tabDiem.Controls.AddRange(new Control[] { lblSV, cboSinhVien, lblMH, cboMonHoc, lblDiem, txtDiemSo, btnThemDiem, dgvAllDiem });

            // --- TAB 2: QUẢN LÝ MÔN HỌC ---
            tabMonHoc = new TabPage("Thêm Môn Học");

            Label lblMaMH = new Label() { Text = "Mã Môn:", Location = new Point(50, 50) };
            txtMaMH = new TextBox() { Location = new Point(150, 50), Width = 200 };

            Label lblTenMH = new Label() { Text = "Tên Môn:", Location = new Point(50, 100) };
            txtTenMH = new TextBox() { Location = new Point(150, 100), Width = 200 };

            Label lblTinChi = new Label() { Text = "Số TC:", Location = new Point(50, 150) };
            txtSoTinChi = new TextBox() { Location = new Point(150, 150), Width = 100 };

            btnThemMon = new Button() { Text = "Thêm Môn Mới", Location = new Point(150, 200), Size = new Size(150, 40) };
            btnThemMon.Click += btnThemMon_Click;

            tabMonHoc.Controls.AddRange(new Control[] { lblMaMH, txtMaMH, lblTenMH, txtTenMH, lblTinChi, txtSoTinChi, btnThemMon });

            // Add Tabs
            tabControl.TabPages.Add(tabDiem);
            tabControl.TabPages.Add(tabMonHoc);
            this.Controls.Add(tabControl);
        }
    }
}