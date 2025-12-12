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
        private TabPage tabGiamSat;
        private TabPage tabSinhVien;

        // CÁC BIẾN MỚI CHO CẤU TRÚC CÀI ĐẶT
        private Button btnSettings;
        private Panel pnlSettingsControls;
        private Button btnDeleteAll; // Thay cho btnSoftDeleteSV, dùng để xem tất cả
        private Button btnCustomize;
        private Panel pnlCustomizeView;

        // DGV và các nút hành động được đặt trong pnlCustomizeView
        private DataGridView dgvSinhVien;
        private Button btnRestoreSV;
        private Button btnHardDeleteSV;

        // Các biến còn lại
        private ComboBox cboSinhVien;
        private ComboBox cboMonHoc;
        private TextBox txtDiemSo;
        private Button btnThemDiem;
        private DataGridView dgvAllDiem;
        private TextBox txtMaMH;
        private TextBox txtTenMH;
        private TextBox txtSoTinChi;
        private Button btnThemMon;
        private DataGridView dgvAuditLog;

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
        private void InsertNhatKyHeThongEntry(string action, string status, string notes)
        {
            // Sử dụng bảng NhatKyHeThong đã được cung cấp
            string sql = "INSERT INTO NhatKyHeThong (TaiKhoan, HanhDong, ThoiGian, TrangThai, GhiChu) " +
                         "VALUES (:user, :action, SYSTIMESTAMP, :status, :notes)";

            // Ghi log trên một kết nối mới để không can thiệp vào transaction chính (nếu có)
            if (_auth.dbHelper.OpenConnection())
            {
                try
                {
                    using (OracleCommand cmd = new OracleCommand(sql, _auth.dbHelper.GetConnection()))
                    {
                        cmd.Parameters.Add("user", _username);
                        cmd.Parameters.Add("action", action);
                        cmd.Parameters.Add("status", status);
                        cmd.Parameters.Add("notes", notes);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi ghi NhatKyHeThong: " + ex.Message);
                }
                finally
                {
                    _auth.dbHelper.CloseConnection();
                }
            }
        }
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
        private void LoadSinhVienList(bool onlyDisabled)
        {
            string filter = onlyDisabled ? "WHERE TrangThai = 'DISABLED'" : "";
            string sql = $@"SELECT MaSV, HoTen, NgaySinh, DiaChi, TrangThai
                           FROM SinhVien
                           {filter}
                           ORDER BY TrangThai DESC, MaSV";

            if (_auth.dbHelper.OpenConnection())
            {
                try
                {
                    using (OracleCommand cmd = new OracleCommand(sql, _auth.dbHelper.GetConnection()))
                    {
                        DataTable dt = new DataTable();
                        OracleDataAdapter da = new OracleDataAdapter(cmd);
                        da.Fill(dt);
                        dgvSinhVien.DataSource = dt;
                        // Cập nhật tên cột
                        if (dgvSinhVien.Columns.Contains("MaSV")) dgvSinhVien.Columns["MaSV"].HeaderText = "Mã SV";
                        if (dgvSinhVien.Columns.Contains("HoTen")) dgvSinhVien.Columns["HoTen"].HeaderText = "Họ Tên";
                        if (dgvSinhVien.Columns.Contains("NgaySinh")) dgvSinhVien.Columns["NgaySinh"].HeaderText = "Ngày Sinh";
                        if (dgvSinhVien.Columns.Contains("DiaChi")) dgvSinhVien.Columns["DiaChi"].HeaderText = "Địa Chỉ";
                        if (dgvSinhVien.Columns.Contains("TrangThai")) dgvSinhVien.Columns["TrangThai"].HeaderText = "Trạng Thái";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải danh sách sinh viên: " + ex.Message);
                }
                finally
                {
                    _auth.dbHelper.CloseConnection();
                }
            }
        }
        
        private void UpdateSinhVienStatus(string maSV, string hoTen, string newStatus, string action)
        {
            string status = "FAILURE";
            string notes = "";
            string sql = "UPDATE SinhVien SET TrangThai = :status WHERE MaSV = :ma";
            string actionName = action == "DISABLE" ? "XOA_TAM_THOI_SV" : "KHOI_PHUC_SV";
            string verb = action == "DISABLE" ? "xóa tạm thời" : "khôi phục";

            if (_auth.dbHelper.OpenConnection())
            {
                try
                {
                    using (OracleCommand cmd = new OracleCommand(sql, _auth.dbHelper.GetConnection()))
                    {
                        cmd.Parameters.Add("status", newStatus);
                        cmd.Parameters.Add("ma", maSV);
                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            status = "SUCCESS";
                            notes = $"{verb} SV thành công: {hoTen} ({maSV})";
                            MessageBox.Show($"{rows} sinh viên ({hoTen}) đã được {verb}!");

                            // Sau khi thay đổi, tải lại view hiện tại
                            if (pnlCustomizeView.Visible)
                            {
                                // Nếu đang ở chế độ Tùy chỉnh (Disabled List), tải lại danh sách Disabled
                                LoadSinhVienList(true);
                            }
                            // Tải lại các control chung
                            LoadComboBoxData();
                            LoadAllDiem();
                        }
                        else
                        {
                            notes = $"Không tìm thấy SV {maSV} để cập nhật.";
                            MessageBox.Show("Không tìm thấy sinh viên để cập nhật.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    notes = $"Lỗi {actionName} SV {maSV}: {ex.Message}";
                    MessageBox.Show($"Lỗi thực hiện {actionName}: {ex.Message}");
                }
                finally
                {
                    _auth.dbHelper.CloseConnection();
                    InsertNhatKyHeThongEntry(actionName, status, notes);
                }
            }
        }
        private void HardDeleteSinhVien(string maSV, string hoTen)
        {
            string status = "FAILURE";
            string notes = "";
            string sql = "DELETE FROM SinhVien WHERE MaSV = :ma";
            if (_auth.dbHelper.OpenConnection())
            {
                try
                {
                    using (OracleCommand cmd = new OracleCommand(sql, _auth.dbHelper.GetConnection()))
                    {
                        cmd.Parameters.Add("ma", maSV);
                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            status = "SUCCESS";
                            notes = $"Xóa VĨNH VIỄN SV thành công: {hoTen} ({maSV}). Dữ liệu liên quan đã bị xóa theo CASCADE.";
                            MessageBox.Show($"Đã XÓA VĨNH VIỄN sinh viên {hoTen} ({maSV}) và toàn bộ dữ liệu liên quan!");
                            // Tải lại các view sau khi xóa
                            LoadSinhVienList(true);
                            LoadComboBoxData();
                            LoadAllDiem();
                        }
                        else
                        {
                            notes = $"Không tìm thấy SV {maSV} để xóa.";
                            MessageBox.Show("Không tìm thấy sinh viên để xóa.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    notes = $"Lỗi Xóa VĨNH VIỄN SV {maSV}: {ex.Message}";
                    MessageBox.Show($"Lỗi thực hiện Xóa VĨNH VIỄN: {ex.Message}");
                }
                finally
                {
                    _auth.dbHelper.CloseConnection();
                    InsertNhatKyHeThongEntry("XOA_VĨNH_VIỄN_SV", status, notes);
                }
            }
        }
        private void btnRestoreSV_Click(object sender, EventArgs e)
        {
            if (dgvSinhVien.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một sinh viên để Khôi phục.");
                return;
            }
            string maSV = dgvSinhVien.SelectedRows[0].Cells["MaSV"].Value.ToString();
            string hoTen = dgvSinhVien.SelectedRows[0].Cells["HoTen"].Value.ToString();
            string trangThaiHienTai = dgvSinhVien.SelectedRows[0].Cells["TrangThai"].Value.ToString();

            // Logic chỉ cho phép Khôi phục (Active) khi trạng thái là DISABLED
            if (trangThaiHienTai == "ACTIVE")
            {
                MessageBox.Show($"Sinh viên {hoTen} đang hoạt động (ACTIVE), không cần Khôi phục.");
                return;
            }
            if (MessageBox.Show($"Bạn có chắc chắn muốn REMOVE XÓA (Active) sinh viên {hoTen} ({maSV}) không?", "Xác nhận Khôi Phục", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                UpdateSinhVienStatus(maSV, hoTen, "ACTIVE", "RESTORE");
            }
        }
        private void btnHardDeleteSV_Click(object sender, EventArgs e)
        {
            if (dgvSinhVien.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một sinh viên để xóa vĩnh viễn.");
                return;
            }

            string maSV = dgvSinhVien.SelectedRows[0].Cells["MaSV"].Value.ToString();
            string hoTen = dgvSinhVien.SelectedRows[0].Cells["HoTen"].Value.ToString();
            DialogResult result = MessageBox.Show(
                "CẢNH BÁO! Hành động này sẽ XÓA VĨNH VIỄN sinh viên {hoTen} ({maSV}) và TOÀN BỘ DỮ LIỆU LIÊN QUAN. Bạn có chắc chắn không?",
                "Xác nhận XÓA VĨNH VIỄN",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Stop);

            if (result == DialogResult.Yes)
            {
                HardDeleteSinhVien(maSV, hoTen);
            }
        }
        private void btnSettings_Click(object sender, EventArgs e)
        {
            // Bật/tắt Panel chứa các tùy chọn (Xóa, Tùy chỉnh)
            pnlSettingsControls.Visible = !pnlSettingsControls.Visible;

            // Nếu tắt Cài đặt, ẩn luôn view Tùy chỉnh
            if (!pnlSettingsControls.Visible)
            {
                pnlCustomizeView.Visible = false;
            }

            btnSettings.Text = pnlSettingsControls.Visible ? "Cài đặt (Đang Mở)" : "Cài đặt";
        }

        private void btnDeleteAll_Click(object sender, EventArgs e)
        {
            // Nút "Xóa" ban đầu, dùng để quay lại view quản lý tất cả sinh viên
            pnlCustomizeView.Visible = true; // Hiện panel chứa DGV
            LoadSinhVienList(false); // Tải TẤT CẢ sinh viên
            // Cập nhật nút cho chế độ xem tất cả (có thể Soft Delete)
            btnRestoreSV.Text = "Xóa (Disable)";
            btnRestoreSV.Click -= btnRestoreSV_Click;
            btnRestoreSV.Click += btnSoftDeleteSV_Click;
            btnHardDeleteSV.Visible = false; // Ẩn Xóa Vĩnh Viễn trong view này để giảm rủi ro
        }

        private void btnSoftDeleteSV_Click(object sender, EventArgs e)
        {
            // Xử lý Soft Delete khi ở chế độ xem TẤT CẢ (Sau khi ấn nút Xóa)
            if (dgvSinhVien.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một sinh viên để xóa.");
                return;
            }
            string maSV = dgvSinhVien.SelectedRows[0].Cells["MaSV"].Value.ToString();
            string hoTen = dgvSinhVien.SelectedRows[0].Cells["HoTen"].Value.ToString();
            string trangThaiHienTai = dgvSinhVien.SelectedRows[0].Cells["TrangThai"].Value.ToString();

            if (trangThaiHienTai == "DISABLED")
            {
                MessageBox.Show($"Sinh viên {hoTen} đã bị xóa (DISABLED) rồi.");
                return;
            }

            if (MessageBox.Show($"Bạn có chắc chắn muốn XÓA (Disable) sinh viên {hoTen} ({maSV}) không?", "Xác nhận Xóa Tạm Thời", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                UpdateSinhVienStatus(maSV, hoTen, "DISABLED", "DISABLE");
            }
        }
        private void btnCustomize_Click(object sender, EventArgs e)
        {
            // Hiển thị Panel chứa DGV (chỉ SV đã bị xóa tạm thời)
            pnlCustomizeView.Visible = true;
            LoadSinhVienList(true); // Tải CHỈ sinh viên DISABLED
            // Cập nhật nút cho chế độ Tùy chỉnh (chỉ Khôi phục và Xóa Vĩnh Viễn)
            btnRestoreSV.Text = "Remove (Khôi phục)";
            btnRestoreSV.Click -= btnSoftDeleteSV_Click; // Gỡ sự kiện cũ
            btnRestoreSV.Click += btnRestoreSV_Click; // Gắn lại sự kiện Khôi phục
            btnHardDeleteSV.Visible = true;
        }
        private void LoadAuditLog()
        {
            // Tải dữ liệu từ bảng NhatKyHeThong
            string sql = @"SELECT TaiKhoan, HanhDong, ThoiGian, TrangThai, GhiChu
                           FROM NhatKyHeThong
                           ORDER BY ThoiGian DESC";

            if (_auth.dbHelper.OpenConnection())
            {
                try
                {
                    _auth.SetUserContext(_username);
                    using (OracleCommand cmd = new OracleCommand(sql, _auth.dbHelper.GetConnection()))
                    {
                        DataTable dt = new DataTable();
                        OracleDataAdapter da = new OracleDataAdapter(cmd);
                        da.Fill(dt);
                        dgvAuditLog.DataSource = dt;

                        // Đặt tên tiêu đề cột cho dễ đọc
                        dgvAuditLog.Columns["TaiKhoan"].HeaderText = "Tài khoản";
                        dgvAuditLog.Columns["HanhDong"].HeaderText = "Hành động";
                        dgvAuditLog.Columns["ThoiGian"].HeaderText = "Thời gian";
                        dgvAuditLog.Columns["TrangThai"].HeaderText = "Trạng thái";
                        dgvAuditLog.Columns["GhiChu"].HeaderText = "Ghi chú";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải Log hoạt động: " + ex.Message);
                }
                finally
                {
                    _auth.dbHelper.CloseConnection();
                }
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
        private void btnTaiLog_Click(object sender, EventArgs e)
        {
            LoadAuditLog();
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
            // --- TAB 3: GIÁM SÁT HOẠT ĐỘNG ---
            tabGiamSat = new TabPage("Giám sát Hoạt động");
            Button btnTaiLog = new Button()
            {
                Text = "Tải Log Hoạt Động",
                Location = new Point(20, 20),
                Size = new Size(150, 30)
            };
            btnTaiLog.Click += btnTaiLog_Click;
            dgvAuditLog = new DataGridView()
            {
                Location = new Point(20, 60),
                Size = new Size(740, 360),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            tabGiamSat.Controls.AddRange(new Control[] { btnTaiLog, dgvAuditLog });
            // --- TAB 4: QUẢN LÝ SINH VIÊN (Cấu trúc mới: Cài đặt) ---
            tabSinhVien = new TabPage("Quản lý Sinh Viên");
            tabSinhVien.AutoScroll = true;

            Label lblSVManagement = new Label() { Text = "Quản lý Sinh viên (Trạng thái/Xóa):", Location = new Point(20, 20), AutoSize = true, Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold) };

            // Nút Cài đặt (Main Entry)
            btnSettings = new Button()
            {
                Text = "Cài đặt",
                Location = new Point(20, 50),
                Size = new Size(150, 30)
            };
            btnSettings.Click += btnSettings_Click;

            // Panel chứa nút Xóa và Tùy chỉnh (Hiện ra sau khi click Cài đặt)
            pnlSettingsControls = new Panel()
            {
                Location = new Point(180, 50),
                Size = new Size(350, 40),
                BorderStyle = BorderStyle.None,
                Visible = false
            };

            btnDeleteAll = new Button()
            { // Thay cho "Xóa"
                Text = "Xóa (Xem Tất Cả SV)",
                Location = new Point(0, 0),
                Size = new Size(150, 30)
            };
            btnDeleteAll.Click += btnDeleteAll_Click;
            btnCustomize = new Button()
            {
                Text = "Tùy chỉnh (SV Đã Xóa)",
                Location = new Point(160, 0),
                Size = new Size(180, 30)
            };
            btnCustomize.Click += btnCustomize_Click;
            pnlSettingsControls.Controls.Add(btnDeleteAll);
            pnlSettingsControls.Controls.Add(btnCustomize);

            // Panel chứa DataGridView và các nút hành động (View Tùy chỉnh)
            pnlCustomizeView = new Panel()
            {
                Location = new Point(20, 100),
                Size = new Size(940, 560),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false // Ban đầu ẩn
            };

            // DataGridView
            dgvSinhVien = new DataGridView()
            {
                Location = new Point(10, 10),
                Size = new Size(pnlCustomizeView.Width - 20, pnlCustomizeView.Height - 60),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            // Nút Remove (Khôi phục) - Góc phải cuối
            btnRestoreSV = new Button()
            {
                Text = "Remove (Khôi phục)",
                Location = new Point(pnlCustomizeView.Width - 360, pnlCustomizeView.Height - 40),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                Size = new Size(170, 30)
            };
            // Gắn sự kiện ban đầu là Khôi phục
            btnRestoreSV.Click += btnRestoreSV_Click;

            // Nút Xóa Vĩnh Viễn - Góc phải cuối
            btnHardDeleteSV = new Button()
            {
                Text = "XÓA VĨNH VIỄN",
                Location = new Point(pnlCustomizeView.Width - 180, pnlCustomizeView.Height - 40),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                Size = new Size(170, 30),
                BackColor = Color.Red,
                ForeColor = Color.White
            };
            btnHardDeleteSV.Click += btnHardDeleteSV_Click;

            pnlCustomizeView.Controls.AddRange(new Control[] { dgvSinhVien, btnRestoreSV, btnHardDeleteSV });

            tabSinhVien.Controls.AddRange(new Control[] { lblSVManagement, btnSettings, pnlSettingsControls, pnlCustomizeView });
            // Add Tabs
            tabControl.TabPages.Add(tabDiem);
            tabControl.TabPages.Add(tabMonHoc);
            tabControl.TabPages.Add(tabGiamSat);
            this.Controls.Add(tabControl);
        }
    }
}
