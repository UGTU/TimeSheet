using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using TimeSheetMvc4WebApplication.ClassesDTO;

namespace TimeSheetMvc4WebApplication.Models.Main
{
    public class JsTimeSheetViewModel
    {
        public DtoDayStatus[] DayStatusList { get; set; }
        public JsTimeSheetModel TimeSheet { get; set; }
    }

    public class JsTimeSheetModel
    {
        public int IdTimeSheet { get; set; }

        public int TimeSheetApproveStep { get; set; }
        public JsEmployeeModel[] Employees { get; set; }
        public string Department { get; set; }
        //public DateTime DateBegin { get; set; }
        //public DateTime DateEnd { get; set; }
        //public DateTime DateComposition { get; set; }
    }

    public class JsEmployeeModel
    {
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public decimal StaffRate { get; set; }
        [DataMember]
        public DtoWorkShedule WorkShedule { get; set; }
        [DataMember]
        public DtoPost Post { get; set; }
        public JsTimeSheetRecordModel[] Records { get; set; }
    }

    [DataContract]
    public class JsTimeSheetRecordModel
    {
        public int Order { get; set; }
        public Guid IdTimeSheetRecord { get; set; }
        public double JobTimeCount { get; set; }
        public double NightTimeCount { get; set; }
        public int IdDayStatus { get; set; }
        public string Date { get; set; }
        public string DayAweek { get; set; }
    }


}