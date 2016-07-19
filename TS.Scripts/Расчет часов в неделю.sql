alter table [dbo].[FactStaffHistory] alter column WorkHoursInWeek numeric(8,2) null

update [dbo].[FactStaffHistory] set WorkHoursInWeek = 
(select Category.ManHourWork
 from FactStaff 
 inner join PlanStaff on FactStaff.idPlanStaff = PlanStaff.id
 inner join Post on Post.id = PlanStaff.idPost
 inner join Category on Post.idCategory = Category.id
 where FactStaff.id = [dbo].[FactStaffHistory].idFactStaff
 ) * [StaffCount]
 where idFactStaff in (select FactStaff.id
					   from FactStaff inner join Employee on FactStaff.idEmployee = Employee.id
					   where Employee.SexBit = 1)
and WorkHoursInWeek is null and idTypeWork <> 19

update [dbo].[FactStaffHistory] set WorkHoursInWeek = 
(select Category.WomanHourWork
 from FactStaff 
 inner join PlanStaff on FactStaff.idPlanStaff = PlanStaff.id
 inner join Post on Post.id = PlanStaff.idPost
 inner join Category on Post.idCategory = Category.id
 where FactStaff.id = [dbo].[FactStaffHistory].idFactStaff
 ) * [StaffCount]
 where idFactStaff in (select FactStaff.id
					   from FactStaff inner join Employee on FactStaff.idEmployee = Employee.id
					   where Employee.SexBit = 0)
and WorkHoursInWeek is null and idTypeWork <> 19

select * from [FactStaffHistory] where WorkHoursInWeek is null and idTypeWork <> 19