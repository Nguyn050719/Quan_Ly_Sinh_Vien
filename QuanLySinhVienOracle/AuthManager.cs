using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace QuanLySinhVienOracle
{
    public class AuthManager
    {
        private OracleHelper dbHelper;

        public AuthManager()
        {
            dbHelper = new OracleHelper();
        }

        // Hàm băm mật khẩu
        // Trong thực tế, nên dùng các thuật toán băm mạnh hơn như SHA256 hoặc bcrypt
        private string HashPassword(string password)
        {
            return password; // Giả định mật khẩu không băm để demo.
        }

        // Chức năng Đăng nhập
        public bool Login(string username, string password)
        {
            string hashedPassword = HashPassword(password);
            string sql = "SELECT COUNT(*) FROM SinhVien WHERE TenDangNhap = :username AND MatKhau = :password";

            if (dbHelper.OpenConnection())
            {
                using (OracleCommand cmd = new OracleCommand(sql, dbHelper.GetConnection()))
                {
                    cmd.Parameters.Add(new OracleParameter("username", username));
                    cmd.Parameters.Add(new OracleParameter("password", hashedPassword));

                    try
                    {
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Lỗi khi đăng nhập: " + ex.Message);
                        return false;
                    }
                    finally
                    {
                        dbHelper.CloseConnection();
                    }
                }
            }
            return false;
        }

        // Chức năng Đăng ký
        public bool Register(string username, string password, string maSV, string hoTen, string sdt, string diaChi)
        {
            // Kiểm tra xem TenDangNhap đã tồn tại chưa
            if (IsUsernameExist(username))
            {
                Console.WriteLine("Tên đăng nhập đã tồn tại.");
                return false;
            }

            string hashedPassword = HashPassword(password);
            string sql = "INSERT INTO SinhVien (MaSV, HoTen, SDT, DiaChi, TenDangNhap, MatKhau) VALUES (:maSV, :hoTen, :sdt, :diaChi, :username, :password)";

            if (dbHelper.OpenConnection())
            {
                using (OracleCommand cmd = new OracleCommand(sql, dbHelper.GetConnection()))
                {
                    cmd.Parameters.Add(new OracleParameter("maSV", maSV));
                    cmd.Parameters.Add(new OracleParameter("hoTen", hoTen));
                    cmd.Parameters.Add(new OracleParameter("sdt", sdt));
                    cmd.Parameters.Add(new OracleParameter("diaChi", diaChi));
                    cmd.Parameters.Add(new OracleParameter("username", username));
                    cmd.Parameters.Add(new OracleParameter("password", hashedPassword));

                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Lỗi khi đăng ký: " + ex.Message);
                        return false;
                    }
                    finally
                    {
                        dbHelper.CloseConnection();
                    }
                }
            }
            return false;
        }

        // Kiểm tra tên đăng nhập đã tồn tại
        private bool IsUsernameExist(string username)
        {
            string sql = "SELECT COUNT(*) FROM SinhVien WHERE TenDangNhap = :username";

            if (dbHelper.OpenConnection())
            {
                using (OracleCommand cmd = new OracleCommand(sql, dbHelper.GetConnection()))
                {
                    cmd.Parameters.Add(new OracleParameter("username", username));
                    try
                    {
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Lỗi khi kiểm tra tên đăng nhập: " + ex.Message);
                        return true; // Giả định đã tồn tại để an toàn
                    }
                    finally
                    {
                        dbHelper.CloseConnection();
                    }
                }
            }
            return true; // Giả định đã tồn tại nếu không kết nối được
        }

        // Chức năng Đăng xuất
        // Trong ứng dụng WinForms, đăng xuất thường là ẩn form hiện tại và hiển thị lại form đăng nhập
        public void Logout()
        {
            // Logic đăng xuất ở đây. Ví dụ:
            // Hiện form đăng nhập
            // Ẩn form chính của ứng dụng
            Console.WriteLine("Đã đăng xuất thành công.");
        }
    }
}
