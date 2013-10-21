using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using TimeSheetClient.TimeSheetServiceReference;


namespace TimeSheetClient
{
    public partial class MainPage : UserControl
    {
        
        TimeSheetServiceReference.TimeSheetServiceClient client = new TimeSheetServiceClient();
        readonly WebClient _webClient = new WebClient();
        DtoApprover _approver = new DtoApprover();
        DtoApproverDepartment _currentApproverDepartment;
        private const string AddNewNimeSheet = "Сформировать табель";
        private string employeeLogin;

        public MainPage()
        {
                InitializeComponent();
                UsersInitializeComponent();
                ApplicationNameTextBlock.Text = "ИС \"Табель\"";
                //==================================================
                // better to keep this in a global config singleton
                var hostName = Application.Current.Host.Source.Host;
                if (Application.Current.Host.Source.Port != 80)
                    hostName += ":" + Application.Current.Host.Source.Port;

                // set the image source
                //image.Source = new BitmapImage(new Uri("http://" + hostName + "/cute_kitten112.jpg", UriKind.Absolute));



                _webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
                _webClient.Headers["Accept"] = "application/xml";
                _webClient.DownloadStringAsync(new Uri("http://" + hostName + "/home/GetUsername", UriKind.Absolute));
                //webClient.DownloadStringAsync(new Uri("../home/GetUsername"));


                //client.GetCurrentApproverCompleted += client_GetCurrentApproverCompleted;
                //client.GetCurrentApproverAsync();
                client.GetTimeSheetListCompleted += client_GetTimeSheetListCompleted;
                client.GetTimeSheetCompleted += client_GetTimeSheetCompleted;
        }

        void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                var webex = (System.Net.WebException)e.Error;
                var webrs = (System.Net.HttpWebResponse)webex.Response;
                var sr = new System.IO.StreamReader(webrs.GetResponseStream());
                var str = sr.ReadToEnd();
                throw new Exception(String.Format("{0} - {1}: {2}", ((int)Enum.Parse(typeof(System.Net.HttpStatusCode), webrs.StatusCode.ToString(), true)).ToString(), webrs.StatusDescription, str), e.Error);
            }
            else
            {
                employeeLogin = e.Result;
                client.GetCurrentApproverByLoginCompleted += client_GetCurrentApproverByLoginCompleted;
                client.GetCurrentApproverByLoginAsync(employeeLogin);
            }
        }

        void client_GetCurrentApproverByLoginCompleted(object sender, GetCurrentApproverByLoginCompletedEventArgs e)
        {
            if (e.Result == null)
            {
                MessageBox.Show("Отсутствует доступ к ИС \"Табель\", обратитесь к администратору.");
                System.Windows.Browser.HtmlPage.Window.Invoke("close");//Закрываем окно
            }
            else
            {
                _approver = e.Result;
                DepartmentComboBox.ItemsSource = _approver.DtoApproverDepartments.OrderBy(o => o.DepartmentSmallName);
                if (_approver.DtoApproverDepartments.Count > 0)
                    DepartmentComboBox.SelectedIndex = 0;
                ApproveNameTextBlock.Text = string.Format("{0} {1} {2}", _approver.Surname, _approver.Name, _approver.Patronymic);
                if (_approver.IsAdministrator)
                {
                    AdministratorButton.Visibility = Visibility.Visible;
                }
            }
            //throw new NotImplementedException();
        }


        void client_GetTimeSheetCompleted(object sender, GetTimeSheetCompletedEventArgs e)
        {
            if(e.Result!=null)
            {
                foreach (TreeViewItem item in TimeSheetTreeView.Items)
                {
                    var timeSheet = item.Tag as DtoTimeSheet;
                    if (timeSheet != null)
                    {
                        if (timeSheet.IdTimeSheet == e.Result.IdTimeSheet)
                        {
                            if (e.Result.Employees != null)
                            {
                                item.Tag = e.Result;
                                if (item.IsSelected)
                                {
                                    TimeSheetView.Initialize(e.Result);
                                }
                                
                                foreach (var employee in e.Result.Employees)
                                {
                                    item.Items.Add(CreateSotrudnikTreeViewItem(employee));
                                }
                                item.IsExpanded = true;
                                return;
                            }
                        }
                    }
                }
            }
        }

        void UsersInitializeComponent()
        {
            TurnAllUserControls();
            ApproveNameTextBlock.Text = "";
            AproverStatusTextBlock.Text = "";
        }

        void TurnAllUserControls()
        {
            AdministratorButton.Visibility = Visibility.Collapsed;
            AdministratorView.Visibility = Visibility.Collapsed;
            EmployeeView.Visibility=Visibility.Collapsed;
            TimeSheetView.Visibility=Visibility.Collapsed;
        }


        ///// <summary>
        ///// Обработчик события загрузки пользователя системы (Предоставление доступа)
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //void client_GetCurrentApproverCompleted(object sender, GetCurrentApproverCompletedEventArgs e)
        //{
        //    if (e.Result == null)
        //    {
        //        MessageBox.Show("Отсутствует доступ к ИС \"Табель\", обратитесь к администратору.");
        //        System.Windows.Browser.HtmlPage.Window.Invoke("close");//Закрываем окно
        //    }
        //    else
        //    {
        //        _approver = e.Result;
        //        DepartmentComboBox.ItemsSource = _approver.DtoApproverDepartments.OrderBy(o=>o.DepartmentSmallName);
        //        if (_approver.DtoApproverDepartments.Count > 0)
        //            DepartmentComboBox.SelectedIndex = 0;
        //        ApproveNameTextBlock.Text = string.Format("{0} {1} {2}", _approver.Surname, _approver.Name,_approver.Patronymic);
        //        if (_approver.IsAdministrator)
        //        {
        //            AdministratorButton.Visibility=Visibility.Visible;
        //        }
        //    }
        //}

        /// <summary>
        /// Обработчик события нажатия кнопки  Администрирование (Переход к представлению администратора)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AdministratorButton_Click_1(object sender, RoutedEventArgs e)
        {
            TurnAllUserControls();
            AdministratorView.Initialize();
        }

        /// <summary>
        /// Оброботчик события выбора Структурного подразделения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DepartmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TurnAllUserControls();
            _currentApproverDepartment = DepartmentComboBox.SelectedItem as DtoApproverDepartment;
            if(_currentApproverDepartment!=null)
            {
                AproverStatusTextBlock.Text = _currentApproverDepartment.ApproveTypeName;
                client.GetTimeSheetListAsync(_currentApproverDepartment.IdDepartment);
                TimeSheetTreeView.Items.Clear();
            }

        }

        /// <summary>
        /// Обработчик события загрузки списка табелей, формирование дерева
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetTimeSheetListCompleted(object sender, GetTimeSheetListCompletedEventArgs e)
        {
            TimeSheetTreeView.Items.Clear();
            if (e.Error == null)
            {
                if (e.Result != null && e.Result.Any())
                {
                    foreach (var timeSheet in e.Result.OrderBy(o => o.DateBegin))
                    {
                        if (timeSheet.Department.IdDepartment == _currentApproverDepartment.IdDepartment)
                        {
                            var tsItem = new TreeViewItem();
                            tsItem.Header = string.Format("{0} {1}г.",
                                timeSheet.DateBegin.ToString("MMMM",
                                    CultureInfo.CurrentCulture),
                                timeSheet.DateBegin.Year);
                            tsItem.Tag = timeSheet;
                            if (timeSheet.Employees != null)
                            {
                                foreach (var employee in timeSheet.Employees)
                                {
                                    tsItem.Items.Add(CreateSotrudnikTreeViewItem(employee));
                                }
                            }
                            TimeSheetTreeView.Items.Add(tsItem);
                        }
                    }
                    //===============Выделяю текущий табель при загрузке==================================================
                    var selectedItem = (TimeSheetTreeView.Items.Last() as TreeViewItem);
                    selectedItem.IsSelected = true;
                    var currentTimeSheet = selectedItem.Tag as DtoTimeSheet;
                    TimeSheetView.Initialize(currentTimeSheet);
                    if (currentTimeSheet != null)
                    {
                        if (currentTimeSheet.Employees == null)
                        {
                            client.GetTimeSheetAsync(currentTimeSheet.IdTimeSheet);
                        }
                    }
                    //=================================================================
                }
            }
            else
            {
                MessageBox.Show("Не удалось загрузить табель. Обратитесь к администратору.");
            }
            if(_approver.IsAdministrator || _currentApproverDepartment.ApproveNumber==1)
            {
                var addTs = new TreeViewItem();
                addTs.Header = AddNewNimeSheet;
                //addTs.Background=new SolidColorBrush(Colors.LightGray);
                TimeSheetTreeView.Items.Add(addTs);
            }
        }


        /// <summary>
        /// Поиск уровня элемента в дереве
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        private int GetTreeViewItemLevel(DependencyObject control)
        {
            var level = 0;
            if (control != null)
            {
                var parent = VisualTreeHelper.GetParent(control);
                while (!(parent is TreeView) && (parent != null))
                {
                    if (parent is TreeViewItem)
                        level++;
                    parent = VisualTreeHelper.GetParent(parent);
                }
            }
            return level;
        }


        /// <summary>
        /// Оброботчик события выбора элемента в дереве
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimeSheetTreeView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = TimeSheetTreeView.SelectedItem as TreeViewItem;
            if(selectedItem!=null)
            {
                TurnAllUserControls();
                switch (GetTreeViewItemLevel(selectedItem))
                {
                    case(0):
                        {
                            if(_approver.IsAdministrator) AdministratorButton.Visibility=Visibility.Visible;
                            if ((string) selectedItem.Header == AddNewNimeSheet)
                            {
                                var createTimeSheetTable = new CreateTimeSheet(_currentApproverDepartment.IdDepartment,employeeLogin);
                                createTimeSheetTable.Closed += createTimeSheetTable_Closed;
                                createTimeSheetTable.Show();
                            }
                            else
                            {
                                var timeSheet = selectedItem.Tag as DtoTimeSheet;
                                TimeSheetView.Initialize(timeSheet);
                                if (timeSheet != null)
                                {
                                    if (timeSheet.Employees == null)
                                    {
                                        client.GetTimeSheetAsync(timeSheet.IdTimeSheet);
                                    }
                                    else
                                    {
                                        selectedItem.IsExpanded = true;
                                    }
                                }
                            }
                            break;
                        }
                    case(1):
                        {
                            if(_currentApproverDepartment.ApproveNumber==1 || _approver.IsAdministrator)
                            {
                                EmployeeView.Visibility = Visibility.Visible;
                                EmployeeView.Initialize(selectedItem.Tag as DtoTimeSheetEmployee, true);
                            }
                            else
                            {
                                EmployeeView.Visibility = Visibility.Visible;
                                EmployeeView.Initialize(selectedItem.Tag as DtoTimeSheetEmployee, false);
                            }
                            break;
                        }
                    default:
                        {
                            MessageBox.Show("Default");
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Обработчик события закрытия окна создания табеля
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void createTimeSheetTable_Closed(object sender, EventArgs e)
        {
            TurnAllUserControls();
            _currentApproverDepartment = DepartmentComboBox.SelectedItem as DtoApproverDepartment;
            var createTimeSheet = sender as CreateTimeSheet;
            if (createTimeSheet != null && createTimeSheet.DialogResult == true)
            {
                if (_currentApproverDepartment != null)
                {
                    AproverStatusTextBlock.Text = _currentApproverDepartment.ApproveTypeName;
                    client.GetTimeSheetListAsync(_currentApproverDepartment.IdDepartment);
                }
            }
        }

        TreeViewItem CreateSotrudnikTreeViewItem(DtoTimeSheetEmployee employee)
        {
            var eplItem = new TreeViewItem();
            var builder = new StringBuilder();
            builder.AppendFormat("{0} {1} {2}", employee.FactStaffEmployee.Surname,
                                 employee.FactStaffEmployee.Name,
                                 employee.FactStaffEmployee.Patronymic);
            builder.AppendLine();
            var staff = string.Format("{0} ст.", employee.FactStaffEmployee.StaffRate.ToString("0.##"));
            var staffFormat = string.Format("{0}{1}", staff, new string(' ', 7 - staff.Length + 1));
            builder.AppendFormat("      {0} {1}", staffFormat,
                     employee.FactStaffEmployee.Post.PostSmallName);
            eplItem.Header = builder;
            eplItem.Tag = employee;
            return eplItem;
        }
    }
}
