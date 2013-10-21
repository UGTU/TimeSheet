
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TimeSheetClient.TimeSheetServiceReference;

namespace TimeSheetClient.Controls
{
    public partial class DepartmentApproverControl : UserControl
    {
        private ObservableCollection<DtoEmployee> _employees = new ObservableCollection<DtoEmployee>();
        private int _idDepartment;
        private int _approverNumber;
        private const int IdKadrDepartment = 154;
        private DtoApprover _currentApprover;
        TimeSheetServiceReference.TimeSheetServiceClient _client = new TimeSheetServiceClient();

        public DepartmentApproverControl()
        {
            InitializeComponent();
            _client.GetDepartmentEmployeesCompleted += _client_GetDepartmentEmployeesCompleted;
            _client.GetDepartmentApproverCompleted += _client_GetDepartmentApproverCompleted;
            _client.AddApproverForDepartmentCompleted += _client_AddApproverForDepartmentCompleted;
            _client.AddEmployeeLoginCompleted += _client_AddEmployeeLoginCompleted;
        }

        void _client_AddEmployeeLoginCompleted(object sender, AddEmployeeLoginCompletedEventArgs e)
        {
            if(e.Result)
            {
                LoginTrigget.Fill=new SolidColorBrush(Colors.Green);
            }
            else
            {
                LoginTrigget.Fill = new SolidColorBrush(Colors.Brown);
            }
            //throw new System.NotImplementedException();
        }

        void _client_AddApproverForDepartmentCompleted(object sender, AddApproverForDepartmentCompletedEventArgs e)
        {
            //throw new System.NotImplementedException();
            if (e.Result)
            {
                ApproverTrigget.Fill = new SolidColorBrush(Colors.Green);
            }
            else
            {
                ApproverTrigget.Fill = new SolidColorBrush(Colors.Brown);
            }
        }

        public void Initialize(int approverNumber, int idDepartment)
        {
            _idDepartment = idDepartment;
            _approverNumber = approverNumber;
            //ApproverLoginTextBox.Text = "";
            this.Visibility=Visibility.Visible;
            switch (_approverNumber)
            {
                case 1:
                    ApproverStatusTextBlock.Text = "Табельщик:";
                    _client.GetDepartmentEmployeesAsync(_idDepartment);
                    break;
                case 2:
                    ApproverStatusTextBlock.Text = "Нач. структ. подразделения:";
                    _client.GetDepartmentEmployeesAsync(_idDepartment);
                    break;
                case 3:
                    ApproverStatusTextBlock.Text = "Каровый работник:";
                    _client.GetDepartmentEmployeesAsync(IdKadrDepartment);
                    break;
                //case 10:
                //    ApproverStatusTextBlock.Text = "Администратор:";
                //    break;
                default:
                    ApproverStatusTextBlock.Text = "Ошибка:";
                    break;
            }
            ApproverTrigget.Visibility = Visibility.Collapsed;
            LoginTrigget.Visibility = Visibility.Collapsed;
        }

        void _client_GetDepartmentEmployeesCompleted(object sender, GetDepartmentEmployeesCompletedEventArgs e)
        {
            if(e.Result!=null)
            {
                _employees = e.Result; 
                ApproverComboBox.ItemsSource = _employees.OrderBy(o=>o.Surname);
                _client.GetDepartmentApproverAsync(_idDepartment,_approverNumber);
            }
            else
            {
                MessageBox.Show("Не удалось загрузить сотрудников");
            }
        }


        void _client_GetDepartmentApproverCompleted(object sender, GetDepartmentApproverCompletedEventArgs e)
        {
            _currentApprover = e.Result;
            if(e.Result!=null)
            {
                //var emps = (ApproverComboBox.ItemsSource as System.Linq.OrderedEnumerable<TimeSheetClient.TimeSheetServiceReference.DtoEmployee, string>);
                //var approver = _employees.Where(w => w.IdEmployee == e.Result.IdEmployee).FirstOrDefault();
                var approver = _employees.FirstOrDefault(f => f.IdEmployee == e.Result.IdEmployee);
                if(approver!=null)
                {
                    ApproverComboBox.SelectedItem = approver;
                }
                ApproverTrigget.Visibility = Visibility.Collapsed;
                LoginTrigget.Visibility = Visibility.Collapsed;
            }
            else
            {
                ApproverLoginTextBox.Text = "";
            }
        }

        public void Save()
        {
            var selectedEmployee = ApproverComboBox.SelectedItem as DtoEmployee;
            if (selectedEmployee != null)
            {
                if (_currentApprover != null)
                {
                    if (selectedEmployee.IdEmployee == _currentApprover.IdEmployee)
                    {
                        if (selectedEmployee.EmployeeLogin != ApproverLoginTextBox.Text)
                        {
                            _client.AddEmployeeLoginAsync(selectedEmployee.IdEmployee, ApproverLoginTextBox.Text);
                            //меням логин работника
                        }
                    }else
                    {
                        _client.AddApproverForDepartmentAsync(selectedEmployee.IdEmployee, _idDepartment, _approverNumber);
                        if (selectedEmployee.EmployeeLogin != ApproverLoginTextBox.Text)
                        {
                            //Добавляем новый логин
                            _client.AddEmployeeLoginAsync(selectedEmployee.IdEmployee, ApproverLoginTextBox.Text);
                        } 
                    }
                }
                else
                {
                    {
                        //добавляем нового согласователя
                        _client.AddApproverForDepartmentAsync(selectedEmployee.IdEmployee, _idDepartment, _approverNumber);
                        if (selectedEmployee.EmployeeLogin != ApproverLoginTextBox.Text)
                        {
                            //Добавляем новый логин
                            _client.AddEmployeeLoginAsync(selectedEmployee.IdEmployee, ApproverLoginTextBox.Text);
                        }
                    }
                }
            }
        }

        private void ApproverComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //var selectedEmployee = ApproverComboBox.SelectedItem as DtoEmployee;
            if((ApproverComboBox.SelectedItem as DtoEmployee!=null))
            {
                ApproverTrigget.Visibility=Visibility.Visible;
                ApproverTrigget.Fill=new SolidColorBrush(Colors.Red);
                ApproverLoginTextBox.Text = (ApproverComboBox.SelectedItem as DtoEmployee).EmployeeLogin==null?string.Empty:(ApproverComboBox.SelectedItem as DtoEmployee).EmployeeLogin;
            }
            else
            {
                ApproverTrigget.Visibility = Visibility.Collapsed;
            }
        }


        private void ApproverLoginTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var selectedEmployee = ApproverComboBox.SelectedItem as DtoEmployee;
            if (selectedEmployee != null)
            {
                if (selectedEmployee.EmployeeLogin != ApproverLoginTextBox.Text)
                {
                    LoginTrigget.Visibility = Visibility.Visible;
                    LoginTrigget.Fill = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    LoginTrigget.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}
