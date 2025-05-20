public static class Session
{
    // Lưu Id người dùng đang đăng nhập hiện tại, mặc định -1 nghĩa là chưa có user nào đăng nhập
    public static int CurrentUserId { get; set; } = -1;

    // Lưu tên đăng nhập của người dùng hiện tại (null nếu chưa đăng nhập)
    public static string CurrentUsername { get; set; }
}
