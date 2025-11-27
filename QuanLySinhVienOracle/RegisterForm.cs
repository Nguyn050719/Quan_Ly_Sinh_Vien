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

            if (authManager.Register(username, password, maSV, hoTen, sdt, diaChi))
            {
                MessageBox.Show("Đăng ký thành công! Bạn có thể đăng nhập ngay bây giờ.");
                this.Close();
            }
            else
            {
                MessageBox.Show("Đăng ký thất bại. Tên đăng nhập có thể đã tồn tại hoặc có lỗi xảy ra.");
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void lblPasswordRegister_Click(object sender, EventArgs e)
        {

        }
    }
}
