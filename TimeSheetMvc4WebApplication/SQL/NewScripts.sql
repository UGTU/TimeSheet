-----	Статус дня
select*from ShemaTabel.DayStatus
Go
ALTER TABLE ShemaTabel.DayStatus
ADD IsVisible bit NOT NULL DEFAULT(1)
go
select*from ShemaTabel.DayStatus
update ShemaTabel.DayStatus
set IsVisible = 0
where id = 22
select*from ShemaTabel.DayStatus

-----	Фейк табель
select*from ShemaTabel.TimeSheet
Go
ALTER TABLE ShemaTabel.TimeSheet
ADD IsFake bit NOT NULL DEFAULT(0)
go
select*from ShemaTabel.TimeSheet

-----	Main department
select*from Department
Go
ALTER TABLE ShemaTabel.TimeSheet
ADD IsFake bit NOT NULL DEFAULT(0)
go
select*from ShemaTabel.TimeSheet

go
create VIEW ShemaTabel.TimeSheetView AS
SELECT id,idDepartment,idCreater,DateBeginPeriod,DateEndPeriod,DateComposition,IsFake, 
	(select count(DISTINCT tsr.idFactStaffHistory)
	 from ShemaTabel.TimeSheetRecord tsr 
	 where tsr.idTimeSheet=ts.id) as EmployeeCount,
	 isnull((select top 1 at.ApproveNumber
	 from ShemaTabel.TimeSheetApproval tsa
		inner join ShemaTabel.Approver a on tsa.idApprover=a.id
		inner join ShemaTabel.ApproverType at on at.id=a.idApproverType
		where tsa.idTimeSheet =ts.id and tsa.Result=1
		order by  tsa.ApprovalDate desc
	 ),0) as ApproveStep
FROM ShemaTabel.TimeSheet ts
go


select*from ShemaTabel.TimeSheetView





go
SELECT id,idDepartment,idCreater,DateBeginPeriod,DateEndPeriod,DateComposition, 
	(select count(DISTINCT tsr.idFactStaffHistory)
	 from ShemaTabel.TimeSheetRecord tsr 
	 where tsr.idTimeSheet=ts.id) as EmployeeCount,
	 isnull((select top 1 at.ApproveNumber
	 from ShemaTabel.TimeSheetApproval tsa
		inner join ShemaTabel.Approver a on tsa.idApprover=a.id
		inner join ShemaTabel.ApproverType at on at.id=a.idApproverType
		where tsa.idTimeSheet =ts.id and tsa.Result=1
		order by  tsa.ApprovalDate desc
	 ),0) as ApproveStep
FROM ShemaTabel.TimeSheet ts
go


