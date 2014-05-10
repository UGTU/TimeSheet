using System;
using System.Collections.Generic;
using TS.AppDomine.DomineModel;

namespace TS.AppDomine.Abstract
{
    public interface IDataProvider
    {
        /// <summary>
        /// Возвращает пользователя системы по логину
        /// </summary>
        /// <param name="login">Пользователь системы в формате usernale@domine</param>
        /// <returns></returns>
        User GetUserByLogin(string login);
        /// <summary>
        /// Возвращает списов всех отделов
        /// </summary>
        /// <returns></returns>
        Department GetDepartmentsList();
        /// <summary>
        /// Сохранят изменения в отделе
        /// </summary>
        /// <param name="department"></param>
        /// <returns></returns>
        bool UpdateDepartment(Department department);
        /// <summary>
        /// Возвращает сотрудников отдела
        /// </summary>
        /// <param name="idDepartment">Идентификатор отдела</param>
        /// <returns></returns>
        IEnumerable<Employee> GetDepartmentEmployees(int idDepartment);
        /// <summary>
        /// Возвращает список сотрудников попадающих в табель
        /// </summary>
        /// <param name="idDepartment">Идентификатор отдела</param>
        /// <param name="dateStart">Дата начала периода</param>
        /// <param name="dateEnd">Дата окончания периода</param>
        /// <returns></returns>
        IEnumerable<Employee> GetEmployeesForTimeSheet(int idDepartment, DateTime dateStart,DateTime dateEnd);
        /// <summary>
        /// Возвращает табель соотвествующий переданному идентификатору
        /// </summary>
        /// <param name="idTimeSheet">Идентификатор табеля</param>
        /// <param name="isEmpty">Флаг, показывающийт возвращать табель с записями или без</param>
        /// <returns></returns>
        BaseTimeSheet GetTimeSheet(int idTimeSheet, bool isEmpty = false);
        /// <summary>
        /// Возвращает список табелей, отсортерованный по дате
        /// </summary>
        /// <param name="idDepartment">Идентификатор отдела для которого возвращатся табеля</param>
        /// <param name="filter">Фильтор статусов табелей</param>
        /// <param name="skip">Сколько табелей от нчала списка пропустить</param>
        /// <param name="take">Сколько табелей извлечь</param>
        /// <returns></returns>
        BaseTimeSheet GetTimeSheetList(int idDepartment, TimeSheetFilter filter, int skip, int take);
        /// <summary>
        /// Редактирует записи в тебеле
        /// </summary>
        /// <param name="recordsForEdit">Список записей для редактирования</param>
        /// <returns>true в случае удачного завершения</returns>
        bool EditTimeSheetRecords(IEnumerable<TimeSheetRecord> recordsForEdit);
        /// <summary>
        /// Добавляет / заменяет согласователя для отдела
        /// </summary>
        /// <param name="idEmployee"></param>
        /// <param name="idDepartment"></param>
        /// <param name="approveNumber"></param>
        /// <returns></returns>
        bool AddApproverForDepartment(int idEmployee, int idDepartment, int approveNumber);
        /// <summary>
        /// Редактирует день исключения
        /// </summary>
        /// <param name="exceptionDay"></param>
        /// <returns></returns>
        bool EditExeptionsDay(ExceptionDay exceptionDay);
        /// <summary>
        /// Удаляет день исключения
        /// </summary>
        /// <param name="exceptionDay"></param>
        /// <returns></returns>
        bool RemoveExeptionsDay(ExceptionDay exceptionDay);
        /// <summary>
        /// Согдасование табеля
        /// </summary>
        /// <param name="idTimeSheet">Идентификатор табеля</param>
        /// <param name="approverResult">Результат согласования тебеля</param>
        /// <returns></returns>
        bool TimeSheetApproval(int idTimeSheet, ApproverResult approverResult);
        /// <summary>
        /// Сохраняет табель в хранилище
        /// </summary>
        /// <param name="timeSheet"></param>
        /// <returns></returns>
        bool SaveTimeSheet(BaseTimeSheet timeSheet);
        /// <summary>
        /// Удаляет табель из хранилица
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool RemoveTimeSheet(int id);
        /// <summary>
        /// Возвращает список статусов дней
        /// </summary>
        /// <returns></returns>
        DayStatus[] GetDayStatusList();
        /// <summary>
        /// Возвращает список режимов работ
        /// </summary>
        /// <returns></returns>
        WorkShedule[] GetWorkScheduleList();



        /// <summary>
        /// Добавляет логин для сотрудника
        /// </summary>
        /// <param name="idEmployee">Идентификатор сотрудника</param>
        /// <param name="login">Логин сотрудника</param>
        /// <returns></returns>
        bool AddEmployeeLogin(int idEmployee, string login);
    }
}
