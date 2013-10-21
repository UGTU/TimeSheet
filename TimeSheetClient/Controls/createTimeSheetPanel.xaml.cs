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


namespace TimeSheetClient
{
    public partial class createTimeSheetPanel : UserControl
    {
        public createTimeSheetPanel()
        {
            InitializeComponent();
        }

        private void dateBeginPeriod_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            dateEndPeriod.SelectedDate = dateBeginPeriod.SelectedDate.Value.AddMonths(1).AddDays(-1);
        }

        private void dateBeginPeriod_Loaded(object sender, RoutedEventArgs e)
        {
            var dateNow = DateTime.Now;
            dateBeginPeriod.SelectedDate = new DateTime(dateNow.Year,dateNow.Month,1);
            dateEndPeriod.SelectedDate = new DateTime(dateNow.Year, dateNow.Month, 1).AddMonths(1).AddDays(-1);
        }
    }
}
