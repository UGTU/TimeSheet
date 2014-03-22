
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