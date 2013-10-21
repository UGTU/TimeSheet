using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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

//using TimeSheetClient.TimeSheetService;

namespace TimeSheetClient
{
    public partial class CreateTimeSheet : ChildWindow
    {
        readonly TimeSheetServiceReference.TimeSheetServiceClient _client = new TimeSheetServiceClient();
        private readonly int _idDepartment;
        private string _employeeLogin;
        private DateTime dateStartTime;

        private DateTime dateEndTime
        {
            get { return dateStartTime.AddMonths(1).AddDays(-1); }
        }

        public CreateTimeSheet(int idDepartment, string employeeLogin)
        {
            InitializeComponent();
            dateStartTime = new DateTime(DateTime.Now.Year,DateTime.Now.Month,1);  
            _employeeLogin = employeeLogin;
            _idDepartment = idDepartment;
            _client.CreateTimeSheetCompleted += _client_CreateTimeSheetCompleted;
            _client.CreateTimeSheetByNameCompleted += _client_CreateTimeSheetByNameCompleted;
            _client.GetEmployeesForTimeSheetCompleted += _client_GetEmployeesForTimeSheetCompleted;
            
            MounthLabelRefrash();
        }



        void _client_GetEmployeesForTimeSheetCompleted(object sender, GetEmployeesForTimeSheetCompletedEventArgs e)
        {
            dtoFactStaffEmployeeDataGrid.ItemsSource = null;
            if (e.Error == null)
            {
                foreach (var res in e.Result)
                {
                    res.IsCheked = true;
                }
                dtoFactStaffEmployeeDataGrid.ItemsSource = e.Result.OrderByDescending(o => o.Post.IsMenager).
                        ThenBy(t => t.Post.Category.OrderBy).
                        ThenBy(o => o.Surname).
                        ToArray(); 
            }
        }


        void _client_CreateTimeSheetCompleted(object sender, CreateTimeSheetCompletedEventArgs e)
        {
            progressBar1.Visibility = Visibility.Collapsed;
            progressBar1.IsIndeterminate = false;
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
                this.DialogResult = false;
            }
            if (e.Result.Result)
            {
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show(e.Result.Message??"Произошла ошибка");
                this.DialogResult = false;
            }
        }

        void _client_CreateTimeSheetByNameCompleted(object sender, CreateTimeSheetByNameCompletedEventArgs e)
        {
            progressBar1.Visibility = Visibility.Collapsed;
            progressBar1.IsIndeterminate = false;
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
                this.DialogResult = false;
            }
            if (e.Result.Result)
            {
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show(e.Result.Message ?? "Произошла ошибка");
                this.DialogResult = false;
            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var employees = dtoFactStaffEmployeeDataGrid.ItemsSource as DtoFactStaffEmployee[];
                if (employees!=null && employees.Any(a => !a.IsCheked))
                {
                    var col = new ObservableCollection<DtoFactStaffEmployee>();
                    foreach (var dtoFactStaffEmployee in employees.Where(w=>w.IsCheked))
                    {
                        col.Add(dtoFactStaffEmployee);
                    }
                    _client.CreateTimeSheetByNameAsync(_idDepartment, dateStartTime, dateEndTime, _employeeLogin,col);
                }
                else
                {
                    _client.CreateTimeSheetAsync(_idDepartment, dateStartTime, dateEndTime, _employeeLogin);
                }
                progressBar1.Visibility = Visibility.Visible;
                progressBar1.IsIndeterminate = true;
                button1.IsEnabled = false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            createTimeSheetPanel1.Visibility = Visibility.Visible;
            button2.Visibility = Visibility.Visible;
            button1.Visibility = Visibility.Collapsed;
        }

        private void checkBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            createTimeSheetPanel1.Visibility = Visibility.Collapsed;
            button2.Visibility = Visibility.Collapsed;
            button1.Visibility = Visibility.Visible;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //client.CreateTimeSheetAsync(createTimeSheetPanel1.dateBeginPeriod.SelectedDate.Value, createTimeSheetPanel1.dateEndPeriod.SelectedDate.Value);
                _client.CreateTimeSheetAsync(_idDepartment, createTimeSheetPanel1.dateBeginPeriod.SelectedDate.Value, createTimeSheetPanel1.dateEndPeriod.SelectedDate.Value,_employeeLogin);
                progressBar1.Visibility = Visibility.Visible;
                progressBar1.IsIndeterminate = true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void MounthLabelRefrash()
        {
            mounthLabel.Content = string.Format("Табель на {0} {1} года", dateStartTime.ToString("MMMM"),
                dateStartTime.Year);
            dtoFactStaffEmployeeDataGrid.ItemsSource = null;
            _client.GetEmployeesForTimeSheetAsync(_idDepartment, _employeeLogin, dateStartTime, dateEndTime);
            AddMounth.Content = dateStartTime.AddMonths(-1).ToString("MMMM");
            RobMounth.Content = dateStartTime.AddMonths(1).ToString("MMMM");
        }

        private void RobMounth_Click(object sender, RoutedEventArgs e)
        {
            dateStartTime=dateStartTime.AddMonths(1);
            MounthLabelRefrash();
        }

        private void AddMounth_Click(object sender, RoutedEventArgs e)
        {
            dateStartTime=dateStartTime.AddMonths(-1);
            MounthLabelRefrash();
        }

        private void ClearView_Click(object sender, RoutedEventArgs e)
        {
            var employees = dtoFactStaffEmployeeDataGrid.ItemsSource as DtoFactStaffEmployee[];
            foreach (var dtoFactStaffEmployee in employees)
            {
                dtoFactStaffEmployee.IsCheked = false;
            }
            dtoFactStaffEmployeeDataGrid.ItemsSource = employees;
        }

        private void CheckAll_Click(object sender, RoutedEventArgs e)
        {
            var employees = dtoFactStaffEmployeeDataGrid.ItemsSource as DtoFactStaffEmployee[];
            foreach (var dtoFactStaffEmployee in employees)
            {
                dtoFactStaffEmployee.IsCheked = true;
            }
            dtoFactStaffEmployeeDataGrid.ItemsSource = employees;

        }
    }
}

