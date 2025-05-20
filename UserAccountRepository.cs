using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

/// <summary>
/// Lớp thao tác với CSDL để quản lý danh sách tài khoản người dùng.
/// </summary>
public class UserAccountRepository
{
    // Chuỗi kết nối đến CSDL, lấy từ file App.config hoặc Web.config
    private readonly string connectionString = ConfigurationManager.ConnectionStrings["QuanLyPasswordDB"].ConnectionString;

    /// <summary>
    /// Lấy tất cả tài khoản của người dùng hiện tại từ bảng UserAccounts.
    /// </summary>
    /// <returns>Danh sách các tài khoản thuộc về người dùng hiện tại.</returns>
    public List<UserAccount> GetAllAccounts()
    {
        var list = new List<UserAccount>();

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            string query = @"SELECT AccountId, UserId, AccountType, LoginUsername, 
                             LoginPassword, Note, CreatedAt 
                             FROM UserAccounts 
                             WHERE UserId = @UserId";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserId", Session.CurrentUserId); // Lấy ID người dùng hiện tại

            conn.Open();

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    // Tạo đối tượng UserAccount từ dữ liệu đọc được
                    list.Add(new UserAccount
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("AccountId")),
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        AccountType = reader.GetString(reader.GetOrdinal("AccountType")),
                        Username = reader.GetString(reader.GetOrdinal("LoginUsername")),
                        Password = reader.GetString(reader.GetOrdinal("LoginPassword")),
                        Note = reader.IsDBNull(reader.GetOrdinal("Note")) ? "" : reader.GetString(reader.GetOrdinal("Note")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                    });
                }
            }
        }

        return list;
    }

    /// <summary>
    /// Thêm một tài khoản mới vào cơ sở dữ liệu.
    /// </summary>
    /// <param name="account">Đối tượng tài khoản cần thêm.</param>
    public void AddAccount(UserAccount account)
    {
        // Kiểm tra người dùng đã đăng nhập chưa
        if (Session.CurrentUserId <= 0)
            throw new InvalidOperationException("User chưa đăng nhập hoặc UserId không hợp lệ.");

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            string query = @"INSERT INTO UserAccounts 
                             (UserId, AccountType, LoginUsername, LoginPassword, Note, CreatedAt) 
                             VALUES (@UserId, @AccountType, @Username, @Password, @Note, @CreatedAt)";

            SqlCommand cmd = new SqlCommand(query, conn);

            // Gán giá trị cho các tham số truy vấn
            cmd.Parameters.AddWithValue("@UserId", Session.CurrentUserId);
            cmd.Parameters.AddWithValue("@AccountType", account.AccountType ?? "");
            cmd.Parameters.AddWithValue("@Username", account.Username ?? "");
            cmd.Parameters.AddWithValue("@Password", account.Password ?? "");
            cmd.Parameters.AddWithValue("@Note", account.Note ?? "");
            cmd.Parameters.AddWithValue("@CreatedAt", account.CreatedAt);

            conn.Open();
            cmd.ExecuteNonQuery(); // Thực thi truy vấn
        }
    }

    /// <summary>
    /// Cập nhật thông tin tài khoản hiện có.
    /// </summary>
    /// <param name="account">Tài khoản với thông tin mới.</param>
    public void UpdateAccount(UserAccount account)
    {
        if (Session.CurrentUserId <= 0)
            throw new InvalidOperationException("User chưa đăng nhập hoặc UserId không hợp lệ.");

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            string query = @"UPDATE UserAccounts 
                             SET AccountType = @AccountType, 
                                 LoginUsername = @Username, 
                                 LoginPassword = @Password, 
                                 Note = @Note 
                             WHERE AccountId = @Id AND UserId = @UserId";

            SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@AccountType", account.AccountType ?? "");
            cmd.Parameters.AddWithValue("@Username", account.Username ?? "");
            cmd.Parameters.AddWithValue("@Password", account.Password ?? "");
            cmd.Parameters.AddWithValue("@Note", account.Note ?? "");
            cmd.Parameters.AddWithValue("@Id", account.Id);
            cmd.Parameters.AddWithValue("@UserId", Session.CurrentUserId);

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Xóa tài khoản khỏi cơ sở dữ liệu.
    /// </summary>
    /// <param name="account">Tài khoản cần xóa.</param>
    public void DeleteAccount(UserAccount account)
    {
        if (Session.CurrentUserId <= 0)
            throw new InvalidOperationException("User chưa đăng nhập hoặc UserId không hợp lệ.");

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            string query = "DELETE FROM UserAccounts WHERE AccountId = @Id AND UserId = @UserId";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", account.Id);
            cmd.Parameters.AddWithValue("@UserId", Session.CurrentUserId);

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
