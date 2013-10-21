using System;
using System.Collections.Generic;
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
    public partial class AdministratorControl : UserControl
    {
        TimeSheetServiceReference.TimeSheetServiceClient client = new TimeSheetServiceClient();

        public AdministratorControl()
        {
            InitializeComponent();
            client.GetDepartmentsListCompleted += client_GetDepartmentsListCompleted;
        }

        public void Initialize()
        {
            DepartmentTreeView.Items.Clear();
            Approver1Control.Visibility = Visibility.Collapsed;
            Approver2Control.Visibility = Visibility.Collapsed;
            Approver3Control.Visibility = Visibility.Collapsed;
            SaveButton.Visibility = Visibility.Collapsed;
            this.Visibility=Visibility.Visible;
            client.GetDepartmentsListAsync();
            client.GetExeptionsDaysInYearCompleted += client_GetExeptionsDaysInYearCompleted;
            client.GetYearsForExeptionDaysCompleted += client_GetYearsForExeptionDaysCompleted;
            client.GetYearsForExeptionDaysAsync();
            client.GetWorkScheduleListCompleted += client_GetWorkScheduleListCompleted;
            client.GetWorkScheduleListAsync();
            client.DeleteExeptionsDayCompleted += client_DeleteExeptionsDayCompleted;
        }

        void client_DeleteExeptionsDayCompleted(object sender, DeleteExeptionsDayCompletedEventArgs e)
        {
            if (e.Result.Result)
            {
                var workSchedule = WorkScheduleComboBox.SelectedItem as DtoWorkShedule;
                if (YearComboBox.SelectedItem != null)
                {
                    var year = (int)YearComboBox.SelectedItem;
                    if (workSchedule != null)
                        client.GetExeptionsDaysInYearAsync(year, workSchedule.IdWorkShedule);
                }
            }
            else MessageBox.Show(e.Result.Message);
            //throw new NotImplementedException();
        }

        void client_GetWorkScheduleListCompleted(object sender, GetWorkScheduleListCompletedEventArgs e)
        {
            WorkScheduleComboBox.ItemsSource = e.Result;
            WorkScheduleComboBox.SelectedItem = e.Result.FirstOrDefault();
        }

        void client_GetYearsForExeptionDaysCompleted(object sender, GetYearsForExeptionDaysCompletedEventArgs e)
        {
            YearComboBox.ItemsSource = e.Result;
            YearComboBox.SelectedItem = e.Result.Max();
        }

        void client_GetExeptionsDaysInYearCompleted(object sender, GetExeptionsDaysInYearCompletedEventArgs e)
        {
            dtoExceptionDayDataGrid.ItemsSource = e.Result;
        }

        void client_GetDepartmentsListCompleted(object sender, GetDepartmentsListCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                TreeBuild(e.Result.ToArray());
                SerachAutoCompleteBox.ItemsSource = e.Result.Select(s => s.DepartmentSmallName).ToArray();
            }
            else
            {
                MessageBox.Show("Не удалось загрузить подразделения.");
            }
        }

        private void DepartmentTreeView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if (DepartmentTreeView.SelectedItem != null)
            //{
            //    var selelctedItem = DepartmentTreeView.SelectedItem as TreeViewItem;
            //    if (selelctedItem != null)
            //    {
            //        var dep = selelctedItem.Tag as DtoDepartment;
            //        if (dep != null)
            //        {
            //            Approver1Control.Initialize(1,dep.IdDepartment);
            //            Approver2Control.Initialize(2, dep.IdDepartment);
            //            Approver3Control.Initialize(3, dep.IdDepartment);
            //        }
            //    }
            //    SaveButton.Visibility = Visibility.Visible;
            //}
            LoadApproversFoeDepartment();
        }

        //=============Вспомогательные методы===================================

        void TreeBuild(DtoDepartment[] departments)
        {
            var d = departments.Where(w => w.IdDepartment == 1).Select(s => s).First();
            var tvi = new TreeViewItem();
            tvi.Header = d.DepartmentFullName;
            tvi.Tag = d;
            DepartmentTreeView.Items.Add(tvi);
            foreach (var dep in departments)
            {
                SerachNode(tvi, dep);
            }
        }

        void SerachNode(TreeViewItem t, DtoDepartment d)
        {

            if ((t.Tag as DtoDepartment).IdDepartment != d.IdManagerDepartment)
                foreach (var item in t.Items)
                {
                    SerachNode(item as TreeViewItem, d);
                }
            else
            {
                var tvi = new TreeViewItem();
                ToolTip tooltip = new ToolTip();
                tooltip.Content = d.DepartmentFullName;
                tvi.Header = d.DepartmentSmallName;
                tvi.Tag = d;
                tooltip.DataContext = tvi;
                ToolTipService.SetToolTip(tvi, tooltip);
                t.Items.Add(tvi);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Approver1Control.Save();
            Approver2Control.Save();
            Approver3Control.Save();
        }

        private void UserControl_Loaded_1(object sender, RoutedEventArgs e)
        {

            // Не загружайте свои данные во время разработки.
            // if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            // {
            // 	//Загрузите свои данные здесь и присвойте результат CollectionViewSource.
            // 	System.Windows.Data.CollectionViewSource myCollectionViewSource = (System.Windows.Data.CollectionViewSource)this.Resources["Resource Key for CollectionViewSource"];
            // 	myCollectionViewSource.Source = your data
            // }
        }

        private void YearComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetExeptionsDays();
        }

        private void WorkScheduleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetExeptionsDays();
        }

        public void GetExeptionsDays()
        {
            var workSchedule = WorkScheduleComboBox.SelectedItem as DtoWorkShedule;
            if (YearComboBox.SelectedItem != null)
            {
                var year = (int)YearComboBox.SelectedItem;
                if (workSchedule != null)
                    client.GetExeptionsDaysInYearAsync(year, workSchedule.IdWorkShedule);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var exceptionDay = dtoExceptionDayDataGrid.SelectedItem as DtoExceptionDay;
            if(exceptionDay!=null)
            {
                client.DeleteExeptionsDayAsync(exceptionDay.IdExceptionDay);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var workSchedule = WorkScheduleComboBox.SelectedItem as DtoWorkShedule;
            if (workSchedule != null)
            {
                var window = new ExceptionDayAddChildWindow(workSchedule);
                window.Closed += window_Closed;
                window.Show();
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var exceptionDay = dtoExceptionDayDataGrid.SelectedItem as DtoExceptionDay;
            if(exceptionDay!=null)
            {
                var window = new ExceptionDayAddChildWindow(exceptionDay);
                window.Closed += window_Closed;
                window.Show();
            }
        }

        void window_Closed(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            if ((bool) (sender as ExceptionDayAddChildWindow).DialogResult)
            {
                GetExeptionsDays();
            }
        }

        private void SerachAutoCompleteBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var str = e.AddedItems[0] as string;
                DeExpandedAll(DepartmentTreeView.Items.First() as TreeViewItem);
                SerachDep(DepartmentTreeView.Items.First() as TreeViewItem, str);
            }
            //if(e.AddedItems.Count == 1)
            //{
            //    var str = e.AddedItems[0] as string;
            //    DeExpandedAll(DepartmentTreeView.Items.First() as TreeViewItem);
            //    SerachDep(DepartmentTreeView.Items.First() as TreeViewItem, str,true);
            //}
        }

        void SerachDep(TreeViewItem t, string depName)
        {
            var dtoDepartment = t.Tag as DtoDepartment;
            if (dtoDepartment != null && dtoDepartment.DepartmentSmallName != depName)
                foreach (var item in t.Items)
                {
                    SerachDep(item as TreeViewItem, depName);
                }
            else
            {
                var parent = t.Parent as TreeViewItem;
                    t.Background = new SolidColorBrush(Colors.LightGray);
                while (parent != null)
                {
                    parent.IsExpanded = true;
                    parent = parent.Parent as TreeViewItem;
                }
                LoadApproversFoeDepartment();
            }
        }

        void LoadApproversFoeDepartment()
        {
            if (DepartmentTreeView.SelectedItem != null)
            {
                var selelctedItem = DepartmentTreeView.SelectedItem as TreeViewItem;
                if (selelctedItem != null)
                {
                    var dep = selelctedItem.Tag as DtoDepartment;
                    if (dep != null)
                    {
                        Approver1Control.Initialize(1, dep.IdDepartment);
                        Approver2Control.Initialize(2, dep.IdDepartment);
                        Approver3Control.Initialize(3, dep.IdDepartment);
                    }
                }
                SaveButton.Visibility = Visibility.Visible;
            }
        }

        void DeExpandedAll(TreeViewItem t)
        {
            t.IsExpanded = false;
            t.Background = null;
            foreach (TreeViewItem item in t.Items)
            {
                DeExpandedAll(item);
            }
        }
    }
}
