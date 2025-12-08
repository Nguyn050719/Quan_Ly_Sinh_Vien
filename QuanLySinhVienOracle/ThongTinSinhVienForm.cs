using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace QuanLySinhVienOracle
{
    public partial class ThongTinSinhVienForm: Form
    {
        private string _username;
        private AuthManager _auth;
        public ThongTinSinhVienForm(string username)
        {
            InitializeComponent();
            _username = username;
            _auth = new AuthManager();
        }

        private void ThongTinSinhVienForm_Load(object sender, EventArgs e)
        {
            HienThiThongTinChiTiet();
        }
        // Trong file ThongTinSinhVienForm.cs

        private void HienThiThongTinChiTiet()
        {
            // CÂU SQL CHUẨN: Phải có đủ cột EncryptedKey và Email
            string sql = @"SELECT MaSV, HoTen, NgaySinh, DiaChi, Email, SDT, Avatar, EncryptedKey
                   FROM SinhVien 
                   WHERE TenDangNhap = :u";

            if (_auth.dbHelper.OpenConnection())
            {
                try
                {
                    using (OracleCommand cmd = new OracleCommand(sql, _auth.dbHelper.GetConnection()))
                    {
                        cmd.BindByName = true;
                        cmd.Parameters.Add("u", OracleDbType.Varchar2).Value = _username;

                        using (OracleDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                // 1. Hiển thị thông tin Text
                                lblMaSV.Text = dr["MaSV"].ToString();
                                lblHoTen.Text = dr["HoTen"].ToString();
                                lblDiaChi.Text = dr["DiaChi"].ToString();

                                // Kiểm tra Email
                                lblEmail.Text = dr["Email"] != DBNull.Value ? dr["Email"].ToString() : "";

                                // Kiểm tra SĐT (Giải mã)
                                if (dr["SDT"] != DBNull.Value)
                                {
                                    try { lblSDT.Text = SecurityHelper.DecryptAES(dr["SDT"].ToString()); }
                                    catch { lblSDT.Text = dr["SDT"].ToString(); }
                                }
                                else lblSDT.Text = "";

                                // Kiểm tra Ngày sinh
                                if (dr["NgaySinh"] != DBNull.Value)
                                {
                                    lblNgaySinh.Text = Convert.ToDateTime(dr["NgaySinh"]).ToString("dd/MM/yyyy");
                                }

                                // 2. XỬ LÝ ẢNH (Giải mã Lai)
                                // Chỉ giải mã khi có đủ Avatar VÀ EncryptedKey
                                if (dr["Avatar"] != DBNull.Value && dr["EncryptedKey"] != DBNull.Value)
                                {
                                    try
                                    {
                                        byte[] encryptedImg = (byte[])dr["Avatar"];
                                        string encryptedKey = dr["EncryptedKey"].ToString();

                                        // Lấy Private Key từ AuthManager
                                        string privateKey = AuthManager.GetPrivateKey();

                                        // Giải mã
                                        byte[] realImage = SecurityHelper.DecryptHybrid(encryptedImg, encryptedKey, privateKey);

                                        if (realImage != null && picAvatar != null)
                                        {
                                            using (MemoryStream ms = new MemoryStream(realImage))
                                            {
                                                picAvatar.Image = Image.FromStream(ms);
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // Nếu ảnh lỗi thì bỏ qua, không hiện popup làm phiền user
                                        Console.WriteLine("Lỗi hiển thị ảnh: " + ex.Message);
                                    }
                                }
                                else
                                {
                                    // Nếu không có ảnh thì xóa ảnh cũ (nếu có)
                                    if (picAvatar != null) picAvatar.Image = null;
                                }
                            }
                            else
                            {
                                MessageBox.Show("Không tìm thấy dữ liệu sinh viên này.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi lấy thông tin: " + ex.Message);
                }
                finally
                {
                    _auth.dbHelper.CloseConnection();
                }
            }
        }
    }
}
