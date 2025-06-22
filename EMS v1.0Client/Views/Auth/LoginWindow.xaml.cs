using System;
using System.Windows;
using System.Windows.Controls;
using EMS_v1._0Client.Views.General;

namespace EMS_v1._0Client.Views.Auth
{
    public partial class LoginWindow : Window
    {
        private readonly AuthApiService _authService;

        public LoginWindow()
        {
            InitializeComponent();
            // Initialize AuthApiService with your API base URL
            _authService = new AuthApiService("https://localhost:5105");
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
                    // QUAN TRỌNG: Truyền cùng instance AuthApiService
                    var dashboardWindow = new DashboardWindow(_authService);
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
            //_authService?.Dispose();
        }
    }
}