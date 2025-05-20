using System;
using System.Collections.Generic;
using System.Windows;

namespace QuanLyPassword_V1
{
    public partial class SettingsWindow : Window
    {
        private User _currentUser;

        // Constructor nhận vào đối tượng User hiện tại
        public SettingsWindow(User currentUser)
        {
            InitializeComponent();

            _currentUser = currentUser;

            // Khởi tạo trạng thái checkbox MFA dựa trên dữ liệu user
            chkEnableMfa.IsChecked = _currentUser.IsMfaEnabled;
        }

        // Xử lý khi người dùng nhấn nút "Lưu"
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Lấy mật khẩu cũ, mới và xác nhận mật khẩu từ giao diện
            string oldPassword = txtOldPassword.Password;
            string newPassword = txtNewPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;

            // Nếu có nhập ít nhất 1 trong 3 ô mật khẩu (có ý định đổi mật khẩu)
            if (!string.IsNullOrEmpty(oldPassword) || !string.IsNullOrEmpty(newPassword) || !string.IsNullOrEmpty(confirmPassword))
            {
                // Kiểm tra nhập đầy đủ các ô mật khẩu
                if (string.IsNullOrEmpty(oldPassword))
                {
                    MessageBox.Show("Vui lòng nhập mật khẩu cũ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(newPassword))
                {
                    MessageBox.Show("Vui lòng nhập mật khẩu mới.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Kiểm tra mật khẩu mới trùng xác nhận
                if (newPassword != confirmPassword)
                {
                    MessageBox.Show("Mật khẩu mới và xác nhận không khớp.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Kiểm tra mật khẩu cũ có đúng không
                if (!UserRepository.VerifyPassword(_currentUser.UserId, oldPassword))
                {
                    MessageBox.Show("Mật khẩu cũ không đúng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Thực hiện cập nhật mật khẩu mới trong CSDL
                bool updateResult = UserRepository.UpdateUserPassword(_currentUser.UserId, newPassword);
                if (updateResult)
                {
                    MessageBox.Show("Đổi mật khẩu thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Đổi mật khẩu thất bại. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // Cập nhật trạng thái bật/tắt MFA
            bool isEnabled = chkEnableMfa.IsChecked == true;

            // Nếu bật MFA mà user chưa có secret MFA thì tạo mới và mở cửa sổ thiết lập
            if (isEnabled && string.IsNullOrEmpty(_currentUser.MfaSecret))
            {
                string newSecret = UserRepository.GenerateRandomSecret(16);

                // Cập nhật secret tạm thời, chưa bật MFA (đợi user hoàn thành setup)
                UserRepository.UpdateMfaSettings(_currentUser.UserId, newSecret, false);

                // Mở cửa sổ cấu hình MFA cho user quét QR code hoặc nhập secret
                var mfaWindow = new MfaSetupWindow(_currentUser.UserId, newSecret, _currentUser.Username);
                mfaWindow.Owner = this;
                bool? result = mfaWindow.ShowDialog();

                if (result == true)
                {
                    // User hoàn thành cấu hình MFA thành công, kích hoạt MFA
                    UserRepository.UpdateMfaSettings(_currentUser.UserId, newSecret, true);
                    _currentUser.MfaSecret = newSecret;
                    _currentUser.IsMfaEnabled = true;
                }
                else
                {
                    // User hủy hoặc thất bại, xóa secret và không bật MFA
                    UserRepository.UpdateMfaSettings(_currentUser.UserId, null, false);
                }
            }
            else
            {
                // Nếu user đã có MFA hoặc tắt MFA thì cập nhật trạng thái tương ứng
                UserRepository.UpdateMfaSettings(_currentUser.UserId, _currentUser.MfaSecret, isEnabled);
                _currentUser.IsMfaEnabled = isEnabled;
            }

            // Đóng cửa sổ sau khi lưu
            this.Close();
        }

        // Xử lý khi nhấn nút tạo mã khôi phục mới
        private void BtnGenerateRecoveryCodes_Click(object sender, RoutedEventArgs e)
        {
            // Tạo 5 mã khôi phục mới
            var codes = UserRepository.GenerateRecoveryCodes(5);

            // Lưu mã khôi phục vào CSDL (thường sẽ lưu dạng hash)
            bool saved = UserRepository.SaveRecoveryCodes(_currentUser.UserId, codes);

            if (saved)
            {
                // Hiển thị mã khôi phục cho user, yêu cầu lưu lại cẩn thận
                string message = "Mã khôi phục mới:\n\n" + string.Join("\n", codes);
                MessageBox.Show(message + "\n\nVui lòng lưu lại mã này để sử dụng khi cần thiết.",
                                "Mã khôi phục", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Thông báo lỗi khi lưu mã khôi phục thất bại
                MessageBox.Show("Không thể tạo mã khôi phục. Vui lòng thử lại sau.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
