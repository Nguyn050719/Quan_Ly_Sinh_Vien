using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace QuanLySinhVienOracle
{
    public class AuthManager
    {
        public OracleHelper dbHelper;

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
            GhiNhatKy("Unknown", "Đăng xuất", "Thành công", "User thoát App");
            Console.WriteLine("Đã đăng xuất thành công.");
        }

        // Chương 2: TV1
        // Giả lập khóa của Admin (thực tế nên lưu trong file config)
        // CẶP KHÓA RSA MỚI (Đã kiểm tra tính toàn vẹn toán học)
        // Copy đè đoạn này vào AuthManager.cs

        private static string _adminPrivateKey = @"<RSAKeyValue><Modulus>1TvB7ucAXkqovKSJb73EC72JpWcQnR5J11ZgoJqCQ8zgGXi7sH1PRJz4XY0QkCSKrLmtS0133LzRAFDcc+diSklXreWCEtFEZtL3yhejjvTsC0iF0v3ueclFoVE9s3eM5wEgYVVd8cOFwqzQmeszEez6kJ66g5rfYj0iO8sJwU/TtAFruRc9UlORiL4lxwkH7yAfjO//aAWQ1dNLom6d+Crt+dng27exfZKOUiQg3ihPt+fNjbIx3SUMydWWSF1NSDCrKKmNoaS2LEFMP1CaqMTh2Zx2B6sdPMXdQqtdJyVZpcQ+5jIeaAoBjWrf4MtW+9KeJnWbF1IunmjM+nuH5Q==</Modulus><Exponent>AQAB</Exponent><P>40+ZI+9PMdEnuRW2nsHaj1a+3Ge1tia/G9qGTOxrMOBJyqEHBL5ysmUS93QEwcI8yutg7OFN5VLQAhWG3vRRDVDDLmJGrdTWDfZqpyDWLZAAaIdwY0Bqy0qcdC4Cq9jLdbygxUkXtc8Nq5dAQU7tAS/h4fFHcyRC7tjrNhrgrSM=</P><Q>8CVQ7dyiUTdNERzpL+p50PjGcwLTMgyzDv/znXfsmbXjn/ts0djBwtZqVDt6Qb2bf+QfDnPI9y7sK7b5DeT3m1RUWnDW+kbGU83Z6lJ2ERh347xXrL9VjgAWvFCq8yCxH3xfaVoJYnrGiISCHH/bZMeuNaINdwXUM5eojXuKG1c=</Q><DP>AxVcVXF0mI9mw4r43DRy+4jItAKwI1VpOD6Bnd+DILmaJqaCitzuf18Bn9uyCNLSskn5GZ2AygCSk8So/LNBujWDQEjS/p6pfK0AC15VqW6Pottbee0wxaswbh8FqEEbmXBCmgqAyNWTMOWvWHNyoZZ9sOvt9TDju4uzvRsmOxM=</DP><DQ>NqCGdFVIJylpQodNFDVGWJ7+pZLy7+Orp7HZfOyWybBygyXybxnbmbKkpVySRvoWsVT3K5ZzFKd72cXZiauYF6FrPteET5Jh8xZUh9USPnlObGOfnhA4KgEjKts+x+eb4wmruo9cqyY6mztpTYpYIei6XEDsxzJwPvw0v1pm5Ck=</DQ><InverseQ>Mjmbpnwpp99dl2ngCRh/K9XBw+rhtCnVfTZPhDPRrQpcSd24kG6eR+v3xQ/QPcQPbUJxeM3zy1zJ9HdL8rzLqwOUPykXoNv990bqalYOeZPKaCWW7RDvvCRwpEZtZvwFO94AsB2Bb8YBR3mtXGynfVML6bZME885kY5YJk8YWOs=</InverseQ><D>pUgHn9WgnHYEawyil2Ghh3QoMHQ+FXt3wxlqFrMNC97PSg8idL/85HNDtMf2MbSRZScbY0YuBYS6ACOtbY3Jy0kkVF+Uvl9FROp8NUKzfcjs+iYBzTUVV4MinNMrH2QobJqb7bZM9mwWz3pCyzvbPybQZa8TZZVzV2OJmI1eB18do8nG46IdzsqG+oLUA5hKYdWPRUthWlhycSyryoPI36iw3YGNreOHnrV6DDYuGH+v/jETattEg3E7f2SZy8dVgjKmQWcJxyXHvz7Pz/oFEhpKzHNxjutzbLz6+4RAmIW7k486B7I3as1LbLEv+SQci6/BPmPVoFznqNL7z3uH1Q==</D></RSAKeyValue>";
        private static string _adminPublicKey = @"<RSAKeyValue><Modulus>1TvB7ucAXkqovKSJb73EC72JpWcQnR5J11ZgoJqCQ8zgGXi7sH1PRJz4XY0QkCSKrLmtS0133LzRAFDcc+diSklXreWCEtFEZtL3yhejjvTsC0iF0v3ueclFoVE9s3eM5wEgYVVd8cOFwqzQmeszEez6kJ66g5rfYj0iO8sJwU/TtAFruRc9UlORiL4lxwkH7yAfjO//aAWQ1dNLom6d+Crt+dng27exfZKOUiQg3ihPt+fNjbIx3SUMydWWSF1NSDCrKKmNoaS2LEFMP1CaqMTh2Zx2B6sdPMXdQqtdJyVZpcQ+5jIeaAoBjWrf4MtW+9KeJnWbF1IunmjM+nuH5Q==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        public void InitKeys()
        {

        }

        public void GenerateNewKeysAndPrint()
        {
            using (System.Security.Cryptography.RSACryptoServiceProvider rsa = new System.Security.Cryptography.RSACryptoServiceProvider(1024))
            {
                string privateKey = rsa.ToXmlString(true);
                string publicKey = rsa.ToXmlString(false);

                // Hiện thông báo chứa khóa để bạn copy
                System.Windows.Forms.MessageBox.Show("Private Key (Copy cái này thay vào code):\n\n" + privateKey);
                System.Windows.Forms.MessageBox.Show("Public Key (Copy cái này thay vào code):\n\n" + publicKey);
            }
        }
        public static string GetPrivateKey()
        {
            return _adminPrivateKey;
        }

        // Hàm lấy Public Key để mã hóa ảnh khi update
        public static string GetPublicKey()
        {
            return _adminPublicKey;
        }
        // Hàm Đăng Ký Bảo Mật (RegisterSecure)
        // Trong AuthManager.cs
        public bool RegisterSecure(string username, string password, string maSV, string hoTen, DateTime ngaySinh, string email, string sdt, string diaChi, string avatarPath, string role)
        {
    // Kiểm tra user cũ
            if (IsUsernameExist(username)) return false; // Nhớ đảm bảo hàm IsUsernameExist truy cập được

            InitKeys(); // Tạo khóa RSA

            // 1. Hash Password & Mã hóa SDT
            string passHash = SecurityHelper.HashPassword(password);
            string sdtEncrypt = SecurityHelper.EncryptAES(sdt);

            // 2. Ký số
            string signature = SecurityHelper.SignData(maSV, _adminPrivateKey);

            // 3. Mã hóa ảnh
            byte[] imgBlob = null;
            string imgKeyEnc = null;
            if (!string.IsNullOrEmpty(avatarPath) && System.IO.File.Exists(avatarPath))
            {
                SecurityHelper.EncryptHybrid(avatarPath, _adminPublicKey, out imgBlob, out imgKeyEnc);
            }

            // --- CÂU SQL ĐÃ BỔ SUNG NGÀY SINH ---
            string sql = "INSERT INTO SinhVien (MaSV, HoTen, NgaySinh, Email, SDT, DiaChi, TenDangNhap, MatKhau, ChuKySo, Avatar, EncryptedKey, VaiTro) " +
                             "VALUES (:maSV, :hoTen, :ngaySinh, :email, :sdt, :diaChi, :username, :pass, :sign, :img, :key, :vaiTro)"; if (dbHelper.OpenConnection())
            {
                using (OracleCommand cmd = new OracleCommand(sql, dbHelper.GetConnection()))
                {
                    cmd.BindByName = true; // Bắt buộc để tránh lỗi tham số

                    cmd.Parameters.Add("maSV", OracleDbType.Varchar2).Value = maSV;
                    cmd.Parameters.Add("hoTen", OracleDbType.Varchar2).Value = hoTen;
                    cmd.Parameters.Add("vaiTro", OracleDbType.Varchar2).Value = role;
                    cmd.Parameters.Add("ngaySinh", OracleDbType.Date).Value = ngaySinh;
                    cmd.Parameters.Add("email", OracleDbType.Varchar2).Value = email;
                    cmd.Parameters.Add("sdt", OracleDbType.Varchar2).Value = sdtEncrypt;
                    cmd.Parameters.Add("diaChi", OracleDbType.Varchar2).Value = diaChi;
                    cmd.Parameters.Add("username", OracleDbType.Varchar2).Value = username;
                    cmd.Parameters.Add("pass", OracleDbType.Varchar2).Value = passHash;
                    cmd.Parameters.Add("sign", OracleDbType.Varchar2).Value = signature;
                    cmd.Parameters.Add("img", OracleDbType.Blob).Value = imgBlob ?? (object)DBNull.Value;
                    cmd.Parameters.Add("key", OracleDbType.Varchar2).Value = imgKeyEnc ?? (object)DBNull.Value;

                    try
                    {
                        int row = cmd.ExecuteNonQuery();
                        return row > 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Lỗi đăng ký: " + ex.Message);
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
        }
        // Chương 2: TV1

        // Chương 3: TV1
        // Trong AuthManager.cs

        // Hàm Login trả về Vai trò của user (nếu sai trả về null hoặc chuỗi rỗng)
        public string LoginAndGetRole(string username, string password)
        {
            // Hash password để so sánh
            string passHash = SecurityHelper.HashPassword(password);

            // Select lấy cột VaiTro
            string sql = "SELECT VaiTro FROM SinhVien WHERE TenDangNhap = :u AND MatKhau = :p";

            if (dbHelper.OpenConnection())
            {
                using (OracleCommand cmd = new OracleCommand(sql, dbHelper.GetConnection()))
                {
                    cmd.Parameters.Add(new OracleParameter("u", username));
                    cmd.Parameters.Add(new OracleParameter("p", passHash));

                    try
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            SetUserContext(username);
                            return result.ToString(); // Trả về "ADMIN" hoặc "SV"
                        }
                        else if (result == DBNull.Value)
                        {
                            // Nếu đăng nhập đúng pass nhưng chưa có vai trò, gán tạm là SV
                            GhiNhatKy(username, "Đăng nhập", "Thành công", "User vào hệ thống");
                            return "SV";
                        }
                    }
                    catch (Exception ex)
                    {
                        GhiNhatKy(username, "Đăng nhập", "Thất bại", "Sai mật khẩu");
                        Console.WriteLine("Lỗi login: " + ex.Message);
                    }
                    finally
                    {
                        dbHelper.CloseConnection();
                    }
                }
            }
            return null; // Đăng nhập thất bại
        }
        // Chương 3: TV1

        // Chương 3: TV1
        // Thêm vào trong class AuthManager
        public void SetUserContext(string username)
        {
            // Lưu ý: Hàm này giả định kết nối ĐÃ ĐƯỢC MỞ trước đó.
            // VPD hoạt động theo session, nếu đóng kết nối, context sẽ mất.

            try
            {
                // Lấy kết nối đang mở từ dbHelper
                OracleConnection conn = dbHelper.GetConnection();

                if (conn.State == System.Data.ConnectionState.Open)
                {
                    using (OracleCommand cmd = new OracleCommand("Pkg_Security.Set_User", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_user", OracleDbType.Varchar2).Value = username;

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi thiết lập Context VPD: " + ex.Message);
            }
        }

        // Thêm vào class AuthManager
        public bool GetUserInfo(string username, out string hoTen, out string vaiTro, out byte[] avatarData)
        {
            hoTen = "";
            vaiTro = "";
            avatarData = null;

            string sql = "SELECT HoTen, VaiTro, Avatar FROM SinhVien WHERE TenDangNhap = :u";

            if (dbHelper.OpenConnection())
            {
                using (OracleCommand cmd = new OracleCommand(sql, dbHelper.GetConnection()))
                {
                    cmd.Parameters.Add(new OracleParameter("u", username));
                    try
                    {
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hoTen = reader["HoTen"].ToString();
                                vaiTro = reader["VaiTro"].ToString();

                                // Kiểm tra nếu có ảnh (cột Avatar không null)
                                if (reader["Avatar"] != DBNull.Value)
                                {
                                    avatarData = (byte[])reader["Avatar"];
                                }
                                return true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Lỗi lấy thông tin: " + ex.Message);
                    }
                    finally
                    {
                        dbHelper.CloseConnection();
                    }
                }
            }
            return false;
        }

        public void GhiNhatKy(string taiKhoan, string hanhDong, string trangThai, string ghiChu)
        {
            string sql = "INSERT INTO NhatKyHeThong (TaiKhoan, HanhDong, TrangThai, GhiChu) VALUES (:tk, :hd, :tt, :gc)";

            if (dbHelper.OpenConnection())
            {
                try
                {
                    using (OracleCommand cmd = new OracleCommand(sql, dbHelper.GetConnection()))
                    {
                        cmd.BindByName = true;
                        cmd.Parameters.Add("tk", OracleDbType.Varchar2).Value = taiKhoan;
                        cmd.Parameters.Add("hd", OracleDbType.Varchar2).Value = hanhDong;
                        cmd.Parameters.Add("tt", OracleDbType.Varchar2).Value = trangThai;
                        cmd.Parameters.Add("gc", OracleDbType.Varchar2).Value = ghiChu;
                        cmd.ExecuteNonQuery();
                    }
                }
                catch { /* Lỗi ghi log thì bỏ qua để không ảnh hưởng app */ }
                finally { dbHelper.CloseConnection(); }
            }
        }
        // Chương 3: TV1
    }
}
