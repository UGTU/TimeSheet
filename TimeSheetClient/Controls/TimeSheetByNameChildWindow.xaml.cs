using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using TimeSheetClient.TimeSheetServiceReference;

namespace TimeSheetClient.Controls
{
    public partial class TimeSheetByNameChildWindow : ChildWindow
    {
        readonly TimeSheetServiceReference.TimeSheetServiceClient _client = new TimeSheetServiceClient();
        private readonly DtoTimeSheet _timeSheet;

        public TimeSheetByNameChildWindow(DtoTimeSheet timeSheet)
        {
            InitializeComponent();
            employeesDataGrid.ItemsSource = timeSheet.Employees.OrderByDescending(o => o.FactStaffEmployee.Post.Category.IsPPS)
                .ThenBy(o => o.FactStaffEmployee.Post.Category.OrderBy).ThenBy(o => o.FactStaffEmployee.Surname);
            _timeSheet = timeSheet;
            _client.TimeSheetByNameCompleted += _client_TimeSheetByNameCompleted;
        }

        void _client_TimeSheetByNameCompleted(object sender, TimeSheetByNameCompletedEventArgs e)
        {
            //if (e.Result.Result)
            //{
            //    System.Windows.Browser.HtmlPage.Window.Navigate(new Uri(string.Format("../home/TimeSheetPdfByName?idTimeSheet={0}", _timeSheet.IdTimeSheet), UriKind.Relative), "_blank");
            //    this.DialogResult = true;
            //}
            //else
            //{
            //    MessageBox.Show(e.Result.Message);
            //}
            //MessageBox.Show(e.Result.ToString());
            if (e.Error == null && e.Result.Result)
            {
                //System.Windows.Browser.HtmlPage.Window.Navigate(
                //    new Uri(string.Format("../home/TimeSheetPdfByName?idTimeSheet={0}", _timeSheet.IdTimeSheet),
                //        UriKind.Relative), "_blank");
                System.Windows.Browser.HtmlPage.Window.Navigate(
                    new Uri(string.Format("../home/TimeSheetPdfByName?idTimeSheet={0}", _timeSheet.IdTimeSheet),
                        UriKind.Relative));
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Произошла ошибка");
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (_timeSheet.Employees.Any(a => a.IsChecked))
            {
                //_client.TimeSheetByNameAsync(_timeSheet);
                _client.TimeSheetByNameAsync(_timeSheet);
            }
            else
            {
                MessageBox.Show("Не выбрано ни одного сотрудника для включения в индивидуальный табель.");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            var empl = _timeSheet.Employees;
            foreach (var employee in empl)
            {
                employee.IsChecked = true;
            }
            employeesDataGrid.ItemsSource = empl.OrderByDescending(o => o.FactStaffEmployee.Post.Category.IsPPS)
               .ThenBy(o => o.FactStaffEmployee.Post.Category.OrderBy).ThenBy(o => o.FactStaffEmployee.Surname);
        }

        private void UnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            var empl = _timeSheet.Employees;
            foreach (var employee in empl)
            {
                employee.IsChecked = false;
            }
            employeesDataGrid.ItemsSource = empl.OrderByDescending(o => o.FactStaffEmployee.Post.Category.IsPPS)
               .ThenBy(o => o.FactStaffEmployee.Post.Category.OrderBy).ThenBy(o => o.FactStaffEmployee.Surname);
        }


    }
}

