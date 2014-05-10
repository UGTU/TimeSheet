using System;

namespace TS.AppDomine.DomineModel
{
    /// <summary>
    /// Запись о согласовании табеля
    /// </summary>
    public class ApproverResult
    {
        public DateTime Date { get; set; }
        public Approver Approver { get; set; }
        public bool Visa { get; set; }
        public string Comment { get; set; }
    }
}