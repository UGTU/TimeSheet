using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web;

namespace TimeSheetMvc4WebApplication
{
    public partial class OK_Otpusk
    {
        private EntityRef<FactStaffWithHistory> _FactStaffWithHistory;

        [global::System.Data.Linq.Mapping.AssociationAttribute(Name = "FactStaffWithHistory_OK_Otpusk", Storage = "_FactStaffWithHistory", ThisKey = "idFactStaff", OtherKey = "id", IsForeignKey = true)]
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
                        previousValue.OK_Otpusk.Remove(this);
                    }
                    this._FactStaffWithHistory.Entity = value;
                    if ((value != null))
                    {
                        value.OK_Otpusk.Add(this);
                        this._idFactStaff = value.id;
                    }
                    else
                    {
                        this._idFactStaff = default(int);
                    }
                    this.SendPropertyChanged("FactStaffWithHistory");
                }
            }
        }
    }
}