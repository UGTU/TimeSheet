using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Razor;
using DotNetDBF;
using Microsoft.Ajax.Utilities;
using TimeSheetMvc4WebApplication.Models;

namespace TimeSheetMvc4WebApplication.Source
{
    public class TimeSheetToDbf
    {
        private TimeSheetModel[] _models;
        public TimeSheetToDbf(TimeSheetModel[] models)
        {
            _models = models;
        }

        public byte[] GenerateDbf()
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
            foreach (var model in _models)
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
    }
}