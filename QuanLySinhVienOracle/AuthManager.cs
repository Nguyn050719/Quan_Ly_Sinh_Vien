using System;
using System.Collections.Generic;
using System.Data;
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

        // Chương 2: TV1
        // Giả lập khóa của Admin (thực tế nên lưu trong file config)
        private static string _adminPrivateKey;
        private static string _adminPublicKey;

        public void InitKeys()
        {
            if (string.IsNullOrEmpty(_adminPublicKey))
            {
                SecurityHelper.GenerateKeys(out _adminPrivateKey, out _adminPublicKey);
            }
        }

        // Hàm Đăng Ký Bảo Mật (RegisterSecure)
        public bool RegisterSecure(string username, string password, string maSV, string hoTen, string sdt, string diaChi, string avatarPath)
        {
            // Gọi hàm kiểm tra user cũ (tận dụng code cũ)
            // Lưu ý: Nếu hàm IsUsernameExist là private, bạn cứ để nguyên.

            InitKeys(); // Tạo khóa RSA nếu chưa có

            // 1. Hash Password & Mã hóa SDT
            string passHash = SecurityHelper.HashPassword(password);
            string sdtEncrypt = SecurityHelper.EncryptAES(sdt);

            // 2. Ký số (Admin xác nhận MaSV)
            string signature = SecurityHelper.SignData(maSV, _adminPrivateKey);

            // 3. Mã hóa ảnh (Hybrid)
            byte[] imgBlob = null;
            string imgKeyEnc = null;
            if (!string.IsNullOrEmpty(avatarPath) && System.IO.File.Exists(avatarPath))
            {
                SecurityHelper.EncryptHybrid(avatarPath, _adminPublicKey, out imgBlob, out imgKeyEnc);
            }

            string sql = "INSERT INTO SinhVien (MaSV, HoTen, SDT, DiaChi, TenDangNhap, MatKhau, ChuKySo, Avatar, EncryptedKey) " +
                         "VALUES (:maSV, :hoTen, :sdt, :diaChi, :username, :pass, :sign, :img, :key)";

            //Thực hiện lưu trữ mã hóa xuống Oracle
            if (dbHelper.OpenConnection())
            {
                using (OracleCommand cmd = new OracleCommand(sql, dbHelper.GetConnection()))
                {
                    cmd.Parameters.Add(new OracleParameter("maSV", maSV));
                    cmd.Parameters.Add(new OracleParameter("hoTen", hoTen));
                    cmd.Parameters.Add(new OracleParameter("sdt", sdtEncrypt)); // Đã mã hóa
                    cmd.Parameters.Add(new OracleParameter("diaChi", diaChi));
                    cmd.Parameters.Add(new OracleParameter("username", username));
                    cmd.Parameters.Add(new OracleParameter("pass", passHash)); // Đã Hash
                    cmd.Parameters.Add(new OracleParameter("sign", signature));
                    cmd.Parameters.Add(new OracleParameter("img", OracleDbType.Blob, imgBlob, ParameterDirection.Input));
                    cmd.Parameters.Add(new OracleParameter("key", imgKeyEnc));

                    try
                    {
                        int row = cmd.ExecuteNonQuery();
                        return row > 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Lỗi: " + ex.Message);
                        return false;
                    }
                    finally { dbHelper.CloseConnection(); }
                }
            }
            return false;
        }

        // Hàm Đăng Nhập Bảo Mật (LoginSecure)
        public bool LoginSecure(string username, string password)
        {
            string passHash = SecurityHelper.HashPassword(password);
            // Query kiểm tra với mật khẩu đã Hash
            string sql = "SELECT COUNT(*) FROM SinhVien WHERE TenDangNhap = :u AND MatKhau = :p";

            if (dbHelper.OpenConnection())
            {
                using (OracleCommand cmd = new OracleCommand(sql, dbHelper.GetConnection()))
                {
                    cmd.Parameters.Add(new OracleParameter("u", username));
                    cmd.Parameters.Add(new OracleParameter("p", passHash));
                    try
                    {
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                    catch { return false; }
                    finally { dbHelper.CloseConnection(); }
                }
            }
            return false;
        }                // Chương 2: TV1
    }
}
