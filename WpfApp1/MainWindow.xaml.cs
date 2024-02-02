using FireSharp.Config;
using FireSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "lapCwsSnX3ryCpdoyMGiibToL1V7aWSiCoiljEIS",
            BasePath= "https://console.firebase.google.com/u/0/project/armtek-2a002/database/armtek-2a002-default-rtdb/data/~2F"
        };

        IFirebaseClient client;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            client = new FireSharp.FirebaseClient(config);

            if (client!=null)
            {
                MessageBox.Show("+");
            }
        }
    }
}
