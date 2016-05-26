using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web;

namespace TimeSheetMvc4WebApplication
{
    public partial class PlanStaff
    {
        private EntityRef<FactStaffWithHistory> _FactStaffWithHistory;

        [global::System.Data.Linq.Mapping.AssociationAttribute(Name = "FactStaffWithHistory_PlanStaff", Storage = "_FactStaffWithHistory", ThisKey = "id", OtherKey = "idPlanStaff", IsForeignKey = true)]
        public FactStaffWithHistory FactStaffWithHistory
        {
            get
            {
                return this._FactStaffWithHistory.Entity;
            }
            set
            {
                FactStaffWithHistory previousValue = this._FactStaffWithHistory.Entity;
                if (((previousValue != value)
                            || (this._FactStaffWithHistory.HasLoadedOrAssignedValue == false)))
                {
                    this.SendPropertyChanging();
                    if ((previousValue != null))
                    {
                        this._FactStaffWithHistory.Entity = null;
                        previousValue.PlanStaff = null;
                    }
                    this._FactStaffWithHistory.Entity = value;
                    if ((value != null))
                    {
                        value.PlanStaff = this;
                        this._id = (int)value.idPlanStaff;
                    }
                    else
                    {
                        this._id = default(int);
                    }
                    this.SendPropertyChanged("FactStaffWithHistory");
                }
            }
        }
    }
}