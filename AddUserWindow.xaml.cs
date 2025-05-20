using System.Windows;
using System.Windows.Controls;

namespace QuanLyPassword_V1
{
    public partial class AddUserWindow : Window
    {
        private bool _isPasswordVisible = false;   // Trạng thái hiển thị mật khẩu (ẩn/hiện)
        private UserAccount _editingAccount;       // Tham chiếu đến tài khoản đang sửa (nếu có)

        // Constructor mặc định - khởi tạo giao diện
        public AddUserWindow()
        {
            InitializeComponent();
        }

        // Constructor khi truyền vào tài khoản cần chỉnh sửa
        public AddUserWindow(UserAccount accountToEdit) : this()
        {
            if (accountToEdit != null)
            {
                _editingAccount = accountToEdit;
                SetFormData(accountToEdit); // Điền dữ liệu tài khoản vào form
            }
        }

        // Điền dữ liệu tài khoản lên form
        public void SetFormData(UserAccount account)
        {
            if (account == null) return;

            // Chọn đúng loại tài khoản trong ComboBox
            foreach (ComboBoxItem item in cbAccountType.Items)
            {
                if ((item.Content as string) == account.AccountType)
                {
                    cbAccountType.SelectedItem = item;
                    break;
                }
            }

            // Điền các trường thông tin
            txtUsername.Text = account.Username;
            pwdPassword.Password = account.Password;
            txtNote.Text = account.Note;

            // Thiết lập mật khẩu ở trạng thái ẩn ban đầu
            pwdPassword.Visibility = Visibility.Visible;
            txtPasswordVisible.Visibility = Visibility.Collapsed;
            txtEyeIcon.Text = "👁";
            _isPasswordVisible = false;
        }

        // Thuộc tính lấy loại tài khoản đang chọn
        public string SelectedAccountType => (cbAccountType.SelectedItem as ComboBoxItem)?.Content.ToString();

        // Thuộc tính lấy username đã nhập
        public string Username => txtUsername.Text.Trim();

        // Thuộc tính lấy mật khẩu, phụ thuộc trạng thái ẩn/hiện mật khẩu
        public string Password => _isPasswordVisible ? txtPasswordVisible.Text : pwdPassword.Password;

        // Thuộc tính lấy ghi chú đã nhập
        public string Note => txtNote.Text.Trim();

        // Xử lý khi nhấn nút toggling hiện/ẩn mật khẩu
        private void BtnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            if (_isPasswordVisible)
            {
                // Nếu đang hiện mật khẩu, chuyển sang ẩn mật khẩu
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                pwdPassword.Visibility = Visibility.Visible;
                pwdPassword.Password = txtPasswordVisible.Text;
                txtEyeIcon.Text = "👁";
            }
            else
            {
                // Nếu đang ẩn mật khẩu, chuyển sang hiện mật khẩu
                txtPasswordVisible.Visibility = Visibility.Visible;
                pwdPassword.Visibility = Visibility.Collapsed;
                txtPasswordVisible.Text = pwdPassword.Password;
                txtEyeIcon.Text = "🙈";
            }
            _isPasswordVisible = !_isPasswordVisible; // Đảo trạng thái
        }

        // Xử lý khi nhấn nút Lưu
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra dữ liệu bắt buộc phải điền đủ
            if (string.IsNullOrEmpty(SelectedAccountType) ||
                string.IsNullOrEmpty(Username) ||
                string.IsNullOrEmpty(Password))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Hiển thị thông báo lưu thành công
            MessageBox.Show("Lưu tài khoản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

            // Đóng cửa sổ và trả về kết quả thành công
            this.DialogResult = true;
            this.Close();
        }

        // Xử lý khi nhấn nút Hủy
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Đóng cửa sổ và trả về kết quả hủy
            this.DialogResult = false;
            this.Close();
        }
    }
}
