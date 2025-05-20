using System;
using System.Windows;
using System.Windows.Media;

namespace QuanLyPassword_V1
{
    public partial class ForgotPasswordWindow : Window
    {
        private User currentUser = null;           // User đang thao tác hiện tại
        private bool isRecoveryCodeVerified = false;  // Trạng thái mã khôi phục đã được xác thực hay chưa

        public ForgotPasswordWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Xử lý khi người dùng nhấn nút "Tiếp tục" sau khi nhập username
        /// </summary>
        private void BtnContinue_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();

            // Kiểm tra username có trống không
            if (string.IsNullOrEmpty(username))
            {
                ShowMessage("Vui lòng nhập tên đăng nhập.", Brushes.Red);
                return;
            }

            // Lấy thông tin user từ database theo username
            currentUser = DatabaseHelper.GetUserByUsername(username);
            if (currentUser == null)
            {
                ShowMessage("Tên đăng nhập không tồn tại.", Brushes.Red);
                return;
            }

            // Hiển thị yêu cầu nhập mã khôi phục
            ShowMessage("Nhập mã khôi phục để xác thực.", Brushes.Black);
            panelRecovery.Visibility = Visibility.Visible; // Hiện panel nhập mã khôi phục
            panelReset.Visibility = Visibility.Collapsed;   // Ẩn panel reset mật khẩu
            txtUsername.IsEnabled = false;                  // Khóa textbox username không cho sửa
        }

        /// <summary>
        /// Xử lý khi người dùng nhấn nút "Xác thực mã khôi phục"
        /// </summary>
        private void BtnVerifyRecoveryCode_Click(object sender, RoutedEventArgs e)
        {
            // Nếu chưa nhập username hoặc chưa xác thực username thì thông báo lỗi
            if (currentUser == null)
            {
                ShowMessage("Vui lòng nhập tên đăng nhập trước.", Brushes.Red);
                return;
            }

            string recoveryCode = txtRecoveryCode.Text.Trim();

            // Kiểm tra mã khôi phục có được nhập không
            if (string.IsNullOrEmpty(recoveryCode))
            {
                ShowMessage("Vui lòng nhập mã khôi phục.", Brushes.Red);
                return;
            }

            // Gọi hàm kiểm tra mã khôi phục trong database
            bool valid = DatabaseHelper.VerifyRecoveryCode(currentUser.UserId, recoveryCode);
            if (valid)
            {
                // Nếu hợp lệ, thông báo thành công và hiển thị panel đặt lại mật khẩu
                ShowMessage("Mã khôi phục hợp lệ. Vui lòng nhập mật khẩu mới.", Brushes.Green);
                panelReset.Visibility = Visibility.Visible;
                panelRecovery.Visibility = Visibility.Collapsed;
                isRecoveryCodeVerified = true;
            }
            else
            {
                // Nếu không hợp lệ, thông báo lỗi
                ShowMessage("Mã khôi phục không hợp lệ hoặc đã được sử dụng.", Brushes.Red);
            }
        }

        /// <summary>
        /// Xử lý khi người dùng nhấn nút "Đặt lại mật khẩu"
        /// </summary>
        private void BtnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra xem mã khôi phục đã được xác thực chưa
            if (!isRecoveryCodeVerified)
            {
                ShowMessage("Vui lòng xác thực mã khôi phục trước.", Brushes.Red);
                return;
            }

            string newPassword = pwdNewPassword.Password;
            string confirmPassword = pwdConfirmPassword.Password;

            // Kiểm tra mật khẩu mới có trống không
            if (string.IsNullOrEmpty(newPassword))
            {
                ShowMessage("Vui lòng nhập mật khẩu mới.", Brushes.Red);
                return;
            }

            // Kiểm tra mật khẩu xác nhận có trùng khớp với mật khẩu mới không
            if (newPassword != confirmPassword)
            {
                ShowMessage("Mật khẩu xác nhận không khớp.", Brushes.Red);
                return;
            }

            try
            {
                // Cập nhật mật khẩu mới cho user trong database
                bool success = DatabaseHelper.UpdateUserPassword(currentUser.UserId, newPassword);
                if (success)
                {
                    ShowMessage("Đặt lại mật khẩu thành công. Bạn có thể đăng nhập lại.", Brushes.Green);
                    MessageBox.Show("Mật khẩu đã được cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    ShowMessage("Đặt lại mật khẩu thất bại. Vui lòng thử lại.", Brushes.Red);
                }
            }
            catch (Exception ex)
            {
                // Bắt lỗi nếu có lỗi hệ thống xảy ra
                ShowMessage("Lỗi hệ thống: " + ex.Message, Brushes.Red);
            }
        }

        /// <summary>
        /// Hiển thị thông báo cho người dùng với màu sắc tùy chỉnh
        /// </summary>
        /// <param name="message">Nội dung thông báo</param>
        /// <param name="color">Màu chữ thông báo</param>
        private void ShowMessage(string message, Brush color)
        {
            txtMessage.Text = message;
            txtMessage.Foreground = color;
        }
    }
}
