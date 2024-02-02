using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Windows.Threading;
using Wpf.Ui.Common;

namespace Armtek.Pages
{
    /// <summary>
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : System.Windows.Controls.Page
    {
        DispatcherTimer timer;
        public event EventHandler AuthenticationSuccess;

        private void OnAuthenticationSuccess()
        {
            AuthenticationSuccess?.Invoke(this, EventArgs.Empty);
        }
        public Authorization()
        {
            InitializeComponent();
        }
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TbAccount.Text))
            {
                if (IsValidUserName(TbAccount.Text) || IsValidEmail(TbAccount.Text) || IsValidPhoneNumber(TbAccount.Text))
                {
                    timer = new DispatcherTimer()
                    {
                        Interval = new TimeSpan(0, 0, 5)
                    };
                    timer.Tick += Timer_Tick;
                    timer.Start();
                    progressRing.IsActive = true;
                    BtnLogin.IsEnabled = false;
                    snackBarSignIn.Title = "Information";
                    snackBarSignIn.Message = "Login successful, Welcome!";
                    snackBarSignIn.Appearance = ControlAppearance.Success;
                    snackBarSignIn.Show();
                    byte[] photo = null;
                    MainWindow.HubProxy.Invoke("Login", TbAccount.Text);
                }
                else
                {
                    snackBarSignIn.Title = "Error";
                    snackBarSignIn.Message = "Invalid input. Please enter a valid UserName, email, or phone number.";
                    snackBarSignIn.Appearance = ControlAppearance.Danger;
                    snackBarSignIn.Show();
                }
            }
            else
            {
                snackBarSignIn.Title = "Error";
                snackBarSignIn.Message = "Invalid input. Please enter a valid UserName, email, or phone number.";
                snackBarSignIn.Appearance = ControlAppearance.Danger;
                snackBarSignIn.Show();
            }

        }


        private void Timer_Tick(object sender, object e)
        {
            timer.Stop();
            progressRing.IsActive = false;
            BtnLogin.IsEnabled = true;
            TbAccount.Clear();
            PwdPassword.Clear();
            OnAuthenticationSuccess();

        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {

        }

        private void TbAccount_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
        private bool IsValidUserName(string input)
        {
            string pattern = @"^[a-zA-Z0-9_-]{3,16}$";
            return Regex.IsMatch(input, pattern);
        }

        private bool IsValidEmail(string input)
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(input, pattern);
        }

        private bool IsValidPhoneNumber(string input)
        {
            string pattern = @"^\+\d{1,3}-\d{3,}-\d{3,}-\d{2,}$";
            return Regex.IsMatch(input, pattern);
        }

        private void NoAccount_Click(object sender, RoutedEventArgs e)
        {
            Registration registrationPage = new Registration();
            Window mainWindow = Application.Current.MainWindow;
            if (mainWindow is MainWindow mainWin)
            {
                mainWin.mainFrame.Navigate(registrationPage);
            }
        }
    }
}
