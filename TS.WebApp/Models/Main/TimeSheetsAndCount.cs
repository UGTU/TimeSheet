using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using TimeSheetMvc4WebApplication.ClassesDTO;

namespace TimeSheetMvc4WebApplication.Models.Main
{
    public class TimeSheetsAndCount
    {
        public DtoTimeSheet[] TimeSheets { get; set; }
        public int Count { get; set; }
    }
}