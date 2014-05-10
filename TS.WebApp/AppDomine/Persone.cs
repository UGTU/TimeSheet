using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeSheetMvc4WebApplication.AppDomine
{
    /// <summary>
    /// Человек
    /// </summary>
    public class Persone
    {
        public int IdEmployee { get; set; }
        public string ItabN { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public string EmployeeLogin { get; set; }
        public bool SexBit { get; set; }
        public string FullName
        {
            get { return string.Format("{0} {1} {2}", Surname, Name, Patronymic); }
        }
    }
}