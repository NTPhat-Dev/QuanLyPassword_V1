using System;
using System.Windows;
using QuanLyPassword_V1.Properties;

namespace QuanLyPassword_V1
{
    public partial class App : Application
    {
        // Hàm được gọi khi ứng dụng khởi động
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Hiển thị SplashScreen khi ứng dụng mới mở
            SplashScreen splash = new SplashScreen();
            splash.Show();

            // Dừng chương trình 2 giây để splash hiển thị
            System.Threading.Thread.Sleep(2000);

            // Đóng SplashScreen sau khi hiển thị đủ thời gian
            splash.Close();

            // Lấy thông tin lưu trạng thái "ghi nhớ đăng nhập"
            bool isRemembered = Settings.Default.IsRemembered;
            string savedUsername = Settings.Default.SavedUsername;

            // Nếu có ghi nhớ đăng nhập và username lưu tồn tại
            if (isRemembered && !string.IsNullOrEmpty(savedUsername))
            {
                // Lấy dữ liệu user từ database theo username đã lưu
                var user = DatabaseHelper.GetUserByUsername(savedUsername);

                // Nếu user tồn tại trong database
                if (user != null)
                {
                    // Khởi tạo session cho user đang đăng nhập
                    Session.CurrentUserId = user.UserId;
                    Session.CurrentUsername = user.Username;

                    // Mở cửa sổ chính MainWindow truyền thông tin user
                    MainWindow mainWindow = new MainWindow(user);
                    this.MainWindow = mainWindow;
                    mainWindow.Show();

                    // Kết thúc hàm để không mở cửa sổ LoginWindow
                    return;
                }
            }

            // Nếu không có thông tin lưu hoặc user không tồn tại,
            // mở cửa sổ đăng nhập LoginWindow để user nhập lại thông tin
            LoginWindow loginWindow = new LoginWindow();
            this.MainWindow = loginWindow;
            loginWindow.Show();
        }
    }
}
