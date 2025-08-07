using EMS_v1._0Client.Views.Auth;
using EMS_v1._0Client.Views.HR;
using EMS_v1._0Client.Views.SalaryStaff;
using EMS_v1._0Client.Views.InsuranceStaff;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace EMS_v1._0Client.Views.General
{
    public partial class NotificationWindow : Window
    {
        private readonly AuthApiService _authService;
        private readonly NotificationApiService _notificationService;
        private readonly IHttpClientFactory _httpClientFactory;
        private UserDto _currentUser;
        private ObservableCollection<NotificationDto> _notifications;

        public NotificationWindow(IHttpClientFactory httpClientFactory)
        {
            InitializeComponent();
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _authService = new AuthApiService("https://localhost:5105/", _httpClientFactory);
            _notificationService = new NotificationApiService("https://localhost:5105/", _httpClientFactory);
            _notifications = new ObservableCollection<NotificationDto>();
            NotificationsListView.ItemsSource = _notifications;
            LoadUserInfo();
        }

        private async void LoadUserInfo()
        {
            try
            {
                Debug.WriteLine("[LoadUserInfo] Starting to load user info...");

                bool isAuthenticated = await _authService.IsAuthenticatedAsync();
                Debug.WriteLine($"[LoadUserInfo] IsAuthenticated result: {isAuthenticated}");

                if (!isAuthenticated)
                {
                    Debug.WriteLine("[LoadUserInfo] User not authenticated, returning to login");
                    MessageBox.Show("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    ReturnToLogin();
                    return;
                }

                Debug.WriteLine("[LoadUserInfo] Getting current user...");
                _currentUser = await _authService.GetCurrentUserAsync();

                if (_currentUser != null)
                {
                    Debug.WriteLine($"[LoadUserInfo] Successfully loaded user: {_currentUser.Username}");
                    UserNameTextBlock.Text = _currentUser.Employee?.Name ?? _currentUser.Username;
                    Debug.WriteLine($"[LoadUserInfo] Set username to: {UserNameTextBlock.Text}");

                    if (!string.IsNullOrEmpty(_currentUser.Employee?.Img))
                    {
                        try
                        {
                            Debug.WriteLine($"[LoadUserInfo] Loading avatar: {_currentUser.Employee.Img}");
                            UserAvatar.Source = new BitmapImage(new Uri(_currentUser.Employee.Img, UriKind.RelativeOrAbsolute));
                        }
                        catch (Exception avatarEx)
                        {
                            Debug.WriteLine($"[LoadUserInfo] Avatar load failed: {avatarEx.Message}");
                        }
                    }

                    Debug.WriteLine($"[LoadUserInfo] User role: {_currentUser.Role}");
                    ShowRoleSpecificButtons(_currentUser.Role);

                    // Load notifications only after user is authenticated
                    LoadNotifications();
                }
                else
                {
                    Debug.WriteLine("[LoadUserInfo] _currentUser is null after GetCurrentUserAsync");
                    MessageBox.Show("Không thể tải thông tin người dùng. Vui lòng đăng nhập lại.", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    ReturnToLogin();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadUserInfo] Exception: {ex.Message}");
                Debug.WriteLine($"[LoadUserInfo] StackTrace: {ex.StackTrace}");
                MessageBox.Show($"Lỗi khi tải thông tin người dùng: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                ReturnToLogin();
            }
        }

        private async void LoadNotifications()
        {
            try
            {
                var response = await _notificationService.GetRecentNotificationsAsync();
                if (response.Success)
                {
                    var notifications = response.Data != null
                        ? JsonSerializer.Deserialize<List<NotificationDto>>(JsonSerializer.Serialize(response.Data), new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        })
                        : new List<NotificationDto>();

                    _notifications.Clear();
                    foreach (var notification in notifications)
                    {
                        _notifications.Add(notification);
                    }

                    if (!notifications.Any())
                    {
                        MessageBox.Show("Không có thông báo nào để hiển thị.", "Thông báo",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show($"Lỗi khi tải thông báo: {response.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadNotifications] Exception: {ex.Message}");
                MessageBox.Show($"Lỗi khi tải thông báo: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowRoleSpecificButtons(string role)
        {
            Debug.WriteLine($"[ShowRoleSpecificButtons] Processing role: {role}");

            EmployeeManagementButton.Visibility = Visibility.Collapsed;
            SalaryManagementButton.Visibility = Visibility.Collapsed;
            InsuranceManagementButton.Visibility = Visibility.Collapsed;

            switch (role)
            {
                case "HR":
                    Debug.WriteLine("[ShowRoleSpecificButtons] Showing HR buttons");
                    EmployeeManagementButton.Visibility = Visibility.Visible;
                    break;
                case "PayrollOfficer":
                    Debug.WriteLine("[ShowRoleSpecificButtons] Showing PayrollOfficer buttons");
                    SalaryManagementButton.Visibility = Visibility.Visible;
                    break;
                case "InsuranceOfficer":
                    Debug.WriteLine("[ShowRoleSpecificButtons] Showing InsuranceOfficer buttons");
                    InsuranceManagementButton.Visibility = Visibility.Visible;
                    break;
                default:
                    Debug.WriteLine($"[ShowRoleSpecificButtons] Unknown role: {role}");
                    break;
            }
        }

        private void ReturnToLogin()
        {
            Debug.WriteLine("[ReturnToLogin] Returning to login window");
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var profileWindow = new MyProfileWindow(_httpClientFactory);
            profileWindow.Show();
            Close();
        }

        private void NotificationsButton_Click(object sender, RoutedEventArgs e)
        {
            // Already in NotificationWindow, no action needed
        }

        private void EmployeeManagementButton_Click(object sender, RoutedEventArgs e)
        {
            var employeeListWindow = new EmployeeListWindow(_httpClientFactory);
            employeeListWindow.Show();
            Close();
        }

        private void SalaryManagementButton_Click(object sender, RoutedEventArgs e)
        {
            var salaryManagementWindow = new SalaryManagementWindow(_httpClientFactory);
            salaryManagementWindow.Show();
            Close();
        }

        private void InsuranceManagementButton_Click(object sender, RoutedEventArgs e)
        {
            var insuranceManagementWindow = new InsuranceManagementWindow(_httpClientFactory);
            insuranceManagementWindow.Show();
            Close();
        }

        private void TodolistButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chuyển đến Todolist của tôi (Chưa được triển khai)", "Thông báo");
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = await _authService.LogoutAsync();
                if (result)
                {
                    var loginWindow = new LoginWindow();
                    loginWindow.Show();
                    Close();
                }
                else
                {
                    MessageBox.Show("Đăng xuất thất bại", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _httpClientFactory?.Dispose();
            _notificationService?.Dispose();
        }
    }

    public class NotificationDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime DateTime { get; set; }
    }
}