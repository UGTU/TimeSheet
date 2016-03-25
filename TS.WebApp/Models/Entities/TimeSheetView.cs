using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Data;
using System.Data.Linq;
using System.Web;

namespace TimeSheetMvc4WebApplication
{
    public partial class TimeSheetView
    {
        private EntitySet<TimeSheetRecord> _TimeSheetRecord;

        [global::System.Data.Linq.Mapping.AssociationAttribute(Name = "TimeSheetView_TimeSheetRecord", Storage = "_TimeSheetRecord", ThisKey = "id", OtherKey = "idTimeSheet")]
        public EntitySet<TimeSheetRecord> TimeSheetRecord
        {
            get
            {
                return this._TimeSheetRecord;
            }
            set
            {
                this._TimeSheetRecord.Assign(value);
            }
        }
    }
}