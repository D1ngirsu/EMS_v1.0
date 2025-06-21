using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EMS_v1._0Client.Views.Auth
{
    public partial class ChangePasswordWindow : Window
    {
        private readonly AuthApiService _authService;

        public ChangePasswordWindow()
        {
            InitializeComponent();
            // Initialize AuthApiService with your API base URL
            _authService = new AuthApiService("https://your-api-base-url");
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Clear previous message
                MessageTextBlock.Visibility = Visibility.Collapsed;
                MessageTextBlock.Text = string.Empty;

                string currentPassword = CurrentPasswordBox.Password;
                string newPassword = NewPasswordBox.Password;
                string confirmPassword = ConfirmPasswordBox.Password;

                // Client-side validation
                if (string.IsNullOrWhiteSpace(currentPassword) ||
                    string.IsNullOrWhiteSpace(newPassword) ||
                    string.IsNullOrWhiteSpace(confirmPassword))
                {
                    ShowMessage("Vui lòng nhập đầy đủ các trường", Brushes.Red);
                    return;
                }

                if (newPassword != confirmPassword)
                {
                    ShowMessage("Mật khẩu mới và xác nhận mật khẩu không khớp", Brushes.Red);
                    return;
                }

                // Disable button during request
                ((Button)sender).IsEnabled = false;

                // Call change password API
                var response = await _authService.ChangePasswordAsync(currentPassword, newPassword);

                if (response.Success)
                {
                    ShowMessage("Đổi mật khẩu thành công", Brushes.Green);
                    // Optionally clear fields
                    CurrentPasswordBox.Clear();
                    NewPasswordBox.Clear();
                    ConfirmPasswordBox.Clear();
                }
                else
                {
                    ShowMessage(response.Message, Brushes.Red);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Đã xảy ra lỗi: {ex.Message}", Brushes.Red);
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

        private void ShowMessage(string message, SolidColorBrush color)
        {
            MessageTextBlock.Text = message;
            MessageTextBlock.Foreground = color;
            MessageTextBlock.Visibility = Visibility.Visible;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _authService?.Dispose();
        }
    }
}