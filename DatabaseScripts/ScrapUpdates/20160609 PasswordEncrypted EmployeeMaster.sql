if not exists (select 1 from syscolumns,sysobjects where syscolumns.id = sysobjects.id 
and sysobjects.name = 'EmployeeMaster'
and syscolumns.name = 'PasswordEncrypted')
ALTER TABLE dbo.EmployeeMaster 
ADD PasswordEncrypted varchar(100)
go



