//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TS.Domain
{
    using System;
    using System.Collections.Generic;
    
    public partial class OK_Inkapacity
    {
        public int idInkapacity { get; set; }
        public string NInkapacity { get; set; }
        public System.DateTime DateBegin { get; set; }
        public Nullable<System.DateTime> DateEnd { get; set; }
        public int idEmployee { get; set; }
    
        public virtual Employee Employee { get; set; }
    }
}