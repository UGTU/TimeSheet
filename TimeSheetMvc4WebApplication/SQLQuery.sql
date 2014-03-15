--Удаление табеля
declare @idTimeSheet int=636
delete from ShemaTabel.TimeSheetRecord where idTimeSheet=@idTimeSheet
delete from ShemaTabel.TimeSheetApproval where idTimeSheet=@idTimeSheet
delete from ShemaTabel.TimeSheet where id=@idTimeSheet

delete from ShemaTabel.TimeSheetRecord 
delete from ShemaTabel.TimeSheetApproval
delete from ShemaTabel.TimeSheet 

--Вывести администраторов
select*from ShemaTabel.Approver a 
	inner join Employee e on e.id=a.idEmployee
	where idApproverType=4

--Добавить администратора по логину
select* from Employee e where e.EmployeeLogin='ochernova@ugtu.net'

declare @idEmpl int = 5575
insert into ShemaTabel.Approver
values(GETDATE(),null,4,51,@idEmpl)
------------------------------------------------------------
declare @idEmpl int = -1000
declare @emplLogin varchar(max)  = 'ochernova@ugtu.net'
select @emplLogin

select* from Employee e where e.EmployeeLogin=@emplLogin

select @idEmpl   =  e.id from Employee e where e.EmployeeLogin=@emplLogin
select @idEmpl

insert into ShemaTabel.Approver
values(GETDATE(),null,4,51,@idEmpl)
------------------------------------------------------------------


declare @emplLogin varchar
set @emplLogin  = 'ochernova@ugtu.net'
select @emplLogin
select * from Employee e where e.EmployeeLogin=@emplLogin

select * from Employee e where e.LastName='Чернова'

select*from ShemaTabel.ApproverType

select*from Employee



--##############################################################################################################
select*from ShemaTabel.DayStatus


update FactStaff
set IDShedule = 1
where  id in (select f.id from FactStaff f inner join PlanStaff p on f.idPlanStaff = p.id
						inner join Department d on d.id = p.idDepartment
						inner join Employee e on e.id=f.idEmployee
						where d.id = 39 and f.DateEnd is NULL )

update PlanStaff
set IDShedule = null
where  id in (select p.id from FactStaff f inner join PlanStaff p on f.idPlanStaff = p.id
						inner join Department d on d.id = p.idDepartment
						inner join Employee e on e.id=f.idEmployee
						where d.id = 39 and f.DateEnd is NULL )

						select * from FactStaff f inner join PlanStaff p on f.idPlanStaff = p.id
						inner join Department d on d.id = p.idDepartment
						inner join Employee e on e.id=f.idEmployee
						where d.id = 39 and f.DateEnd is NULL




--###############################################################################################################

select*from ShemaTabel.GetFactStaffForTimeSheet ('1.10.2012','31.10.2012') where idDepartment = 39

select*from TimeSheetScheduleType

select* from dbo.FactStaffWithHistory

delete from ShemaTabel.TimeSheetApproval
delete from ShemaTabel.TimeSheetRecord
delete from ShemaTabel.TimeSheet

select*from Category order by OrderBy

select*from Post order by PostName


select*from ShemaTabel.Approver

select*from ShemaTabel.TimeSheetApproval





select*from ShemaTabel.WorkShedule

--id	NameShedule
--1	Пятидневная рабочая неделя
--2	Шестидневная рабочая неделя


select*from Department order by DepartmentName

--39	Информационно-вычислительный центр	ИВЦ	3	9	148	2006-01-01 00:00:00.000	NULL	NULL	46	NULL	50	30	11EF3FD5-664D-E111-96A2-0018FE865BEC

select*from FactStaff f inner join PlanStaff p on f.idPlanStaff = p.id
						inner join Department d on d.id = p.idDepartment
						where d.id = 39 and p.DateEnd is NULL;


select * from FactStaff where   DateEnd is NULL and idPlanStaff in (select PlanStaff.id from PlanStaff,Department where Department.id=PlanStaff.idDepartment and Department.id=39)


update FactStaff
set IDShedule = 1
where   DateEnd is NULL and idPlanStaff in (select PlanStaff.id from PlanStaff,Department where Department.id=PlanStaff.idDepartment and Department.id=39)


update FactStaff
set IDShedule = 1
where  idPlanStaff in (select p.id from FactStaff f inner join PlanStaff p on f.idPlanStaff = p.id
						inner join Department d on d.id = p.idDepartment
						where d.id = 39 and p.DateEnd is NULL)

select p.id from FactStaff f inner join PlanStaff p on f.idPlanStaff = p.id
						inner join Department d on d.id = p.idDepartment
						where d.id = 51 and p.DateEnd is NULL and p.IDShedule is null


declare   @idts int  = 0
exec ShemaTabel.CreateTimeSheet 39,5,'1.12.2012','31.12.2012','20.12.2012',@idts

select*from ShemaTabel.TimeSheet

select*from ShemaTabel.TimeSheetRecord

select*from ShemaTabel.Approver a inner join Employee e on a.idEmployee=e.id


delete from ShemaTabel.TimeSheetRecord

delete from ShemaTabel.TimeSheetApproval

delete from ShemaTabel.TimeSheet


select * from FactStaff f inner join PlanStaff p on f.idPlanStaff = p.id
						inner join Department d on d.id = p.idDepartment
						inner join Employee e on e.id=f.idEmployee
						where d.id = 51 and f.DateEnd is NULL order by e.LastName;


update FactStaff
set IDShedule = 1
where  id in (select f.id from FactStaff f inner join PlanStaff p on f.idPlanStaff = p.id
						inner join Department d on d.id = p.idDepartment
						inner join Employee e on e.id=f.idEmployee
						where d.id = 39 and f.DateEnd is NULL )





declare @PeriodBegin	DATETIME='1.10.2012'
declare @PeriodEnd	DATETIME='31.10.2011'
	SELECT DISTINCT FactStaff.id, FactStaff.StaffCount, idDepartment,WorkType.IsReplacement, WorkType.id, idPost,
	idPlanStaff, idEmployee, 0, ISNULL(FactStaff.DateBegin, @PeriodBegin), 
	ISNULL(FactStaff.DateEnd,@PeriodEnd), FactStaff.id IDShedule,SexBit, idCategory=Post.idCategory
	FROM  dbo.FactStaffWithHistory as FactStaff, PlanStaff, dbo.WorkType,dbo.Employee,dbo.Post
	WHERE
		((FactStaff.DateBegin<=@PeriodBegin AND (FactStaff.DateEnd>=@PeriodBegin OR FactStaff.DateEnd IS NULL))
								OR (FactStaff.DateBegin>=@PeriodBegin AND FactStaff.DateBegin<=@PeriodEnd))
		AND FactStaff.idPlanStaff=PlanStaff.id
		AND FactStaff.idTypeWork=WorkType.id
		and idEmployee=dbo.Employee.id
		and PlanStaff.idPost = Post.id
		and PlanStaff.idDepartment=39



select*from ShemaTabel.WorkSheduleEvent

select*from FactStaff


select*from ShemaTabel.DayStatus

select*from ShemaTabel.WorkShedule

select*from Category



select e.LastName from FactStaffCurrent f inner join PlanStaff p on p.id = f.idPlanStaff
			inner join Employee e on e.id = f.idEmployee
			where p.idDepartment = 39 and f.DateEnd  IS NULL 
			order by e.LastName

select e.LastName from FactStaff f inner join PlanStaff p on p.id = f.idPlanStaff
			inner join Employee e on e.id = f.idEmployee
			where p.idDepartment = 39 and f.DateEnd  IS NULL 
			order by e.LastName


select*from Employee where LastName='Николаева'


select*from OK_Otpusk


select*from OK_Otpuskvid

select*from ShemaTabel.DayStatus

select*from OK_Otpuskvid o inner join ShemaTabel.DayStatus d on o.idDayStatus=d.id


update OK_Otpuskvid
set idDayStatus = 23
where idotpuskvid=19
select*from OK_Otpuskvid o inner join ShemaTabel.DayStatus d on o.idDayStatus=d.id


select*from OK_Otpuskvid

select*from ShemaTabel.DayStatus


select*from Category


select*from WorkType

select*from FactStaffCurrent

select*from WorkSuperType


select*from Post order by PostName

update Post
set ManagerBit=1
where id = 42

select*from ShemaTabel.WorkShedule

select*from ShemaTabel.TimeSheet where id=305

select*from ShemaTabel.Approver 

update ShemaTabel.Approver
set DateEnd = '22.11.2012'
where id = 42




--Удаление табеля
declare @idTimeSheet int=382
delete from ShemaTabel.TimeSheetRecord where idTimeSheet=@idTimeSheet
delete from ShemaTabel.TimeSheetApproval where idTimeSheet=@idTimeSheet
delete from ShemaTabel.TimeSheet where id=@idTimeSheet

delete from ShemaTabel.TimeSheetRecord 
delete from ShemaTabel.TimeSheetApproval
delete from ShemaTabel.TimeSheet 


select*from ShemaTabel.TimeSheet

--Согласование
declare @idTimeSheet int=353
select*from ShemaTabel.TimeSheetApproval ta where ta.idTimeSheet=@idTimeSheet

delete from ShemaTabel.TimeSheetApproval where idTimeSheet=@idTimeSheet


select*from ShemaTabel.DayStatus
insert into ShemaTabel.DayStatus
values('ОКД','Оплачиваемый отпуск за счет средств работодателя (п. 5.26 колдоговора - похороны близких родственников)')


select*from FactStaff

select*from FactStaffHistory



select*from FactStaff

select*from FactStaffHistory order by DateBegin


select*from ShemaTabel.TimeSheetRecord


alter table ShemaTabel.TimeSheetRecord add  idFactStaffHistory int

alter table ShemaTabel.TimeSheetRecord add constraint TimeSheetRecordFactStaffHistoryFK foreign key (idFactStaffHistory) references FactStaffHistory (id)

alter table ShemaTabel.TimeSheetRecord drop column IDTimeSheetRecord

alter table ShemaTabel.TimeSheetRecord drop constraint TimeSheetRecordTablePrimary

alter table ShemaTabel.TimeSheetRecord alter column  idFactStaffHistory int Not null


select*from FactStaffWithHistory order by id

alter table ShemaTabel.TimeSheetRecord add  IdTimeSheetRecord  uniqueidentifier DEFAULT newsequentialid() NOT null

--alter table ShemaTabel.TimeSheetRecord drop constraint TimeSheetRecordTablePrimary not null

alter table ShemaTabel.TimeSheetRecord alter column  IdTimeSheetRecord uniqueidentifier Not null

alter table ShemaTabel.TimeSheetRecord add constraint TimeSheetRecordTablePrimary Primary key (IdTimeSheetRecord)


 



go
alter PROC ShemaTabel.TimeSheetRecordInsert(@ValidXMLInput XML)
AS BEGIN
       INSERT INTO ShemaTabel.TimeSheetRecord(RecordDate,JobTimeCount,idTimeSheet,idDayStatus,IsChecked,idFactStaffHistory,IdTimeSheetRecord)
       SELECT	Col.value('(RecordDate/text())[1]','datetime') ,
				Col.value('(JobTimeCount/text())[1]','FLOAT'),
				Col.value('(idTimeSheet/text())[1]','INT'),
				Col.value('(idDayStatus/text())[1]','INT'),
				Col.value('(IsChecked/text())[1]','BIT'),
				Col.value('(idFactStaffHistory/text())[1]','INT'),
				Col.value('(IdTimeSheetRecord/text())[1]','UNIQUEIDENTIFIER')
       FROM @ValidXMLInput.nodes('//TimeSheetRecords/Record') Tab(Col)
END




alter table ShemaTabel.TimeSheet drop constraint DateAndDepartmentUnique


select*from ShemaTabel.TimeSheetApproval a where a.idTimeSheet=600	

delete from ShemaTabel.TimeSheetApproval  where idTimeSheet=600	

select*from ShemaTabel.Approver where id=70

select*from ShemaTabel.Approver where idDepartment = 43

select*from ShemaTabel.ApproverType



select*from FactStaffWithHistory f inner join PlanStaff p 
	on f.idPlanStaff=p.id


select*from ShemaTabel.FactStaffWithHistory

select*from ShemaTabel.DayStatus d where d.DayStatusName = 'X'

update ShemaTabel.DayStatus
set DayStatusName = 'Х'
where DayStatusName = 'X'

select*from ShemaTabel.DayStatus d where d.DayStatusName = 'Х'
select*from ShemaTabel.DayStatus d where d.DayStatusName = 'Х'


select*from ShemaTabel.TimeSheetRecord

declare @xml xml = cast('
<TimeSheetRecords>
 <Record>
  <IdTimeSheetRecord>50fd85b3-490d-47a0-8040-1e4159deea41</IdTimeSheetRecord> 
  <JobTimeCount>0</JobTimeCount> 
  <idTimeSheet>684</idTimeSheet> 
  <idDayStatus>6</idDayStatus> 
  <IsChecked>false</IsChecked> 
  <idFactStaffHistory>2749</idFactStaffHistory> 
  <RecordDate>2013-12-16</RecordDate> 
 </Record>
</TimeSheetRecords>' as xml)

exec ShemaTabel.TimeSheetRecordInsert @xml

select*from ShemaTabel.TimeSheetRecord where IdTimeSheetRecord='50fd85b3-490d-47a0-8040-1e4159deea41'

delete from ShemaTabel.TimeSheetRecord where IdTimeSheetRecord='50fd85b3-490d-47a0-8040-1e4159deea41'



select convert(varchar(10),getdate(),112)

select convert(char(10), GETDATE(), 127)