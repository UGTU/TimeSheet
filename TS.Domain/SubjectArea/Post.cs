using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS.Domain.SubjectArea
{
    class Post
    {
        public int IdPost { get; set; }
        public string PostFullName { get; set; }
        public string PostSmallName { get; set; }
        public bool IsMenager { get; set; }
        public Category Category { get; set; }  
    }
}
