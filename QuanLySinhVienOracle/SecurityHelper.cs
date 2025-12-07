using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace QuanLySinhVienOracle
{
    public static class SecurityHelper
    {
        // --- 1. MÃ HÓA ĐỐI XỨNG (AES) ---
        // Key cứng (Demo): 32 bytes
        private static readonly string AesKey = "12345678123456781234567812345678";
        private static readonly string AesIV = "1234567812345678"; // 16 bytes

        public static string EncryptAES(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return "";
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(AesKey);
                aes.IV = Encoding.UTF8.GetBytes(AesIV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    using (StreamWriter sw = new StreamWriter(cs)) { sw.Write(plainText); }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string DecryptAES(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return "";
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(AesKey);
                    aes.IV = Encoding.UTF8.GetBytes(AesIV);
                    using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    using (StreamReader sr = new StreamReader(cs)) { return sr.ReadToEnd(); }
                }
            }
            catch { return "Lỗi giải mã"; }
        }

        // --- 2. HASH (SHA256) ---
        public static string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        // --- 3. MÃ HÓA BẤT ĐỐI XỨNG (RSA - Chữ ký số) ---
        // Tạo cặp khóa (Giả lập Admin)
        public static void GenerateKeys(out string privateKey, out string publicKey)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
            {
                privateKey = rsa.ToXmlString(true);
                publicKey = rsa.ToXmlString(false);
            }
        }

        public static string SignData(string data, string privateKeyXml)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKeyXml);
                return Convert.ToBase64String(rsa.SignData(Encoding.UTF8.GetBytes(data), new SHA256CryptoServiceProvider()));
            }
        }

        // --- 4. MÃ HÓA LAI (Hybrid - Ảnh) ---
        public static void EncryptHybrid(string filePath, string publicKeyXml, out byte[] encryptedData, out string encryptedKey)
        {
            // A. Tạo Key AES ngẫu nhiên (Session Key)
            using (Aes aes = Aes.Create())
            {
                byte[] sessionKey = aes.Key;
                byte[] sessionIV = aes.IV;

                // B. Mã hóa file ảnh bằng AES
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(sessionIV, 0, sessionIV.Length); // Ghi IV đầu file
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] fileBytes = File.ReadAllBytes(filePath);
                        cs.Write(fileBytes, 0, fileBytes.Length);
                    }
                    encryptedData = ms.ToArray();
                }

                // C. Mã hóa Session Key bằng RSA
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(publicKeyXml);
                    encryptedKey = Convert.ToBase64String(rsa.Encrypt(sessionKey, false));
                }
            }
        }
    }
}