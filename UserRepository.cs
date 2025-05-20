using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyPassword_V1
{
    /// <summary>
    /// Lớp tĩnh xử lý dữ liệu liên quan đến người dùng (Users) trong CSDL.
    /// </summary>
    public static class UserRepository
    {
        // Chuỗi kết nối tới CSDL, được cấu hình trong App.config/Web.config
        private static string connectionString = System.Configuration.ConfigurationManager
                                                    .ConnectionStrings["QuanLyPasswordDB"]
                                                    .ConnectionString;

        /// <summary>
        /// Kiểm tra mật khẩu người dùng có hợp lệ không (dùng SHA-256 hash).
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="password">Mật khẩu nhập vào</param>
        /// <returns>True nếu đúng mật khẩu, False nếu sai</returns>
        public static bool VerifyPassword(int userId, string password)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT PasswordHash FROM Users WHERE UserId = @UserId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);

                string storedHash = cmd.ExecuteScalar() as string;
                if (storedHash == null) return false;

                // Băm mật khẩu nhập vào và so sánh
                string hashInput = HashHelper.ComputeSha256Hash(password);
                return storedHash == hashInput;
            }
        }

        /// <summary>
        /// Cập nhật mật khẩu mới cho người dùng (đã băm).
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="newPassword">Mật khẩu mới (dạng plain text)</param>
        /// <returns>True nếu cập nhật thành công</returns>
        public static bool UpdateUserPassword(int userId, string newPassword)
        {
            string hashedPassword = HashHelper.ComputeSha256Hash(newPassword);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE Users SET PasswordHash = @PasswordHash WHERE UserId = @UserId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                cmd.Parameters.AddWithValue("@UserId", userId);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        /// <summary>
        /// Cập nhật cài đặt MFA (xác thực 2 bước) cho người dùng.
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="secret">Khóa MFA bí mật (null nếu tắt MFA)</param>
        /// <param name="isEnabled">Trạng thái bật/tắt MFA</param>
        public static void UpdateMfaSettings(int userId, string secret, bool isEnabled)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = "UPDATE Users SET MfaSecret = @Secret, IsMfaEnabled = @IsEnabled WHERE UserId = @UserId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Secret", (object)secret ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsEnabled", isEnabled);
                cmd.Parameters.AddWithValue("@UserId", userId);

                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Lưu danh sách mã khôi phục cho người dùng. Các mã sẽ được băm trước khi lưu.
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="recoveryCodes">Danh sách mã khôi phục (plain text)</param>
        /// <returns>True nếu lưu thành công</returns>
        public static bool SaveRecoveryCodes(int userId, List<string> recoveryCodes)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Xóa các mã khôi phục cũ
                string deleteQuery = "DELETE FROM RecoveryCodes WHERE UserId = @UserId";
                SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn);
                deleteCmd.Parameters.AddWithValue("@UserId", userId);
                deleteCmd.ExecuteNonQuery();

                // Thêm mã mới (băm SHA-256)
                string insertQuery = "INSERT INTO RecoveryCodes (UserId, CodeHash, IsUsed) VALUES (@UserId, @CodeHash, 0)";
                foreach (var code in recoveryCodes)
                {
                    string hashedCode = HashHelper.ComputeSha256Hash(code);

                    SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                    insertCmd.Parameters.AddWithValue("@UserId", userId);
                    insertCmd.Parameters.AddWithValue("@CodeHash", hashedCode);
                    insertCmd.ExecuteNonQuery();
                }
            }

            return true;
        }

        /// <summary>
        /// Tạo danh sách mã khôi phục ngẫu nhiên.
        /// </summary>
        /// <param name="count">Số lượng mã cần tạo</param>
        /// <returns>Danh sách mã khôi phục (chuỗi hex)</returns>
        public static List<string> GenerateRecoveryCodes(int count)
        {
            var codes = new List<string>();

            using (var rng = RandomNumberGenerator.Create())
            {
                for (int i = 0; i < count; i++)
                {
                    byte[] data = new byte[8];
                    rng.GetBytes(data); // Sinh ngẫu nhiên
                    string code = BitConverter.ToString(data).Replace("-", "");
                    codes.Add(code);
                }
            }

            return codes;
        }

        /// <summary>
        /// Sinh chuỗi bí mật ngẫu nhiên để dùng cho MFA (Base32).
        /// </summary>
        /// <param name="length">Độ dài chuỗi bí mật</param>
        /// <returns>Chuỗi MFA secret</returns>
        public static string GenerateRandomSecret(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567"; // Base32 alphabet
            var random = new Random();

            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
