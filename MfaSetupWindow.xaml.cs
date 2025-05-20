using System.Windows;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.IO;
using OtpNet; // Thư viện hỗ trợ kiểm tra OTP

namespace QuanLyPassword_V1
{
    public partial class MfaSetupWindow : Window
    {
        private readonly string _secretKey; // Mã bí mật dùng để tạo và xác thực OTP
        private readonly int _userId;       // ID người dùng đang thiết lập MFA

        // Constructor nhận vào userId, secretKey và username để khởi tạo giao diện
        public MfaSetupWindow(int userId, string secretKey, string username)
        {
            InitializeComponent();

            _userId = userId;
            _secretKey = secretKey;
            txtSecretKey.Text = secretKey;

            // Tạo chuỗi otpauth URI theo định dạng chuẩn để tạo QR code
            string otpauthUri = $"otpauth://totp/{username}?secret={secretKey}&issuer=QuanLyPassword_V1";

            // Khởi tạo QR code từ chuỗi otpauth URI
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(otpauthUri, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);

            // Chuyển QR code sang ảnh Bitmap và hiển thị trên Image control
            using (Bitmap qrBitmap = qrCode.GetGraphic(20))
            {
                imgQrCode.Source = BitmapToImageSource(qrBitmap);
            }
        }

        // Chuyển Bitmap sang BitmapImage để hiển thị trong WPF Image control
        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        // Xử lý sự kiện khi người dùng bấm nút "Xác nhận" để verify mã OTP
        private void BtnVerify_Click(object sender, RoutedEventArgs e)
        {
            string enteredOtp = txtOtp.Text.Trim();

            // Kiểm tra mã OTP nhập vào có hợp lệ hay không
            if (VerifyOtp(_secretKey, enteredOtp))
            {
                // Nếu hợp lệ, lưu cấu hình MFA (bật MFA và lưu secret) vào database
                UserRepository.UpdateMfaSettings(_userId, _secretKey, true);

                MessageBox.Show("Xác thực MFA thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true; // Đóng cửa sổ và trả về kết quả thành công
                this.Close();
            }
            else
            {
                // Nếu không hợp lệ, thông báo lỗi và cho phép nhập lại
                MessageBox.Show("Mã OTP không hợp lệ, vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Hàm kiểm tra tính hợp lệ của mã OTP dựa trên secret key
        private bool VerifyOtp(string secretKey, string otp)
        {
            try
            {
                byte[] secretBytes = Base32Encoding.ToBytes(secretKey);
                var totp = new Totp(secretBytes);

                // Verify với cửa sổ cho phép lệch 2 bước (tương đương 2*30 giây)
                return totp.VerifyTotp(otp, out long timeStepMatched, new VerificationWindow(2, 2));
            }
            catch
            {
                // Trả về false nếu có lỗi trong quá trình giải mã hoặc verify
                return false;
            }
        }

        // Xử lý sự kiện nút Hủy: đóng cửa sổ và trả về false
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
