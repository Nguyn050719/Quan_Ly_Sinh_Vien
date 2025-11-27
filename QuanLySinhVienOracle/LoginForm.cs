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
    public partial class LoginForm : Form
    {
        // Khai báo biến authManager ở cấp độ lớp
        private AuthManager authManager;

        public LoginForm()
        {
            InitializeComponent();
            // Khởi tạo đối tượng authManager trong hàm tạo
            authManager = new AuthManager();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {

        }

        private void lblMatKhau_Click(object sender, EventArgs e)
        {

        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            // Logic cho nút đăng ký sẽ được thêm vào đây
            // Ví dụ: hiển thị form đăng ký
            RegisterForm registerForm = new RegisterForm();
            registerForm.ShowDialog();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtTenDangNhap.Text;
            string password = txtMatKhau.Text;

            if (authManager.Login(username, password))
            {
                MessageBox.Show("Đăng nhập thành công!");
                // Chuyển sang MainForm
                MainForm mainForm = new MainForm();
                mainForm.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng.");
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}