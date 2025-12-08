using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuanLySinhVienOracle
{
    public partial class RegisterForm: Form
    {
        //Chương 2: TV1
        // Thêm biến này để lưu đường dẫn ảnh
        private string avatarPath = "";
        // Chương 2: TV1
        private AuthManager authManager;
        public RegisterForm()
        {
            InitializeComponent();
            authManager = new AuthManager();
        }

        private void RegisterForm_Load(object sender, EventArgs e)
        {

        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtTenDangNhap.Text;
            string password = txtMatKhau.Text;
            string maSV = txtMaSV.Text;
            string hoTen = txtHoTen.Text;
            string sdt = txtSDT.Text;
            string diaChi = txtDiaChi.Text;
            DateTime ngaySinh = dtpNgaySinh.Value;
            string email = txtEmail.Text;
            //if (authManager.Register(username, password, maSV, hoTen, sdt, diaChi))
            //{
            //    MessageBox.Show("Đăng ký thành công! Bạn có thể đăng nhập ngay bây giờ.");
            //    this.Close();
            //}
            //else
            //{
            //    MessageBox.Show("Đăng ký thất bại. Tên đăng nhập có thể đã tồn tại hoặc có lỗi xảy ra.");
            //}
            // --- PHẦN SỬA ĐỔI: Gọi hàm RegisterSecure thay vì Register ---
            // Code cũ: if (authManager.Register(...)) 

            // Chương 2: TV1
            // Code mới:
            // 1. Xác định vai trò dựa trên RadioButton
            string role = "SINHVIEN"; // Mặc định
            if (rdoAdmin.Checked)
            {
                role = "ADMIN";
            }

            // Kiểm tra nhập liệu cơ bản
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }
            if (authManager.RegisterSecure(username, password, maSV, hoTen, ngaySinh, email, sdt, diaChi, avatarPath, role))
            {
                MessageBox.Show("Đăng ký thành công (Đã mã hóa dữ liệu)!");
                this.Close();
            }
            else
            {
                MessageBox.Show("Đăng ký thất bại hoặc Tên đăng nhập đã tồn tại.");
            }
            // Chương 2: TV1
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void lblPasswordRegister_Click(object sender, EventArgs e)
        {

        }

        // Chương 2: TV1
        private void btnChonAnh_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files|*.jpg;*.png;*.jpeg";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                avatarPath = dlg.FileName;
                MessageBox.Show("Đã chọn ảnh: " + System.IO.Path.GetFileName(avatarPath));
            }
        }


        // Chương 2: TV1
    }
}
