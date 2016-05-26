using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Linq;
using System.Web;

namespace TimeSheetMvc4WebApplication
{
    public partial class FactStaffWithHistory : INotifyPropertyChanging, INotifyPropertyChanged
    {
        private static PropertyChangingEventArgs _emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);

        private EntitySet<OK_Otpusk> _OK_Otpusk;

        private EntityRef<Employee> _Employee;

        private EntityRef<FactStaff> _FactStaff;

        private EntityRef<PlanStaff> _PlanStaff;

        private EntityRef<FactStaffHistory> _FactStaffHistory;

        public FactStaffWithHistory()
        {
            this._OK_Otpusk = new EntitySet<OK_Otpusk>(new Action<OK_Otpusk>(this.attach_OK_Otpusk), new Action<OK_Otpusk>(this.detach_OK_Otpusk));
            this._Employee = default(EntityRef<Employee>);
            this._FactStaff = default(EntityRef<FactStaff>);
            this._PlanStaff = default(EntityRef<PlanStaff>);
            this._FactStaffHistory = default(EntityRef<FactStaffHistory>);
            OnCreated();
        }

        private void attach_OK_Otpusk(OK_Otpusk entity)
        {
            this.SendPropertyChanging();
            entity.FactStaffWithHistory = this;
        }

        private void detach_OK_Otpusk(OK_Otpusk entity)
        {
            this.SendPropertyChanging();
            entity.FactStaffWithHistory = null;
        }


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

        [global::System.Data.Linq.Mapping.AssociationAttribute(Name = "FactStaffWithHistory_OK_Otpusk", Storage = "_OK_Otpusk", ThisKey = "id", OtherKey = "idFactStaff")]
        public EntitySet<OK_Otpusk> OK_Otpusk
        {
            get
            {
                return this._OK_Otpusk;
            }
            set
            {
                this._OK_Otpusk.Assign(value);
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
        [global::System.Data.Linq.Mapping.AssociationAttribute(Name = "FactStaffWithHistory_PlanStaff", Storage = "_PlanStaff", ThisKey = "idPlanStaff", OtherKey = "id", IsUnique = true, IsForeignKey = false)]
        public PlanStaff PlanStaff
        {
            get
            {
                return this._PlanStaff.Entity;
            }
            set
            {
                PlanStaff previousValue = this._PlanStaff.Entity;
                if (((previousValue != value)
                            || (this._PlanStaff.HasLoadedOrAssignedValue == false)))
                {
                    this.SendPropertyChanging();
                    if ((previousValue != null))
                    {
                        this._PlanStaff.Entity = null;
                        previousValue.FactStaffWithHistory = null;
                    }
                    this._PlanStaff.Entity = value;
                    if ((value != null))
                    {
                        value.FactStaffWithHistory = this;
                    }
                    this.SendPropertyChanged("PlanStaff");
                }
            }
        }

    }
}