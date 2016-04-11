
--установить режимы работ для должностей по-умолчанию, исходя из категории
update [dbo].[PlanStaff] set [IdWorkShedule] = 1 where [idPost] not in (select [Post].id from [dbo].[Post] inner join [dbo].[Category] 
																	on [Post].idCategory = [dbo].[Category].id  where [dbo].[Category].IsPPS = 1)