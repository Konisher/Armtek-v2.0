using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Armtek.Pages.Sales
{
    /// <summary>
    /// Логика взаимодействия для CustomerOrders.xaml
    /// </summary>
    public partial class CustomerOrders : System.Windows.Controls.Page
    {

        public CustomerOrders()
        {
            InitializeComponent();
        }
        private void Calendar_DisplayDateChanged(object sender, CalendarDateChangedEventArgs e)
        {
            DateTime selectedMonth = CustomCalendar.DisplayDate;
            DateTime firstDayOfMonth = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            foreach (var dayButton in FindVisualChildren<CalendarDayButton>(CustomCalendar)) // You can replace FindVisualChildren with your own implementation
            {
                DateTime day = (DateTime)dayButton.DataContext;
                if (day < firstDayOfMonth || day > lastDayOfMonth)
                {
                    ProgressRing progressRing = FindVisualChild<ProgressRing>(dayButton); // You can replace FindVisualChild with your own implementation
                    progressRing.Visibility = Visibility.Collapsed;
                }
            }
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            T child = null;
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                DependencyObject visual = VisualTreeHelper.GetChild(parent, i);
                child = visual as T;
                if (child == null)
                {
                    child = FindVisualChild<T>(visual);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                    if (child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T descendant in FindVisualChildren<T>(child))
                    {
                        yield return descendant;
                    }
                }
            }
        }
    }
}
