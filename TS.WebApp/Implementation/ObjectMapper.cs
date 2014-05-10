using TimeSheetMvc4WebApplication.AppDomine;

namespace TimeSheetMvc4WebApplication.Implementation
{
    public class ObjectMapper
    {
        public BaseTimeSheet BaseTimeSheet(TimeSheetView tsv)
        {
            return new BaseTimeSheet
            {
                IdTimeSheet = tsv.id,
                DateComposition = tsv.DateComposition,
                DateBegin = tsv.DateBeginPeriod,
                DateEnd = tsv.DateEndPeriod,
                ApproveStep = tsv.ApproveStep,
                IsCorrection = false,
                IsFake = tsv.IsFake,
                EmployeesCount = tsv.EmployeeCount,
                Department = Department(tsv.Dep.Department)
            };
        }

        public AppDomine.TimeSheet TimeSheet(TimeSheetView tsv)
        {
            return new AppDomine.TimeSheet
            {
                IdTimeSheet = tsv.id,
                DateComposition = tsv.DateComposition,
                DateBegin = tsv.DateBeginPeriod,
                DateEnd = tsv.DateEndPeriod,
                ApproveStep = tsv.ApproveStep,
                IsCorrection = false,
                IsFake = tsv.IsFake,
                EmployeesCount = tsv.EmployeeCount,
                Department = Department(tsv.Dep.Department)
            };
        }

        public AppDomine.Department Department(Department dep)
        {
            return new AppDomine.Department
            {
                IdDepartment = dep.id,
                IdManagerDepartment = dep.idManagerDepartment,
                HasTimeSheet = dep.HasTimeSheet,
                FullName = dep.DepartmentName,
                SmallName = dep.DepartmentSmallName
            };
        }
    }
}