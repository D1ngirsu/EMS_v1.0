using Microsoft.Win32;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace EMS_v1._0Client.Views.HR
{
    public partial class AddContractWindow : Window
    {
        private readonly EmployeeCLService _clService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly int _eid;
        private byte[] _contractImageData;
        private string _contractImageFileName;

        public AddContractWindow(IHttpClientFactory httpClientFactory, int eid)
        {
            InitializeComponent();
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _eid = eid;
            _clService = new EmployeeCLService("https://localhost:5105/", _httpClientFactory);
            SignDatePicker.SelectedDate = DateTime.Now;
        }

        private void ContractTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContractTypeComboBox.SelectedItem != null)
            {
                var selectedType = (ContractTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

                if (selectedType == "Vô thời hạn")
                {
                    EndDatePicker.IsEnabled = false;
                    EndDatePicker.SelectedDate = null;
                }
                else
                {
                    EndDatePicker.IsEnabled = true;
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

        private async void SaveContractButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (StartDatePicker.SelectedDate == null || ContractTypeComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin bắt buộc (Loại hợp đồng, Ngày bắt đầu).", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var contractType = (ContractTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

                var contract = new EmployeeCLDto
                {
                    Eid = _eid,
                    StartDate = StartDatePicker.SelectedDate.Value,
                    EndDate = null,
                    ExpectedEndDate = contractType == "Vô thời hạn" ? null : EndDatePicker.SelectedDate,
                    Status = "Hiệu lực",
                    Type = contractType,
                    EmployeeUser = "VTB",
                    SignDate = SignDatePicker.SelectedDate ?? DateTime.Now
                };

                var clResponse = await _clService.CreateAsync(contract, _contractImageData, _contractImageFileName);
                if (!clResponse.Success)
                {
                    MessageBox.Show($"Lỗi khi tạo hợp đồng: {clResponse.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBox.Show("Thêm hợp đồng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu hợp đồng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}