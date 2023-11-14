using System.Windows.Controls;
using System;
using System.Windows;
using Wpf.Ui.Controls;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Text.RegularExpressions;
using Windows.UI.ViewManagement;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace Armtek.Pages.Sales
{
    public partial class CustomerOrders : System.Windows.Controls.Page
    {
        public CustomerOrders()
        {
            InitializeComponent();
            string currentDirectory = Directory.GetCurrentDirectory();
            string pathToHtmlFile = Path.Combine(currentDirectory, "index.html");

            if (File.Exists(pathToHtmlFile))
            {
                webYandexAPI.Navigate(new Uri(pathToHtmlFile));
            }
            else
            {
                System.Windows.MessageBox.Show("Файл не найден!");
            }
        }

    }
}