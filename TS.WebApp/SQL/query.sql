CREATE VIEW ShemaTabel.TimeSheetEmployee
as 
select DISTINCT tsr.idTimeSheet, tsr.idFactStaffHistory  from ShemaTabel.TimeSheetRecord tsr
 

select DISTINCT tsr.idTimeSheet, tsr.idFactStaffHistory  from ShemaTabel.TimeSheetRecord tsr where tsr.idTimeSheet=802



-------------------------------------------------------------------------------------------------------------------------------------
select*from ShemaTabel.TimeSheet
alter table ShemaTabel.TimeSheet drop column ApproveStep
alter table ShemaTabel.TimeSheet drop column idWF
select*from ShemaTabel.TimeSheet

select*from ShemaTabel.TimeSheetRecord
alter table ShemaTabel.TimeSheetRecord drop column IsChecked
select*from ShemaTabel.TimeSheetRecord

ALTER TABLE ShemaTabel.TimeSheetRecord
ADD {COLUMNNAME} {TYPE} {NULL|NOT NULL} 
CONSTRAINT {CONSTRAINT_NAME} DEFAULT {DEFAULT_VALUE}


---------------------------------
select*from ShemaTabel.Approver where id=612

select*from ShemaTabel.Approver where idEmployee is null

delete from ShemaTabel.Approver where idEmployee is null

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

--Ограничение нот налл для сотрудника в таблице таблице согласователь 
ALTER TABLE ShemaTabel.Approver alter column idEmployee  int not null;