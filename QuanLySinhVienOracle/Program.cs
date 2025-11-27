using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuanLySinhVienOracle
{
    static class Program
    {
        /// <summary>
        /// Điểm vào chính cho ứng dụng.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm()); // Đặt LoginForm là form khởi đầu
        }
    }
}