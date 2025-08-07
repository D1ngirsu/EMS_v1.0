using EMS_v1._0Client.Views.Auth;
using EMS_v1._0Client.Views.General;
using EMS_v1._0Client.Views.HR;
using EMS_v1._0Client.Views.SalaryStaff;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace EMS_v1._0Client.Views.InsuranceStaff
{
    // Wrapper class để xử lý hiển thị tỷ lệ đóng góp
    public class InsuranceDisplayDto
    {
        public int? Iid { get; set; }
        public int Eid { get; set; }
        public string EmployeeName { get; set; }
        public string UnitName { get; set; }
        public string InsuranceContent { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? ContributePercent { get; set; }

        // Property để hiển thị tỷ lệ đóng góp với format đúng
        public string ContributePercentDisplay
        {
            get
            {
                if (ContributePercent == null) return "N/A";

                decimal value = ContributePercent.Value;

                // Kiểm tra nếu giá trị < 1, có thể đây là decimal (0.05 = 5%)
                if (value < 1 && value > 0)
                {
                    return $"{(value * 100):F1}%";
                }
                // Nếu giá trị >= 1, có thể đây đã là phần trăm (5 = 5%)
                else
                {
                    return $"{value:F1}%";
                }
            }
        }
    }

    public partial class InsuranceManagementWindow : Window
    {
        private readonly AuthApiService _authService;
        private readonly EmployeeInsuranceService _insuranceService;
        private readonly OrganizationApiService _organizationService;
        private readonly IHttpClientFactory _httpClientFactory;
        private UserDto _currentUser;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private string _searchName = "";
        private string _unitName = null;
        private string _sortOrder = null;

        public InsuranceManagementWindow(IHttpClientFactory httpClientFactory)
        {
            InitializeComponent();
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _authService = new AuthApiService("https://localhost:5105/", _httpClientFactory);
            _insuranceService = new EmployeeInsuranceService("https://localhost:5105/", _httpClientFactory);
            _organizationService = new OrganizationApiService("https://localhost:5105/", _httpClientFactory);

            this.Loaded += async (s, e) => {
                await LoadUserInfo();
                await InitializeComboBoxes();
            };
        }

        private async Task LoadUserInfo()
        {
            try
            {
                Debug.WriteLine("[LoadUserInfo] Starting to load user info...");
                bool isAuthenticated = await _authService.IsAuthenticatedAsync();
                if (!isAuthenticated)
                {
                    Debug.WriteLine("[LoadUserInfo] User not authenticated, returning to login");
                    MessageBox.Show("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    ReturnToLogin();
                    return;
                }

                _currentUser = await _authService.GetCurrentUserAsync();
                if (_currentUser != null)
                {
                    UserNameTextBlock.Text = _currentUser.Employee?.Name ?? _currentUser.Username;
                    if (!string.IsNullOrEmpty(_currentUser.Employee?.Img))
                    {
                        try
                        {
                            UserAvatar.Source = new BitmapImage(new Uri(_currentUser.Employee.Img, UriKind.RelativeOrAbsolute));
                        }
                        catch (Exception avatarEx)
                        {
                            Debug.WriteLine($"[LoadUserInfo] Avatar load failed: {avatarEx.Message}");
                        }
                    }
                    ShowRoleSpecificButtons(_currentUser.Role);
                    await LoadInsuranceData();
                }
                else
                {
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
            EmployeeManagementButton.Visibility = Visibility.Collapsed;
            SalaryManagementButton.Visibility = Visibility.Collapsed;
            InsuranceManagementButton.Visibility = Visibility.Collapsed;

            switch (role)
            {
                case "HR":
                    EmployeeManagementButton.Visibility = Visibility.Visible;
                    break;
                case "PayrollOfficer":
                    SalaryManagementButton.Visibility = Visibility.Visible;
                    break;
                case "InsuranceOfficer":
                    InsuranceManagementButton.Visibility = Visibility.Visible;
                    break;
            }
        }

        private async Task InitializeComboBoxes()
        {
            if (_organizationService == null)
            {
                Debug.WriteLine("[InitializeComboBoxes] _organizationService is null");
                return;
            }

            SortOrderComboBox.SelectedIndex = 0;

            try
            {
                var response = await _organizationService.GetDepartmentsAsync();
                if (response.Success)
                {
                    UnitFilterComboBox.Items.Clear();
                    UnitFilterComboBox.Items.Add("Tất cả phòng ban");
                    foreach (var department in response.Data)
                    {
                        UnitFilterComboBox.Items.Add(department.UnitName);
                    }
                    UnitFilterComboBox.SelectedIndex = 0;
                }
                else
                {
                    Debug.WriteLine($"[InitializeComboBoxes] Failed to load departments: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[InitializeComboBoxes] Error loading departments: {ex.Message}");
            }
        }

        private async Task LoadInsuranceData()
        {
            try
            {
                if (_insuranceService == null)
                {
                    Debug.WriteLine("[LoadInsuranceData] _insuranceService is null");
                    return;
                }

                var response = await _insuranceService.GetInsurancesAsync(_searchName, _unitName, _sortOrder, _currentPage, _pageSize);

                // Chuyển đổi sang InsuranceDisplayDto để xử lý hiển thị
                var displayData = response.Data.Select(item => new InsuranceDisplayDto
                {
                    Iid = item.Iid,
                    Eid = item.Eid,
                    EmployeeName = item.EmployeeName,
                    UnitName = item.UnitName,
                    InsuranceContent = item.InsuranceContent,
                    FromDate = item.FromDate,
                    ToDate = item.ToDate,
                    ContributePercent = item.ContributePercent
                }).ToList();

                InsuranceDataGrid.ItemsSource = displayData;
                PageInfoTextBlock.Text = $"Trang {response.Page} / {response.TotalPages}";
                PreviousPageButton.IsEnabled = _currentPage > 1;
                NextPageButton.IsEnabled = _currentPage < response.TotalPages;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadInsuranceData] Error: {ex.Message}");
                MessageBox.Show($"Lỗi khi tải dữ liệu bảo hiểm: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateInsuranceButton_Click(object sender, RoutedEventArgs e)
        {
            if (InsuranceDataGrid.SelectedItem is InsuranceDisplayDto selectedDisplay)
            {
                // Chuyển đổi ngược lại sang EmployeeInsuranceListDto
                var selectedInsurance = new EmployeeInsuranceListDto
                {
                    Iid = selectedDisplay.Iid,
                    Eid = selectedDisplay.Eid,
                    EmployeeName = selectedDisplay.EmployeeName,
                    UnitName = selectedDisplay.UnitName,
                    InsuranceContent = selectedDisplay.InsuranceContent,
                    FromDate = selectedDisplay.FromDate,
                    ToDate = selectedDisplay.ToDate,
                    ContributePercent = selectedDisplay.ContributePercent
                };

                var updateInsuranceWindow = new UpdateInsuranceWindow(_httpClientFactory, selectedInsurance);
                updateInsuranceWindow.InsuranceUpdated += async () => await LoadInsuranceData();
                updateInsuranceWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một bản ghi bảo hiểm để cập nhật.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SearchNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchName = SearchNameTextBox.Text;
            if (_searchName == "Tìm kiếm theo tên") _searchName = "";
            _currentPage = 1;
            LoadInsuranceData();
        }

        private void UnitFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _unitName = UnitFilterComboBox.SelectedIndex == 0 ? null : UnitFilterComboBox.SelectedItem.ToString();
            _currentPage = 1;
            LoadInsuranceData();
        }

        private void SortOrderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _sortOrder = SortOrderComboBox.SelectedIndex switch
            {
                1 => "asc",
                2 => "desc",
                3 => "no-insurance",
                _ => null
            };
            _currentPage = 1;
            LoadInsuranceData();
        }

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                LoadInsuranceData();
            }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            _currentPage++;
            LoadInsuranceData();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var profileWindow = new MyProfileWindow(_httpClientFactory);
            profileWindow.Show();
            Close();
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

        private void NotificationsButton_Click(object sender, RoutedEventArgs e)
        {
            var notificationWindow = new NotificationWindow(_httpClientFactory);
            notificationWindow.Show();
            Close();
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

        private void ReturnToLogin()
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _httpClientFactory?.Dispose();
            _insuranceService?.Dispose();
            _organizationService?.Dispose();
        }
    }
}