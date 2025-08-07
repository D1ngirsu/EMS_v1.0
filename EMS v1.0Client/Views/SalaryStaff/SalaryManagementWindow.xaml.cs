using EMS_v1._0Client.Views.Auth;
using EMS_v1._0Client.Views.General;
using EMS_v1._0Client.Views.HR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace EMS_v1._0Client.Views.SalaryStaff
{
    public partial class SalaryManagementWindow : Window
    {
        private readonly AuthApiService _authService;
        private readonly EmployeeSalaryService _salaryService;
        private readonly IHttpClientFactory _httpClientFactory;
        private UserDto _currentUser;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private string _searchName = "";
        private string _unitName = null;
        private string _sortOrder = null;

        public SalaryManagementWindow(IHttpClientFactory httpClientFactory)
        {
            InitializeComponent();
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _authService = new AuthApiService("https://localhost:5105/", _httpClientFactory);
            _salaryService = new EmployeeSalaryService("https://localhost:5105/", _httpClientFactory);

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
                    await LoadSalaryData();
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
            if (_salaryService == null)
            {
                Debug.WriteLine("[InitializeComboBoxes] _salaryService is null");
                return;
            }

            SortOrderComboBox.SelectedIndex = 0;

            try
            {
                var response = await _salaryService.GetDepartmentsAsync();
                if (response.Success)
                {
                    UnitFilterComboBox.Items.Clear();
                    UnitFilterComboBox.Items.Add("Tất cả phòng ban");
                    foreach (var department in response.Data)
                    {
                        UnitFilterComboBox.Items.Add(department);
                    }
                    UnitFilterComboBox.SelectedIndex = 0;
                }
                else
                {
                    Debug.WriteLine($"[InitializeComboBoxes] Failed to load departments");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[InitializeComboBoxes] Error loading departments: {ex.Message}");
            }
        }

        private async Task LoadSalaryData()
        {
            try
            {
                if (_salaryService == null)
                {
                    Debug.WriteLine("[LoadSalaryData] _salaryService is null");
                    return;
                }

                var response = await _salaryService.GetAllAsync(_currentPage, _pageSize, _searchName, _unitName, _sortOrder);
                SalaryDataGrid.ItemsSource = response.Data;
                PageInfoTextBlock.Text = $"Trang {response.Page} / {response.TotalPages}";
                PreviousPageButton.IsEnabled = _currentPage > 1;
                NextPageButton.IsEnabled = _currentPage < response.TotalPages;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadSalaryData] Error: {ex.Message}");
                MessageBox.Show($"Lỗi khi tải dữ liệu lương: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSalaryButton_Click(object sender, RoutedEventArgs e)
        {
            if (SalaryDataGrid.SelectedItem is EmployeeSalaryListDto selectedEmployee)
            {
                var updateSalaryWindow = new UpdateSalaryWindow(_httpClientFactory, selectedEmployee);
                updateSalaryWindow.SalaryUpdated += async () => await LoadSalaryData();
                updateSalaryWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một nhân viên để cập nhật lương.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SearchNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchName = SearchNameTextBox.Text;
            if (_searchName == "Tìm kiếm theo tên") _searchName = "";
            _currentPage = 1;
            LoadSalaryData();
        }

        private void UnitFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _unitName = UnitFilterComboBox.SelectedIndex == 0 ? null : UnitFilterComboBox.SelectedItem.ToString();
            _currentPage = 1;
            LoadSalaryData();
        }

        private void SortOrderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _sortOrder = SortOrderComboBox.SelectedIndex switch
            {
                1 => "asc",
                2 => "desc",
                3 => "no-salary",
                _ => null
            };
            _currentPage = 1;
            LoadSalaryData();
        }

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                LoadSalaryData();
            }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            _currentPage++;
            LoadSalaryData();
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
            MessageBox.Show("Chuyển đến Quản lý bảo hiểm (Chưa được triển khai)", "Thông báo");
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
            _salaryService?.Dispose();
        }
    }
}