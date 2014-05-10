using System;

namespace TS.AppDomine.DomineModel
{
    /// <summary>
    /// Согласвоатель табеля
    /// </summary>
    public class Approver:Persone
    {
        public int IdApprover { get; set; }
        public DateTime AppointmentDate { get; set; }
        public DateTime TerminationDate { get; set; }
        public ApproveType ApproveType { get; set; }

    }
}