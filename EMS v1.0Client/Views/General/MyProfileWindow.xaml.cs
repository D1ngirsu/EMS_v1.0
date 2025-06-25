using EMS_v1._0Client.Views.Auth;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace EMS_v1._0Client.Views.General
{
    public partial class MyProfileWindow : Window
    {
        private readonly AuthApiService _authService;
        private readonly IHttpClientFactory _httpClientFactory;
        private UserDto _currentUser;

        public MyProfileWindow(IHttpClientFactory httpClientFactory)
        {
            InitializeComponent();
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _authService = new AuthApiService("https://localhost:5105/", _httpClientFactory);
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

                    // Set user info in UI
                    UserNameTextBlock.Text = _currentUser.Employee?.Name ?? _currentUser.Username;
                    ProfileNameTextBlock.Text = _currentUser.Employee?.Name ?? _currentUser.Username;
                    UsernameTextBlock.Text = _currentUser.Username ?? "N/A";
                    EmailTextBlock.Text = _currentUser.Employee?.Email ?? "N/A";
                    RoleTextBlock.Text = _currentUser.Role ?? "N/A";

                    // Handle organization unit and group
                    if (_currentUser.Employee?.OrganizationUnit != null)
                    {
                        Debug.WriteLine($"[LoadUserInfo] UnitType: {_currentUser.Employee.OrganizationUnit.UnitType}");
                        Debug.WriteLine($"[LoadUserInfo] ParentOrganizationUnit: {_currentUser.Employee.ParentOrganizationUnit?.UnitName ?? "null"}");
                        if (_currentUser.Employee.OrganizationUnit.UnitType.Equals("Nhom", StringComparison.OrdinalIgnoreCase) &&
                            _currentUser.Employee.ParentOrganizationUnit != null)
                        {
                            UnitTextBlock.Text = _currentUser.Employee.ParentOrganizationUnit.UnitName ?? "N/A";
                            GroupTextBlock.Text = _currentUser.Employee.OrganizationUnit.UnitName ?? "N/A";
                        }
                        else
                        {
                            UnitTextBlock.Text = _currentUser.Employee.OrganizationUnit.UnitName ?? "N/A";
                            GroupTextBlock.Text = "N/A";
                        }
                    }

                    PositionTextBlock.Text = _currentUser.Employee?.Position?.PositionName ?? "N/A";
                    DateOfBirthTextBlock.Text = _currentUser.Employee?.DoB.ToString("dd/MM/yyyy") ?? "N/A";
                    GenderTextBlock.Text = _currentUser.Employee?.Gender ?? "N/A";
                    AddressTextBlock.Text = _currentUser.Employee?.Address ?? "N/A";

                    // Load avatar if available
                    if (!string.IsNullOrEmpty(_currentUser.Employee?.Img))
                    {
                        try
                        {
                            Debug.WriteLine($"[LoadUserInfo] Loading avatar: {_currentUser.Employee.Img}");
                            var avatarSource = new BitmapImage(new Uri(_currentUser.Employee.Img, UriKind.RelativeOrAbsolute));
                            UserAvatar.Source = avatarSource;
                            ProfileAvatar.Source = avatarSource;
                        }
                        catch (Exception avatarEx)
                        {
                            Debug.WriteLine($"[LoadUserInfo] Avatar load failed: {avatarEx.Message}");
                        }
                    }

                    // Show role-specific buttons
                    Debug.WriteLine($"[LoadUserInfo] User role: {_currentUser.Role}");
                    ShowRoleSpecificButtons(_currentUser.Role);
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
                MessageBox.Show($"Lỗi khi tải thông tin người dùng: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                ReturnToLogin();
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

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            var changePasswordWindow = new ChangePasswordWindow(_httpClientFactory);
            changePasswordWindow.Show();
            Close();
        }

        private void TodolistButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chuyển đến Todolist của tôi (Chưa được triển khai)", "Thông báo");
        }

        private void EmployeeManagementButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chuyển đến Quản lý nhân sự (Chưa được triển khai)", "Thông báo");
        }

        private void SalaryManagementButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chuyển đến Quản lý lương (Chưa được triển khai)", "Thông báo");
        }

        private void InsuranceManagementButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chuyển đến Quản lý bảo hiểm (Chưa được triển khai)", "Thông báo");
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
        }
    }
}