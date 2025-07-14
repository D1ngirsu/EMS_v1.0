
using System;
using System.Windows;
using System.ComponentModel;

namespace EMS_v1._0Client.Views.SalaryStaff
{
    public partial class UpdateSalaryWindow : Window, INotifyPropertyChanged
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly EmployeeSalaryService _salaryService;
        private readonly EmployeeSalaryListDto _employee;
        private decimal _salary;

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action SalaryUpdated;

        public string EmployeeName => _employee.EmployeeName;
        public decimal Salary
        {
            get => _salary;
            set
            {
                _salary = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Salary)));
            }
        }

        public UpdateSalaryWindow(IHttpClientFactory httpClientFactory, EmployeeSalaryListDto employee)
        {
            InitializeComponent();
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _salaryService = new EmployeeSalaryService("https://localhost:5105/", _httpClientFactory);
            _employee = employee;
            _salary = employee.Salary ?? 0;
            DataContext = this;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(SalaryTextBox.Text, out decimal newSalary) || newSalary < 0)
            {
                MessageBox.Show("Vui lòng nhập mức lương hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var salaryDto = new EmployeeSalaryDto
                {
                    Eid = _employee.EmployeeId,
                    Salary = newSalary
                };

                EmployeeSalaryResponse response;
                if (_employee.Salary == null)
                {
                    // Create new salary record if none exists
                    response = await _salaryService.CreateAsync(salaryDto);
                }
                else
                {
                    // Update existing salary record
                    response = await _salaryService.UpdateAsync(_employee.EmployeeId, salaryDto);
                }

                if (response.Success)
                {
                    MessageBox.Show("Cập nhật lương thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    SalaryUpdated?.Invoke();
                    Close();
                }
                else
                {
                    MessageBox.Show($"Lỗi khi cập nhật lương: {response.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
