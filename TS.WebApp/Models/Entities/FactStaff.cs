using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web;

namespace TimeSheetMvc4WebApplication
{
    public partial class FactStaff
    {
        private EntityRef<FactStaffWithHistory> _FactStaffWithHistory;

        [global::System.Data.Linq.Mapping.AssociationAttribute(Name = "FactStaffWithHistory_FactStaff", Storage = "_FactStaffWithHistory", ThisKey = "id", OtherKey = "id", IsForeignKey = true)]
        public FactStaffWithHistory FactStaffWithHistory
        {
            get
            {
                return this._FactStaffWithHistory.Entity;
            }
            set
            {
                var previousValue = this._FactStaffWithHistory.Entity;
                if (((previousValue != value)
                            || (this._FactStaffWithHistory.HasLoadedOrAssignedValue == false)))
                {
                    this.SendPropertyChanging();
                    if ((previousValue != null))
                    {
                        this._FactStaffWithHistory.Entity = null;
                        previousValue.FactStaff = null;
                    }
                    this._FactStaffWithHistory.Entity = value;
                    if ((value != null))
                    {
                        value.FactStaff = this;
                        this._id = value.id;
                    }
                    else
                    {
                        this._id = default(int);
                    }
                    this.SendPropertyChanged("FactStaffWithHistory");
                }
            }
        }

        public FactStaffHistory CurrentChange
        {
            get
            {
                return FactStaffHistories.Where(fcStHist => fcStHist.DateBegin <= DateTime.Today.Date).OrderBy(fcStHist => fcStHist.DateBegin).LastOrDefault() ??
                    FactStaffHistories.FirstOrDefault();
            }
        }
    }
}