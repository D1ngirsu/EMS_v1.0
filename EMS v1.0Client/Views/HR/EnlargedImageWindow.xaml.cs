using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Imaging;

namespace EMS_v1._0Client.Views.HR
{
    public partial class EnlargedImageWindow : Window
    {
        public EnlargedImageWindow(string imageUrl)
        {
            InitializeComponent();
            LoadImage(imageUrl);
        }

        private void LoadImage(string imageUrl)
        {
            try
            {
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    if (Uri.TryCreate(imageUrl, UriKind.Absolute, out Uri uri))
                    {
                        EnlargedImage.Source = new BitmapImage(uri);
                    }
                    else if (Uri.TryCreate(new Uri("https://localhost:5105/"), imageUrl, out uri))
                    {
                        EnlargedImage.Source = new BitmapImage(uri);
                    }
                    else
                    {
                        Debug.WriteLine($"Invalid URI for enlarged image: {imageUrl}");
                        EnlargedImage.Source = null;
                        MessageBox.Show("Không thể tải ảnh.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    EnlargedImage.Source = null;
                    MessageBox.Show("Không có ảnh để hiển thị.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading enlarged image: {ex.Message}");
                MessageBox.Show($"Lỗi khi tải ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}