using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace EMS_v1._0Client.Views.HR
{
    public partial class AddEmployeeWindow : Window
    {
        private readonly EmployeeApiService _employeeService;
        private readonly EmployeeCLService _clService;
        private readonly EmployeeCDService _cdService;
        private readonly EmployeeASService _asService;
        private readonly EmployeeRelativesService _relativesService;
        private readonly OrganizationApiService _orgService;
        private readonly IHttpClientFactory _httpClientFactory;
        private byte[] _avatarImageData;
        private string _avatarImageFileName;
        private byte[] _contractImageData;
        private string _contractImageFileName;
        private byte[] _degreeImg1Data;
        private byte[] _degreeImg2Data;
        private List<EmployeeRelativesDto> _relatives = new List<EmployeeRelativesDto>();

        public AddEmployeeWindow(IHttpClientFactory httpClientFactory)
        {
            InitializeComponent();
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _employeeService = new EmployeeApiService("https://localhost:5105/", _httpClientFactory);
            _clService = new EmployeeCLService("https://localhost:5105/", _httpClientFactory);
            _cdService = new EmployeeCDService("https://localhost:5105/", _httpClientFactory);
            _asService = new EmployeeASService("https://localhost:5105/", _httpClientFactory);
            _relativesService = new EmployeeRelativesService("https://localhost:5105/", _httpClientFactory);
            _orgService = new OrganizationApiService("https://localhost:5105/", _httpClientFactory);
            LoadDepartmentsAndPositions();
            RelativesDataGrid.ItemsSource = _relatives;
            // Set default value for SignDatePicker
            SignDatePicker.SelectedDate = DateTime.Now;
        }

        private async void LoadDepartmentsAndPositions()
        {
            try
            {
                // Load departments
                var deptResponse = await _orgService.GetDepartmentsAsync();
                if (deptResponse.Success && deptResponse.Data != null)
                {
                    UnitComboBox.ItemsSource = deptResponse.Data;
                }
                else
                {
                    MessageBox.Show("Không thể tải danh sách phòng ban.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Load positions
                var posResponse = await _orgService.GetPositionsAsync();
                if (posResponse.Success && posResponse.Data != null)
                {
                    PositionComboBox.ItemsSource = posResponse.Data;
                }
                else
                {
                    MessageBox.Show("Không thể tải danh sách chức vụ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải phòng ban/chức vụ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Giả lập phương thức lấy tên người dùng hiện tại
        private string GetCurrentUserName()
        {
            return System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        }

        private async void SaveEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate required fields for employee
                if (string.IsNullOrWhiteSpace(NameTextBox.Text) || DoBDatePicker.SelectedDate == null ||
                    UnitComboBox.SelectedItem == null || PositionComboBox.SelectedItem == null ||
                    string.IsNullOrWhiteSpace(EmailTextBox.Text) || string.IsNullOrWhiteSpace(PhoneTextBox.Text) ||
                    string.IsNullOrWhiteSpace(GenderComboBox.Text) || string.IsNullOrWhiteSpace(BankNumberTextBox.Text) ||
                    string.IsNullOrWhiteSpace(BankTextBox.Text) || string.IsNullOrWhiteSpace(SourceComboBox.Text))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin bắt buộc (Họ tên, Ngày sinh, Phòng ban, Chức vụ, Email, Số điện thoại, Giới tính, Số tài khoản ngân hàng, Ngân hàng, Nguồn).", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create Employee
                var employee = new Employee
                {
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
                    Source = (SourceComboBox.SelectedItem as ComboBoxItem)?.Content.ToString()
                };

                var employeeResponse = await _employeeService.CreateEmployeeAsync(employee, _avatarImageData, _avatarImageFileName);

                if (!employeeResponse.Success || employeeResponse.Data == null)
                {
                    string errorMessage = employeeResponse.Message ?? "Không nhận được dữ liệu nhân viên từ server.";
                    if (employeeResponse.Errors != null)
                    {
                        errorMessage += $"\nChi tiết lỗi: {JsonSerializer.Serialize(employeeResponse.Errors)}";
                    }
                    MessageBox.Show($"Lỗi khi tạo nhân viên: {errorMessage}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int eid = employeeResponse.Data.Eid;

                // Create Labor Contract
                if (StartDatePicker.SelectedDate != null && EndDatePicker.SelectedDate != null && ContractStatusComboBox.SelectedItem != null)
                {
                    var contract = new EmployeeCLDto
                    {
                        Eid = eid,
                        StartDate = StartDatePicker.SelectedDate.Value,
                        EndDate = EndDatePicker.SelectedDate.Value,
                        Status = (ContractStatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                        EmployeeUser = "VTB",
                        SignDate = SignDatePicker.SelectedDate ?? DateTime.Now
                    };
                    var clResponse = await _clService.CreateAsync(contract, _contractImageData, _contractImageFileName);
                    if (!clResponse.Success)
                    {
                        MessageBox.Show($"Lỗi khi tạo hợp đồng: {clResponse.Errors}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                if (!string.IsNullOrWhiteSpace(IdNumberTextBox.Text))
                {
                    // Validate all required fields when creating CD
                    if (string.IsNullOrWhiteSpace(IssuePlaceTextBox.Text))
                    {
                        MessageBox.Show("Vui lòng điền Nơi cấp CCCD.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(CountryTextBox.Text))
                    {
                        MessageBox.Show("Vui lòng điền Quốc gia.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var cd = new EmployeeCDDto
                    {
                        Eid = eid,
                        IdNumber = IdNumberTextBox.Text,
                        IssueDay = CDIssueDayDatePicker.SelectedDate ?? DateTime.Now,
                        IssuePlace = IssuePlaceTextBox.Text,
                        Country = CountryTextBox.Text
                    };

                    var cdResponse = await _cdService.CreateAsync(cd);
                    if (!cdResponse.Success)
                    {
                        MessageBox.Show($"Lỗi khi tạo thông tin xác nhận: {cdResponse.Errors}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }


                // Create Academic Skills - Validate required fields
                if (!string.IsNullOrWhiteSpace(AcademicRankTextBox.Text) || !string.IsNullOrWhiteSpace(DegreeTextBox.Text) || !string.IsNullOrWhiteSpace(ASIssuePlaceTextBox.Text))
                {
                    if (string.IsNullOrWhiteSpace(AcademicRankTextBox.Text))
                    {
                        MessageBox.Show("Vui lòng điền Học hàm.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(DegreeTextBox.Text))
                    {
                        MessageBox.Show("Vui lòng điền Bằng cấp.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(ASIssuePlaceTextBox.Text))
                    {
                        MessageBox.Show("Vui lòng điền Nơi cấp.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var asData = new EmployeeASDto
                    {
                        Eid = eid,
                        AcademicRank = AcademicRankTextBox.Text,
                        Degree = DegreeTextBox.Text,
                        PlaceIssue = ASIssuePlaceTextBox.Text,
                        IssueDay = ASIssueDayDatePicker.SelectedDate ?? DateTime.Now
                    };
                    var asResponse = await _asService.CreateAsync(asData, _degreeImg1Data, _degreeImg2Data);
                    if (!asResponse.Success)
                    {
                        MessageBox.Show($"Lỗi khi tạo trình độ học vấn: {asResponse.Errors}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                // Create Relatives
                foreach (var relative in _relatives)
                {
                    relative.Eid = eid;
                    var relResponse = await _relativesService.CreateAsync(relative);
                    if (!relResponse.Success)
                    {
                        MessageBox.Show($"Lỗi khi tạo người thân: {relResponse.Errors}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                MessageBox.Show("Thêm nhân viên thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu nhân viên: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UploadAvatarButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _avatarImageData = File.ReadAllBytes(openFileDialog.FileName);
                    _avatarImageFileName = Path.GetFileName(openFileDialog.FileName);

                    if (_avatarImageData == null || _avatarImageData.Length == 0)
                    {
                        MessageBox.Show("Không thể đọc dữ liệu ảnh. Vui lòng chọn lại file.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    Console.WriteLine($"Avatar image loaded: {_avatarImageFileName}, Size: {_avatarImageData.Length} bytes");
                    EmployeeAvatar.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi đọc file ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UploadContractImageButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _contractImageData = File.ReadAllBytes(openFileDialog.FileName);
                    _contractImageFileName = Path.GetFileName(openFileDialog.FileName);

                    if (_contractImageData == null || _contractImageData.Length == 0)
                    {
                        MessageBox.Show("Không thể đọc dữ liệu ảnh hợp đồng. Vui lòng chọn lại file.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    Console.WriteLine($"Contract image loaded: {_contractImageFileName}, Size: {_contractImageData.Length} bytes");
                    ContractImage.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi đọc file ảnh hợp đồng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UploadDegreeImg1Button_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                _degreeImg1Data = File.ReadAllBytes(openFileDialog.FileName);
                DegreeImg1.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            }
        }

        private void UploadDegreeImg2Button_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                _degreeImg2Data = File.ReadAllBytes(openFileDialog.FileName);
                DegreeImg2.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            }
        }

        private void AddRelativeButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RelativeNameTextBox.Text) || string.IsNullOrWhiteSpace(RelativeRelationTextBox.Text))
            {
                MessageBox.Show("Vui lòng điền Tên và Quan hệ của người thân.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var relative = new EmployeeRelativesDto
            {
                
                RName = RelativeNameTextBox.Text,
                RRelativity = RelativeRelationTextBox.Text,
                RContact = RelativeContactTextBox.Text,
                Type = byte.Parse((RelativeTypeComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString() ?? "0")
            };

            _relatives.Add(relative);
            RelativesDataGrid.ItemsSource = null;
            RelativesDataGrid.ItemsSource = _relatives;

            // Clear input fields
            RelativeNameTextBox.Text = string.Empty;
            RelativeRelationTextBox.Text = string.Empty;
            RelativeContactTextBox.Text = string.Empty;
        }
    }
}