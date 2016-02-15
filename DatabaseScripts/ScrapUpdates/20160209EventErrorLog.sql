--Script to add an auto-incrementing SQL int field to the ErrorLog and the EventLog tables.
--The current primary key will be replaced by a indwx
--The new primary key will be the new int field
--Included in the comments is code to revert the primary key back the original

if not exists (select 1 from syscolumns,sysobjects where syscolumns.id = sysobjects.id 
and sysobjects.name = 'eventlog'
and syscolumns.name = 'EventId')
ALTER TABLE eventlog add
	EventId int NOT NULL  IDENTITY(1,1)
GO

ALTER TABLE eventlog
DROP CONSTRAINT [PK_EventLog]; 
GO

ALTER TABLE eventlog
ADD CONSTRAINT [PK_EventLog] PRIMARY KEY CLUSTERED 
(
	EventId ASC 
)ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_EventLog] ON eventlog
(
	EventDateTime ASC,
	EventSeqNo ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
--To revert, run the following
--DROP INDEX [IX_EventLog] ON [dbo].[EventLog]
--GO
--ALTER TABLE eventlog
--DROP CONSTRAINT [PK_EventLog]; 
--GO

--ALTER TABLE eventlog
--ADD CONSTRAINT [PK_EventLog] PRIMARY KEY CLUSTERED 
--(
--	[EventDateTime] ASC,
--	[EventSeqNo] ASC 
--)ON [PRIMARY]
--GO

--ERROR LOG
if not exists (select 1 from syscolumns,sysobjects where syscolumns.id = sysobjects.id 
and sysobjects.name = 'ErrorLog'
and syscolumns.name = 'ErrorId')
ALTER TABLE ErrorLog add
	ErrorId int NOT NULL  IDENTITY(1,1)
GO

ALTER TABLE ErrorLog
DROP CONSTRAINT [PK_ErrorLog]; 
GO

ALTER TABLE ErrorLog
ADD CONSTRAINT [PK_ErrorLog] PRIMARY KEY CLUSTERED 
(
	ErrorId ASC 
)ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_ErrorLog] ON errorlog
(
	ErrorDateTime ASC,
	ErrorSeqNo ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
--To revert, run the following
--DROP INDEX [IX_ErrorLog] ON [dbo].[ErrorLog]
--GO
--ALTER TABLE ErrorLog
--DROP CONSTRAINT [PK_ErrorLog]; 
--GO

--ALTER TABLE ErrorLog
--ADD CONSTRAINT [PK_ErrorLog] PRIMARY KEY CLUSTERED 
--(
--	[ErrorDateTime] ASC ,
--	[ErrorSeqNo] ASC
--)ON [PRIMARY]
--GO
--