using System.Windows.Controls;
using System;
using System.Windows;
using Wpf.Ui.Controls;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Windows.Controls.Primitives;

namespace Armtek.Pages.Sales
{
    public partial class CustomerOrders : System.Windows.Controls.Page
    {
        public SolidColorBrush ProgressRingColor { get; set; }
        public CustomerOrders()
        {
            InitializeComponent();
            DataContext = this;
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
                    progressRing.Foreground = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    progressRing.Foreground = new SolidColorBrush(Colors.Red);
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


    }
}
