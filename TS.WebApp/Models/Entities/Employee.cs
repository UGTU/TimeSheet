using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web;

namespace TimeSheetMvc4WebApplication
{
    public partial class Employee
    {
        private EntityRef<FactStaffWithHistory> _FactStaffWithHistory;

        [global::System.Data.Linq.Mapping.AssociationAttribute(Name = "FactStaffWithHistory_Employee", Storage = "_FactStaffWithHistory", ThisKey = "id", OtherKey = "idEmployee", IsForeignKey = true)]
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
                        previousValue.Employee = null;
                    }
                    this._FactStaffWithHistory.Entity = value;
                    if ((value != null))
                    {
                        value.Employee = this;
                        this._id = value.idEmployee;
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