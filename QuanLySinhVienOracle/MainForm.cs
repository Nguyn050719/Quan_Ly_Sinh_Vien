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
    public partial class MainForm : Form
    {
        private AuthManager authManager;

        public MainForm()
        {
            //InitializeComponent();
            authManager = new AuthManager();
        }

        private void btnDangXuat_Click(object sender, EventArgs e)
        {
            authManager.Logout();
            MessageBox.Show("Đã đăng xuất thành công.");

            // Ẩn form chính và hiển thị lại form đăng nhập
            // Để đảm bảo form đăng nhập không bị tạo lại nhiều lần, 
            // bạn có thể truyền instance của nó qua các form khác.
            // Tuy nhiên, cách đơn giản nhất là tạo lại nó như sau.
            LoginForm loginForm = new LoginForm();
            loginForm.Show();
            this.Close();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Các xử lý khác khi form chính được load
        }
    }
}
