namespace QuanLyPassword_V1
{
    public class User
    {
        // ID duy nhất của user trong cơ sở dữ liệu
        public int UserId { get; set; }

        // Tên đăng nhập của user
        public string Username { get; set; }

        // Mật khẩu đã được hash, không lưu mật khẩu thô
        public string PasswordHash { get; set; }

        // Trạng thái bật/tắt xác thực đa yếu tố (MFA)
        public bool IsMfaEnabled { get; set; }

        // Secret dùng để thiết lập MFA (ví dụ: mã QR hoặc key)
        public string MfaSecret { get; set; }
    }
}
