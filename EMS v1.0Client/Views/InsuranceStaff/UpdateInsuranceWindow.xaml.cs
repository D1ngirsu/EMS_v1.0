using System;
using System.Windows;
using System.ComponentModel;

namespace EMS_v1._0Client.Views.InsuranceStaff
{
    public partial class UpdateInsuranceWindow : Window, INotifyPropertyChanged
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly EmployeeInsuranceService _insuranceService;
        private readonly EmployeeInsuranceListDto _insurance;
        private string _insuranceContent;
        private decimal _contributePercent;
        private DateTime _fromDate;
        private DateTime _toDate;

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action InsuranceUpdated;

        public string EmployeeName => _insurance.EmployeeName;
        public string InsuranceContent
        {
            get => _insuranceContent;
            set
            {
                _insuranceContent = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InsuranceContent)));
            }
        }
        public decimal ContributePercent
        {
            get => _contributePercent;
            set
            {
                _contributePercent = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContributePercent)));
            }
        }
        public DateTime FromDate
        {
            get => _fromDate;
            set
            {
                _fromDate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FromDate)));
            }
        }
        public DateTime ToDate
        {
            get => _toDate;
            set
            {
                _toDate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToDate)));
            }
        }

        public UpdateInsuranceWindow(IHttpClientFactory httpClientFactory, EmployeeInsuranceListDto insurance)
        {
            InitializeComponent();
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _insuranceService = new EmployeeInsuranceService("https://localhost:5105/", _httpClientFactory);
            _insurance = insurance;
            _insuranceContent = insurance.InsuranceContent;
            // Chuyển đổi từ decimal thành phần trăm để hiển thị (ví dụ: 0.05 thành 5)
            _contributePercent = (insurance.ContributePercent ?? 0) * 100;
            _fromDate = insurance.FromDate ?? DateTime.Now;
            _toDate = insurance.ToDate ?? DateTime.Now;
            DataContext = this;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(InsuranceContentTextBox.Text))
            {
                MessageBox.Show("Vui lòng nhập nội dung bảo hiểm.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!decimal.TryParse(ContributePercentTextBox.Text, out decimal contributePercent) || contributePercent < 0 || contributePercent > 100)
            {
                MessageBox.Show("Vui lòng nhập tỷ lệ đóng góp hợp lệ (0-100%).", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (FromDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Vui lòng chọn ngày bắt đầu.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (ToDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Vui lòng chọn ngày kết thúc.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var insuranceDto = new EmployeeInsuranceDto
                {
                    Iid = _insurance.Iid ?? 0,
                    Eid = _insurance.Eid,
                    InsuranceContent = InsuranceContent,
                    // Chuyển đổi từ phần trăm về decimal để lưu vào database (ví dụ: 5 thành 0.05)
                    ContributePercent = contributePercent / 100,
                    FromDate = FromDatePicker.SelectedDate.Value,
                    ToDate = ToDatePicker.SelectedDate.Value
                };

                EmployeeInsuranceResponse response;
                if (_insurance.Iid == null || _insurance.Iid == 0)
                {
                    // Create new insurance record if none exists
                    response = await _insuranceService.CreateAsync(insuranceDto);
                }
                else
                {
                    // Update existing insurance record
                    response = await _insuranceService.UpdateAsync(_insurance.Iid.Value, insuranceDto);
                }

                if (response.Success)
                {
                    MessageBox.Show("Cập nhật bảo hiểm thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    InsuranceUpdated?.Invoke();
                    Close();
                }
                else
                {
                    MessageBox.Show($"Lỗi khi cập nhật bảo hiểm: {response.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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