using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace QuanLyPassword_V1
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        // Xử lý sự kiện khi người dùng nhấn nút Đăng ký
        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            // Lấy dữ liệu nhập từ UI, xử lý trường hợp hiển thị mật khẩu
            string username = txtUsername.Text.Trim();
            string password = chkShowPassword.IsChecked == true ? txtPasswordVisible.Text : pwdPassword.Password;
            string confirmPassword = chkShowPassword.IsChecked == true ? txtConfirmPasswordVisible.Text : pwdConfirmPassword.Password;

            // Kiểm tra dữ liệu nhập bắt buộc
            if (string.IsNullOrEmpty(username))
            {
                SetMessage("Vui lòng nhập tên đăng nhập.", Brushes.Red);
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                SetMessage("Vui lòng nhập mật khẩu.", Brushes.Red);
                return;
            }

            if (password != confirmPassword)
            {
                SetMessage("Mật khẩu xác nhận không khớp.", Brushes.Red);
                return;
            }

            try
            {
                // Kiểm tra tên đăng nhập đã tồn tại trong CSDL chưa
                if (DatabaseHelper.IsUsernameTaken(username))
                {
                    SetMessage("Tên đăng nhập đã tồn tại.", Brushes.Red);
                    return;
                }

                // Thực hiện đăng ký người dùng mới
                bool isSuccess = DatabaseHelper.RegisterUser(username, password);
                if (isSuccess)
                {
                    // Lấy thông tin user mới tạo để sử dụng UserId
                    var user = DatabaseHelper.GetUserByUsername(username);
                    if (user != null)
                    {
                        // Tạo 5 mã khôi phục (recovery codes) ngẫu nhiên
                        var recoveryCodes = GenerateRecoveryCodes(5);

                        // Lưu mã khôi phục vào CSDL (nên hash mã trước khi lưu)
                        DatabaseHelper.SaveRecoveryCodes(user.UserId, recoveryCodes);

                        // Hiển thị mã khôi phục cho user, yêu cầu họ lưu lại
                        string codesText = string.Join(Environment.NewLine, recoveryCodes);
                        MessageBox.Show("Đăng ký thành công!\n\nMã khôi phục của bạn (vui lòng lưu lại):\n" + codesText,
                            "Mã khôi phục", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    // Chuyển về màn hình đăng nhập
                    LoginWindow loginWindow = new LoginWindow();
                    loginWindow.Show();
                    this.Close();
                }
                else
                {
                    SetMessage("Đăng ký thất bại. Vui lòng thử lại.", Brushes.Red);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Lỗi kết nối dữ liệu: " + ex.Message, Brushes.Red);
            }
        }

        // Tạo danh sách mã khôi phục gồm 'count' mã, mỗi mã 8 ký tự chữ và số ngẫu nhiên
        private List<string> GenerateRecoveryCodes(int count)
        {
            var codes = new List<string>();
            var rng = new Random();

            for (int i = 0; i < count; i++)
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                char[] codeChars = new char[8];
                for (int j = 0; j < 8; j++)
                {
                    codeChars[j] = chars[rng.Next(chars.Length)];
                }
                codes.Add(new string(codeChars));
            }

            return codes;
        }

        // Hiển thị hoặc ẩn thông báo trạng thái với màu sắc tương ứng
        private void SetMessage(string text, Brush color)
        {
            if (string.IsNullOrEmpty(text))
            {
                txtMessage.Visibility = Visibility.Collapsed;
                txtMessage.Text = string.Empty;
            }
            else
            {
                txtMessage.Visibility = Visibility.Visible;
                txtMessage.Text = text;
                txtMessage.Foreground = color;
            }
        }

        // Xử lý khi checkbox "Hiện mật khẩu" được check
        private void ChkShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            // Hiển thị TextBox hiện mật khẩu, ẩn PasswordBox
            txtPasswordVisible.Text = pwdPassword.Password;
            txtPasswordVisible.Visibility = Visibility.Visible;
            pwdPassword.Visibility = Visibility.Collapsed;

            txtConfirmPasswordVisible.Text = pwdConfirmPassword.Password;
            txtConfirmPasswordVisible.Visibility = Visibility.Visible;
            pwdConfirmPassword.Visibility = Visibility.Collapsed;

            UpdatePasswordMatchMessage();
        }

        // Xử lý khi checkbox "Hiện mật khẩu" bị uncheck
        private void ChkShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            // Ẩn TextBox hiện mật khẩu, hiện PasswordBox
            pwdPassword.Password = txtPasswordVisible.Text;
            pwdPassword.Visibility = Visibility.Visible;
            txtPasswordVisible.Visibility = Visibility.Collapsed;

            pwdConfirmPassword.Password = txtConfirmPasswordVisible.Text;
            pwdConfirmPassword.Visibility = Visibility.Visible;
            txtConfirmPasswordVisible.Visibility = Visibility.Collapsed;

            UpdatePasswordMatchMessage();
        }

        // Cập nhật thông báo khi mật khẩu thay đổi
        private void PwdPassword_PasswordChanged(object sender, RoutedEventArgs e) => UpdatePasswordMatchMessage();
        private void PwdConfirmPassword_PasswordChanged(object sender, RoutedEventArgs e) => UpdatePasswordMatchMessage();
        private void TxtPasswordVisible_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => UpdatePasswordMatchMessage();
        private void TxtConfirmPasswordVisible_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => UpdatePasswordMatchMessage();

        // Kiểm tra và hiển thị trạng thái khớp mật khẩu
        private void UpdatePasswordMatchMessage()
        {
            string pass = chkShowPassword.IsChecked == true ? txtPasswordVisible.Text : pwdPassword.Password;
            string confirm = chkShowPassword.IsChecked == true ? txtConfirmPasswordVisible.Text : pwdConfirmPassword.Password;

            if (string.IsNullOrEmpty(pass) && string.IsNullOrEmpty(confirm))
            {
                SetMessage(string.Empty, Brushes.Red);
                return;
            }

            if (pass == confirm)
            {
                SetMessage("Mật khẩu khớp.", Brushes.Green);
            }
            else
            {
                SetMessage("Mật khẩu không khớp.", Brushes.Red);
            }
        }

        // Xử lý chuyển về màn hình đăng nhập khi click link
        private void LoginLink_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
