using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TS.AppDomine.Abstract;
using TS.AppDomine.DomineModel;
using TS.AppDomine.Implementation;

namespace TimeSheetMvc4WebApplication.Tests.Implementation_Test
{
    [TestClass]
    public class TimeSheetGeneratorTest
    {
        private TimeSheetGenerator _generator;

        [TestInitialize]
        public void TimeSheetGenerator_Init()
        {
            var mock = new Mock<IDataProvider>();
            mock.Setup(s => s.SaveTimeSheet(It.IsAny<BaseTimeSheet>())).Returns(true);
            //_generator = new TimeSheetGenerator(mock.Object);
        }

        [TestMethod]
        public void TimeSheetGenerator_Save()
        {
            //var ts = _generator.Save();
            //Assert.IsFalse(ts.IsFake);
            //ts = _generator.Save(false);
            //Assert.IsFalse(ts.IsFake);
            //ts = _generator.Save(true);
            //Assert.IsTrue(ts.IsFake);
        }
    }
}
