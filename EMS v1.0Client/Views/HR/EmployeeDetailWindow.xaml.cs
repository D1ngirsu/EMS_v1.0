
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        private readonly OrganizationApiService _orgService;
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
            _orgService = new OrganizationApiService("https://localhost:5105/", _httpClientFactory);
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
                    EidTextBox.Text = employee.Eid.ToString();
                    NameTextBox.Text = employee.Name;
                    DoBDatePicker.SelectedDate = employee.DoB;
                    EmailTextBox.Text = employee.Email ?? string.Empty;
                    PhoneTextBox.Text = employee.Phone ?? string.Empty;
                    AddressTextBox.Text = employee.Address ?? string.Empty;
                    GenderComboBox.SelectedItem = GenderComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(i => i.Content.ToString() == (employee.Gender ?? string.Empty));
                    ExperienceTextBox.Text = employee.Experience.ToString();
                    BankNumberTextBox.Text = employee.BankNumber ?? string.Empty;
                    BankTextBox.Text = employee.Bank ?? string.Empty;

                    // Load departments and positions
                    var deptResponse = await _orgService.GetDepartmentsAsync();
                    if (deptResponse.Success && deptResponse.Data != null)
                    {
                        UnitComboBox.ItemsSource = deptResponse.Data;
                        UnitComboBox.SelectedItem = deptResponse.Data.FirstOrDefault(u => u.UnitId == employee.OrganizationUnit?.UnitId);
                    }

                    var posResponse = await _orgService.GetPositionsAsync();
                    if (posResponse.Success && posResponse.Data != null)
                    {
                        PositionComboBox.ItemsSource = posResponse.Data;
                        PositionComboBox.SelectedItem = posResponse.Data.FirstOrDefault(p => p.PositionId == employee.Position?.PositionId);
                    }

                    if (!string.IsNullOrEmpty(employee.Img))
                    {
                        if (Uri.TryCreate(employee.Img, UriKind.Absolute, out Uri uri))
                        {
                            EmployeeAvatar.Source = new BitmapImage(uri);
                        }
                        else if (Uri.TryCreate(new Uri("https://localhost:5105/"), employee.Img, out uri))
                        {
                            EmployeeAvatar.Source = new BitmapImage(uri);
                        }
                        else
                        {
                            Debug.WriteLine($"Invalid URI for employee image: {employee.Img}");
                            EmployeeAvatar.Source = null;
                        }
                    }
                    else
                    {
                        EmployeeAvatar.Source = null;
                    }
                }
                else
                {
                    MessageBox.Show(response.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading employee details: {ex.Message}");
                MessageBox.Show($"Lỗi khi tải thông tin nhân viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(NameTextBox.Text) || DoBDatePicker.SelectedDate == null ||
                    UnitComboBox.SelectedItem == null || PositionComboBox.SelectedItem == null ||
                    string.IsNullOrWhiteSpace(EmailTextBox.Text) || string.IsNullOrWhiteSpace(PhoneTextBox.Text) ||
                    GenderComboBox.SelectedItem == null || string.IsNullOrWhiteSpace(BankNumberTextBox.Text) ||
                    string.IsNullOrWhiteSpace(BankTextBox.Text))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin bắt buộc (Họ tên, Ngày sinh, Phòng ban, Chức vụ, Email, Số điện thoại, Giới tính, Số tài khoản ngân hàng, Ngân hàng).", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate Date of Birth (must be at least 18 years old)
                if (DoBDatePicker.SelectedDate.HasValue)
                {
                    var age = DateTime.Now.Year - DoBDatePicker.SelectedDate.Value.Year;
                    if (DoBDatePicker.SelectedDate.Value > DateTime.Now.AddYears(-age)) age--;
                    if (age < 18)
                    {
                        MessageBox.Show("Nhân viên phải từ 18 tuổi trở lên.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // Validate Email format
                string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!Regex.IsMatch(EmailTextBox.Text, emailPattern))
                {
                    MessageBox.Show("Email không đúng định dạng (ví dụ: example@example.com).", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate Phone number format
                string phonePattern = @"^\d{10}$";
                if (!Regex.IsMatch(PhoneTextBox.Text, phonePattern))
                {
                    MessageBox.Show("Số điện thoại phải có đúng 10 chữ số.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check for duplicate Email and Phone
                var uniqueCheckResponse = await _employeeService.CheckEmailAndPhoneUniqueAsync(EmailTextBox.Text, PhoneTextBox.Text);
                if (!uniqueCheckResponse)
                {
                    MessageBox.Show("Email hoặc số điện thoại đã tồn tại trong cơ sở dữ liệu.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var employee = new Employee
                {
                    Eid = _eid,
                    Name = NameTextBox.Text,
                    DoB = DoBDatePicker.SelectedDate.Value,
                    UnitId = (UnitComboBox.SelectedItem as OrganizationUnitDto).UnitId,
                    PositionId = (PositionComboBox.SelectedItem as PositionDto).PositionId,
                    Email = EmailTextBox.Text,
                    Phone = PhoneTextBox.Text,
                    Address = AddressTextBox.Text,
                    Gender = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    Experience = int.TryParse(ExperienceTextBox.Text, out int exp) ? exp : 0,
                    BankNumber = BankNumberTextBox.Text,
                    Bank = BankTextBox.Text,
                    Source = (await _employeeService.GetEmployeeAsync(_eid)).Data.Source // Preserve existing Source
                };

                var response = await _employeeService.UpdateEmployeeAsync(_eid, employee);
                if (response.Success)
                {
                    MessageBox.Show("Cập nhật thông tin nhân viên thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadEmployeeDetails(); // Refresh the data
                }
                else
                {
                    string errorMessage = response.Message ?? "Không nhận được dữ liệu từ server.";
                    if (response.Errors != null)
                    {
                        errorMessage += $"\nChi tiết lỗi: {System.Text.Json.JsonSerializer.Serialize(response.Errors)}";
                    }
                    MessageBox.Show($"Lỗi khi cập nhật nhân viên: {errorMessage}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving employee changes: {ex.Message}");
                MessageBox.Show($"Lỗi khi lưu thay đổi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                if (response.Success && response.Data != null)
                {
                    foreach (var contract in response.Data)
                    {
                        if (!string.IsNullOrEmpty(contract.Img))
                        {
                            if (!Uri.TryCreate(contract.Img, UriKind.Absolute, out Uri uri))
                            {
                                if (Uri.TryCreate(new Uri("https://localhost:5105/"), contract.Img, out uri))
                                {
                                    contract.Img = uri.ToString();
                                }
                                else
                                {
                                    Debug.WriteLine($"Invalid URI for contract image: {contract.Img}");
                                    contract.Img = null;
                                }
                            }
                        }
                    }
                    ContractsDataGrid.ItemsSource = response.Data;
                    UpdateAddNewContractButtonState(response.Data);
                }
                else
                {
                    ContractsDataGrid.ItemsSource = null;
                    MessageBox.Show(response.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading contracts: {ex.Message}");
                MessageBox.Show($"Lỗi khi tải hợp đồng lao động: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateAddNewContractButtonState(System.Collections.Generic.List<EmployeeCLDto> contracts)
        {
            bool hasActiveContract = contracts.Any(c => c.Status == "Hiệu lực");
            AddNewContractButton.IsEnabled = !hasActiveContract;
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
                    MessageBox.Show(response.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private async void TerminateContractButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContractsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một hợp đồng để chấm dứt.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var selectedContract = ContractsDataGrid.SelectedItem as EmployeeCLDto;
            if (selectedContract.Status != "Hiệu lực")
            {
                MessageBox.Show("Hợp đồng đã chọn không ở trạng thái 'Hiệu lực', không thể chấm dứt.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show("Bạn có chắc chắn muốn chấm dứt hợp đồng này?", "Xác nhận chấm dứt", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    selectedContract.Status = "Không Hiệu lực";
                    selectedContract.EndDate = DateTime.Now;

                    var response = await _clService.UpdateAsync(selectedContract.Cid, selectedContract);
                    if (response.Success)
                    {
                        MessageBox.Show("Hợp đồng đã được chấm dứt thành công.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadContracts(); // Refresh the contracts list
                    }
                    else
                    {
                        MessageBox.Show($"Lỗi khi chấm dứt hợp đồng: {response.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi chấm dứt hợp đồng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddNewContractButton_Click(object sender, RoutedEventArgs e)
        {
            var addContractWindow = new AddContractWindow(_httpClientFactory, _eid);
            addContractWindow.ShowDialog();
            LoadContracts(); // Refresh contracts after adding a new one
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

        private void ContractImage_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image image && image.DataContext is EmployeeCLDto contract)
            {
                if (!string.IsNullOrEmpty(contract.Img))
                {
                    try
                    {
                        var enlargedImageWindow = new EnlargedImageWindow(contract.Img);
                        enlargedImageWindow.ShowDialog();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error opening enlarged image: {ex.Message}");
                        MessageBox.Show($"Lỗi khi mở ảnh lớn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void DegreeImage1_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var response = await _asService.GetByEidAsync(_eid);
                if (response.Success && response.Data != null && !string.IsNullOrEmpty(response.Data.DegreeImg1))
                {
                    var enlargedImageWindow = new EnlargedImageWindow(response.Data.DegreeImg1);
                    enlargedImageWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Không có ảnh để hiển thị.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening enlarged degree image 1: {ex.Message}");
                MessageBox.Show($"Lỗi khi mở ảnh bằng cấp 1: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DegreeImage2_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var response = await _asService.GetByEidAsync(_eid);
                if (response.Success && response.Data != null && !string.IsNullOrEmpty(response.Data.DegreeImg2))
                {
                    var enlargedImageWindow = new EnlargedImageWindow(response.Data.DegreeImg2);
                    enlargedImageWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Không có ảnh để hiển thị.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening enlarged degree image 2: {ex.Message}");
                MessageBox.Show($"Lỗi khi mở ảnh bằng cấp 2: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
