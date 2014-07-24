using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS.Domain.SubjectArea
{
    public class Employee
    {
        public int IdEmployee { get; set; }
        public string ItabNo { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public string Login { get; set; }
        public bool SexBit { get; set; }
        public string FullName
        {
            get { return string.Format("{0} {1} {2}", Surname, Name, Patronymic); }
        }
    }
}
