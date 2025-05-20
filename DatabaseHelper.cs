using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace QuanLyPassword_V1
{
    public static class DatabaseHelper
    {
        // Chuỗi kết nối lấy từ App.config
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["QuanLyPasswordDB"].ConnectionString;

        /// <summary>
        /// Kiểm tra xem username đã tồn tại trong database chưa
        /// </summary>
        /// <param name="username">Tên đăng nhập cần kiểm tra</param>
        /// <returns>True nếu username đã tồn tại, false nếu chưa</returns>
        public static bool IsUsernameTaken(string username)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", username);

                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        /// <summary>
        /// Đăng ký user mới với username và password (đã được hash)
        /// </summary>
        /// <param name="username">Tên đăng nhập</param>
        /// <param name="password">Mật khẩu chưa hash</param>
        /// <returns>True nếu đăng ký thành công, false nếu thất bại</returns>
        public static bool RegisterUser(string username, string password)
        {
            // Hash mật khẩu bằng SHA-256 trước khi lưu
            string hashedPassword = HashHelper.ComputeSha256Hash(password);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Users (Username, PasswordHash) VALUES (@Username, @PasswordHash)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        /// <summary>
        /// Lấy thông tin user theo username
        /// </summary>
        /// <param name="username">Tên đăng nhập cần tìm</param>
        /// <returns>Đối tượng User nếu tồn tại, null nếu không</returns>
        public static User GetUserByUsername(string username)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT UserId, Username, PasswordHash, IsMfaEnabled, MfaSecret FROM Users WHERE Username = @Username";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", username);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                            Username = reader.GetString(reader.GetOrdinal("Username")),
                            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                            IsMfaEnabled = reader.GetBoolean(reader.GetOrdinal("IsMfaEnabled")),
                            MfaSecret = reader.IsDBNull(reader.GetOrdinal("MfaSecret")) ? null : reader.GetString(reader.GetOrdinal("MfaSecret"))
                        };
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// Lưu danh sách mã khôi phục (đã hash) vào cơ sở dữ liệu
        /// </summary>
        /// <param name="userId">ID user</param>
        /// <param name="recoveryCodes">Danh sách mã khôi phục chưa hash</param>
        public static void SaveRecoveryCodes(int userId, List<string> recoveryCodes)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                foreach (var code in recoveryCodes)
                {
                    // Hash từng mã khôi phục trước khi lưu
                    string codeHash = HashHelper.ComputeSha256Hash(code);
                    string query = "INSERT INTO RecoveryCodes (UserId, CodeHash, IsUsed) VALUES (@UserId, @CodeHash, 0)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@CodeHash", codeHash);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Kiểm tra mã khôi phục có hợp lệ và chưa sử dụng
        /// Nếu hợp lệ thì đánh dấu mã đã dùng
        /// </summary>
        /// <param name="userId">ID user</param>
        /// <param name="recoveryCode">Mã khôi phục chưa hash</param>
        /// <returns>True nếu mã hợp lệ và được cập nhật trạng thái, false nếu không hợp lệ</returns>
        public static bool VerifyRecoveryCode(int userId, string recoveryCode)
        {
            string hashedCode = HashHelper.ComputeSha256Hash(recoveryCode);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(1) FROM RecoveryCodes WHERE UserId = @UserId AND CodeHash = @CodeHash AND IsUsed = 0";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@CodeHash", hashedCode);

                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                if (count > 0)
                {
                    // Nếu mã hợp lệ, cập nhật trạng thái đã dùng
                    string updateQuery = "UPDATE RecoveryCodes SET IsUsed = 1 WHERE UserId = @UserId AND CodeHash = @CodeHash";
                    SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
                    updateCmd.Parameters.AddWithValue("@UserId", userId);
                    updateCmd.Parameters.AddWithValue("@CodeHash", hashedCode);
                    updateCmd.ExecuteNonQuery();

                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Cập nhật mật khẩu mới (đã hash) cho user theo userId
        /// </summary>
        /// <param name="userId">ID user</param>
        /// <param name="newPassword">Mật khẩu mới chưa hash</param>
        /// <returns>True nếu cập nhật thành công, false nếu thất bại</returns>
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
    }
}
