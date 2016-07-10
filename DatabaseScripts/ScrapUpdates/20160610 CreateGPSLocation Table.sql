--Change C:\Scrap\Router to location of router folder
if not exists (select 1 from [Preferences] where parameter = 'DEFRouterPath')
INSERT INTO [Preferences] VALUES('0000','DEFRouterPath','C:\Scrap\Router','Router Path')
else
update preferences set ParameterValue = 'C:\Scrap\Router' where parameter = 'DEFRouterPath'
GO

IF OBJECT_ID('[dbo].[GPSLocation]', 'U') IS NOT NULL 
  DROP TABLE [dbo].[GPSLocation]
GO

CREATE TABLE [dbo].[GPSLocation](
	[GPSSeqId] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [varchar](10) NOT NULL,
	[TerminalId] [varchar](10) NOT NULL,
	[RegionId] [varchar](10) NOT NULL,
	[GPSID] [smallint] NULL,
	[GPSDateTime] [datetimeoffset](7) NULL,
	[GPSLatitude] [int] NULL,
	[GPSLongitude] [int] NULL,
	[GPSSpeed] [smallint] NULL,
	[GPSHeading] [smallint] NULL,
	[GPSSendFlag] [smallint] NULL

 CONSTRAINT [PK_GPSLocation] PRIMARY KEY CLUSTERED 
(
	[GPSSeqId] ASC
)
 ON [PRIMARY])
GO
