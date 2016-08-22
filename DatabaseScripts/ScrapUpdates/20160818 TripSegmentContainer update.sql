
if not exists (select 1 from syscolumns,sysobjects where syscolumns.id = sysobjects.id 
and sysobjects.name = 'TripSegmentContainer'
and syscolumns.name = 'TripSegContainerDriverNotes')
ALTER TABLE dbo.TripSegmentContainer 
ADD TripSegContainerDriverNotes varchar(300)
go

if not exists (select 1 from syscolumns,sysobjects where syscolumns.id = sysobjects.id 
and sysobjects.name = 'HistTripSegmentContainer'
and syscolumns.name = 'TripSegContainerDriverNotes')
ALTER TABLE dbo.HistTripSegmentContainer 
ADD TripSegContainerDriverNotes varchar(300)
go

if not exists (select 1 from syscolumns,sysobjects where syscolumns.id = sysobjects.id 
and sysobjects.name = 'ArcTripSegmentContainer'
and syscolumns.name = 'TripSegContainerDriverNotes')
ALTER TABLE dbo.ArcTripSegmentContainer 
ADD TripSegContainerDriverNotes varchar(300)
go

if not exists (select 1 from syscolumns,sysobjects where syscolumns.id = sysobjects.id 
and sysobjects.name = 'DriverStatus'
and syscolumns.name = 'ServicesFlag')
ALTER TABLE dbo.DriverStatus 
ADD ServicesFlag char(1) NOT NULL DEFAULT 'N'
go

if not exists (select 1 from syscolumns,sysobjects where syscolumns.id = sysobjects.id 
and sysobjects.name = 'DriverHistory'
and syscolumns.name = 'ServicesFlag')
ALTER TABLE dbo.DriverHistory 
ADD ServicesFlag char(1) NOT NULL DEFAULT 'N'
go

if not exists (select 1 from syscolumns,sysobjects where syscolumns.id = sysobjects.id 
and sysobjects.name = 'ArcDriverHistory'
and syscolumns.name = 'ServicesFlag')
ALTER TABLE dbo.ArcDriverHistory 
ADD ServicesFlag char(1) NOT NULL DEFAULT 'N'
go