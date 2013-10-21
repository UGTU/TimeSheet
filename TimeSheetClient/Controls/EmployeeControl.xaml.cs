using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using TimeSheetClient.TimeSheetServiceReference;

namespace TimeSheetClient
{
	public partial class EmployeeControl : UserControl
	{
	    readonly TimeSheetServiceReference.TimeSheetServiceClient _client = new TimeSheetServiceClient();
	    private DtoTimeSheetEmployee _currentEmployee;
        ObservableCollection<DtoTimeSheetRecord> _copyRecords = new ObservableCollection<DtoTimeSheetRecord>(); 
		
        public EmployeeControl()
		{
			// Required to initialize variables
			InitializeComponent();
            _client.GetDayStatusListCompleted += client_GetDayStatusListCompleted;
            _client.EditTimeSheetRecordsCompleted += _client_EditTimeSheetRecordsCompleted;
            _client.GetDayStatusListAsync();
            _client.CanTimeSheeEditCompleted+=_client_CanTimeSheeEditCompleted;
		}


        void _client_CanTimeSheeEditCompleted(object sender, CanTimeSheeEditCompletedEventArgs e)
        {
            if(e.Result)
                {
                    AppyButton.IsEnabled = true;
                    CopyButton.IsEnabled = true;
                    PastButton.IsEnabled = true;
                    TimeTextBox.IsEnabled = true;
                    DayStatusComboBox.IsEnabled = true;
                    if (_copyRecords.Count == 0) PastButton.IsEnabled = false;
                }
                else
                {
                    AppyButton.IsEnabled = false;
                    CopyButton.IsEnabled = false;
                    PastButton.IsEnabled = false;
                    TimeTextBox.IsEnabled = false;
                    DayStatusComboBox.IsEnabled = false;
                }
            //throw new NotImplementedException();
        }

        void _client_EditTimeSheetRecordsCompleted(object sender, EditTimeSheetRecordsCompletedEventArgs e)
        {
            if (!e.Result)
                MessageBox.Show("Ошибка при редактировании.");
        }



        public void Initialize(DtoTimeSheetEmployee employee, bool canCorrect)
        {
            _currentEmployee = employee;
            EmployeeNameTextBlock.Text = string.Format("{0} {1} {2}     Ставка: {3} {4}", _currentEmployee.FactStaffEmployee.Surname,
                                                       _currentEmployee.FactStaffEmployee.Name,
                                                       _currentEmployee.FactStaffEmployee.Patronymic,
                                                       (double)_currentEmployee.FactStaffEmployee.StaffRate,
                                                       _currentEmployee.FactStaffEmployee.Post.PostSmallName);
            recordsDataGrid.ItemsSource = _currentEmployee.Records.OrderBy(o=>o.Date);

            _client.CanTimeSheeEditAsync(employee.Records[0].IdTimeSheetRecord);
            if(canCorrect)
            {
                AppyButton.IsEnabled = true;
                CopyButton.IsEnabled = true;
                PastButton.IsEnabled = true;
                TimeTextBox.IsEnabled = true;
                DayStatusComboBox.IsEnabled = true;
                if (_copyRecords.Count == 0) PastButton.IsEnabled = false;
            }
            else
            {
                AppyButton.IsEnabled = false;
                CopyButton.IsEnabled = false;
                PastButton.IsEnabled = false;
                TimeTextBox.IsEnabled = false;
                DayStatusComboBox.IsEnabled = false;
            }
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

        private void UserControl_Loaded_2(object sender, RoutedEventArgs e)
        {

            // Не загружайте свои данные во время разработки.
            // if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            // {
            // 	//Загрузите свои данные здесь и присвойте результат CollectionViewSource.
            // 	System.Windows.Data.CollectionViewSource myCollectionViewSource = (System.Windows.Data.CollectionViewSource)this.Resources["Resource Key for CollectionViewSource"];
            // 	myCollectionViewSource.Source = your data
            // }
        }

        void client_GetDayStatusListCompleted(object sender, GetDayStatusListCompletedEventArgs e)
        {
            DayStatusComboBox.ItemsSource = e.Result;
            DayStatusComboBox.SelectedIndex = 0;
            //throw new NotImplementedException();
        }

        private void AppyButton_Click(object sender, RoutedEventArgs e)
        {
            double jobTimeCount;
            if(double.TryParse(TimeTextBox.Text.Replace('.',','),out jobTimeCount))
            {
                var dayStatus = DayStatusComboBox.SelectedItem as DtoDayStatus;
                if(recordsDataGrid.SelectedItems.Count>0)
                {
                    ObservableCollection<DtoTimeSheetRecord> recordsForEdit = new ObservableCollection<DtoTimeSheetRecord>();
                    foreach (DtoTimeSheetRecord selectedRecord in recordsDataGrid.SelectedItems)
                    {
                        selectedRecord.DayStays = dayStatus;
                        selectedRecord.JobTimeCount = jobTimeCount;
                        recordsForEdit.Add(selectedRecord);
                    }
                    recordsDataGrid.ItemsSource = null;
                    recordsDataGrid.ItemsSource = _currentEmployee.Records;
                    _client.EditTimeSheetRecordsAsync(recordsForEdit);
                }
            }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (recordsDataGrid.SelectedItems.Count > 0)
            {
                foreach (DtoTimeSheetRecord selectedRecord in recordsDataGrid.SelectedItems)
                {
                    _copyRecords.Add(selectedRecord);
                }
            }
            if (_copyRecords.Count != 0) PastButton.IsEnabled = true;
        }

        private void PastButton_Click(object sender, RoutedEventArgs e)
        {

            if(_copyRecords.Count>0)
            {
                var recordsForEdit = new ObservableCollection<DtoTimeSheetRecord>();
                foreach (var employeeRecord in _currentEmployee.Records)
                {
                    foreach (var copyRecord in _copyRecords)
                    {
                        if(employeeRecord.Date.Day==copyRecord.Date.Day)
                        {
                            employeeRecord.DayStays = copyRecord.DayStays;
                            employeeRecord.JobTimeCount = copyRecord.JobTimeCount;
                            recordsForEdit.Add(employeeRecord);
                            break;
                        }
                    }
                }
                recordsDataGrid.ItemsSource = null;
                recordsDataGrid.ItemsSource = _currentEmployee.Records;
                _client.EditTimeSheetRecordsAsync(recordsForEdit);
            }
        }
	}

    public class JobTimeCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //var temp = parameter as DtoTimeSheetRecord;
            var Color1 = new Color();
            Color1 = Colors.Yellow;
            //f3eabd
            //Color1.R = 0x1;
            //Color1.G = 0x1;
            //Color1.B = 0x1;
            Color1.A = 50;
            //dae1e8
            var Color2 = new Color();
            Color2.R = 0xda;
            Color2.G = 0xe1;
            Color2.B = 0xe8;
            Color2.A = 200;
            return (double)value == 0
                       ? new SolidColorBrush(Color1)
                       : new SolidColorBrush(Color2);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //throw new NotImplementedException();
            return new object();
        }
    }
}