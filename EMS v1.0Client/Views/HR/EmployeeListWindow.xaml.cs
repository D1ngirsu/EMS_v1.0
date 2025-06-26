using EMS_v1._0Client.Views.Auth;
using EMS_v1._0Client.Views.General;
using System;
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
            LoadEmployees();
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
            // _searchName = SearchTextBox.Text;
            // _currentPage = 1;
            // LoadEmployees();
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
            // Implement todolist navigation
        }

        private void EmployeeManagementButton_Click(object sender, RoutedEventArgs e)
        {
            // Already in employee management, no action needed
        }

        private void SalaryManagementButton_Click(object sender, RoutedEventArgs e)
        {
            // Implement salary management navigation
        }

        private void InsuranceManagementButton_Click(object sender, RoutedEventArgs e)
        {
            // Implement insurance management navigation
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