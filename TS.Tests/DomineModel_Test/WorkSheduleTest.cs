using Microsoft.VisualStudio.TestTools.UnitTesting;
using TS.AppDomine.DomineModel;

namespace TimeSheetMvc4WebApplication.Tests.DomineModel_Test
{
    [TestClass]
    public class WorkSheduleTest
    {
        private TS.AppDomine.DomineModel.WorkShedule a;
        private TS.AppDomine.DomineModel.WorkShedule b;
        private TS.AppDomine.DomineModel.WorkShedule c;

        [TestInitialize]
        public void WorkShedule_Test()
        {
            a = new TS.AppDomine.DomineModel.WorkShedule
            {
                Id = 1,
                WorkSheduleName = "Пятидневная рабочая неделя"
            };
            b = new TS.AppDomine.DomineModel.WorkShedule
            {
                Id = 2,
                WorkSheduleName = "Шестидневная рабочая неделя"
            };
            c = new TS.AppDomine.DomineModel.WorkShedule
            {
                Id = 1,
                WorkSheduleName = "Пятидневная рабочая неделя"
            };
        }
        
        [TestMethod]
        public void WorkShedule_Equals_True()
        {
            Assert.IsTrue(a.Equals(c));
        }

        [TestMethod]
        public void WorkShedule_Equals_False()
        {
            Assert.IsFalse(a.Equals(b));
        }

        [TestMethod]
        public void WorkShedule_Comparison_True()
        {
            Assert.IsTrue(a == c);
            Assert.IsTrue(a != b);
        }

        [TestMethod]
        public void WorkShedule_Comparison_False()
        {
            Assert.IsFalse(a == b);
            Assert.IsFalse(a != c);            
        }
    }
}
