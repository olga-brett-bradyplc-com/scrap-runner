USE [scraprunnerhost]
GO

/****** Object: Schema [exp_view]    ******/
CREATE SCHEMA [exp_view]
GO

/****** Object: Schema [exp_credential]    ******/
CREATE SCHEMA [exp_credential]
GO


/****** Object:  Table [dbo].[ExplorerVersion]    Script Date: 08/06/2012 09:01:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExplorerVersion](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Version] [nvarchar](50) NOT NULL,
	[Major] [int] NOT NULL,
	[Minor] [int] NOT NULL,
	[Patch] [int] NOT NULL,
	[Compile] [int] NOT NULL,
	[TimeStamp] [datetime] NOT NULL,
 CONSTRAINT [PK_ExplorerVersion] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UK_ExplorerVersion_Version] UNIQUE NONCLUSTERED 
(
	[Version] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


/****** Object:  Table [exp_view].[DataService]    Script Date: 08/06/2012 13:41:13 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [exp_view].[DataService](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](64) NOT NULL,
	[System] [nvarchar](64) NOT NULL,
	[ServiceAddress] [nvarchar](256) NOT NULL,
	[ClientAddress] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_DataService] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UK_DataService_ClientAddress] UNIQUE NONCLUSTERED 
(
	[ClientAddress] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UK_DataService_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UK_DataService_ServiceAddress] UNIQUE NONCLUSTERED 
(
	[ServiceAddress] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UK_DataService_System] UNIQUE NONCLUSTERED 
(
	[System] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [exp_view].[DataService]  WITH CHECK ADD  CONSTRAINT [FK_DataService_DataService] FOREIGN KEY([Id])
REFERENCES [exp_view].[DataService] ([Id])
GO

ALTER TABLE [exp_view].[DataService] CHECK CONSTRAINT [FK_DataService_DataService]
GO


/****** Object:  Table [exp_view].[Presentation]    Script Date: 08/06/2012 13:41:36 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [exp_view].[Presentation](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](64) NOT NULL,
	[System] [nvarchar](64) NOT NULL,
	[BaseType] [nvarchar](64) NOT NULL,
 CONSTRAINT [PK_Presentation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UK_Presentation_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO



/****** Object:  Table [exp_view].[GraphPresentation]    Script Date: 08/06/2012 13:41:56 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [exp_view].[GraphPresentation](
	[PresentationId] [bigint] NOT NULL,
	[XAxis] [nvarchar](64) NOT NULL,
	[IsHorizontal] [bit] NOT NULL,
 CONSTRAINT [PK_GraphPresentation] PRIMARY KEY CLUSTERED 
(
	[PresentationId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [exp_view].[GraphPresentation]  WITH CHECK ADD  CONSTRAINT [FK_GraphPresentation_Presentation] FOREIGN KEY([PresentationId])
REFERENCES [exp_view].[Presentation] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [exp_view].[GraphPresentation] CHECK CONSTRAINT [FK_GraphPresentation_Presentation]
GO



/****** Object:  Table [exp_view].[GridPresentation]    Script Date: 08/06/2012 13:42:08 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [exp_view].[GridPresentation](
	[PresentationId] [bigint] NOT NULL,
	[OrderBy] [nvarchar](256) NULL,
 CONSTRAINT [PK_GridPresentation] PRIMARY KEY CLUSTERED 
(
	[PresentationId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [exp_view].[GridPresentation]  WITH CHECK ADD  CONSTRAINT [FK_GridPresentation_Presentation] FOREIGN KEY([PresentationId])
REFERENCES [exp_view].[Presentation] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [exp_view].[GridPresentation] CHECK CONSTRAINT [FK_GridPresentation_Presentation]
GO


/****** Object:  Table [exp_view].[Property]    Script Date: 08/06/2012 13:42:22 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [exp_view].[Property](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](64) NOT NULL,
	[Summary] [nvarchar](256) NULL,
	[GridPresentationId] [bigint] NULL,
 CONSTRAINT [PK_Property] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [exp_view].[Property]  WITH CHECK ADD  CONSTRAINT [FK_Property_GridPresentation] FOREIGN KEY([GridPresentationId])
REFERENCES [exp_view].[GridPresentation] ([PresentationId])
GO

ALTER TABLE [exp_view].[Property] CHECK CONSTRAINT [FK_Property_GridPresentation]
GO


/****** Object:  Table [exp_view].[YAxis]    Script Date: 08/06/2012 13:42:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [exp_view].[YAxis](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Property] [nvarchar](64) NOT NULL,
	[Type] [nvarchar](64) NOT NULL,
	[AdditionalAxisNumber] [int] NULL,
	[GraphPresentationId] [bigint] NULL,
 CONSTRAINT [PK_YAxis] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [exp_view].[YAxis]  WITH CHECK ADD  CONSTRAINT [FK_YAxis_GraphPresentation] FOREIGN KEY([GraphPresentationId])
REFERENCES [exp_view].[GraphPresentation] ([PresentationId])
GO

ALTER TABLE [exp_view].[YAxis] CHECK CONSTRAINT [FK_YAxis_GraphPresentation]
GO


/****** Object:  Table [exp_view].[Selection]    Script Date: 08/06/2012 13:43:04 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [exp_view].[Selection](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](64) NOT NULL,
	[System] [nvarchar](64) NOT NULL,
	[BaseType] [nvarchar](64) NOT NULL,
	[Filter] [nvarchar](2048) NULL,
 CONSTRAINT [PK_Selection] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UK_Selection_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO




/****** Object:  Table [exp_view].[ExplorerView]    Script Date: 08/06/2012 13:43:23 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [exp_view].[ExplorerView]    (
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](64) NOT NULL,
	[SelectionId] [bigint] NOT NULL,
	[PresentationId] [bigint] NOT NULL,
 CONSTRAINT [PK_View] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UK_View_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [exp_view].[ExplorerView]    WITH CHECK ADD  CONSTRAINT [FK_View_Presentation] FOREIGN KEY([PresentationId])
REFERENCES [exp_view].[Presentation] ([Id])
GO

ALTER TABLE [exp_view].[ExplorerView]    CHECK CONSTRAINT [FK_View_Presentation]
GO

ALTER TABLE [exp_view].[ExplorerView]    WITH CHECK ADD  CONSTRAINT [FK_View_Selection] FOREIGN KEY([SelectionId])
REFERENCES [exp_view].[Selection] ([Id])
GO

ALTER TABLE [exp_view].[ExplorerView]    CHECK CONSTRAINT [FK_View_Selection]
GO



/****** Object:  Table [exp_view].[ViewParameter]    Script Date: 08/06/2012 13:43:36 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [exp_view].[ViewParameter](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](64) NOT NULL,
	[Operator] [nvarchar](20) NOT NULL,
	[ViewId] [bigint] NULL,
 CONSTRAINT [PK_ViewParameter] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [exp_view].[ViewParameter]  WITH CHECK ADD  CONSTRAINT [FK_ViewParameter_View] FOREIGN KEY([ViewId])
REFERENCES [exp_view].[ExplorerView]    ([Id])
GO

ALTER TABLE [exp_view].[ViewParameter] CHECK CONSTRAINT [FK_ViewParameter_View]
GO


/****** Object:  Table [exp_view].[ViewParameterValue]    Script Date: 08/06/2012 13:43:51 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [exp_view].[ViewParameterValue](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ExplorerUser] [nvarchar](256) NOT NULL,
	[Value] [nvarchar](64) NOT NULL,
	[Value2] [nvarchar](64) NOT NULL,
	[Position] [int] NOT NULL,
	[ValueType] [nvarchar](64) NOT NULL,
	[ViewParameterId] [bigint] NULL,
 CONSTRAINT [PK_ViewParameterValue] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [exp_view].[ViewParameterValue]  WITH CHECK ADD  CONSTRAINT [FK_ViewParameterValue_ViewParameter] FOREIGN KEY([ViewParameterId])
REFERENCES [exp_view].[ViewParameter] ([Id])
GO

ALTER TABLE [exp_view].[ViewParameterValue] CHECK CONSTRAINT [FK_ViewParameterValue_ViewParameter]
GO


/****** Object:  Table [exp_credential].[TrinityCredential]    Script Date: 08/06/2012 14:07:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [exp_credential].[TrinityCredential](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[SiteUser] [nvarchar](256) NOT NULL,
	[UserName] [nvarchar](256) NOT NULL,
	[Password] [nvarchar](256) NOT NULL,
	[DataSource] [nvarchar](64) NOT NULL,
	[IsDefault] [bit] NULL,
 CONSTRAINT [PK_TrinityCredential] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO



/****** Insert: Version Number ******/ 
INSERT INTO [dbo].[ExplorerVersion]
           ([Version]
           ,[Major]
           ,[Minor]
           ,[Patch]
           ,[Compile]
           ,[TimeStamp])
     VALUES
           ('2012.1.0.1'
           ,2012
           ,1 
           ,0
           ,1
           ,getdate())
GO

IF ((SELECT [Version] FROM [dbo].[ExplorerVersion] WHERE [Version]='2012.1.0.1') is null)
	raiserror('Cannot upgrade to 2012.3.0.1 before database is upgraded to 2012.1.0.1', 20, -1) with log
ELSE IF ((SELECT [Version] FROM [dbo].[ExplorerVersion] WHERE [Version]='2012.3.0.1') is not null)
	raiserror('Cannot upgrade to 2012.3.0.1 as database is already at this version or greater', 20, -1) with log
GO


-- move the version table under a new schema
CREATE SCHEMA [exp_common] AUTHORIZATION [dbo]
GO
ALTER SCHEMA exp_common TRANSFER dbo.ExplorerVersion
GO

BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT

-- convert to hilo strategy
-- create NextHigh tables
-- Credential NextHigh table
BEGIN TRANSACTION
GO
CREATE TABLE exp_credential.NextHigh
	(
	Id int NOT NULL IDENTITY (1, 1),
	NextHigh bigint NOT NULL,
	EntityName nvarchar(30) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE exp_credential.NextHigh ADD CONSTRAINT
	PK_NextHigh_1 PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE UNIQUE NONCLUSTERED INDEX UK_Cred_NextHigh_EntityName ON exp_credential.NextHigh
	(
	EntityName
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE exp_credential.NextHigh SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'exp_credential.NextHigh', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'exp_credential.NextHigh', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'exp_credential.NextHigh', 'Object', 'CONTROL') as Contr_Per 

-- Views NextHigh table
BEGIN TRANSACTION
GO
CREATE TABLE exp_view.NextHigh
	(
	Id int NOT NULL IDENTITY (1, 1),
	NextHigh bigint NOT NULL,
	EntityName nvarchar(30) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE exp_view.NextHigh ADD CONSTRAINT
	PK_NextHigh PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE UNIQUE NONCLUSTERED INDEX UK_View_NextHigh_EntityName ON exp_view.NextHigh
	(
	EntityName
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE exp_view.NextHigh SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'exp_view.NextHigh', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'exp_view.NextHigh', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'exp_view.NextHigh', 'Object', 'CONTROL') as Contr_Per 

-- Remove Identity from tables
-- DataService
BEGIN TRANSACTION
GO
CREATE TABLE exp_view.Tmp_DataService
	(
	Id bigint NOT NULL,
	Name nvarchar(64) NOT NULL,
	System nvarchar(64) NOT NULL,
	ServiceAddress nvarchar(256) NOT NULL,
	ClientAddress nvarchar(256) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE exp_view.Tmp_DataService SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM exp_view.DataService)
	 EXEC('INSERT INTO exp_view.Tmp_DataService (Id, Name, System, ServiceAddress, ClientAddress)
		SELECT Id, Name, System, ServiceAddress, ClientAddress FROM exp_view.DataService WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE exp_view.DataService
	DROP CONSTRAINT FK_DataService_DataService
GO
DROP TABLE exp_view.DataService
GO
EXECUTE sp_rename N'exp_view.Tmp_DataService', N'DataService', 'OBJECT' 
GO
ALTER TABLE exp_view.DataService ADD CONSTRAINT
	PK_DataService PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE exp_view.DataService ADD CONSTRAINT
	UK_DataService_System UNIQUE NONCLUSTERED 
	(
	System
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE exp_view.DataService ADD CONSTRAINT
	UK_DataService_ServiceAddress UNIQUE NONCLUSTERED 
	(
	ServiceAddress
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE exp_view.DataService ADD CONSTRAINT
	UK_DataService_Name UNIQUE NONCLUSTERED 
	(
	Name
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE exp_view.DataService ADD CONSTRAINT
	UK_DataService_ClientAddress UNIQUE NONCLUSTERED 
	(
	ClientAddress
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE exp_view.DataService ADD CONSTRAINT
	FK_DataService_DataService FOREIGN KEY
	(
	Id
	) REFERENCES exp_view.DataService
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
select Has_Perms_By_Name(N'exp_view.DataService', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'exp_view.DataService', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'exp_view.DataService', 'Object', 'CONTROL') as Contr_Per 

-- ExplorerView
BEGIN TRANSACTION
GO
ALTER TABLE exp_view.ExplorerView
	DROP CONSTRAINT FK_View_Selection
GO
ALTER TABLE exp_view.Selection SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'exp_view.Selection', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'exp_view.Selection', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'exp_view.Selection', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE exp_view.ExplorerView
	DROP CONSTRAINT FK_View_Presentation
GO
ALTER TABLE exp_view.Presentation SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'exp_view.Presentation', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'exp_view.Presentation', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'exp_view.Presentation', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
CREATE TABLE exp_view.Tmp_ExplorerView
	(
	Id bigint NOT NULL,
	Name nvarchar(64) NOT NULL,
	SelectionId bigint NOT NULL,
	PresentationId bigint NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE exp_view.Tmp_ExplorerView SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM exp_view.ExplorerView)
	 EXEC('INSERT INTO exp_view.Tmp_ExplorerView (Id, Name, SelectionId, PresentationId)
		SELECT Id, Name, SelectionId, PresentationId FROM exp_view.ExplorerView WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE exp_view.ViewParameter
	DROP CONSTRAINT FK_ViewParameter_View
GO
DROP TABLE exp_view.ExplorerView
GO
EXECUTE sp_rename N'exp_view.Tmp_ExplorerView', N'ExplorerView', 'OBJECT' 
GO
ALTER TABLE exp_view.ExplorerView ADD CONSTRAINT
	PK_View PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE exp_view.ExplorerView ADD CONSTRAINT
	UK_View_Name UNIQUE NONCLUSTERED 
	(
	Name
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE exp_view.ExplorerView ADD CONSTRAINT
	FK_View_Presentation FOREIGN KEY
	(
	PresentationId
	) REFERENCES exp_view.Presentation
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE exp_view.ExplorerView ADD CONSTRAINT
	FK_View_Selection FOREIGN KEY
	(
	SelectionId
	) REFERENCES exp_view.Selection
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
select Has_Perms_By_Name(N'exp_view.ExplorerView', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'exp_view.ExplorerView', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'exp_view.ExplorerView', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE exp_view.ViewParameter ADD CONSTRAINT
	FK_ViewParameter_View FOREIGN KEY
	(
	ViewId
	) REFERENCES exp_view.ExplorerView
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE exp_view.ViewParameter SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'exp_view.ViewParameter', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'exp_view.ViewParameter', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'exp_view.ViewParameter', 'Object', 'CONTROL') as Contr_Per 

-- Presentation
BEGIN TRANSACTION
GO
CREATE TABLE exp_view.Tmp_Presentation
	(
	Id bigint NOT NULL,
	Name nvarchar(64) NOT NULL,
	System nvarchar(64) NOT NULL,
	BaseType nvarchar(64) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE exp_view.Tmp_Presentation SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM exp_view.Presentation)
	 EXEC('INSERT INTO exp_view.Tmp_Presentation (Id, Name, System, BaseType)
		SELECT Id, Name, System, BaseType FROM exp_view.Presentation WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE exp_view.GraphPresentation
	DROP CONSTRAINT FK_GraphPresentation_Presentation
GO
ALTER TABLE exp_view.GridPresentation
	DROP CONSTRAINT FK_GridPresentation_Presentation
GO
ALTER TABLE exp_view.ExplorerView
	DROP CONSTRAINT FK_View_Presentation
GO
DROP TABLE exp_view.Presentation
GO
EXECUTE sp_rename N'exp_view.Tmp_Presentation', N'Presentation', 'OBJECT' 
GO
ALTER TABLE exp_view.Presentation ADD CONSTRAINT
	PK_Presentation PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE exp_view.Presentation ADD CONSTRAINT
	UK_Presentation_Name UNIQUE NONCLUSTERED 
	(
	Name
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
select Has_Perms_By_Name(N'exp_view.Presentation', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'exp_view.Presentation', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'exp_view.Presentation', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE exp_view.ExplorerView ADD CONSTRAINT
	FK_View_Presentation FOREIGN KEY
	(
	PresentationId
	) REFERENCES exp_view.Presentation
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE exp_view.ExplorerView SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'exp_view.ExplorerView', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'exp_view.ExplorerView', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'exp_view.ExplorerView', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE exp_view.GridPresentation ADD CONSTRAINT
	FK_GridPresentation_Presentation FOREIGN KEY
	(
	PresentationId
	) REFERENCES exp_view.Presentation
	(
	Id
	) ON UPDATE  CASCADE 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE exp_view.GridPresentation SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'exp_view.GridPresentation', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'exp_view.GridPresentation', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'exp_view.GridPresentation', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE exp_view.GraphPresentation ADD CONSTRAINT
	FK_GraphPresentation_Presentation FOREIGN KEY
	(
	PresentationId
	) REFERENCES exp_view.Presentation
	(
	Id
	) ON UPDATE  CASCADE 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE exp_view.GraphPresentation SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'exp_view.GraphPresentation', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'exp_view.GraphPresentation', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'exp_view.GraphPresentation', 'Object', 'CONTROL') as Contr_Per 

-- Property
BEGIN TRANSACTION
GO
ALTER TABLE exp_view.Property
	DROP CONSTRAINT FK_Property_GridPresentation
GO
ALTER TABLE exp_view.GridPresentation SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'exp_view.GridPresentation', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'exp_view.GridPresentation', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'exp_view.GridPresentation', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
CREATE TABLE exp_view.Tmp_Property
	(
	Id bigint NOT NULL,
	Name nvarchar(64) NOT NULL,
	Summary nvarchar(256) NULL,
	GridPresentationId bigint NULL
	)  ON [PRIMARY]
GO
ALTER TABLE exp_view.Tmp_Property SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM exp_view.Property)
	 EXEC('INSERT INTO exp_view.Tmp_Property (Id, Name, Summary, GridPresentationId)
		SELECT Id, Name, Summary, GridPresentationId FROM exp_view.Property WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE exp_view.Property
GO
EXECUTE sp_rename N'exp_view.Tmp_Property', N'Property', 'OBJECT' 
GO
ALTER TABLE exp_view.Property ADD CONSTRAINT
	PK_Property PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE exp_view.Property ADD CONSTRAINT
	FK_Property_GridPresentation FOREIGN KEY
	(
	GridPresentationId
	) REFERENCES exp_view.GridPresentation
	(
	PresentationId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
select Has_Perms_By_Name(N'exp_view.Property', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'exp_view.Property', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'exp_view.Property', 'Object', 'CONTROL') as Contr_Per 

-- Selection
BEGIN TRANSACTION
GO
CREATE TABLE exp_view.Tmp_Selection
	(
	Id bigint NOT NULL,
	Name nvarchar(64) NOT NULL,
	System nvarchar(64) NOT NULL,
	BaseType nvarchar(64) NOT NULL,
	Filter nvarchar(2048) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE exp_view.Tmp_Selection SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM exp_view.Selection)
	 EXEC('INSERT INTO exp_view.Tmp_Selection (Id, Name, System, BaseType, Filter)
		SELECT Id, Name, System, BaseType, Filter FROM exp_view.Selection WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE exp_view.ExplorerView
	DROP CONSTRAINT FK_View_Selection
GO
DROP TABLE exp_view.Selection
GO
EXECUTE sp_rename N'exp_view.Tmp_Selection', N'Selection', 'OBJECT' 
GO
ALTER TABLE exp_view.Selection ADD CONSTRAINT
	PK_Selection PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE exp_view.Selection ADD CONSTRAINT
	UK_Selection_Name UNIQUE NONCLUSTERED 
	(
	Name
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
select Has_Perms_By_Name(N'exp_view.Selection', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'exp_view.Selection', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'exp_view.Selection', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE exp_view.ExplorerView ADD CONSTRAINT
	FK_View_Selection FOREIGN KEY
	(
	SelectionId
	) REFERENCES exp_view.Selection
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE exp_view.ExplorerView SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'exp_view.ExplorerView', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'exp_view.ExplorerView', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'exp_view.ExplorerView', 'Object', 'CONTROL') as Contr_Per

-- View Parameter
BEGIN TRANSACTION
GO
ALTER TABLE exp_view.ViewParameter
	DROP CONSTRAINT FK_ViewParameter_View
GO
ALTER TABLE exp_view.ExplorerView SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'exp_view.ExplorerView', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'exp_view.ExplorerView', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'exp_view.ExplorerView', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
CREATE TABLE exp_view.Tmp_ViewParameter
	(
	Id bigint NOT NULL,
	Name nvarchar(64) NOT NULL,
	Operator nvarchar(20) NOT NULL,
	ViewId bigint NULL
	)  ON [PRIMARY]
GO
ALTER TABLE exp_view.Tmp_ViewParameter SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM exp_view.ViewParameter)
	 EXEC('INSERT INTO exp_view.Tmp_ViewParameter (Id, Name, Operator, ViewId)
		SELECT Id, Name, Operator, ViewId FROM exp_view.ViewParameter WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE exp_view.ViewParameterValue
	DROP CONSTRAINT FK_ViewParameterValue_ViewParameter
GO
DROP TABLE exp_view.ViewParameter
GO
EXECUTE sp_rename N'exp_view.Tmp_ViewParameter', N'ViewParameter', 'OBJECT' 
GO
ALTER TABLE exp_view.ViewParameter ADD CONSTRAINT
	PK_ViewParameter PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE exp_view.ViewParameter ADD CONSTRAINT
	FK_ViewParameter_View FOREIGN KEY
	(
	ViewId
	) REFERENCES exp_view.ExplorerView
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
select Has_Perms_By_Name(N'exp_view.ViewParameter', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'exp_view.ViewParameter', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'exp_view.ViewParameter', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE exp_view.ViewParameterValue ADD CONSTRAINT
	FK_ViewParameterValue_ViewParameter FOREIGN KEY
	(
	ViewParameterId
	) REFERENCES exp_view.ViewParameter
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE exp_view.ViewParameterValue SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'exp_view.ViewParameterValue', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'exp_view.ViewParameterValue', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'exp_view.ViewParameterValue', 'Object', 'CONTROL') as Contr_Per 

-- ViewParameterValue
BEGIN TRANSACTION
GO
ALTER TABLE exp_view.ViewParameterValue
	DROP CONSTRAINT FK_ViewParameterValue_ViewParameter
GO
ALTER TABLE exp_view.ViewParameter SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'exp_view.ViewParameter', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'exp_view.ViewParameter', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'exp_view.ViewParameter', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
CREATE TABLE exp_view.Tmp_ViewParameterValue
	(
	Id bigint NOT NULL,
	ExplorerUser nvarchar(256) NOT NULL,
	Value nvarchar(64) NOT NULL,
	Value2 nvarchar(64) NOT NULL,
	Position int NOT NULL,
	ValueType nvarchar(64) NOT NULL,
	ViewParameterId bigint NULL
	)  ON [PRIMARY]
GO
ALTER TABLE exp_view.Tmp_ViewParameterValue SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM exp_view.ViewParameterValue)
	 EXEC('INSERT INTO exp_view.Tmp_ViewParameterValue (Id, ExplorerUser, Value, Value2, Position, ValueType, ViewParameterId)
		SELECT Id, ExplorerUser, Value, Value2, Position, ValueType, ViewParameterId FROM exp_view.ViewParameterValue WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE exp_view.ViewParameterValue
GO
EXECUTE sp_rename N'exp_view.Tmp_ViewParameterValue', N'ViewParameterValue', 'OBJECT' 
GO
ALTER TABLE exp_view.ViewParameterValue ADD CONSTRAINT
	PK_ViewParameterValue PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE exp_view.ViewParameterValue ADD CONSTRAINT
	FK_ViewParameterValue_ViewParameter FOREIGN KEY
	(
	ViewParameterId
	) REFERENCES exp_view.ViewParameter
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
select Has_Perms_By_Name(N'exp_view.ViewParameterValue', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'exp_view.ViewParameterValue', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'exp_view.ViewParameterValue', 'Object', 'CONTROL') as Contr_Per 

-- YAxis
BEGIN TRANSACTION
GO
ALTER TABLE exp_view.YAxis
	DROP CONSTRAINT FK_YAxis_GraphPresentation
GO
ALTER TABLE exp_view.GraphPresentation SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'exp_view.GraphPresentation', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'exp_view.GraphPresentation', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'exp_view.GraphPresentation', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
CREATE TABLE exp_view.Tmp_YAxis
	(
	Id bigint NOT NULL,
	Property nvarchar(64) NOT NULL,
	Type nvarchar(64) NOT NULL,
	AdditionalAxisNumber int NULL,
	GraphPresentationId bigint NULL
	)  ON [PRIMARY]
GO
ALTER TABLE exp_view.Tmp_YAxis SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM exp_view.YAxis)
	 EXEC('INSERT INTO exp_view.Tmp_YAxis (Id, Property, Type, AdditionalAxisNumber, GraphPresentationId)
		SELECT Id, Property, Type, AdditionalAxisNumber, GraphPresentationId FROM exp_view.YAxis WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE exp_view.YAxis
GO
EXECUTE sp_rename N'exp_view.Tmp_YAxis', N'YAxis', 'OBJECT' 
GO
ALTER TABLE exp_view.YAxis ADD CONSTRAINT
	PK_YAxis PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE exp_view.YAxis ADD CONSTRAINT
	FK_YAxis_GraphPresentation FOREIGN KEY
	(
	GraphPresentationId
	) REFERENCES exp_view.GraphPresentation
	(
	PresentationId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
select Has_Perms_By_Name(N'exp_view.YAxis', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'exp_view.YAxis', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'exp_view.YAxis', 'Object', 'CONTROL') as Contr_Per

-- TrinityCredential
BEGIN TRANSACTION
GO
CREATE TABLE exp_credential.Tmp_TrinityCredential
	(
	Id bigint NOT NULL,
	SiteUser nvarchar(256) NOT NULL,
	UserName nvarchar(256) NOT NULL,
	Password nvarchar(256) NOT NULL,
	DataSource nvarchar(64) NOT NULL,
	IsDefault bit NULL
	)  ON [PRIMARY]
GO
ALTER TABLE exp_credential.Tmp_TrinityCredential SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM exp_credential.TrinityCredential)
	 EXEC('INSERT INTO exp_credential.Tmp_TrinityCredential (Id, SiteUser, UserName, Password, DataSource, IsDefault)
		SELECT Id, SiteUser, UserName, Password, DataSource, IsDefault FROM exp_credential.TrinityCredential WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE exp_credential.TrinityCredential
GO
EXECUTE sp_rename N'exp_credential.Tmp_TrinityCredential', N'TrinityCredential', 'OBJECT' 
GO
ALTER TABLE exp_credential.TrinityCredential ADD CONSTRAINT
	PK_TrinityCredential PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
select Has_Perms_By_Name(N'exp_credential.TrinityCredential', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'exp_credential.TrinityCredential', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'exp_credential.TrinityCredential', 'Object', 'CONTROL') as Contr_Per 

-- Insert rows into hilo tables - need to get current highest values of id from each of the tables
INSERT INTO exp_view.NextHigh (NextHigh, EntityName) SELECT (ISNULL(MAX(Id)/(10+1)+1,0)), 'DataService' FROM exp_view.DataService;
INSERT INTO exp_view.NextHigh (NextHigh, EntityName) SELECT (ISNULL(MAX(Id)/(10+1)+1,0)), 'View' FROM exp_view.ExplorerView;
INSERT INTO exp_view.NextHigh (NextHigh, EntityName) SELECT (ISNULL(MAX(Id)/(10+1)+1,0)), 'Presentation' FROM exp_view.Presentation;
INSERT INTO exp_view.NextHigh (NextHigh, EntityName) SELECT (ISNULL(MAX(Id)/(10+1)+1,0)), 'Property' FROM exp_view.Property;
INSERT INTO exp_view.NextHigh (NextHigh, EntityName) SELECT (ISNULL(MAX(Id)/(10+1)+1,0)), 'Selection' FROM exp_view.Selection;
INSERT INTO exp_view.NextHigh (NextHigh, EntityName) SELECT (ISNULL(MAX(Id)/(10+1)+1,0)), 'ViewParameter' FROM exp_view.ViewParameter;
INSERT INTO exp_view.NextHigh (NextHigh, EntityName) SELECT (ISNULL(MAX(Id)/(10+1)+1,0)), 'ViewParameterValue' FROM exp_view.ViewParameterValue;
INSERT INTO exp_view.NextHigh (NextHigh, EntityName) SELECT (ISNULL(MAX(Id)/(10+1)+1,0)), 'YAxis' FROM exp_view.YAxis;

INSERT INTO exp_credential.NextHigh (NextHigh, EntityName) SELECT (ISNULL(MAX(Id)/(10+1)+1,0)), 'TrinityCredential' FROM exp_credential.TrinityCredential;

/****** Insert: Version Number ******/ 
INSERT INTO [exp_common].[ExplorerVersion]
           ([Version]
           ,[Major]
           ,[Minor]
           ,[Patch]
           ,[Compile]
           ,[TimeStamp])
     VALUES
           ('2012.3.0.1'
           ,2012
           ,3 
           ,0
           ,1
           ,getdate())
GO

/****** Check: Preceeding Version Number ******/
IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2012.3.0.1') is null)
	raiserror('Cannot upgrade to 2013.2.0.1 before database is upgraded to 2012.3.0.1', 20, -1) with log
ELSE IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2013.2.0.1') is not null)
	raiserror('Cannot upgrade to 2013.2.0.1 as database is already at this version or greater', 20, -1) with log
GO

/****** Insert: Version Number ******/ 
INSERT INTO [exp_common].[ExplorerVersion]
           ([Version]
           ,[Major]
           ,[Minor]
           ,[Patch]
           ,[Compile]
           ,[TimeStamp])
     VALUES
           ('2013.2.0.1'
           ,2013
           ,2 
           ,0
           ,1
           ,getdate())
GO

/****** Check: Preceeding Version Number ******/
IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2013.2.0.1') is null)
	raiserror('Cannot upgrade to 2013.2.0.2 before database is upgraded to 2013.2.0.1', 20, -1) with log
ELSE IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2013.2.0.2') is not null)
	raiserror('Cannot upgrade to 2013.2.0.2 as database is already at this version or greater', 20, -1) with log
GO

ALTER TABLE exp_view.DataService DROP CONSTRAINT UK_DataService_ClientAddress

ALTER TABLE exp_view.DataService
DROP COLUMN ClientAddress


/****** Insert: Version Number ******/ 
INSERT INTO [exp_common].[ExplorerVersion]
           ([Version]
           ,[Major]
           ,[Minor]
           ,[Patch]
           ,[Compile]
           ,[TimeStamp])
     VALUES
           ('2013.2.0.2'
           ,2013
           ,2 
           ,0
           ,2
           ,getdate())
GO

/****** Check: Preceeding Version Number ******/
IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2013.2.0.2') is null)
	raiserror('Cannot upgrade to 2013.4.0.1 before database is upgraded to 2013.2.0.4', 20, -1) with log
ELSE IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2013.4.0.1') is not null)
	raiserror('Cannot upgrade to 2013.4.0.1 as database is already at this version or greater', 20, -1) with log
GO

DELETE FROM exp_view.ViewParameterValue
GO

/****** Insert: Version Number ******/ 
INSERT INTO [exp_common].[ExplorerVersion]
           ([Version]
           ,[Major]
           ,[Minor]
           ,[Patch]
           ,[Compile]
           ,[TimeStamp])
     VALUES
           ('2013.4.0.1'
           ,2013
           ,4 
           ,0
           ,1
           ,getdate())
GO

/****** Check: Preceeding Version Number ******/
IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2013.4.0.1') is null)
	raiserror('Cannot upgrade to 2013.4.0.2 before database is upgraded to 2013.4.0.1', 20, -1) with log
ELSE IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2013.4.0.2') is not null)
	raiserror('Cannot upgrade to 2013.4.0.2 as database is already at this version or greater', 20, -1) with log
GO

UPDATE	exp_view.Presentation
SET		System = 'Valuation', BaseType = 'TradeValuation'
WHERE	System = 'Dynamite' AND BaseType = 'TradeValuationResult'
GO

UPDATE	exp_view.Selection
SET		System = 'Valuation', BaseType = 'TradeValuation'
WHERE	System = 'Dynamite' AND BaseType = 'TradeValuationResult'
GO

/****** Insert: Version Number ******/ 
INSERT INTO [exp_common].[ExplorerVersion]
           ([Version]
           ,[Major]
           ,[Minor]
           ,[Patch]
           ,[Compile]
           ,[TimeStamp])
     VALUES
           ('2013.4.0.2'
           ,2013
           ,4 
           ,0
           ,2
           ,getdate())
GO

/****** Check: Preceeding Version Number ******/
IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2013.4.0.2') is null)
	raiserror('Cannot upgrade to 2013.4.0.3 before database is upgraded to 2013.4.0.2', 20, -1) with log
ELSE IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2013.4.0.3') is not null)
	raiserror('Cannot upgrade to 2013.4.0.3 as database is already at this version or greater', 20, -1) with log
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [exp_view].[DefaultView](
	[Id] [bigint] NOT NULL,
	[BaseType] [nvarchar](64) NOT NULL,
	[DataService] [nvarchar](64) NOT NULL,
	[Username] [nvarchar](256) NOT NULL,
	[ViewId] [bigint] NOT NULL,
 CONSTRAINT [PK_DefaultView] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UK_DefaultView_1] UNIQUE NONCLUSTERED 
(
	[BaseType] ASC,
	[DataService] ASC,
	[Username] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [exp_view].[DefaultView]  WITH CHECK ADD  CONSTRAINT [FK_DefaultView_ExplorerView] FOREIGN KEY([ViewId])
REFERENCES [exp_view].[ExplorerView] ([Id])
GO

ALTER TABLE [exp_view].[DefaultView] CHECK CONSTRAINT [FK_DefaultView_ExplorerView]
GO

INSERT INTO [exp_view].[NextHigh]
           ([NextHigh]
           ,[EntityName])
     VALUES
           (0
           ,'DefaultView')
GO

/****** Insert: Version Number ******/ 
INSERT INTO [exp_common].[ExplorerVersion]
           ([Version]
           ,[Major]
           ,[Minor]
           ,[Patch]
           ,[Compile]
           ,[TimeStamp])
     VALUES
           ('2013.4.0.3'
           ,2013
           ,4 
           ,0
           ,3
           ,getdate())
GO

/****** Check: Preceeding Version Number ******/
IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2013.4.0.3') is null)
	raiserror('Cannot upgrade to 2013.4.0.4 before database is upgraded to 2013.4.0.3', 20, -1) with log
ELSE IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2013.4.0.4') is not null)
	raiserror('Cannot upgrade to 2013.4.0.4 as database is already at this version or greater', 20, -1) with log
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/* Removing value2 and valuetype columns from viewparametervalue table */
ALTER TABLE exp_view.ViewParameterValue
	DROP COLUMN Value2
GO

ALTER TABLE exp_view.ViewParameterValue
	DROP COLUMN ValueType
GO

/* Adding Position columns to property and viewparameter tables */
ALTER TABLE exp_view.Property ADD
	Position bigint NOT NULL CONSTRAINT DF_Property_Position DEFAULT 0
GO

ALTER TABLE exp_view.ViewParameter ADD
	Position bigint NOT NULL CONSTRAINT DF_ViewParameter_Position DEFAULT 0
GO

/* Adding System and BaseType columns to ExplorerView table */
ALTER TABLE exp_view.ExplorerView ADD
	System nvarchar(64) NULL
GO

ALTER TABLE exp_view.ExplorerView ADD
	BaseType nvarchar(64) NULL
GO

UPDATE exp_view.ExplorerView SET 
	System = (SELECT System FROM exp_view.Selection WHERE Id = selectionid)
GO

UPDATE exp_view.ExplorerView set 
	BaseType = (SELECT BaseType FROM exp_view.Selection WHERE Id = selectionid)
GO

ALTER TABLE exp_view.ExplorerView ALTER COLUMN 
	System nvarchar(64) NOT NULL
GO

ALTER TABLE exp_view.ExplorerView ALTER COLUMN 
	BaseType nvarchar(64) NOT NULL
GO

/****** Insert: Version Number ******/ 
INSERT INTO [exp_common].[ExplorerVersion]
           ([Version]
           ,[Major]
           ,[Minor]
           ,[Patch]
           ,[Compile]
           ,[TimeStamp])
     VALUES
           ('2013.4.0.4'
           ,2013
           ,4 
           ,0
           ,4
           ,getdate())
GO

/****** Check: Preceeding Version Number ******/
IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2013.4.0.4') is null)
	raiserror('Cannot upgrade to 2014.1.0.1 before database is upgraded to 2013.4.0.4', 20, -1) with log
ELSE IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2014.1.0.1') is not null)
	raiserror('Cannot upgrade to 2014.1.0.1 as database is already at this version or greater', 20, -1) with log
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [exp_view].[HelpLink](
	[Id] [bigint] NOT NULL,
	[Product] [nvarchar](64) NOT NULL,
	[Module] [nvarchar](64) NOT NULL,
	[Item] [nvarchar](64) NOT NULL,
	[Culture] [nvarchar](32) NOT NULL,
	[Url] [nvarchar](512) NOT NULL,
 CONSTRAINT [PK_HelpLink] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

INSERT INTO [exp_view].[NextHigh]
           ([NextHigh]
           ,[EntityName])
     VALUES
           (0
           ,'HelpLink')
GO

/****** Insert: Version Number ******/ 
INSERT INTO [exp_common].[ExplorerVersion]
           ([Version]
           ,[Major]
           ,[Minor]
           ,[Patch]
           ,[Compile]
           ,[TimeStamp])
     VALUES
           ('2014.1.0.1'
           ,2014
           ,1 
           ,0
           ,1
           ,getdate())
GO

/****** Check: Preceeding Version Number ******/
IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2014.1.0.1') is null)
	raiserror('Cannot upgrade to 2014.1.0.2 before database is upgraded to 2014.1.0.1', 20, -1) with log
ELSE IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2014.1.0.2') is not null)
	raiserror('Cannot upgrade to 2014.1.0.2 as database is already at this version or greater', 20, -1) with log
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Insert: Version Number ******/ 
INSERT INTO [exp_common].[ExplorerVersion]
           ([Version]
           ,[Major]
           ,[Minor]
           ,[Patch]
           ,[Compile]
           ,[TimeStamp])
     VALUES
           ('2014.1.0.2'
           ,2014
           ,1 
           ,0
           ,2
           ,getdate())
GO

/****** Check: Preceeding Version Number ******/
IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2014.1.0.2') is null)
	raiserror('Cannot upgrade to 2014.1.0.3 before database is upgraded to 2014.1.0.2', 20, -1) with log
ELSE IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2014.1.0.3') is not null)
	raiserror('Cannot upgrade to 2014.1.0.3 as database is already at this version or greater', 20, -1) with log
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

UPDATE exp_view.dataservice  SET SYSTEM = LOWER(SYSTEM)
GO
UPDATE exp_view.explorerview SET SYSTEM = LOWER(SYSTEM)
GO
UPDATE exp_view.presentation SET SYSTEM = LOWER(SYSTEM)
GO
UPDATE exp_view.selection    SET SYSTEM = LOWER(SYSTEM)
GO

/****** Insert: Version Number ******/ 
INSERT INTO [exp_common].[ExplorerVersion]
           ([Version]
           ,[Major]
           ,[Minor]
           ,[Patch]
           ,[Compile]
           ,[TimeStamp])
     VALUES
           ('2014.1.0.3'
           ,2014
           ,1 
           ,0
           ,3
           ,getdate())
GO

/****** Check: Preceeding Version Number ******/
IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2014.1.0.3') is null)
	raiserror('Cannot upgrade to 2014.1.0.4 before database is upgraded to 2014.1.0.3', 20, -1) with log
ELSE IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2014.1.0.4') is not null)
	raiserror('Cannot upgrade to 2014.1.0.4 as database is already at this version or greater', 20, -1) with log
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Insert: Version Number ******/ 
INSERT INTO [exp_common].[ExplorerVersion]
           ([Version]
           ,[Major]
           ,[Minor]
           ,[Patch]
           ,[Compile]
           ,[TimeStamp])
     VALUES
           ('2014.1.0.4'
           ,2014
           ,1 
           ,0
           ,4
           ,getdate())
GO

/****** Check: Preceeding Version Number ******/
IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2014.1.0.4') is null)
    raiserror('Cannot upgrade to 2014.2.0.1 before database is upgraded to 2014.1.0.4', 20, -1) with log
ELSE IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2014.2.0.1') is not null)
    raiserror('Cannot upgrade to 2014.2.0.1 as database is already at this version or greater', 20, -1) with log
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Begin Upgrade ******/

BEGIN TRANSACTION
GO

  CREATE TABLE exp_view.DataServiceHost
    (
      Id int NOT NULL,
      Url nvarchar(255) NOT NULL
    ) ON [PRIMARY]
  GO

INSERT INTO exp_view.DataServiceHost
SELECT
  ROW_NUMBER() OVER (PARTITION BY 1 ORDER BY SUBSTRING(ServiceAddress, 0, CHARINDEX('/api/', ServiceAddress))) as C,
  SUBSTRING(ServiceAddress, 0, CHARINDEX('/api/', ServiceAddress)) as Url
FROM exp_view.DataService
GROUP BY SUBSTRING(ServiceAddress, 0, CHARINDEX('/api/', ServiceAddress))

INSERT INTO exp_view.NextHigh
SELECT (COUNT(*) / 10) + 1, 'DataServiceHost' FROM exp_view.DataServiceHost

DELETE FROM exp_view.DataServiceHost WHERE Url = '';

COMMIT

/****** End Upgrade ******/

/****** Insert: Version Number ******/
INSERT INTO [exp_common].[ExplorerVersion]
           ([Version]
           ,[Major]
           ,[Minor]
           ,[Patch]
           ,[Compile]
           ,[TimeStamp])
     VALUES
           ('2014.2.0.1'
           ,2014
           ,2
           ,0
           ,1
           ,getdate())
GO

/****** Check: Preceeding Version Number ******/
IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2014.2.0.1') is null)
	raiserror('Cannot upgrade to 2014.3.0.1 before database is upgraded to 2014.2.0.1', 20, -1) with log
ELSE IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2014.3.0.1') is not null)
	raiserror('Cannot upgrade to 2014.3.0.1 as database is already at this version or greater', 20, -1) with log
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [exp_view].[BwfDataLevelPermission]
(
	[Id] [bigint] NOT NULL,
	[EntityType] [nvarchar](256) NOT NULL,
	[EntityId] [bigint] NOT NULL,
	[PermissionId] [bigint] NOT NULL,
	[RoleId] [bigint] NOT NULL,
	[RoleName] [nvarchar](256) NOT NULL,
	[PermissionName] [nvarchar](256) NOT NULL,
	[EntityDescription] [nvarchar](256) NOT NULL
CONSTRAINT [PK_BwfDataLevelPermission] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]



GO

INSERT INTO [exp_view].[NextHigh]
	([NextHigh],
	[EntityName])
VALUES
	(1,
	'BwfDataLevelPermission')

GO

CREATE TABLE [exp_view].[BwfServiceLevelPermission]
(
	[Id] [bigint] NOT NULL,
	[RoleId] [bigint] NOT NULL,
	[Type] [nvarchar](256) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[RoleName] [nvarchar](256) NOT NULL
CONSTRAINT [PK_BwfServiceLevelPermission] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]



GO

INSERT INTO [exp_view].[NextHigh]
	([NextHigh],
	[EntityName])
VALUES
	(1,
	'BwfServiceLevelPermission')

GO
/****** Insert: Version Number ******/ 
INSERT INTO [exp_common].[ExplorerVersion]
           ([Version]
           ,[Major]
           ,[Minor]
           ,[Patch]
           ,[Compile]
           ,[TimeStamp])
     VALUES
           ('2014.3.0.1',2014,3,0,1,getdate())
GO

/****** Check: Preceeding Version Number ******/
IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2014.3.0.1') is null)
    raiserror('Cannot upgrade to 2015.1.0.1 before database is upgraded to 2014.3.0.1', 20, -1) with log
ELSE IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2015.1.0.1') is not null)
    raiserror('Cannot upgrade to 2015.1.0.1 as database is already at this version or greater', 20, -1) with log
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [exp_view].[BwfRecordLock]
(
    [Id] [bigint] NOT NULL,
    [EntityType] [nvarchar](256) NOT NULL,
    [EntityId] [bigint] NOT NULL,
    [UserId] [bigint] NOT NULL,
    [Username] [nvarchar](256) NOT NULL,
    [Reason] [nvarchar](256) NOT NULL,
    [Context] [nvarchar](256) NOT NULL,
    [TimeStamp] [datetime2] NOT NULL
    CONSTRAINT [PK_BwfRecordLock] PRIMARY KEY CLUSTERED
    (
        [Id] ASC
    ) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

INSERT INTO [exp_view].[NextHigh]
    ([NextHigh],
    [EntityName])
VALUES (1, 'BwfRecordLock')
GO

INSERT INTO [exp_common].[ExplorerVersion]
           ([Version]
           ,[Major]
           ,[Minor]
           ,[Patch]
           ,[Compile]
           ,[TimeStamp])
     VALUES
           ('2015.1.0.1',2015,1,0,1,getdate())
GO

/****** Check: Preceeding Version Number ******/
IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2015.1.0.1') is null)
    raiserror('Cannot upgrade to 2015.1.0.2 before database is upgraded to 2015.1.0.1', 20, -1) with log
ELSE IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2015.1.0.2') is not null)
    raiserror('Cannot upgrade to 2015.1.0.2 as database is already at this version or greater', 20, -1) with log
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE [exp_view].[BwfRecordLock] DROP COLUMN [UserId]
GO

INSERT INTO [exp_common].[ExplorerVersion]
           ([Version]
           ,[Major]
           ,[Minor]
           ,[Patch]
           ,[Compile]
           ,[TimeStamp])
     VALUES
           ('2015.1.0.2',2015,1,0,2,getdate())
GO

/****** Check: Preceeding Version Number ******/
IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2015.1.0.2') is null)
    raiserror('Cannot upgrade to 2015.1.0.3 before database is upgraded to 2015.1.0.2', 20, -1) with log
ELSE IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2015.1.0.3') is not null)
    raiserror('Cannot upgrade to 2015.1.0.3 as database is already at this version or greater', 20, -1) with log
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Table [exp_view].[BwfAudit]    Script Date: 03/17/2015 10:54:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [exp_view].[BwfAudit](
  [Id] [bigint] NOT NULL,
  [Action] [nvarchar](50) NOT NULL,
  [Timestamp] [datetime2](7) NOT NULL,
  [Username] [nvarchar](50) NULL,
 CONSTRAINT [PK_BwfAudit] PRIMARY KEY CLUSTERED
(
  [Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [exp_view].[a_ViewParameter]    Script Date: 03/17/2015 10:54:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [exp_view].[a_ViewParameter](
  [bwf_auditId] [bigint] NOT NULL,
  [bwf_actionId] [bigint] NOT NULL,
  [bwf_auditSummary] [nvarchar](1024) NOT NULL,
  [Id] [bigint] NOT NULL,
  [Name] [nvarchar](50) NOT NULL,
  [Operator] [nvarchar](10) NOT NULL,
  [ViewAuditId] [bigint] NOT NULL,
  [Position] [nvarchar](10) NOT NULL,
 CONSTRAINT [PK_a_ViewParameter] PRIMARY KEY CLUSTERED
(
  [bwf_auditId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [exp_view].[a_View]    Script Date: 03/17/2015 10:54:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [exp_view].[a_View](
  [bwf_auditId] [bigint] NOT NULL,
  [bwf_actionId] [bigint] NOT NULL,
  [bwf_auditSummary] [nvarchar](1024) NOT NULL,
  [Id] [bigint] NOT NULL,
  [Name] [nvarchar](255) NOT NULL,
  [SystemId] [bigint] NOT NULL,
  [BaseType] [nvarchar](255) NOT NULL,
  [SelectionId] [bigint] NOT NULL,
  [GridPresentationId] [bigint] NOT NULL,
 CONSTRAINT [PK_a_View] PRIMARY KEY CLUSTERED
(
  [bwf_auditId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [exp_view].[a_Selection]    Script Date: 03/17/2015 10:54:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [exp_view].[a_Selection](
  [bwf_auditId] [bigint] NOT NULL,
  [bwf_actionId] [bigint] NOT NULL,
  [bwf_auditSummary] [nvarchar](1024) NOT NULL,
  [Id] [bigint] NOT NULL,
  [Name] [nvarchar](255) NOT NULL,
  [SystemId] [bigint] NOT NULL,
  [BaseType] [nvarchar](255) NOT NULL,
  [Filter] [nvarchar](255) NULL,
 CONSTRAINT [PK_a_Selection] PRIMARY KEY CLUSTERED
(
  [bwf_auditId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [exp_view].[a_Property]    Script Date: 03/17/2015 10:54:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [exp_view].[a_Property](
  [bwf_auditId] [bigint] NOT NULL,
  [bwf_actionId] [bigint] NOT NULL,
  [bwf_auditSummary] [nvarchar](1024) NOT NULL,
  [Id] [bigint] NOT NULL,
  [Name] [nvarchar](255) NOT NULL,
  [Aggregations] [nvarchar](255) NULL,
  [Position] [bigint] NOT NULL,
  [GridPresentationId] [bigint] NOT NULL,
 CONSTRAINT [PK_auditProperty] PRIMARY KEY CLUSTERED
(
  [bwf_auditId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [exp_view].[a_Presentation]    Script Date: 03/17/2015 10:54:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [exp_view].[a_Presentation](
  [bwf_auditId] [bigint] NOT NULL,
  [bwf_actionId] [bigint] NOT NULL,
  [bwf_auditSummary] [nvarchar](1024) NOT NULL,
  [Id] [bigint] NOT NULL,
  [BaseType] [nvarchar](255) NOT NULL,
  [Name] [nvarchar](255) NOT NULL,
  [System_Id] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_auditPresentation] PRIMARY KEY CLUSTERED
(
  [bwf_auditId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [exp_view].[a_HelpLink]    Script Date: 03/17/2015 10:54:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [exp_view].[a_HelpLink](
  [bwf_auditId] [bigint] NOT NULL,
  [bwf_actionId] [bigint] NOT NULL,
  [bwf_auditSummary] [nvarchar](1024) NOT NULL,
  [Id] [bigint] NOT NULL,
  [Product] [nvarchar](255) NOT NULL,
  [Module] [nvarchar](255) NOT NULL,
  [Item] [nvarchar](255) NOT NULL,
  [Url] [nvarchar](255) NOT NULL,
  [Culture] [nvarchar](25) NOT NULL,
 CONSTRAINT [PK_auditHelpLink] PRIMARY KEY CLUSTERED
(
  [bwf_auditId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [exp_view].[a_GridPresentation]    Script Date: 03/17/2015 10:54:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [exp_view].[a_GridPresentation](
  [bwf_auditId] [bigint] NOT NULL,
  [bwf_actionId] [bigint] NOT NULL,
  [bwf_auditSummary] [nvarchar](1024) NOT NULL,
  [Id] [bigint] NOT NULL,
  [BaseType] [nvarchar](255) NOT NULL,
  [Name] [nvarchar](255) NOT NULL,
  [OrderBy] [nvarchar](255) NOT NULL,
  [SystemId] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_auditGridPresentation] PRIMARY KEY CLUSTERED
(
  [bwf_auditId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [exp_view].[a_DefaultView]    Script Date: 03/17/2015 10:54:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [exp_view].[a_DefaultView](
  [bwf_auditId] [bigint] NOT NULL,
  [bwf_actionId] [bigint] NOT NULL,
  [bwf_auditSummary] [nvarchar](1024) NOT NULL,
  [Id] [bigint] NOT NULL,
  [BaseType] [nvarchar](255) NOT NULL,
  [Username] [nvarchar](255),
  [DataServiceId] [bigint] NOT NULL,
  [ViewId] [bigint] NOT NULL,
 CONSTRAINT [PK_auditDefaultView] PRIMARY KEY CLUSTERED
(
  [bwf_auditId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [exp_view].[a_DataServiceHost]    Script Date: 03/17/2015 10:54:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [exp_view].[a_DataServiceHost](
  [bwf_auditId] [bigint] NOT NULL,
  [bwf_actionId] [bigint] NOT NULL,
  [bwf_auditSummary] [nvarchar](1024) NOT NULL,
  [Id] [bigint] NOT NULL,
  [Url] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_a_DataServiceHost] PRIMARY KEY CLUSTERED
(
  [bwf_auditId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [exp_view].[a_DataService]    Script Date: 03/17/2015 10:54:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [exp_view].[a_DataService](
  [bwf_auditId] [bigint] NOT NULL,
  [bwf_actionId] [bigint] NOT NULL,
  [bwf_auditSummary] [nvarchar](1024) NOT NULL,
  [Id] [bigint] NOT NULL,
  [Name] [nvarchar](50) NOT NULL,
  [System] [nvarchar](255) NOT NULL,
  [Url] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_a_DataService] PRIMARY KEY CLUSTERED
(
  [bwf_auditId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

INSERT INTO [exp_view].[NextHigh] ([NextHigh], [EntityName]) VALUES (0, 'BwfAudit')
INSERT INTO [exp_view].[NextHigh] ([NextHigh], [EntityName]) VALUES (0, 'DataServiceHostAudit')
INSERT INTO [exp_view].[NextHigh] ([NextHigh], [EntityName]) VALUES (0, 'DataServiceAudit')
INSERT INTO [exp_view].[NextHigh] ([NextHigh], [EntityName]) VALUES (0, 'HelpLinkAudit')
INSERT INTO [exp_view].[NextHigh] ([NextHigh], [EntityName]) VALUES (0, 'GridPresentationAudit')
INSERT INTO [exp_view].[NextHigh] ([NextHigh], [EntityName]) VALUES (0, 'PropertyAudit')
INSERT INTO [exp_view].[NextHigh] ([NextHigh], [EntityName]) VALUES (0, 'DefaultViewAudit')
INSERT INTO [exp_view].[NextHigh] ([NextHigh], [EntityName]) VALUES (0, 'SelectionAudit')
INSERT INTO [exp_view].[NextHigh] ([NextHigh], [EntityName]) VALUES (0, 'ViewAudit')
INSERT INTO [exp_view].[NextHigh] ([NextHigh], [EntityName]) VALUES (0, 'ViewParameterAudit')
GO

INSERT INTO [exp_common].[ExplorerVersion]
           ([Version]
           ,[Major]
           ,[Minor]
           ,[Patch]
           ,[Compile]
           ,[TimeStamp])
     VALUES
           ('2015.1.0.3',2015,1,0,3,getdate())
GO

IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2015.1.0.3') is null)
    raiserror('Cannot upgrade to 2015.2.0.1 before database is upgraded to 2015.1.0.3', 20, -1) with log
ELSE IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2015.2.0.1') is not null)
    raiserror('Cannot upgrade to 2015.2.0.1 as database is already at this version or greater', 20, -1) with log
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE [exp_view].[ExplorerView] ADD [Description] NVARCHAR(1024) NULL;
ALTER TABLE [exp_view].[a_View] ADD [Description] NVARCHAR(1024) NULL;

ALTER TABLE [exp_view].[Selection] ADD [Description] NVARCHAR(1024) NULL;
ALTER TABLE [exp_view].[a_Selection] ADD [Description] NVARCHAR(1024) NULL;

ALTER TABLE [exp_view].[Presentation] ADD [Description] NVARCHAR(1024) NULL;
ALTER TABLE [exp_view].[a_GridPresentation] ADD [Description] NVARCHAR(1024) NULL;
GO

INSERT INTO [exp_common].[ExplorerVersion]
           ([Version]
           ,[Major]
           ,[Minor]
           ,[Patch]
           ,[Compile]
           ,[TimeStamp])
     VALUES
           ('2015.2.0.1',2015,2,0,1,getdate())
GO

IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2015.2.0.1') is null)
    raiserror('Cannot upgrade to 2015.3.0.1 before database is upgraded to 2015.2.0.1', 20, -1) with log
ELSE IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2015.3.0.1') is not null)
    raiserror('Cannot upgrade to 2015.3.0.1 as database is already at this version or greater', 20, -1) with log
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE [exp_view].[Property] ADD [Alias] NVARCHAR(512) NULL;
GO

ALTER TABLE [exp_view].[a_Property] ADD [Alias] NVARCHAR(512) NULL;
GO



INSERT INTO [exp_common].[ExplorerVersion]
           ([Version]
           ,[Major]
           ,[Minor]
           ,[Patch]
           ,[Compile]
           ,[TimeStamp])
     VALUES
           ('2015.3.0.1',2015,3,0,1,getdate())
GO

