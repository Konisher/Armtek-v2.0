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

namespace Armtek.Pages.Sales
{
    public partial class CustomerOrders : System.Windows.Controls.Page
    {
        public CustomerOrders()
        {
            InitializeComponent();
            int selectedMonth = CustomCalendar.DisplayDate.Month;
            int selectedYear = CustomCalendar.DisplayDate.Year;

            var progressRings = FindVisualChildren<ProgressRing>(CustomCalendar);

            foreach (var progressRing in progressRings)
            {
                DateTime buttonDate = (DateTime)progressRing.DataContext;
                if (buttonDate.Month == selectedMonth && buttonDate.Year == selectedYear)
                {
                    /*progressRing.Foreground = new SolidColorBrush(Colors.Green);*/
                    progressRing.Visibility = Visibility.Visible;
                }
                else
                {
                    /*progressRing.Foreground = new SolidColorBrush(Colors.Red);*/
                    progressRing.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void Calendar_DisplayDateChanged(object sender, CalendarDateChangedEventArgs e)
        {
            
            int selectedMonth = CustomCalendar.DisplayDate.Month;
            int selectedYear = CustomCalendar.DisplayDate.Year;

            var progressRings = FindVisualChildren<ProgressRing>(CustomCalendar);

            foreach (var progressRing in progressRings)
            {
                DateTime buttonDate = (DateTime)progressRing.DataContext;
                if (buttonDate.Month == selectedMonth && buttonDate.Year == selectedYear)
                {
                    /*progressRing.Foreground = new SolidColorBrush(Colors.Green);*/
                    progressRing.Visibility = Visibility.Visible;
                }
                else
                {
                    /*progressRing.Foreground = new SolidColorBrush(Colors.Red);*/
                    progressRing.Visibility = Visibility.Collapsed;
                }
            }

        }
        private async void textbox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textbox1.Text.Length == 2 || textbox1.Text.Length == 5)
            {
                textbox1.Text += ".";
                textbox1.CaretIndex = textbox1.Text.Length;
            }
            else if (textbox1.Text.Length == 10)
            {
                if (DateTime.TryParseExact(textbox1.Text, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime inputDate))
                {
                    await UpdateProgressRingsAsync(inputDate);
                }
            }
        }

        private async Task UpdateProgressRingsAsync(DateTime inputDate)
        {
            var progressRings = FindVisualChildren<ProgressRing>(CustomCalendar);
            foreach (var progressRing in progressRings)
            {
                DateTime buttonDate = (DateTime)progressRing.Tag;
                if (buttonDate.Day == inputDate.Day && buttonDate.Month == inputDate.Month && buttonDate.Year == inputDate.Year)
                {
                    progressRing.Foreground = new SolidColorBrush(Colors.Green);

                    await Task.Run(() =>
                    {
                        for (int i = 100; i >= 0; i--)
                        {
                            // Обновляем значение Progress в UI потоке
                            Dispatcher.Invoke(() => progressRing.Progress = i);
                            Thread.Sleep(50);
                        }
                    });
                }
            }
        }



        private IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            var children = new List<T>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    children.Add(typedChild);
                }
                else
                {
                    children.AddRange(FindVisualChildren<T>(child));
                }
            }
            return children;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            infoBar1.Visibility = Visibility.Collapsed;
            int selectedMonth = CustomCalendar.DisplayDate.Month;
            int selectedYear = CustomCalendar.DisplayDate.Year;

            var progressRings = FindVisualChildren<ProgressRing>(CustomCalendar);

            foreach (var progressRing in progressRings)
            {
                DateTime buttonDate = (DateTime)progressRing.DataContext;
                progressRing.Tag = buttonDate;
                if (buttonDate.Month == selectedMonth && buttonDate.Year == selectedYear)
                {
                    /*progressRing.Foreground = new SolidColorBrush(Colors.Green);*/
                    progressRing.Visibility = Visibility.Visible;
                }
                else
                {
                    /*progressRing.Foreground = new SolidColorBrush(Colors.Red);*/
                    progressRing.Visibility = Visibility.Collapsed;
                }
            }
        }


        

        private void CalendarDayButton_MouseEnter(object sender, MouseEventArgs e)
        {
            infoBar1.Visibility = Visibility.Visible;
            if (sender is CalendarDayButton calendarDayButton)
            {
                DateTime buttonDate = (DateTime)calendarDayButton.DataContext;
                infoBar1.Message = buttonDate.ToString("dd.MM.yyyy");
            }
        }

        private void CalendarDayButton_MouseLeave(object sender, MouseEventArgs e)
        {
            infoBar1.Message = string.Empty;
            infoBar1.Visibility = Visibility.Collapsed;
        }

    }
}
