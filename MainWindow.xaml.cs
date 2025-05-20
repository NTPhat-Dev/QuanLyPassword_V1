using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using ClosedXML.Excel;
using Microsoft.Win32;
using System.IO;
using System.ComponentModel;
using System.Windows.Data;

namespace QuanLyPassword_V1
{
    public partial class MainWindow : Window
    {
        private UserAccountRepository repo = new UserAccountRepository();

        // Dữ liệu gốc chứa danh sách tài khoản
        private ObservableCollection<UserAccount> userAccounts;

        // CollectionView hỗ trợ filter (lọc) dữ liệu cho DataGrid
        private ICollectionView userAccountsView;

        public User CurrentUser { get; private set; }

        public MainWindow(User currentUser)
        {
            InitializeComponent();
            CurrentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));

            // Load dữ liệu tài khoản từ database
            var dataFromDb = repo.GetAllAccounts();
            userAccounts = new ObservableCollection<UserAccount>(dataFromDb);

            // Tạo CollectionView để hỗ trợ filter tìm kiếm
            userAccountsView = CollectionViewSource.GetDefaultView(userAccounts);

            // Đưa dữ liệu lên DataGrid
            dgUserList.ItemsSource = userAccountsView;

            // Gán sự kiện focus/lost focus cho TextBox tìm kiếm để xử lý placeholder
            txtSearch.GotFocus += TxtSearch_GotFocus;
            txtSearch.LostFocus += TxtSearch_LostFocus;

            UpdatePlaceholderVisibility();
        }

        #region Thêm, Sửa, Xóa tài khoản

        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            var addUserWindow = new AddUserWindow { Owner = this };
            bool? result = addUserWindow.ShowDialog();

            if (result == true)
            {
                var newAccount = new UserAccount
                {
                    UserId = CurrentUser.UserId,
                    AccountType = addUserWindow.SelectedAccountType,
                    Username = addUserWindow.Username,
                    Password = addUserWindow.Password,
                    Note = addUserWindow.Note,
                    CreatedAt = DateTime.Now
                };

                repo.AddAccount(newAccount);
                userAccounts.Add(newAccount);
            }
        }

        private void BtnEditUser_Click(object sender, RoutedEventArgs e)
        {
            if (dgUserList.SelectedItem is UserAccount selectedAccount)
            {
                var editWindow = new AddUserWindow(selectedAccount) { Owner = this };
                bool? result = editWindow.ShowDialog();

                if (result == true)
                {
                    selectedAccount.AccountType = editWindow.SelectedAccountType;
                    selectedAccount.Username = editWindow.Username;
                    selectedAccount.Password = editWindow.Password;
                    selectedAccount.Note = editWindow.Note;

                    repo.UpdateAccount(selectedAccount);
                    dgUserList.Items.Refresh();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn tài khoản cần sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (dgUserList.SelectedItem is UserAccount selectedUser)
            {
                var confirm = MessageBox.Show($"Bạn có chắc muốn xóa tài khoản '{selectedUser.Username}' không?",
                                              "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm == MessageBoxResult.Yes)
                {
                    repo.DeleteAccount(selectedUser);
                    userAccounts.Remove(selectedUser);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn tài khoản để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #endregion

        #region Đăng xuất

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IsRemembered = false;
            Properties.Settings.Default.SavedUsername = string.Empty;
            Properties.Settings.Default.Save();

            Session.CurrentUserId = 0;
            Session.CurrentUsername = null;

            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        #endregion

        #region Tìm kiếm và lọc dữ liệu

        private void txtSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ApplyFilter();
            UpdatePlaceholderVisibility();
        }

        // Áp dụng filter dựa trên từ khóa tìm kiếm
        private void ApplyFilter()
        {
            string keyword = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(keyword))
            {
                userAccountsView.Filter = null;
            }
            else
            {
                userAccountsView.Filter = obj =>
                {
                    if (obj is UserAccount account)
                    {
                        return (!string.IsNullOrEmpty(account.Username) && account.Username.ToLower().Contains(keyword)) ||
                               (!string.IsNullOrEmpty(account.Note) && account.Note.ToLower().Contains(keyword)) ||
                               (!string.IsNullOrEmpty(account.AccountType) && account.AccountType.ToLower().Contains(keyword));
                    }
                    return false;
                };
            }

            userAccountsView.Refresh();
        }

        // Cập nhật hiển thị placeholder tìm kiếm (TextBlock)
        private void UpdatePlaceholderVisibility()
        {
            txtPlaceholder.Visibility = string.IsNullOrEmpty(txtSearch.Text) && !txtSearch.IsFocused
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e) => UpdatePlaceholderVisibility();

        private void TxtSearch_LostFocus(object sender, RoutedEventArgs e) => UpdatePlaceholderVisibility();

        #endregion

        #region Hiện/ẩn mật khẩu trong DataGrid

        private void TxtPassword_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBlock txtBlock)
            {
                var row = FindVisualParent<System.Windows.Controls.DataGridRow>(txtBlock);
                if (row != null && row.Item is UserAccount account)
                {
                    if (txtBlock.Text == "●●●●●●●" && !string.IsNullOrEmpty(account.Password))
                    {
                        txtBlock.Text = account.Password;
                        txtBlock.Foreground = Brushes.Black;
                    }
                    else
                    {
                        txtBlock.Text = "●●●●●●●";
                        txtBlock.Foreground = Brushes.Gray;
                    }
                }
            }
        }

        #endregion

        #region Copy mật khẩu vào Clipboard (menu chuột phải)

        private void CopyPasswordMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.MenuItem menuItem)
            {
                var contextMenu = menuItem.Parent as System.Windows.Controls.ContextMenu;
                if (contextMenu?.PlacementTarget is System.Windows.Controls.TextBlock txtBlock)
                {
                    var row = FindVisualParent<System.Windows.Controls.DataGridRow>(txtBlock);
                    if (row != null && row.Item is UserAccount account)
                    {
                        Clipboard.SetText(account.Password ?? "");
                    }
                }
            }
        }

        #endregion

        #region Hàm tiện ích tìm visual parent trong VisualTree WPF

        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }

        #endregion

        #region Mở các cửa sổ chức năng khác

        private void BtnGeneratePassword_Click(object sender, RoutedEventArgs e)
        {
            var generatePasswordWindow = new GeneratePasswordWindow { Owner = this };
            generatePasswordWindow.ShowDialog();
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow(CurrentUser);
            settingsWindow.ShowDialog();
        }

        #endregion

        #region Làm mới dữ liệu từ database

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            // Lấy dữ liệu mới từ database
            var dataFromDb = repo.GetAllAccounts();

            // Cập nhật ObservableCollection hiện tại (để DataGrid tự động cập nhật)
            userAccounts.Clear();
            foreach (var account in dataFromDb)
            {
                userAccounts.Add(account);
            }

            // Xóa filter tìm kiếm và cập nhật placeholder
            txtSearch.Text = string.Empty;
            userAccountsView.Filter = null;
            UpdatePlaceholderVisibility();
        }

        #endregion

        #region Xuất danh sách tài khoản ra file Excel bằng ClosedXML

        private void BtnExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            if (userAccounts == null || userAccounts.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Workbook|*.xlsx",
                Title = "Lưu file Excel",
                FileName = "DanhSachTaiKhoan.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Tài khoản");

                        // Tiêu đề cột
                        worksheet.Cell(1, 1).Value = "Loại tài khoản";
                        worksheet.Cell(1, 2).Value = "Tên đăng nhập";
                        worksheet.Cell(1, 3).Value = "Mật khẩu";
                        worksheet.Cell(1, 4).Value = "Ghi chú";
                        worksheet.Cell(1, 5).Value = "Ngày tạo";

                        int row = 2;
                        foreach (var account in userAccounts)
                        {
                            worksheet.Cell(row, 1).Value = account.AccountType;
                            worksheet.Cell(row, 2).Value = account.Username;
                            worksheet.Cell(row, 3).Value = account.Password;
                            worksheet.Cell(row, 4).Value = account.Note;
                            worksheet.Cell(row, 5).Value = account.CreatedAt.ToString("dd/MM/yyyy HH:mm");
                            row++;
                        }

                        // Tự động điều chỉnh kích thước các cột cho vừa nội dung
                        worksheet.Columns().AdjustToContents();

                        // Lưu file Excel
                        workbook.SaveAs(saveFileDialog.FileName);
                    }

                    MessageBox.Show("Xuất file Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (IOException ioEx)
                {
                    MessageBox.Show("Lỗi khi lưu file: " + ioEx.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion
    }
}
