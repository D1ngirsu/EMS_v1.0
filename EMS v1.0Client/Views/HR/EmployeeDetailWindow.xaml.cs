using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace EMS_v1._0Client.Views.HR
{
    public partial class EmployeeDetailWindow : Window
    {
        private readonly EmployeeApiService _employeeService;
        private readonly EmployeeApplicationService _applicationService;
        private readonly EmployeeASService _asService;
        private readonly EmployeeCDService _cdService;
        private readonly EmployeeRelativesService _relativesService;
        private readonly EmployeeCLService _clService;
        private readonly EmployeeTodolistService _todolistService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly int _eid;
        private int _appCurrentPage = 1;
        private int _appPageSize = 10;
        private int _appTotalPages = 1;
        private int _todoCurrentPage = 1;
        private int _todoPageSize = 10;
        private int _todoTotalPages = 1;

        public EmployeeDetailWindow(IHttpClientFactory httpClientFactory, int eid)
        {
            InitializeComponent();
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _eid = eid;
            _employeeService = new EmployeeApiService("https://localhost:5105/", _httpClientFactory);
            _applicationService = new EmployeeApplicationService("https://localhost:5105/", _httpClientFactory);
            _asService = new EmployeeASService("https://localhost:5105/", _httpClientFactory);
            _cdService = new EmployeeCDService("https://localhost:5105/", _httpClientFactory);
            _relativesService = new EmployeeRelativesService("https://localhost:5105/", _httpClientFactory);
            _clService = new EmployeeCLService("https://localhost:5105/", _httpClientFactory);
            _todolistService = new EmployeeTodolistService("https://localhost:5105/", _httpClientFactory);
            LoadEmployeeDetails();
            LoadApplications();
            LoadAcademicSkills();
            LoadConfirmationDocuments();
            LoadRelatives();
            LoadContracts();
            LoadTodolist();
        }

        private async void LoadEmployeeDetails()
        {
            try
            {
                var response = await _employeeService.GetEmployeeAsync(_eid);
                if (response.Success && response.Data != null)
                {
                    var employee = response.Data;
                    EmployeeNameTextBlock.Text = employee.Name;
                    EidTextBlock.Text = employee.Eid.ToString();
                    NameTextBlock.Text = employee.Name;
                    DoBTextBlock.Text = employee.DoB.ToString("d/M/yyyy");
                    UnitTextBlock.Text = new ParentUnitConverter().Convert(employee, typeof(string), null, CultureInfo.CurrentCulture).ToString();
                    PositionTextBlock.Text = employee.Position?.PositionName ?? string.Empty;
                    EmailTextBlock.Text = employee.Email ?? string.Empty;
                    PhoneTextBlock.Text = employee.Phone ?? string.Empty;
                    AddressTextBlock.Text = employee.Address ?? string.Empty;
                    GenderTextBlock.Text = employee.Gender ?? string.Empty;
                    ExperienceTextBlock.Text = employee.Experience.ToString();
                    BankNumberTextBlock.Text = employee.BankNumber ?? string.Empty;
                    BankTextBlock.Text = employee.Bank ?? string.Empty;

                    if (!string.IsNullOrEmpty(employee.Img))
                    {
                        if (Uri.TryCreate(employee.Img, UriKind.Absolute, out Uri uri))
                        {
                            EmployeeAvatar.Source = new BitmapImage(uri);
                        }
                        else if (Uri.TryCreate(new Uri("https://localhost:5105/"), employee.Img, out uri)) // Convert relative to absolute
                        {
                            EmployeeAvatar.Source = new BitmapImage(uri);
                        }
                        else
                        {
                            Debug.WriteLine($"Invalid URI for employee image: {employee.Img}");
                            EmployeeAvatar.Source = null; // Fallback to no image
                        }
                    }
                    else
                    {
                        EmployeeAvatar.Source = null;
                    }
                }
                else
                {
                    //MessageBox.Show(response.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading employee details: {ex.Message}");
                MessageBox.Show($"Lỗi khi tải thông tin nhân viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadApplications()
        {
            try
            {
                var response = await _applicationService.GetByEmployeeIdAsync(_eid, _appCurrentPage, _appPageSize);
                if (response.Success)
                {
                    ApplicationsDataGrid.ItemsSource = response.Data;
                    _appTotalPages = response.TotalPages;
                    AppPageInfoTextBlock.Text = $"Trang {_appCurrentPage} / {_appTotalPages}";
                    UpdateAppPaginationButtons();
                }
                else
                {
                    MessageBox.Show(response.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải đơn từ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadAcademicSkills()
        {
            try
            {
                var response = await _asService.GetByEidAsync(_eid);
                if (response.Success && response.Data != null)
                {
                    var asData = response.Data;
                    AcademicRankTextBlock.Text = asData.AcademicRank ?? string.Empty;
                    DegreeTextBlock.Text = asData.Degree ?? string.Empty;
                    PlaceIssueTextBlock.Text = asData.PlaceIssue ?? string.Empty;
                    IssueDayTextBlock.Text = asData.IssueDay.ToString("d/M/yyyy");

                    if (!string.IsNullOrEmpty(asData.DegreeImg1))
                    {
                        if (Uri.TryCreate(asData.DegreeImg1, UriKind.Absolute, out Uri uri1))
                        {
                            DegreeImg1.Source = new BitmapImage(uri1);
                        }
                        else if (Uri.TryCreate(new Uri("https://localhost:5105/"), asData.DegreeImg1, out uri1))
                        {
                            DegreeImg1.Source = new BitmapImage(uri1);
                        }
                        else
                        {
                            Debug.WriteLine($"Invalid URI for DegreeImg1: {asData.DegreeImg1}");
                            DegreeImg1.Source = null;
                        }
                    }
                    else
                    {
                        DegreeImg1.Source = null;
                    }

                    if (!string.IsNullOrEmpty(asData.DegreeImg2))
                    {
                        if (Uri.TryCreate(asData.DegreeImg2, UriKind.Absolute, out Uri uri2))
                        {
                            DegreeImg2.Source = new BitmapImage(uri2);
                        }
                        else if (Uri.TryCreate(new Uri("https://localhost:5105/"), asData.DegreeImg2, out uri2))
                        {
                            DegreeImg2.Source = new BitmapImage(uri2);
                        }
                        else
                        {
                            Debug.WriteLine($"Invalid URI for DegreeImg2: {asData.DegreeImg2}");
                            DegreeImg2.Source = null;
                        }
                    }
                    else
                    {
                        DegreeImg2.Source = null;
                    }
                }
                else
                {
                    AcademicRankTextBlock.Text = string.Empty;
                    DegreeTextBlock.Text = string.Empty;
                    PlaceIssueTextBlock.Text = string.Empty;
                    IssueDayTextBlock.Text = string.Empty;
                    DegreeImg1.Source = null;
                    DegreeImg2.Source = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading academic skills: {ex.Message}");
                MessageBox.Show($"Lỗi khi tải trình độ học vấn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadConfirmationDocuments()
        {
            try
            {
                var response = await _cdService.GetByEidAsync(_eid);
                if (response.Success && response.Data != null)
                {
                    var cdData = response.Data;
                    IdNumberTextBlock.Text = cdData.IdNumber ?? string.Empty;
                    CDIssueDayTextBlock.Text = cdData.IssueDay.ToString("d/M/yyyy");
                    IssuePlaceTextBlock.Text = cdData.IssuePlace ?? string.Empty;
                    CountryTextBlock.Text = cdData.Country ?? string.Empty;
                }
                else
                {
                    IdNumberTextBlock.Text = string.Empty;
                    CDIssueDayTextBlock.Text = string.Empty;
                    IssuePlaceTextBlock.Text = string.Empty;
                    CountryTextBlock.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thông tin xác nhận: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadRelatives()
        {
            try
            {
                var response = await _relativesService.GetByEmployeeIdAsync(_eid, 1, int.MaxValue);
                if (response.Success)
                {
                    RelativesDataGrid.ItemsSource = response.Data;
                }
                else
                {
                    //MessageBox.Show(response.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thông tin người thân: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadContracts()
        {
            try
            {
                var response = await _clService.GetByEidAsync(_eid, 1, int.MaxValue);
                if (response.Success)
                {
                    ContractsDataGrid.ItemsSource = response.Data;
                }
                else
                {
                    //MessageBox.Show(response.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải hợp đồng lao động: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadTodolist()
        {
            try
            {
                var response = await _todolistService.GetAllAsync(_eid, null, _todoCurrentPage, _todoPageSize);
                if (response.Success)
                {
                    TodolistDataGrid.ItemsSource = response.Data;
                    _todoTotalPages = response.TotalPages;
                    TodoPageInfoTextBlock.Text = $"Trang {_todoCurrentPage} / {_todoTotalPages}";
                    UpdateTodoPaginationButtons();
                }
                else
                {
                    //MessageBox.Show(response.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải todolist: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateAppPaginationButtons()
        {
            AppFirstPageButton.IsEnabled = _appCurrentPage > 1;
            AppPreviousPageButton.IsEnabled = _appCurrentPage > 1;
            AppNextPageButton.IsEnabled = _appCurrentPage < _appTotalPages;
            AppLastPageButton.IsEnabled = _appCurrentPage < _appTotalPages;
        }

        private void UpdateTodoPaginationButtons()
        {
            TodoFirstPageButton.IsEnabled = _todoCurrentPage > 1;
            TodoPreviousPageButton.IsEnabled = _todoCurrentPage > 1;
            TodoNextPageButton.IsEnabled = _todoCurrentPage < _todoTotalPages;
            TodoLastPageButton.IsEnabled = _todoCurrentPage < _todoTotalPages;
        }

        private void AppFirstPageButton_Click(object sender, RoutedEventArgs e)
        {
            _appCurrentPage = 1;
            LoadApplications();
        }

        private void AppPreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_appCurrentPage > 1)
            {
                _appCurrentPage--;
                LoadApplications();
            }
        }

        private void AppNextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_appCurrentPage < _appTotalPages)
            {
                _appCurrentPage++;
                LoadApplications();
            }
        }

        private void AppLastPageButton_Click(object sender, RoutedEventArgs e)
        {
            _appCurrentPage = _appTotalPages;
            LoadApplications();
        }

        private void TodoFirstPageButton_Click(object sender, RoutedEventArgs e)
        {
            _todoCurrentPage = 1;
            LoadTodolist();
        }

        private void TodoPreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_todoCurrentPage > 1)
            {
                _todoCurrentPage--;
                LoadTodolist();
            }
        }

        private void TodoNextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_todoCurrentPage < _todoTotalPages)
            {
                _todoCurrentPage++;
                LoadTodolist();
            }
        }

        private void TodoLastPageButton_Click(object sender, RoutedEventArgs e)
        {
            _todoCurrentPage = _todoTotalPages;
            LoadTodolist();
        }
    }
}