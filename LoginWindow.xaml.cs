using System;
using System.Windows;
using QuanLyPassword_V1.Properties;

namespace QuanLyPassword_V1
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            // Nếu đã nhớ tài khoản, điền sẵn username, và ẩn mật khẩu
            if (Settings.Default.IsRemembered)
            {
                txtUsername.Text = Settings.Default.SavedUsername;
                chkRememberMe.IsChecked = true;
                pwdPassword.Password = "";
                txtPasswordVisible.Text = "";
                ShowPasswordBox(false);
            }
            else
            {
                ShowPasswordBox(false);
            }

            // Đăng ký sự kiện đồng bộ mật khẩu khi nhập ở hai ô
            pwdPassword.PasswordChanged += pwdPassword_PasswordChanged;
            txtPasswordVisible.TextChanged += txtPasswordVisible_TextChanged;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = chkShowPassword.IsChecked == true ? txtPasswordVisible.Text : pwdPassword.Password;

            txtMessage.Visibility = Visibility.Collapsed;
            txtMessage.Text = "";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                txtMessage.Text = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.";
                txtMessage.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                var user = DatabaseHelper.GetUserByUsername(username);
                if (user == null)
                {
                    txtMessage.Text = "Tên đăng nhập không tồn tại.";
                    txtMessage.Visibility = Visibility.Visible;
                    return;
                }

                string hashedInputPassword = HashHelper.ComputeSha256Hash(password);

                if (string.Equals(user.PasswordHash, hashedInputPassword, StringComparison.OrdinalIgnoreCase))
                {
                    if (user.IsMfaEnabled)
                    {
                        // Mở cửa sổ xác thực đa yếu tố
                        MfaVerifyWindow mfaWindow = new MfaVerifyWindow(user);
                        bool? mfaResult = mfaWindow.ShowDialog();
                        if (mfaResult != true)
                        {
                            txtMessage.Text = "Xác thực đa yếu tố không thành công.";
                            txtMessage.Visibility = Visibility.Visible;
                            return;
                        }
                    }

                    // Lưu thông tin đăng nhập vào session
                    Session.CurrentUserId = user.UserId;
                    Session.CurrentUsername = user.Username;

                    // Lưu hoặc xóa thông tin "nhớ mật khẩu"
                    if (chkRememberMe.IsChecked == true)
                    {
                        Settings.Default.IsRemembered = true;
                        Settings.Default.SavedUsername = username;
                    }
                    else
                    {
                        Settings.Default.IsRemembered = false;
                        Settings.Default.SavedUsername = "";
                    }
                    Settings.Default.Save();

                    // Mở cửa sổ chính và đóng cửa sổ đăng nhập
                    MainWindow mainWindow = new MainWindow(user);
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    txtMessage.Text = "Sai mật khẩu.";
                    txtMessage.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                txtMessage.Text = "Lỗi kết nối dữ liệu: " + ex.Message;
                txtMessage.Visibility = Visibility.Visible;
            }
        }

        private void ChkShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            ShowPasswordBox(true);
        }

        private void ChkShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowPasswordBox(false);
        }

        // Chuyển đổi hiển thị mật khẩu giữa TextBox và PasswordBox
        private void ShowPasswordBox(bool showPlain)
        {
            if (showPlain)
            {
                txtPasswordVisible.Text = pwdPassword.Password;
                txtPasswordVisible.Visibility = Visibility.Visible;
                pwdPassword.Visibility = Visibility.Collapsed;
            }
            else
            {
                pwdPassword.Password = txtPasswordVisible.Text;
                pwdPassword.Visibility = Visibility.Visible;
                txtPasswordVisible.Visibility = Visibility.Collapsed;
            }
        }

        private void pwdPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (chkShowPassword.IsChecked == true)
            {
                txtPasswordVisible.Text = pwdPassword.Password;
            }
        }

        private void txtPasswordVisible_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (chkShowPassword.IsChecked == true)
            {
                pwdPassword.Password = txtPasswordVisible.Text;
            }
        }

        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();
            this.Close();
        }

        private void ForgotPasswordLink_Click(object sender, RoutedEventArgs e)
        {
            ForgotPasswordWindow forgotPasswordWindow = new ForgotPasswordWindow();
            forgotPasswordWindow.ShowDialog();
        }
    }
}
