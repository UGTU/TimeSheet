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
    public partial class ExceptionDayAddChildWindow : ChildWindow
    {
        private bool isInsert;
        private DtoExceptionDay ExceptionDay;
        TimeSheetServiceClient Client = new TimeSheetServiceClient();

        public ExceptionDayAddChildWindow(DtoWorkShedule workSchedule)
        {
            InitializeComponent();
            isInsert = true;
            Title = "Добавить день - исключение";
            OKButton.Content = " Добавить ";
            Init();
            ExceptionDay = new DtoExceptionDay();
            ExceptionDay.WorkShedule = workSchedule;
            ExceptionDate.SelectedDate = DateTime.Now;
        }

        public ExceptionDayAddChildWindow(DtoExceptionDay exceptionDay)
        {
            InitializeComponent();
            ExceptionDay = exceptionDay;
            isInsert = false;
            Title = "Редактировать день - исключение";
            OKButton.Content = " Сохранить изменения ";
            Init();
            ExceptionDate.SelectedDate = exceptionDay.Date;
            ExceptionName.Text = exceptionDay.Name;
            MPS.Text = exceptionDay.MPS.ToString();
            MNS.Text = exceptionDay.MNS.ToString();
            GPS.Text = exceptionDay.GPS.ToString();
            GNS.Text = exceptionDay.GNS.ToString();
        }

        void Init()
        {
            Client.InsertExeptionsDayCompleted += Client_InsertExeptionsDayCompleted;
            Client.EditExeptionsDayCompleted += Client_EditExeptionsDayCompleted;
            Client.GetDayStatusListCompleted += Client_GetDayStatusListCompleted;
            Client.GetDayStatusListAsync();
        }

        void Client_GetDayStatusListCompleted(object sender, GetDayStatusListCompletedEventArgs e)
        {
            DayStatusComboBox.ItemsSource = e.Result;
            if (ExceptionDay.DayStatus != null)
            {
                DayStatusComboBox.SelectedItem =
                    e.Result.FirstOrDefault(f => f.IdDayStatus == ExceptionDay.DayStatus.IdDayStatus);
            }
            else
            {
                DayStatusComboBox.SelectedItem =
                    e.Result.FirstOrDefault(f => f.SmallDayStatusName == "В");
            }
            //throw new NotImplementedException();
        }

        void Client_EditExeptionsDayCompleted(object sender, EditExeptionsDayCompletedEventArgs e)
        {
            if(e.Result.Result)
            {
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show(e.Result.Message);
            }
            //throw new NotImplementedException();
        }

        void Client_InsertExeptionsDayCompleted(object sender, InsertExeptionsDayCompletedEventArgs e)
        {
            if (e.Result.Result)
            {
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show(e.Result.Message);
            }
            //throw new NotImplementedException();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ExceptionDate.SelectedDate != null) ExceptionDay.Date = (DateTime) ExceptionDate.SelectedDate;
                ExceptionDay.Name = ExceptionName.Text;
                ExceptionDay.MPS = float.Parse(MPS.Text);
                ExceptionDay.MNS = float.Parse(MNS.Text);
                ExceptionDay.GPS = float.Parse(GPS.Text);
                ExceptionDay.GNS = float.Parse(GNS.Text);
                ExceptionDay.DayStatus = DayStatusComboBox.SelectedItem as DtoDayStatus;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
            if(isInsert)
            {
                Client.InsertExeptionsDayAsync(ExceptionDay);
                if (AddPpExeptoinDay.IsChecked==true)
                {
                    var ppDay = new DtoExceptionDay
                    {
                        Date = ExceptionDay.Date.AddDays(-1),
                        Name = "Предпраздничный день",
                        DayStatus = DayStatusComboBox.Items.First(f =>
                        {
                            var dtoDayStatus = f as DtoDayStatus;
                            return dtoDayStatus != null && dtoDayStatus.SmallDayStatusName == "ПП";
                        }) as DtoDayStatus,
                        WorkShedule = ExceptionDay.WorkShedule
                    };
                    Client.InsertExeptionsDayAsync(ppDay);
                }
            }
            else
            {
                Client.EditExeptionsDayAsync(ExceptionDay);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

