using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;


namespace TimeSheetClient.TimeSheetServiceReference
{
    public partial class DtoEmployee
    {
        public override string ToString()
        {
            return string.Format("{0} {1} {2}",this.Surname,this.Name,this.Patronymic);
        }
    }

    public partial class DtoApproverDepartment
    {
        public override string ToString()
        {
            return this.DepartmentSmallName;
        }
    }

    public partial class DtoDayStatus
    {
        public override string ToString()
        {
            //return base.ToString();
            return SmallDayStatusName;
        }
    }

    public partial class DtoWorkShedule
    {
        public override string ToString()
        {
            //return base.ToString();
            return WorkSheduleName;
        }
    }
}