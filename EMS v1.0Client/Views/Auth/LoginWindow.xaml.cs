using System;
using System.Windows;
using System.Windows.Controls;
using EMS_v1._0Client.Views.General;

namespace EMS_v1._0Client.Views.Auth
{
    public partial class LoginWindow : Window
    {
        private readonly AuthApiService _authService;
        private readonly IHttpClientFactory _httpClientFactory;

        public LoginWindow()
        {
            InitializeComponent();
            _httpClientFactory = new HttpClientFactory();
            _authService = new AuthApiService("https://localhost:5105/", _httpClientFactory);
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Clear previous error message
                ErrorMessageTextBlock.Visibility = Visibility.Collapsed;
                ErrorMessageTextBlock.Text = string.Empty;

                string username = UsernameTextBox.Text;
                string password = PasswordBox.Password;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    ShowErrorMessage("Vui lòng nhập tên đăng nhập và mật khẩu");
                    return;
                }

                // Disable button during login
                ((Button)sender).IsEnabled = false;

                // Call login API
                var response = await _authService.LoginAsync(username, password);

                if (response.Success)
                {
                    // Pass the same IHttpClientFactory instance to DashboardWindow
                    var dashboardWindow = new DashboardWindow(_httpClientFactory);
                    dashboardWindow.Show();
                    Close();
                }
                else
                {
                    ShowErrorMessage(response.Message);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Đã xảy ra lỗi: {ex.Message}");
            }
            finally
            {
                ((Button)sender).IsEnabled = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ShowErrorMessage(string message)
        {
            ErrorMessageTextBlock.Text = message;
            ErrorMessageTextBlock.Visibility = Visibility.Visible;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            // Dispose the HttpClientFactory to clean up resources
            _httpClientFactory?.Dispose();
        }
    }
}