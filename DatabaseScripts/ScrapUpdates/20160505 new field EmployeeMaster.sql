if not exists (select 1 from syscolumns,sysobjects where syscolumns.id = sysobjects.id 
and sysobjects.name = 'EmployeeMaster'
and syscolumns.name = 'RefreshTripListRate')
ALTER TABLE dbo.EmployeeMaster 
ADD RefreshTripListRate smallint
go

if not exists (select 1 from syscolumns,sysobjects where syscolumns.id = sysobjects.id 
and sysobjects.name = 'EmployeeMaster'
and syscolumns.name = 'DisplayContainerContents')
ALTER TABLE dbo.EmployeeMaster 
ADD DisplayContainerContents char(1)
go


