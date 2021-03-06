﻿using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web;

namespace TimeSheetMvc4WebApplication
{
    public partial class Employee
    {
        private EntityRef<FactStaffWithHistory> _FactStaffWithHistory;

        public Employee()
        {
            this._Approvers = new EntitySet<Approver>(new Action<Approver>(this.attach_Approvers), new Action<Approver>(this.detach_Approvers));
            this._OK_Inkapacities = new EntitySet<OK_Inkapacity>(new Action<OK_Inkapacity>(this.attach_OK_Inkapacities), new Action<OK_Inkapacity>(this.detach_OK_Inkapacities));
            this._FactStaffs = new EntitySet<FactStaff>(new Action<FactStaff>(this.attach_FactStaffs), new Action<FactStaff>(this.detach_FactStaffs));
            this._FactStaffWithHistory = default(EntityRef<FactStaffWithHistory>);
            OnCreated();
        }

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
                        this._id = (int)value.idEmployee;
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