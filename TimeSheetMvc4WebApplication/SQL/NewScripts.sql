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
