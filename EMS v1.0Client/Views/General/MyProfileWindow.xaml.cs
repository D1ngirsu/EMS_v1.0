using EMS_v1._0Client.Views.Auth;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EMS_v1._0Client.Views.General
{
    public partial class MyProfileWindow : Window
    {
        private readonly AuthApiService _authService;
        private HttpClientHandler _sharedHandler;
        private EmployeeApiService _employeeService;
        private EmployeeASService _employeeASService;
        private EmployeeCDService _employeeCDService;
        private EmployeeApplicationService _employeeApplicationService;
        private EmployeeRelativesService _employeeRelativesService;
        private EmployeeInsuranceService _employeeInsuranceService;

        public MyProfileWindow(AuthApiService authService)
        {
            InitializeComponent();
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            InitializeServicesAsync().GetAwaiter().GetResult();
        }

        private async Task InitializeServicesAsync()
        {
            try
            {
                Debug.WriteLine("[MyProfileWindow] Fetching current user...");
                var user = await _authService.GetCurrentUserAsync();
                if (user == null)
                {
                    Debug.WriteLine("[MyProfileWindow] Failed to get current user, redirecting to login...");
                    MessageBox.Show("Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    var loginWindow = new LoginWindow();
                    loginWindow.Show();
                    Close();
                    return;
                }

                Debug.WriteLine($"[MyProfileWindow] Current user: {user.Username}, Role: {user.Role}, Eid: {user.Employee?.Eid}");

                _sharedHandler = _authService.CreateSharedHandler();

                var initialCookies = _sharedHandler.CookieContainer.GetCookies(new Uri("http://localhost:5165"));
                Debug.WriteLine($"[MyProfileWindow] Initial cookies count: {initialCookies.Count}");
                foreach (Cookie cookie in initialCookies)
                {
                    Debug.WriteLine($"[MyProfileWindow] Initial cookie: {cookie.Name} = {cookie.Value}");
                }

                if (initialCookies.Count == 0)
                {
                    Debug.WriteLine("[MyProfileWindow] Warning: No cookies found in shared handler");
                }

                _employeeService = new EmployeeApiService("http://localhost:5165", _sharedHandler);
                _employeeASService = new EmployeeASService("http://localhost:5165", _sharedHandler);
                _employeeCDService = new EmployeeCDService("http://localhost:5165", _sharedHandler);
                _employeeApplicationService = new EmployeeApplicationService("http://localhost:5165", _sharedHandler);
                _employeeRelativesService = new EmployeeRelativesService("http://localhost:5165", _sharedHandler);
                _employeeInsuranceService = new EmployeeInsuranceService("http://localhost:5165", _sharedHandler);

                await LoadUserInfoAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[InitializeServicesAsync] Exception: {ex.Message}\nStackTrace: {ex.StackTrace}");
                MessageBox.Show("Đã xảy ra lỗi khi khởi tạo. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                Close();
            }
        }

        private async Task LoadUserInfoAsync()
        {
            try
            {
                Debug.WriteLine("[LoadUserInfoAsync] Loading user info...");
                var user = await _authService.GetCurrentUserAsync();
                if (user == null)
                {
                    Debug.WriteLine("[LoadUserInfoAsync] User is null, redirecting to login...");
                    MessageBox.Show("Không thể tải thông tin người dùng. Vui lòng đăng nhập lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    var loginWindow = new LoginWindow();
                    loginWindow.Show();
                    Close();
                    return;
                }

                UserNameTextBlock.Text = user.Username ?? string.Empty;
                HeaderUserNameTextBlock.Text = user.Username ?? string.Empty;

                if (user.Employee != null)
                {
                    EmployeeNameTextBlock.Text = $"Họ tên: {user.Employee.Name ?? string.Empty}";
                    EmployeeEmailTextBlock.Text = $"Email: {user.Employee.Email ?? string.Empty}";
                    EmployeePhoneTextBlock.Text = $"Số điện thoại: {user.Employee.Phone ?? string.Empty}";
                    EmployeeAddressTextBlock.Text = $"Địa chỉ: {user.Employee.Address ?? string.Empty}";

                    Debug.WriteLine($"[LoadUserInfoAsync] Fetching employee info for Eid: {user.Employee.Eid}");
                    var employeeResponse = await _employeeService.GetEmployeeAsync(user.Employee.Eid);
                    if (employeeResponse.Success && employeeResponse.Data != null)
                    {
                        Debug.WriteLine($"[LoadUserInfoAsync] Employee info loaded: {employeeResponse.Data.Name}");
                        // Cập nhật lại các trường từ GetEmployeeAsync nếu cần
                        EmployeeAddressTextBlock.Text = $"Địa chỉ: {employeeResponse.Data.Address ?? string.Empty}";

                        // Giả định các trường khác từ EmployeeDto
                        if (employeeResponse.Data.OrganizationUnit != null)
                        {
                            EmployeeNameTextBlock.Text += $"\nĐơn vị: {employeeResponse.Data.OrganizationUnit.UnitName ?? string.Empty}";
                        }
                        if (employeeResponse.Data.Position != null)
                        {
                            EmployeeNameTextBlock.Text += $"\nChức vụ: {employeeResponse.Data.Position.PositionName ?? string.Empty}";
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"[LoadUserInfoAsync] Failed to load employee info: {employeeResponse.Message}");
                        MessageBox.Show($"Không thể tải thông tin chi tiết: {employeeResponse.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    // Tải thông tin học hàm/học vị
                    var asData = await _employeeASService.GetByEidAsync(user.Employee.Eid);
                    if (asData != null && asData.Success && asData.Data != null)
                    {
                        AcademicRankTextBlock.Text = $"Học hàm: {asData.Data.AcademicRank ?? string.Empty}";
                        DegreeTextBlock.Text = $"Học vị: {asData.Data.Degree ?? string.Empty}";
                        PlaceIssueTextBlock.Text = $"Nơi cấp: {asData.Data.PlaceIssue ?? string.Empty}";
                        IssueDayTextBlock.Text = $"Ngày cấp: {asData.Data.IssueDay.ToString("dd/MM/yyyy")}";
                    }

                    // Tải thông tin xác nhận
                    var cdData = await _employeeCDService.GetByEidAsync(user.Employee.Eid);
                    if (cdData != null && cdData.Success && cdData.Data != null)
                    {
                        IdNumberTextBlock.Text = $"Số CMND/CCCD: {cdData.Data.IdNumber ?? string.Empty}";
                        IssueDayCDTextBlock.Text = $"Ngày cấp: {cdData.Data.IssueDay.ToString("dd/MM/yyyy")}";
                        IssuePlaceTextBlock.Text = $"Nơi cấp: {cdData.Data.IssuePlace ?? string.Empty}";
                        CountryTextBlock.Text = $"Quốc gia: {cdData.Data.Country ?? string.Empty}";
                    }

                    // Tải danh sách đơn từ
                    var applications = await _employeeApplicationService.GetByEmployeeIdAsync(user.Employee.Eid);
                    if (applications != null && applications.Success)
                    {
                        ApplicationsListView.ItemsSource = applications.Data;
                    }

                    // Tải danh sách người thân
                    var relatives = await _employeeRelativesService.GetByEmployeeIdAsync(user.Employee.Eid);
                    if (relatives != null && relatives.Success)
                    {
                        RelativesListView.ItemsSource = relatives.Data;
                    }

                    // Tải danh sách bảo hiểm
                    var insurances = await _employeeInsuranceService.GetByEidAsync(user.Employee.Eid);
                    if (insurances != null && insurances.Success)
                    {
                        InsurancesListView.ItemsSource = insurances.Data;
                    }

                    ShowRoleSpecificButtons(user.Role);
                }
                else
                {
                    Debug.WriteLine("[LoadUserInfoAsync] Employee data is null");
                    MessageBox.Show("Không tìm thấy thông tin nhân viên.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadUserInfoAsync] Exception: {ex.Message}\nStackTrace: {ex.StackTrace}");
                MessageBox.Show("Đã xảy ra lỗi khi tải thông tin. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowRoleSpecificButtons(string role)
        {
            Debug.WriteLine($"[ShowRoleSpecificButtons] Role: {role}");
            EmployeeManagementButton.Visibility = role == "Manager" || role == "Admin" || role == "HR"
                ? Visibility.Visible : Visibility.Collapsed;
            SalaryManagementButton.Visibility = role == "Manager" || role == "Admin" || role == "PayrollOfficer"
                ? Visibility.Visible : Visibility.Collapsed;
            InsuranceManagementButton.Visibility = role == "Manager" || role == "Admin" || role == "InsuranceOfficer"
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[DashboardButton_Click] Navigating to dashboard...");
            var dashboardWindow = new DashboardWindow(_authService);
            dashboardWindow.Show();
            Close();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[ProfileButton_Click] Already on profile page...");
        }

        private void TodolistButton_Click(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine("[TodolistButton_Click] Navigating to todolist...");
            //var todolistWindow = new TodolistWindow(_authService);
            //todolistWindow.Show();
            //Close();
        }

        private void EmployeeManagementButton_Click(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine("[EmployeeManagementButton_Click] Navigating to employee management...");
            //var employeeManagementWindow = new EmployeeManagementWindow(_employeeService);
            //employeeManagementWindow.Show();
        }

        private void SalaryManagementButton_Click(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine("[SalaryManagementButton_Click] Navigating to salary management...");
            //var salaryManagementWindow = new SalaryManagementWindow(_employeeService);
            //salaryManagementWindow.Show();
        }

        private void InsuranceManagementButton_Click(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine("[InsuranceManagementButton_Click] Navigating to insurance management...");
            //var insuranceManagementWindow = new InsuranceManagementWindow(_employeeInsuranceService);
            //insuranceManagementWindow.Show();
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[LogoutButton_Click] Attempting to logout...");
                var success = await _authService.LogoutAsync();
                if (success)
                {
                    Debug.WriteLine("[LogoutButton_Click] Logout successful");
                    var loginWindow = new LoginWindow();
                    loginWindow.Show();
                    Close();
                }
                else
                {
                    Debug.WriteLine("[LogoutButton_Click] Logout failed");
                    MessageBox.Show("Đăng xuất thất bại. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LogoutButton_Click] Exception: {ex.Message}\nStackTrace: {ex.StackTrace}");
                MessageBox.Show("Đã xảy ra lỗi khi đăng xuất. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}