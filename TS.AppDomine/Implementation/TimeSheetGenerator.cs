using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS.AppDomine.Abstract;
using TS.AppDomine.DomineModel;

namespace TS.AppDomine.Implementation
{
    public class TimeSheetGenerator : ITimeSheetGenerator
    {
        private readonly IDataProvider _provider;
        private readonly BaseTimeSheet _timeSheet;

        public TimeSheetGenerator(IDataProvider provider)
        {
            _provider = provider;
            _timeSheet = new BaseTimeSheet();
        }

        public void GenerateTimeSheet(IEnumerable<Employee> employees = null)
        {
            throw new NotImplementedException();
        }

        public BaseTimeSheet Save(bool saveAsFake=false)
        {
            _timeSheet.IsFake = saveAsFake;
            return _provider.SaveTimeSheet(_timeSheet) ? _timeSheet : null;
        }
    }
}
