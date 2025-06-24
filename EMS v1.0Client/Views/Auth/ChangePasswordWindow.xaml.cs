using System;
using System.Windows;
using System.Windows.Controls;

namespace EMS_v1._0Client.Views.General
{
    public partial class ChangePasswordWindow : Window
    {
        private readonly AuthApiService _authService;
        private readonly IHttpClientFactory _httpClientFactory;

        public ChangePasswordWindow(IHttpClientFactory httpClientFactory)
        {
            InitializeComponent();
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _authService = new AuthApiService("https://localhost:5105/", _httpClientFactory);
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string currentPassword = CurrentPasswordBox.Password;
            string newPassword = NewPasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Vui lòng điền đầy đủ các trường.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Mật khẩu mới và xác nhận mật khẩu không khớp.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var response = await _authService.ChangePasswordAsync(currentPassword, newPassword);
                if (response.Success)
                {
                    MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    var myProfileWindow = new MyProfileWindow(_httpClientFactory);
                    myProfileWindow.Show();
                    Close();
                }
                else
                {
                    MessageBox.Show($"Đổi mật khẩu thất bại: {response.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var myProfileWindow = new MyProfileWindow(_httpClientFactory);
            myProfileWindow.Show();
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _httpClientFactory?.Dispose();
        }
    }
}