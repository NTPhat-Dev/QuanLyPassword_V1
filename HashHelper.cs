using System;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyPassword_V1
{
    public static class HashHelper
    {
        /// <summary>
        /// Tính toán giá trị băm SHA-256 của chuỗi đầu vào và trả về dưới dạng chuỗi Base64.
        /// </summary>
        /// <param name="rawData">Chuỗi gốc cần băm</param>
        /// <returns>Chuỗi băm SHA-256 đã được mã hóa Base64</returns>
        public static string ComputeSha256Hash(string rawData)
        {
            // Tạo đối tượng SHA256 để tính toán băm
            using (var sha256 = SHA256.Create())
            {
                // Chuyển chuỗi rawData thành mảng byte UTF8
                byte[] bytes = Encoding.UTF8.GetBytes(rawData);

                // Tính băm SHA256 trên mảng byte
                byte[] hashBytes = sha256.ComputeHash(bytes);

                // Chuyển mảng byte băm thành chuỗi Base64 để dễ lưu trữ và so sánh
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
