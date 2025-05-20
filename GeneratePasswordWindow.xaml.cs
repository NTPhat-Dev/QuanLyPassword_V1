using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyPassword_V1
{
    public partial class GeneratePasswordWindow : Window
    {
        // Đối tượng Random dùng để tạo số ngẫu nhiên
        private static readonly Random random = new Random();

        public GeneratePasswordWindow()
        {
            InitializeComponent();
            UpdatePassword(); // Tạo mật khẩu khi mở cửa sổ
        }

        // Xử lý thay đổi giá trị slider độ dài mật khẩu
        private void SliderLength_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (txtLengthValue != null)
            {
                txtLengthValue.Text = ((int)sliderLength.Value).ToString();
                UpdatePassword(); // Cập nhật lại mật khẩu khi thay đổi độ dài
            }
        }

        // Xử lý sự kiện nhấn nút "Tạo lại"
        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            UpdatePassword();
        }

        // Xử lý sự kiện nhấn nút "Sao chép"
        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtGeneratedPassword.Text))
            {
                Clipboard.SetText(txtGeneratedPassword.Text); // Sao chép mật khẩu vào clipboard
            }
        }

        // Hàm tạo mật khẩu ngẫu nhiên dựa trên lựa chọn người dùng
        private void UpdatePassword()
        {
            int length = (int)sliderLength.Value;

            // Các tập ký tự tương ứng với checkbox
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string numbers = "0123456789";
            const string symbols = "!@#$%^&*";

            StringBuilder charPool = new StringBuilder();

            // Thêm ký tự tương ứng nếu checkbox được chọn
            if (chkLowercase.IsChecked == true)
                charPool.Append(lower);
            if (chkUppercase.IsChecked == true)
                charPool.Append(upper);
            if (chkNumbers.IsChecked == true)
                charPool.Append(numbers);
            if (chkSymbols.IsChecked == true)
                charPool.Append(symbols);

            // Nếu không có ký tự nào được chọn thì báo lỗi
            if (charPool.Length == 0)
            {
                txtGeneratedPassword.Text = "";
                MessageBox.Show("Vui lòng chọn ít nhất một loại ký tự để tạo mật khẩu.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Tạo mật khẩu ngẫu nhiên từ tập ký tự được chọn
            var password = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                int index = random.Next(charPool.Length);
                password.Append(charPool[index]);
            }

            txtGeneratedPassword.Text = password.ToString();
        }
    }
}
