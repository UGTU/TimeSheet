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
SELECT id,idDepartment,idCreater,DateBeginPeriod,DateEndPeriod,DateComposition,IsFake, 
	(select count(DISTINCT tsr.idFactStaffHistory)
	 from ShemaTabel.TimeSheetRecord tsr 
	 where tsr.idTimeSheet=ts.id) as EmployeeCount,
	 isnull((select top 1 iif(tsa.Result=1,at.ApproveNumber,0)
	 from ShemaTabel.TimeSheetApproval tsa
		inner join ShemaTabel.Approver a on tsa.idApprover=a.id
		inner join ShemaTabel.ApproverType at on at.id=a.idApproverType
		---
		where tsa.idTimeSheet =ts.id-- and tsa.Result=1
		---
		order by  tsa.ApprovalDate desc
	 ),0) as ApproveStep
FROM ShemaTabel.TimeSheet ts
where ts.id=1803
go




--модифицированая
go
alter VIEW ShemaTabel.TimeSheetView AS
SELECT id,idDepartment,idCreater,DateBeginPeriod,DateEndPeriod,DateComposition,IsFake, 
	(select count(DISTINCT tsr.idFactStaffHistory)
	 from ShemaTabel.TimeSheetRecord tsr 
	 where tsr.idTimeSheet=ts.id) as EmployeeCount,
	 isnull((select top 1 iif(tsa.Result=1,at.ApproveNumber,0)
	 from ShemaTabel.TimeSheetApproval tsa
		inner join ShemaTabel.Approver a on tsa.idApprover=a.id
		inner join ShemaTabel.ApproverType at on at.id=a.idApproverType
		where tsa.idTimeSheet =ts.id
		order by  tsa.ApprovalDate desc
	 ),0) as ApproveStep
FROM ShemaTabel.TimeSheet ts
go

select*from ShemaTabel.TimeSheetView



--Назначаем согласователями начальников структурных подразделений
insert into ShemaTabel.Approver
--values(GETDATE(),null,2, idDep,UdEmpl)
select GETDATE(),null,2, idDepartment,idEmployee from GetStaffByPeriod(getDate(),getDate()) fs 
	inner join Post p on p.id=fs.idPost
	where p.ManagerBit=1 and fs.idDepartment not in( select a.idDepartment from ShemaTabel.Approver a where a.DateEnd is null and a.idApproverType=2)


--удалить всех начальников структурных подразделение
update ShemaTabel.Approver 
set DateEnd= GETDATE()
where  DateEnd is null and idApproverType=2
