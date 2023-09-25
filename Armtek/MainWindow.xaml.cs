using Armtek.Properties;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.AspNet.SignalR.Client;
using Armtek.Pages.Sales;
using Armtek.Pages;

namespace Armtek
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static HubConnection Connection { get; private set; }
        public static IHubProxy HubProxy { get; private set; }
        public MainWindow()
        {
            InitializeComponent();
            InitializeSignalR();
        }
        private void InitializeSignalR()
        {
            string serverUrl = "http://localhost:8080/signalchat";
            Connection = new HubConnection(serverUrl);
            HubProxy = Connection.CreateHubProxy("ChatHub");
            try
            {
                Connection.Start().Wait();
                Connection.Closed += () =>
                {
                    MessageBox.Show("Disconnected from the server.");
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to the server: {ex.Message}");
            }
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!CheckConnected())
            {
                this.DefaultGrid.Effect = (BlurEffect)this.Resources["BlurEffect"];

                this.DefaultGrid.IsHitTestVisible = false;

                await Task.Delay(100);

                EnternetMessage.Title = "Information";
                EnternetMessage.IsPrimaryButtonEnabled = true;
                EnternetMessage.PrimaryButtonText = "OK";
                EnternetMessage.DefaultButton = ContentDialogButton.Primary;
                var selectButton = await EnternetMessage.ShowAsync();
                switch (selectButton)
                {
                    case ModernWpf.Controls.ContentDialogResult.Primary:
                        if ((bool)waitingConntected.IsChecked)
                        {
                            loadingEnternet.Visibility = Visibility.Visible;
                            loadingEnternet.IsActive = true;
                            while (true)
                            {
                                if (CheckConnected())
                                {
                                    break;
                                }
                                await Task.Delay(5000);
                            }
                        }
                        else
                        {
                            Application.Current.Shutdown();
                        }
                        break;
                    default:
                        break;
                }
                this.DefaultGrid.Effect = (BlurEffect)this.Resources["BlurEffect"];
                loadingEnternet.IsActive = false;
                this.DefaultGrid.IsHitTestVisible = true;
                this.DefaultGrid.Effect = null;
            }
        }

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItem.Equals("Authorization") && !(mainFrame.Content is Pages.Authorization))
            {
                Pages.Authorization authorizationPage = new Pages.Authorization();
                if (mainFrame.Navigate(authorizationPage))
                {
                    navigationView.SelectedItem = AuthorizationMenu;
                }
            }
            else if (args.InvokedItem.Equals("Registration") && !(mainFrame.Content is Registration))
            {
                Registration registrationPage = new Registration();
                if (mainFrame.Navigate(registrationPage))
                {
                    navigationView.SelectedItem = RegistrationMenu;
                }
            }
            else if (args.InvokedItem.Equals("Заказы клиентов"))
            {
                CustomerOrders defaultWindowPage = new CustomerOrders();
                if (mainFrame.Navigate(defaultWindowPage))
                {
                    navigationView.SelectedItem = CustomerOrdersMenu;
                }
            }
            else if (args.InvokedItem.Equals("Договоры с клиентами"))
            {
                AgreementsWithClients defaultWindowPage = new AgreementsWithClients();
                if (mainFrame.Navigate(defaultWindowPage))
                {
                    navigationView.SelectedItem = AgreementsWithClientsMenu;
                }
            }
            else if (args.InvokedItem.Equals("Клиенты"))
            {
                Clients defaultWindowPage = new Clients();
                if (mainFrame.Navigate(defaultWindowPage))
                {
                    navigationView.SelectedItem = ClientsMenu;
                }
            }
            else if (args.InvokedItem.Equals("Заказы клиентов"))
            {
                CustomerOrders defaultWindowPage = new CustomerOrders();
                if (mainFrame.Navigate(defaultWindowPage))
                {
                    navigationView.SelectedItem = CustomerOrdersMenu;
                }
            }
            else if (args.InvokedItem.Equals("Документы продажи"))
            {
                SalesDocuments defaultWindowPage = new SalesDocuments();
                if (mainFrame.Navigate(defaultWindowPage))
                {
                    navigationView.SelectedItem = SalesDocumentsMenu;
                }
            }
            else if (args.InvokedItem.Equals("Отчёт по продажам"))
            {
                SalesReport defaultWindowPage = new SalesReport();
                if (mainFrame.Navigate(defaultWindowPage))
                {
                    navigationView.SelectedItem = SalesReportMenu;
                }
            }
            else if (args.InvokedItem.Equals("Помощник продаж"))
            {
                SalesAssistant defaultWindowPage = new SalesAssistant();
                if (mainFrame.Navigate(defaultWindowPage))
                {
                    navigationView.SelectedItem = SalesAssistantMenu;
                }
            }
            else if (args.InvokedItem.Equals("Settings"))
            {
                Pages.Settings settingsPage = new Pages.Settings();
                if (mainFrame.Navigate(settingsPage))
                {
                    navigationView.SelectedItem = SettingsMenu;
                }
            }
        }



        private void mainFrame_Navigated(object sender, NavigationEventArgs e)
        {

            if (e.Content is Pages.Authorization)
            {
                var authorizationPage = e.Content as Pages.Authorization;
                if (authorizationPage != null)
                {
                    authorizationPage.AuthenticationSuccess += AuthorizationPage_AuthenticationSuccess;
                }
                navigationView.SelectedItem = AuthorizationMenu;
            }
            else if (e.Content is Registration)
            {
                navigationView.SelectedItem = RegistrationMenu;
            }
            else if (e.Content is CustomerOrders)
            {
                navigationView.SelectedItem = CustomerOrdersMenu;
            }
            else if (e.Content is AgreementsWithClients)
            {
                navigationView.SelectedItem = AgreementsWithClientsMenu;
            }
            else if (e.Content is CustomerOrders)
            {
                navigationView.SelectedItem = CustomerOrdersMenu;

            }
            else if (e.Content is SalesDocuments)
            {
                navigationView.SelectedItem = SalesDocumentsMenu;
            }
            else if (e.Content is SalesReport)
            {
                navigationView.SelectedItem = SalesReportMenu;
            }
            else if (e.Content is SalesAssistant)
            {
                navigationView.SelectedItem = SalesAssistantMenu;
            }
            else if (e.Content is Pages.Settings)
            {
                navigationView.SelectedItem = SettingsMenu;
            }
        }

        private void AuthorizationPage_AuthenticationSuccess(object sender, EventArgs e)
        {
            SalesMenu.Visibility = Visibility.Visible;
            PurchasesMenu.Visibility = Visibility.Visible;
            ExchequerMenu.Visibility = Visibility.Visible;
            AuthorizationMenu.Visibility = Visibility.Collapsed;
            RegistrationMenu.Visibility = Visibility.Collapsed;
        }
        private bool CheckConnected()
        {
            var ping = new Ping();
            var reply = ping.Send("www.google.com");
            return reply.Status == IPStatus.Success;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Connection != null && Connection.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected)
            {
                Connection.Stop();
                Connection.Dispose();
            }
        }
    }
}
