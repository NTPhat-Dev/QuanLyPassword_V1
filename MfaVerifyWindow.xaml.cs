using System.Windows;
using OtpNet;  // Thư viện hỗ trợ OTP đã cài đặt

namespace QuanLyPassword_V1
{
    public partial class MfaVerifyWindow : Window
    {
        private readonly User _user; // Đối tượng User cần xác thực MFA

        // Constructor nhận vào đối tượng User để lấy thông tin secret MFA
        public MfaVerifyWindow(User user)
        {
            InitializeComponent();
            _user = user;
        }

        // Xử lý sự kiện khi người dùng bấm nút "Xác nhận" để verify mã OTP
        private void BtnVerify_Click(object sender, RoutedEventArgs e)
        {
            string inputOtp = txtOtp.Text.Trim();

            // Kiểm tra xem người dùng đã nhập mã OTP chưa
            if (string.IsNullOrEmpty(inputOtp))
            {
                MessageBox.Show("Vui lòng nhập mã OTP.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Kiểm tra xem người dùng có mã bí mật MFA không
            if (string.IsNullOrEmpty(_user.MfaSecret))
            {
                MessageBox.Show("Không tìm thấy mã bí mật MFA của người dùng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false; // Kết thúc với trạng thái không thành công
                Close();
                return;
            }

            // Chuyển mã bí mật từ Base32 sang byte array để tạo đối tượng TOTP
            var secretBytes = Base32Encoding.ToBytes(_user.MfaSecret);
            var totp = new Totp(secretBytes);

            // Kiểm tra tính hợp lệ của mã OTP nhập vào
            // VerificationWindow.RfcSpecifiedNetworkDelay cho phép lệch thời gian theo chuẩn RFC
            if (totp.VerifyTotp(inputOtp, out long timeStepMatched, VerificationWindow.RfcSpecifiedNetworkDelay))
            {
                DialogResult = true; // OTP hợp lệ, đóng cửa sổ với kết quả thành công
                Close();
            }
            else
            {
                // OTP không hợp lệ, thông báo lỗi cho người dùng
                MessageBox.Show("Mã OTP không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Xử lý sự kiện nút Hủy: đóng cửa sổ và trả về false
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
