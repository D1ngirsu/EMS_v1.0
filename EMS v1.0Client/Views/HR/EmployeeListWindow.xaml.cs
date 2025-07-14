using EMS_v1._0Client.Views.Auth;
using EMS_v1._0Client.Views.General;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace EMS_v1._0Client.Views.HR
{
    public partial class EmployeeListWindow : Window
    {
        private readonly EmployeeApiService _employeeService;
        private readonly AuthApiService _authService;
        private readonly IHttpClientFactory _httpClientFactory;
        private UserDto? _currentUser;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages = 1;
        private string _searchName = string.Empty;

        public EmployeeListWindow(IHttpClientFactory httpClientFactory)
        {
            InitializeComponent();
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _employeeService = new EmployeeApiService("https://localhost:5105/", _httpClientFactory);
            _authService = new AuthApiService("https://localhost:5105/", _httpClientFactory);
            LoadUserInfo();
            LoadEmployees();
        }

        private async void LoadUserInfo()
        {
            try
            {
                Debug.WriteLine("[LoadUserInfo] Starting to load user info...");

                // Check if authenticated
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

                    // Load avatar if available
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
                            // Avatar load failed, use default
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
                Debug.WriteLine($"[LoadUserInfo] StackTrace: {ex.StackTrace}");

                MessageBox.Show($"Lỗi khi tải thông tin người dùng: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                ReturnToLogin();
            }
        }

        private void ShowRoleSpecificButtons(string role)
        {
            Debug.WriteLine($"[ShowRoleSpecificButtons] Processing role: {role}");

            // Hide all role-specific buttons first
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

        private async void LoadEmployees()
        {
            try
            {
                var response = await _employeeService.GetEmployeesAsync(_currentPage, _pageSize, _searchName);
                if (response.Success)
                {
                    EmployeeDataGrid.ItemsSource = response.Data;
                    _totalPages = response.TotalPages;
                    PageInfoTextBlock.Text = $"Trang {_currentPage} / {_totalPages}";
                    UpdatePaginationButtons();
                }
                else
                {
                    MessageBox.Show(response.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách nhân viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePaginationButtons()
        {
            FirstPageButton.IsEnabled = _currentPage > 1;
            PreviousPageButton.IsEnabled = _currentPage > 1;
            NextPageButton.IsEnabled = _currentPage < _totalPages;
            LastPageButton.IsEnabled = _currentPage < _totalPages;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _searchName = SearchTextBox.Text;
            _currentPage = 1;
            LoadEmployees();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Optional: Implement live search by uncommenting below
            _searchName = SearchTextBox.Text;
            _currentPage = 1;
            LoadEmployees();
        }

        private void FirstPageButton_Click(object sender, RoutedEventArgs e)
        {
            _currentPage = 1;
            LoadEmployees();
        }

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                LoadEmployees();
            }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                LoadEmployees();
            }
        }

        private void LastPageButton_Click(object sender, RoutedEventArgs e)
        {
            _currentPage = _totalPages;
            LoadEmployees();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var profileWindow = new MyProfileWindow(_httpClientFactory);
            profileWindow.Show();
            Close();
        }

        private void TodolistButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chuyển đến Todolist của tôi (Chưa được triển khai)", "Thông báo");
        }

        private void EmployeeManagementButton_Click(object sender, RoutedEventArgs e)
        {
            // Already in employee management, no action needed
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

        private void ViewDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int eid)
            {
                var employeeDetailWindow = new EmployeeDetailWindow(_httpClientFactory, eid);
                employeeDetailWindow.Show();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            // Dispose the HttpClientFactory to clean up resources
            _httpClientFactory?.Dispose();
        }

        private void AddEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            var addEmployeeWindow = new AddEmployeeWindow(_httpClientFactory);
            addEmployeeWindow.Show();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadEmployees();
        }
    }

    // Converter to display parent unit or direct unit based on ParentOrganizationUnit
    public class ParentUnitConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EmployeeDto employee)
            {
                // If ParentOrganizationUnit is null, display OrganizationUnit.UnitName
                // Otherwise, display ParentOrganizationUnit.UnitName
                return employee.ParentOrganizationUnit == null
                    ? employee.OrganizationUnit?.UnitName ?? string.Empty
                    : employee.ParentOrganizationUnit.UnitName ?? string.Empty;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}