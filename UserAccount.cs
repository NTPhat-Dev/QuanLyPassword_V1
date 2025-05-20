using System;
using System.ComponentModel;

public class UserAccount : INotifyPropertyChanged
{
    // Các biến private (trường dữ liệu)
    private int id;
    private int userId;
    private string accountType;
    private string username;
    private string password;
    private string note;
    private DateTime createdAt = DateTime.Now;

    // Các thuộc tính công khai với sự kiện thông báo thay đổi

    /// <summary>
    /// Mã định danh duy nhất của tài khoản.
    /// </summary>
    public int Id
    {
        get => id;
        set
        {
            if (id != value)
            {
                id = value;
                OnPropertyChanged(nameof(Id));
            }
        }
    }

    /// <summary>
    /// Mã định danh của người dùng sở hữu tài khoản này.
    /// </summary>
    public int UserId
    {
        get => userId;
        set
        {
            if (userId != value)
            {
                userId = value;
                OnPropertyChanged(nameof(UserId));
            }
        }
    }

    /// <summary>
    /// Loại tài khoản (ví dụ: email, mạng xã hội, ngân hàng...).
    /// </summary>
    public string AccountType
    {
        get => accountType;
        set
        {
            if (accountType != value)
            {
                accountType = value;
                OnPropertyChanged(nameof(AccountType));
            }
        }
    }

    /// <summary>
    /// Tên đăng nhập của tài khoản.
    /// </summary>
    public string Username
    {
        get => username;
        set
        {
            if (username != value)
            {
                username = value;
                OnPropertyChanged(nameof(Username));
            }
        }
    }

    /// <summary>
    /// Mật khẩu của tài khoản.
    /// </summary>
    public string Password
    {
        get => password;
        set
        {
            if (password != value)
            {
                password = value;
                OnPropertyChanged(nameof(Password));
            }
        }
    }

    /// <summary>
    /// Ghi chú thêm cho tài khoản.
    /// </summary>
    public string Note
    {
        get => note;
        set
        {
            if (note != value)
            {
                note = value;
                OnPropertyChanged(nameof(Note));
            }
        }
    }

    /// <summary>
    /// Thời điểm tạo tài khoản.
    /// </summary>
    public DateTime CreatedAt
    {
        get => createdAt;
        set
        {
            if (createdAt != value)
            {
                createdAt = value;
                OnPropertyChanged(nameof(CreatedAt));
            }
        }
    }

    // Sự kiện dùng để thông báo khi giá trị thuộc tính thay đổi (phục vụ cho ràng buộc dữ liệu - data binding)
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Phương thức gọi sự kiện PropertyChanged khi thuộc tính thay đổi.
    /// </summary>
    /// <param name="propName">Tên thuộc tính bị thay đổi.</param>
    protected void OnPropertyChanged(string propName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
