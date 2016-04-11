using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web;

namespace TimeSheetMvc4WebApplication
{
    public partial class FactStaffWithHistory
    {
        private EntityRef<Employee> _Employee;

        private EntityRef<FactStaff> _FactStaff;

        [global::System.Data.Linq.Mapping.AssociationAttribute(Name = "FactStaffWithHistory_Employee", Storage = "_Employee", ThisKey = "idEmployee", OtherKey = "id", IsUnique = true, IsForeignKey = false)]
        public Employee Employee
        {
            get
            {
                return this._Employee.Entity;
            }
            set
            {
                var previousValue = this._Employee.Entity;
                if (((previousValue != value)
                            || (this._Employee.HasLoadedOrAssignedValue == false)))
                {
                    this.SendPropertyChanging();
                    if ((previousValue != null))
                    {
                        this._Employee.Entity = null;
                        previousValue.FactStaffWithHistory = null;
                    }
                    this._Employee.Entity = value;
                    if ((value != null))
                    {
                        value.FactStaffWithHistory = this;
                    }
                    this.SendPropertyChanged("Employee");
                }
            }
        }

        [global::System.Data.Linq.Mapping.AssociationAttribute(Name = "FactStaffWithHistory_FactStaff", Storage = "_FactStaff", ThisKey = "id", OtherKey = "id", IsUnique = true, IsForeignKey = false)]
        public FactStaff FactStaff
        {
            get
            {
                return this._FactStaff.Entity;
            }
            set
            {
                FactStaff previousValue = this._FactStaff.Entity;
                if (((previousValue != value)
                            || (this._FactStaff.HasLoadedOrAssignedValue == false)))
                {
                    this.SendPropertyChanging();
                    if ((previousValue != null))
                    {
                        this._FactStaff.Entity = null;
                        previousValue.FactStaffWithHistory = null;
                    }
                    this._FactStaff.Entity = value;
                    if ((value != null))
                    {
                        value.FactStaffWithHistory = this;
                    }
                    this.SendPropertyChanged("FactStaff");
                }
            }
        }
    }
}