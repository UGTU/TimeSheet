using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml.Linq;
using TimeSheetClient.TimeSheetServiceReference;


namespace TimeSheetClient.Controls
{
    public partial class TimeSheetControl : UserControl
    {
        TimeSheetServiceReference.TimeSheetServiceClient _client = new TimeSheetServiceClient();
        private DtoTimeSheet _timeSheet;

        public TimeSheetControl()
        {
            InitializeComponent();
            _client.GetTimeSheetApproveHistoryCompleted += _client_GetTimeSheetApproveHistoryCompleted;
            _client.GetTimeSheetApproveStepCompleted += _client_GetTimeSheetApproveStepCompleted;
            _client.RefreshTimeSheetCompleted += _client_RefreshTimeSheetCompleted;
            ApprovedBorder.Visibility = Visibility.Collapsed;
        }

        void _client_RefreshTimeSheetCompleted(object sender, RefreshTimeSheetCompletedEventArgs e)
        {
            if(!e.Result.Result)
                MessageBox.Show(e.Result.Message);
            RefrashImage.Visibility=Visibility.Visible;
            //throw new NotImplementedException();
        }

        void _client_GetTimeSheetApproveStepCompleted(object sender, GetTimeSheetApproveStepCompletedEventArgs e)
        {
            if(e.Result==3)
            {
                ApprovedBorder.Visibility = Visibility.Visible;
            }
            //MessageBox.Show(e.Result.ToString());
            //if (e.Error != null)
            //    MessageBox.Show(e.Error.Message);
            ////throw new NotImplementedException();
        }

        void _client_GetTimeSheetApproveHistoryCompleted(object sender, GetTimeSheetApproveHistoryCompletedEventArgs e)
        {
            dtoTimeSheetApproveHistiryDataGrid.ItemsSource = e.Result;
            //throw new NotImplementedException();
        }

        public void Initialize(DtoTimeSheet timeSheet)
        {
            ApprovedBorder.Visibility = Visibility.Collapsed;
            this.Visibility=Visibility.Visible;
            DepartmentNameTextBox.Text = timeSheet.Department.DepartmentFullName;
            TimeSheetNumberTextBlock.Text = timeSheet.IdTimeSheet.ToString();
            DateCompositionTextBlock.Text = timeSheet.DateComposition.ToShortDateString();
            DateBeginTextBlock.Text = timeSheet.DateBegin.ToShortDateString();
            DateEndTextBlock.Text = timeSheet.DateEnd.ToShortDateString();
            TimeSheetShowHyperlinkButton.NavigateUri = new Uri(string.Format("/../Home/TimeSheetShow?idTimeSheet={0}", timeSheet.IdTimeSheet), UriKind.RelativeOrAbsolute);
            TimeSheetPrintHyperlinkButton.NavigateUri = new Uri(string.Format("/../Home/TimeSheetPdf?idTimeSheet={0}", timeSheet.IdTimeSheet), UriKind.RelativeOrAbsolute);
            TimeSheetShowAndApproveHyperlinkButton.NavigateUri = new Uri(string.Format("/../Home/TimeSheetApproval?idTimeSheet={0}", timeSheet.IdTimeSheet), UriKind.RelativeOrAbsolute);
            _client.GetTimeSheetApproveHistoryAsync(timeSheet.IdTimeSheet);
            _client.GetTimeSheetApproveStepAsync(timeSheet.IdTimeSheet);
            _timeSheet = timeSheet;
            //DataContractSerializer serializer = new DataContractSerializer(typeof(DtoTimeSheet));

            //serializer.WriteObject(fileStream, obj);
            //var serializer = new DataContractJsonSerializer(typeof(MyClass));
            //TimeSheetPrintHyperlinkButton1.NavigateUri = new Uri(string.Format("/../Home/TimeSheetPrint1?timeSheet={0}", timeSheet.Approvers.Select(s=>s.IdEmployee.ToString())), UriKind.RelativeOrAbsolute);


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

        private void TimeSheetShowAndApproveHyperlinkButton1_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("");
            var timeSheetByName = new TimeSheetByNameChildWindow(_timeSheet);
            timeSheetByName.Show();
        }

        private void Image_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            _client.RefreshTimeSheetAsync(_timeSheet.IdTimeSheet);
            RefrashImage.Visibility=Visibility.Collapsed;
        }



    }
}
