using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS.Domain.SubjectArea
{
    class Category
    {
        public int IdCategory { get; set; }
        public string CategorySmallName { get; set; }
        public string CategoryFullName { get; set; }
        public int? OrderBy { get; set; }
        public bool IsPPS { get; set; }
    }
}
