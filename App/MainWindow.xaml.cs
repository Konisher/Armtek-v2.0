using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace App
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string serverUrl = "http://localhost:5000/BusStop/GetBusStops";
        public MainWindow()
        {
            InitializeComponent();
        }
        private async void GetBusStopsButton_Click(object sender, RoutedEventArgs e)
        {
            string userQuery = textBox1.Text;

            using (HttpClient client = new HttpClient())
            {
                string apiUrl = $"{serverUrl}?userQuery={userQuery}";
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    // Обработка и отображение результатов, например, в TextBox или другом элементе управления
                    textBox2.Text = responseBody;
                }
                else
                {
                    MessageBox.Show("Ошибка при выполнении запроса");
                }
            }
        }
    }
}
