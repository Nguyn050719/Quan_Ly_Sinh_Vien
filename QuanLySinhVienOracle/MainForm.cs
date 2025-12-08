using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;

namespace QuanLySinhVienOracle
{
    public partial class MainForm : Form
    {
        // 1. KHAI BÁO BIẾN DỮ LIỆU
        private string currentRole;
        private string currentUsername;
        private AuthManager authManager;

        // 2. KHAI BÁO GIAO DIỆN (Nút bấm, Nhãn, Ảnh...)
        private PictureBox picAvatar;
        private Label lblHoTen;
        private Label lblVaiTro;
        private Button bthThongTin;
        private Button btnHocTap;
        private Button btnDangXuat;
        private Button btnDoiAnh;
        private Button btnQuanLyAdmin;
        // 3. CONSTRUCTOR (Khởi tạo Form)
        public MainForm(string role, string username)
        {
            // Gọi hàm vẽ giao diện trước
            InitializeComponent();

            // Khởi tạo dữ liệu sau
            authManager = new AuthManager();
            this.currentRole = role;
            this.currentUsername = username;
        }

        // 4. SỰ KIỆN LOAD FORM
        private void MainForm_Load(object sender, EventArgs e)
        {
            PhanQuyenGiaoDien();
            HienThiThongTinUser();
        }

        // ==========================================================
        // PHẦN LOGIC XỬ LÝ (Code chức năng)
        // ==========================================================

        private void HienThiThongTinUser()
        {
            // SQL lấy tên, vai trò và ẢNH (kèm khóa giải mã)
            string sql = @"SELECT HoTen, VaiTro, Avatar, EncryptedKey 
                           FROM SinhVien 
                           WHERE TenDangNhap = :u";

            if (authManager.dbHelper.OpenConnection())
            {
                try
                {
                    authManager.SetUserContext(currentUsername);
                    using (OracleCommand cmd = new OracleCommand(sql, authManager.dbHelper.GetConnection()))
                    {
                        cmd.BindByName = true;
                        cmd.Parameters.Add("u", OracleDbType.Varchar2).Value = currentUsername;

                        using (OracleDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                // Hiện Tên & Vai trò
                                lblHoTen.Text = "Xin chào: " + dr["HoTen"].ToString();
                                string vt = dr["VaiTro"] != DBNull.Value ? dr["VaiTro"].ToString() : "SINHVIEN";
                                lblVaiTro.Text = "Vai trò: " + vt;

                                // GIẢI MÃ ẢNH HYBRID
                                if (dr["Avatar"] != DBNull.Value && dr["EncryptedKey"] != DBNull.Value)
                                {
                                    try
                                    {
                                        byte[] encryptedImg = (byte[])dr["Avatar"];
                                        string encryptedKey = dr["EncryptedKey"].ToString();
                                        string privateKey = AuthManager.GetPrivateKey();

                                        byte[] realImage = SecurityHelper.DecryptHybrid(encryptedImg, encryptedKey, privateKey);

                                        if (realImage != null && picAvatar != null)
                                        {
                                            using (MemoryStream ms = new MemoryStream(realImage))
                                            {
                                                picAvatar.Image = Image.FromStream(ms);
                                            }
                                        }
                                    }
                                    catch { /* Lỗi ảnh thì bỏ qua */ }
                                }
                                else if (picAvatar != null) picAvatar.Image = null;
                            }
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
                finally { authManager.dbHelper.CloseConnection(); }
            }
        }

        private void PhanQuyenGiaoDien()
        {
            if (currentRole == "ADMIN")
            {
                this.Text = "Quản Lý Sinh Viên - Administrator";
                btnQuanLyAdmin.Visible = true; // Hiện nút nếu là Admin
            }
            else
            {
                this.Text = "Quản Lý Sinh Viên - Sinh Viên";
                btnQuanLyAdmin.Visible = false;
            }
        }

        // Các sự kiện Click nút bấm
        private void bthThongTin_Click(object sender, EventArgs e)
        {
            ThongTinSinhVienForm frm = new ThongTinSinhVienForm(currentUsername);
            frm.ShowDialog();
        }

        private void btnHocTap_Click(object sender, EventArgs e)
        {
            HocTapForm frm = new HocTapForm(currentUsername);
            frm.ShowDialog();
        }

        private void btnDangXuat_Click(object sender, EventArgs e)
        {
            authManager.Logout();
            LoginForm login = new LoginForm();
            login.Show();
            this.Close();
        }

        // ==========================================================
        // PHẦN VẼ GIAO DIỆN (Thay thế cho file Designer)
        // ==========================================================
        private void InitializeComponent()
        {
            this.picAvatar = new System.Windows.Forms.PictureBox();
            this.lblHoTen = new System.Windows.Forms.Label();
            this.lblVaiTro = new System.Windows.Forms.Label();
            this.bthThongTin = new System.Windows.Forms.Button();
            this.btnHocTap = new System.Windows.Forms.Button();
            this.btnDangXuat = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).BeginInit();
            this.SuspendLayout();

            // 
            // picAvatar
            // 
            this.picAvatar.Location = new System.Drawing.Point(563, 12);
            this.picAvatar.Name = "picAvatar";
            this.picAvatar.Size = new System.Drawing.Size(120, 120);
            this.picAvatar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom; // Chế độ Zoom cho đẹp
            this.picAvatar.TabIndex = 0;
            this.picAvatar.TabStop = false;
            this.picAvatar.BorderStyle = BorderStyle.FixedSingle; // Thêm khung viền

            // 
            // lblHoTen
            // 
            this.lblHoTen.AutoSize = true;
            this.lblHoTen.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.lblHoTen.Location = new System.Drawing.Point(20, 20);
            this.lblHoTen.Name = "lblHoTen";
            this.lblHoTen.Size = new System.Drawing.Size(80, 17);
            this.lblHoTen.TabIndex = 1;
            this.lblHoTen.Text = "Xin chào...";

            // 
            // lblVaiTro
            // 
            this.lblVaiTro.AutoSize = true;
            this.lblVaiTro.Location = new System.Drawing.Point(20, 50);
            this.lblVaiTro.Name = "lblVaiTro";
            this.lblVaiTro.Size = new System.Drawing.Size(50, 13);
            this.lblVaiTro.TabIndex = 2;
            this.lblVaiTro.Text = "Vai trò...";

            // 
            // bthThongTin (Nút Thông Tin)
            // 
            this.bthThongTin.Location = new System.Drawing.Point(100, 100);
            this.bthThongTin.Name = "bthThongTin";
            this.bthThongTin.Size = new System.Drawing.Size(150, 40);
            this.bthThongTin.TabIndex = 3;
            this.bthThongTin.Text = "Thông tin cá nhân";
            this.bthThongTin.UseVisualStyleBackColor = true;
            this.bthThongTin.Click += new System.EventHandler(this.bthThongTin_Click);

            // 
            // btnHocTap (Nút Kết quả học)
            // 
            this.btnHocTap.Location = new System.Drawing.Point(100, 150);
            this.btnHocTap.Name = "btnHocTap";
            this.btnHocTap.Size = new System.Drawing.Size(150, 40);
            this.btnHocTap.TabIndex = 4;
            this.btnHocTap.Text = "Kết quả học tập";
            this.btnHocTap.UseVisualStyleBackColor = true;
            this.btnHocTap.Click += new System.EventHandler(this.btnHocTap_Click);

            // 
            // btnDangXuat (Nút Đăng xuất)
            // 
            this.btnDangXuat.Location = new System.Drawing.Point(100, 200);
            this.btnDangXuat.Name = "btnDangXuat";
            this.btnDangXuat.Size = new System.Drawing.Size(150, 40);
            this.btnDangXuat.Text = "Đăng xuất";
            this.btnDangXuat.Click += new System.EventHandler(this.btnDangXuat_Click);

            // --- THÊM NÚT QUẢN LÝ ADMIN ---
            this.btnQuanLyAdmin = new System.Windows.Forms.Button();
            this.btnQuanLyAdmin.Location = new System.Drawing.Point(100, 250); // Vị trí dưới nút Đăng xuất
            this.btnQuanLyAdmin.Name = "btnQuanLyAdmin";
            this.btnQuanLyAdmin.Size = new System.Drawing.Size(150, 40);
            this.btnQuanLyAdmin.Text = "Quản Lý Đào Tạo";
            this.btnQuanLyAdmin.UseVisualStyleBackColor = true;
            this.btnQuanLyAdmin.Visible = false; // Mặc định ẩn
            this.btnQuanLyAdmin.Click += new System.EventHandler(this.btnQuanLyAdmin_Click);

            this.Controls.Add(this.btnQuanLyAdmin);

            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(750, 450);
            this.Controls.Add(this.btnDangXuat);
            this.Controls.Add(this.btnHocTap);
            this.Controls.Add(this.bthThongTin);
            this.Controls.Add(this.lblVaiTro);
            this.Controls.Add(this.lblHoTen);
            this.Controls.Add(this.picAvatar);
            this.Name = "MainForm";
            this.Text = "Chương trình Quản Lý Sinh Viên";
            this.Load += new System.EventHandler(this.MainForm_Load); // Gắn sự kiện Load quan trọng
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

            this.btnDoiAnh = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).BeginInit();
            this.SuspendLayout();
            this.btnDoiAnh.Location = new System.Drawing.Point(563, 142);
            this.btnDoiAnh.Name = "btnDoiAnh";
            this.btnDoiAnh.Size = new System.Drawing.Size(120, 30);
            this.btnDoiAnh.Text = "Đổi Ảnh";
            this.btnDoiAnh.UseVisualStyleBackColor = true;
            this.btnDoiAnh.Click += new System.EventHandler(this.btnDoiAnh_Click);
            this.Controls.Add(this.btnDoiAnh);

        }

        private void btnQuanLyAdmin_Click(object sender, EventArgs e)
        {
            AdminForm frm = new AdminForm(currentUsername);
            frm.ShowDialog();
        }

        // Trong file MainForm.cs, thêm vào vùng các sự kiện Click

        // Sự kiện click nút Đổi Ảnh
        private void btnDoiAnh_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files|*.jpg;*.png;*.jpeg|All Files|*.*";
            dlg.Title = "Chọn ảnh đại diện mới";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                // Nếu chọn được file, tiến hành xử lý
                TienHanhDoiAnh(dlg.FileName);
            }
        }

        // Hàm xử lý Mã hóa và Update Database
        private void TienHanhDoiAnh(string filePath)
        {
            // 1. Chuẩn bị dữ liệu mã hóa
            byte[] encryptedImgBlob = null;
            string encryptedSessionKey = null;
            string adminPublicKey = AuthManager.GetPublicKey(); // Lấy Public Key

            try
            {
                // Gọi hàm mã hóa Lai (Hybrid)
                SecurityHelper.EncryptHybrid(filePath, adminPublicKey, out encryptedImgBlob, out encryptedSessionKey);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xử lý/mã hóa ảnh: " + ex.Message);
                return;
            }

            // 2. Cập nhật vào CSDL
            string sql = "UPDATE SinhVien SET Avatar = :img, EncryptedKey = :key WHERE TenDangNhap = :u";

            if (authManager.dbHelper.OpenConnection())
            {
                try
                {
                    // Cần SetUserContext nếu có dùng VPD (an toàn thì cứ thêm)
                    authManager.SetUserContext(currentUsername);

                    using (OracleCommand cmd = new OracleCommand(sql, authManager.dbHelper.GetConnection()))
                    {
                        cmd.BindByName = true; // Quan trọng khi dùng nhiều tham số
                        // Truyền tham số BLOB ảnh đã mã hóa
                        cmd.Parameters.Add("img", OracleDbType.Blob).Value = encryptedImgBlob;
                        // Truyền tham số Khóa phiên đã mã hóa
                        cmd.Parameters.Add("key", OracleDbType.Varchar2).Value = encryptedSessionKey;
                        // Username hiện tại
                        cmd.Parameters.Add("u", OracleDbType.Varchar2).Value = currentUsername;

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            MessageBox.Show("Đổi ảnh đại diện thành công!");
                            // GỌI LẠI HÀM NÀY ĐỂ TẢI LẠI ẢNH MỚI LÊN GIAO DIỆN NGAY LẬP TỨC
                            HienThiThongTinUser();
                        }
                        else
                        {
                            MessageBox.Show("Không thể cập nhật ảnh. Vui lòng thử lại.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi CSDL: " + ex.Message);
                }
                finally
                {
                    authManager.dbHelper.CloseConnection();
                }
            }
        }
    }
}