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
