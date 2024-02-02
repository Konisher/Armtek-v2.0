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
using Armtek.Pages;
using static System.Collections.Specialized.BitVector32;
using Armtek.Pages.DefaultWindows;

namespace Armtek
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static HubConnection Connection { get; private set; }
        public static IHubProxy HubProxy { get; private set; }
        private Dictionary<Type, NavigationViewItem> pageToMenuItemMap;
        private Dictionary<string, Type> menuItemToPageTypeMap;
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
            if (menuItemToPageTypeMap.TryGetValue(args.InvokedItem.ToString(), out Type pageType))
            {
                if (!(mainFrame.Content is System.Windows.Controls.Page currentPage) || currentPage.GetType() != pageType)
                {
                    System.Windows.Controls.Page newPage = (System.Windows.Controls.Page)Activator.CreateInstance(pageType);
                    if (mainFrame.Navigate(newPage))
                    {
                        navigationView.SelectedItem = args.InvokedItem;
                    }
                }
            }
            string name = args.InvokedItem.ToString();
            HubProxy.Invoke("RecordUserAction", name);
        }

        private void mainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (pageToMenuItemMap.ContainsKey(e.Content.GetType()))
            {
                navigationView.SelectedItem = pageToMenuItemMap[e.Content.GetType()];
                if (e.Content is Pages.Authorization authorizationPage)
                {
                    authorizationPage.AuthenticationSuccess += AuthorizationPage_AuthenticationSuccess;
                }
            }
        }

        private void AuthorizationPage_AuthenticationSuccess(object sender, EventArgs e)
        {
            AuthorizationMenu.Visibility = Visibility.Collapsed;
            RegistrationMenu.Visibility = Visibility.Collapsed;
            foreach(var item in navigationView.MenuItems)
            {
                if(item is NavigationViewItemSeparator itemSeparator)
                {
                    itemSeparator.Visibility = Visibility.Visible;
                }
            }
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
