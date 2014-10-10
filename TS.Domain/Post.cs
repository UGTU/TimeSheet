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
    
    public partial class Post
    {
        public int id { get; set; }
        public string PostName { get; set; }
        public bool ManagerBit { get; set; }
        public int idGlobalPrikaz { get; set; }
        public int idPKCategory { get; set; }
        public Nullable<int> idCategory { get; set; }
        public System.Guid PostGUID { get; set; }
        public Nullable<System.DateTime> DateEnd { get; set; }
        public string Comment { get; set; }
        public string PostCode { get; set; }
        public Nullable<int> idPostType { get; set; }
        public string PostShortName { get; set; }
        public Nullable<int> idCategoryVPO { get; set; }
        public Nullable<int> idCategoryZP { get; set; }
    
        public virtual Category Category { get; set; }
        public virtual PostType PostType { get; set; }
    }
}
