using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Razor;
using DotNetDBF;
using Microsoft.Ajax.Utilities;
using TimeSheetMvc4WebApplication.ClassesDTO;
using TimeSheetMvc4WebApplication.Models;

namespace TimeSheetMvc4WebApplication.Source
{
    public class TimeSheetToDbf
    {
        //private TimeSheetModel[] _models;
        //public TimeSheetToDbf(TimeSheetModel[] models)
        //{
        //    _models = models;
        //}

        public byte[] GenerateDbf(TimeSheetModel[] models)
        {
            var fields = new List<DBFField>
            {
                new DBFField("IdDep", NativeDbType.Numeric, 50, 0),
                new DBFField("DepSName", NativeDbType.Char, 350, 0),
                new DBFField("IdTs", NativeDbType.Numeric, 50, 0),
                new DBFField("TsDateSt", NativeDbType.Date),
                new DBFField("TsDateEnd", NativeDbType.Date),
                new DBFField("TsDateComp", NativeDbType.Date),
                new DBFField("IdEmployee", NativeDbType.Numeric, 50, 0),
                new DBFField("EFirstname", NativeDbType.Char, 50, 0),
                new DBFField("ELastName", NativeDbType.Char, 50, 0),
                new DBFField("EPatronymi", NativeDbType.Char, 50, 0),
                new DBFField("EPostname", NativeDbType.Char, 50, 0),
                new DBFField("EStufRate", NativeDbType.Float, 50, 0),
                new DBFField("RecDate", NativeDbType.Date),
                new DBFField("RecStatus", NativeDbType.Char, 50, 0),
                new DBFField("RecHours", NativeDbType.Char, 50, 0),
            };


            var writer = new DBFWriter { Fields =  fields.ToArray() };
            //writer.AddRecord(3, "s", 3, DateTime.Now, DateTime.Now, DateTime.Now, 3,
            //    "3", "3", "3", "3", 3, DateTime.Now, "3", 3);

            var addRecList1 = new List<object>();
            var addRecList2 = new List<object>();
            var addRecList3 = new List<object>();
            foreach (var model in models)
            {
                addRecList1.Clear();
                addRecList1.Add(1);
                addRecList1.Add(model.DepartmentName);
                addRecList1.Add(1);
                addRecList1.Add(model.DateBeginPeriod);
                addRecList1.Add(model.DateEndPeriod);
                addRecList1.Add(model.DateComposition);
                foreach (var paper in model.Papers)
                {
                    foreach (var empl in paper.Employees)
                    {
                        addRecList2.Clear();
                        addRecList2.AddRange(addRecList1.ToArray());
                        addRecList2.Add(1);
                        addRecList2.Add(empl.Surname);
                        addRecList2.Add(empl.Name);
                        addRecList2.Add(empl.Patronymic);
                        addRecList2.Add(empl.Post);
                        addRecList2.Add((float)empl.StaffRate);
                        //addRecList2.Add(1);
                        foreach (var rec in empl.Records)
                        {
                            addRecList3.Clear();
                            addRecList3.AddRange(addRecList2.ToArray());
                            addRecList3.Add(DateTime.Now);
                            addRecList3.Add(rec.DayStatus);
                            addRecList3.Add(rec.Value);
                            writer.AddRecord(addRecList3.ToArray());
                        }
                    }
                }
            }


            var fos = new MemoryStream();
            {
                writer.Write(fos);
            }
            return fos.ToArray();
        }

        public byte[] GenerateDbf(DtoTimeSheet timeSheet)
        {
            var fields = new List<DBFField>
            {
                new DBFField("IdTs", NativeDbType.Numeric, 50, 0),          //Идентификатор табеля
                new DBFField("TsDateSt", NativeDbType.Date),                //Табель с
                new DBFField("TsDateEnd", NativeDbType.Date),               //Табель по
                new DBFField("TsDateComp", NativeDbType.Date),              //Дата составления табеля
                new DBFField("IdDep", NativeDbType.Numeric, 50, 0),         //Идентификатор структурного подразделения
                new DBFField("DepSName", NativeDbType.Char, 350, 0),        //Короткое название структ. подразделения
                new DBFField("DepFName", NativeDbType.Char, 350, 0),        //Полное название структ подразделение
                new DBFField("ETabNum", NativeDbType.Char, 350, 0),         //Табельный номер сотрудника
                new DBFField("IdEmployee", NativeDbType.Numeric, 50, 0),    //Идентификатор сотрудника
                new DBFField("IdFactStaf", NativeDbType.Numeric, 50, 0),    //Идентификатор фактической ставки сотр.
                new DBFField("EFirstname", NativeDbType.Char, 50, 0),       //Фамилия
                new DBFField("ELastName", NativeDbType.Char, 50, 0),        //Имя
                new DBFField("EPatronymi", NativeDbType.Char, 50, 0),       //Отчество
                new DBFField("IdPost", NativeDbType.Numeric, 50, 0),        //Идентификатор должности
                new DBFField("EPostname", NativeDbType.Char, 50, 0),        //Название должности
                new DBFField("EStufRate", NativeDbType.Float, 50, 0),       //Ставка
                new DBFField("IdShedule", NativeDbType.Numeric, 50, 0),     //Идентификатор графика работы
                new DBFField("SheduleNam", NativeDbType.Char, 50, 0),       //Название графика работы
                new DBFField("RecDate", NativeDbType.Date),                 //Дата
                new DBFField("IdDayStat", NativeDbType.Numeric, 50, 0),     //Идентификатор статуса дня
                new DBFField("RecStatus", NativeDbType.Char, 50, 0),        //Статус дня
                new DBFField("IdRec", NativeDbType.Char, 50, 0),            //Идентификатор записи табеля
                new DBFField("RecHours", NativeDbType.Float, 50, 0),        //Отработано часов
            };

            var writer = new DBFWriter {Fields = fields.ToArray(), CharEncoding = Encoding.Default};
            var addRecList1 = new List<object>
            {

                timeSheet.IdTimeSheet,
                timeSheet.DateBegin,
                timeSheet.DateEnd,
                timeSheet.DateComposition,
                timeSheet.Department.IdDepartment,
                timeSheet.Department.DepartmentSmallName,
                timeSheet.Department.DepartmentFullName
            };
            var addRecList2 = new List<object>();
            var addRecList3 = new List<object>();
            foreach (var empl in timeSheet.Employees)
            {
                addRecList2.Clear();
                addRecList2.AddRange(addRecList1.ToArray());
                addRecList2.Add(empl.FactStaffEmployee.ItabN);
                addRecList2.Add(empl.FactStaffEmployee.IdEmployee);
                addRecList2.Add(empl.FactStaffEmployee.IdFactStaff);
                addRecList2.Add(empl.FactStaffEmployee.Surname);
                addRecList2.Add(empl.FactStaffEmployee.Name);
                addRecList2.Add(empl.FactStaffEmployee.Patronymic);
                addRecList2.Add(empl.FactStaffEmployee.Post.IdPost);
                addRecList2.Add(empl.FactStaffEmployee.Post.PostFullName);
                addRecList2.Add(empl.FactStaffEmployee.StaffRate);
                addRecList2.Add(empl.FactStaffEmployee.WorkShedule.IdWorkShedule);
                addRecList2.Add(empl.FactStaffEmployee.WorkShedule.WorkSheduleName);
                foreach (var rec in empl.Records)
                {
                    addRecList3.Clear();
                    addRecList3.AddRange(addRecList2.ToArray());
                    addRecList3.Add(rec.Date);
                    addRecList3.Add(rec.DayStays.IdDayStatus);
                    addRecList3.Add(rec.DayStays.SmallDayStatusName);
                    addRecList3.Add(rec.IdTimeSheetRecord.ToString());
                    addRecList3.Add(rec.JobTimeCount);
                    writer.AddRecord(addRecList3.ToArray());
                }
            }
            var fos = new MemoryStream();
            {
                writer.Write(fos);
            }
            return fos.ToArray();
        }
    }
}