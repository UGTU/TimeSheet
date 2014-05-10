using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeSheetMvc4WebApplication.AppDomine
{
    /// <summary>
    /// Запись о согласовании табеля
    /// </summary>
    public class ApproverResult
    {
        public DateTime Date { get; set; }
        public Approver Approver { get; set; }
        public bool Result { get; set; }
    }
}