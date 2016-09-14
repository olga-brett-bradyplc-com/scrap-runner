
--SELECT TOP 1000 *  FROM [ScrapTest].[exp_common].[ExplorerVersion]
USE ScrapTest 
 GO 
 
 -----------------------------------------------------------------------------------
--explorer_2016_2_0_1.sql
-----------------------------------------------------------------------------------

 IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2016.1.0.4') is null) 
     raiserror('Cannot upgrade to 2016.2.0.1 before database is upgraded to 2016.1.0.4', 20, -1) with log 
 ELSE IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2016.2.0.1') is not null) 
     raiserror('Cannot upgrade to 2016.2.0.1 as database is already at this version or greater', 20, -1) with log 
 GO 
 
 
 SET ANSI_NULLS ON 
 GO 
 SET QUOTED_IDENTIFIER ON 
 GO 
 

 CREATE TABLE [exp_view].[a_Dashboard]( 
 	[bwf_auditId] [bigint] NOT NULL, 
 	[bwf_actionId] [bigint] NOT NULL, 
 	[bwf_auditSummary] [nvarchar](1024) NOT NULL, 
 	[Id] [bigint] NOT NULL, 
 	[Name] [nvarchar](500) NOT NULL, 
  CONSTRAINT [PK_a_Dashboard] PRIMARY KEY CLUSTERED  
 ( 
 	[bwf_auditId] ASC 
 )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] 
 ) ON [PRIMARY] 
 

 GO 
 

 CREATE TABLE [exp_view].[a_Tile]( 
 	[bwf_auditId] [bigint] NOT NULL, 
 	[bwf_actionId] [bigint] NOT NULL, 
 	[bwf_auditSummary] [nvarchar](1024) NOT NULL, 
 	[Content] [nvarchar](2000) NULL, 
 	[TileType] [nvarchar](100) NOT NULL, 
 	[Title] [nvarchar](150) NOT NULL, 
 	[Subtitle] [nvarchar](150) NULL, 
 	[DisplayTitle] [bit] NOT NULL, 
 	[DisplaySubtitle] [bit] NOT NULL, 
 	[PositionX] [int] NOT NULL, 
 	[PositionY] [int] NOT NULL, 
 	[SizeX] [int] NOT NULL, 
 	[SizeY] [int] NOT NULL, 
 	[DashboardAuditId] [bigint] NULL 
 ) ON [PRIMARY] 
 

 GO 
 

 CREATE TABLE [exp_view].[Dashboard]( 
 	[Id] [bigint] NOT NULL, 
 	[Name] [nvarchar](500) NOT NULL, 
  CONSTRAINT [PK_Dashboard] PRIMARY KEY CLUSTERED  
 ( 
 	[Id] ASC 
 )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] 
 ) ON [PRIMARY] 
 

 GO 
 

 CREATE TABLE [exp_view].[Tile]( 
 	[Id] [bigint] NOT NULL, 
 	[Content] [nvarchar](2000) NULL, 
 	[TileType] [nvarchar](100) NOT NULL, 
 	[Title] [nvarchar](150) NOT NULL, 
 	[Subtitle] [nvarchar](150) NULL, 
 	[DisplayTitle] [bit] NOT NULL, 
 	[DisplaySubtitle] [bit] NOT NULL, 
 	[PositionX] [int] NOT NULL, 
 	[PositionY] [int] NOT NULL, 
 	[SizeX] [int] NOT NULL, 
 	[SizeY] [int] NOT NULL, 
 	[DashboardId] [bigint] NULL 
  CONSTRAINT [PK_Tile] PRIMARY KEY CLUSTERED  
 ( 
 	[Id] ASC 
 )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] 
 ) ON [PRIMARY] 
 

 GO 
 ALTER TABLE [exp_view].[Tile]  WITH CHECK ADD  CONSTRAINT [FK_Tile_Dashboard] FOREIGN KEY([DashboardId]) 
 REFERENCES [exp_view].[Dashboard] ([Id]) 
 ON UPDATE CASCADE 
 ON DELETE CASCADE 
 GO 
 ALTER TABLE [exp_view].[Tile] CHECK CONSTRAINT [FK_Tile_Dashboard] 
 GO 
 

 

 INSERT INTO [exp_view].[NextHigh] ([NextHigh], [EntityName]) VALUES (0, 'Dashboard') 
 INSERT INTO [exp_view].[NextHigh] ([NextHigh], [EntityName]) VALUES (0, 'Tile') 
 GO 
 

 INSERT INTO [exp_view].[NextHigh] ([NextHigh], [EntityName]) VALUES (0, 'DashboardAudit') 
 INSERT INTO [exp_view].[NextHigh] ([NextHigh], [EntityName]) VALUES (0, 'TileAudit') 
 GO 
 

 -- Finish up 
 

 INSERT INTO [exp_common].[ExplorerVersion] 
            ([Version] 
            ,[Major] 
            ,[Minor] 
            ,[Patch] 
            ,[Compile] 
            ,[TimeStamp]) 
      VALUES 
            ('2016.2.0.1',2016,2,0,1,getdate()) 
 GO 

-----------------------------------------------------------------------------------
--explorer_2016_2_0_2.sql
-----------------------------------------------------------------------------------


 IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2016.2.0.1') is null) 
     raiserror('Cannot upgrade to 2016.2.0.2 before database is upgraded to 2016.2.0.1', 20, -1) with log 
 ELSE IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2016.2.0.2') is not null) 
     raiserror('Cannot upgrade to 2016.2.0.2 as database is already at this version or greater', 20, -1) with log 
 GO 
 

 SET ANSI_NULLS ON 
 GO 
 

 SET QUOTED_IDENTIFIER ON 
 GO 
 

 CREATE TABLE [exp_view].[ViewParameterOption]( 
     [Id] [bigint] NOT NULL, 
     [Username] [nvarchar](256) NOT NULL, 
     [ViewParameterId] [bigint] NULL, 
     [IncludeEmpty] [bit] NOT NULL, 
  CONSTRAINT [PK_ViewParameterOptions] PRIMARY KEY CLUSTERED  
 ( 
     [Id] ASC 
 )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] 
 ) ON [PRIMARY] 
 GO 
 

 DELETE FROM [exp_view].[ViewParameterValue] 
 GO 
 

 ALTER TABLE [exp_view].[ViewParameterValue] DROP CONSTRAINT [FK_ViewParameterValue_ViewParameter] 
 GO 
 

 ALTER TABLE [exp_view].[ViewParameterValue] 
     DROP COLUMN [ExplorerUser], [ViewParameterId] 
 GO 
 

 ALTER TABLE [exp_view].[ViewParameterValue] 
     ADD ViewParameterOptionId bigint 
 GO 
 

 INSERT INTO [exp_view].[NextHigh] (NextHigh, EntityName) VALUES (0, 'ViewParameterOption') 
 

 INSERT INTO [exp_common].[ExplorerVersion] 
            ([Version] 
            ,[Major] 
            ,[Minor] 
            ,[Patch] 
            ,[Compile] 
            ,[TimeStamp]) 
      VALUES 
            ('2016.2.0.2',2016,2,0,2,getdate()) 
 GO 

-----------------------------------------------------------------------------------
--explorer_2016_2_0_3.sql
-----------------------------------------------------------------------------------


 IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2016.2.0.2') is null) 
     raiserror('Cannot upgrade to 2016.2.0.3 before database is upgraded to 2016.2.0.2', 20, -1) with log 
 ELSE IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2016.2.0.3') is not null) 
     raiserror('Cannot upgrade to 2016.2.0.3 as database is already at this version or greater', 20, -1) with log 
 GO 
 
 
 SET ANSI_NULLS ON 
 GO 
 

 SET QUOTED_IDENTIFIER ON 
 GO 
 

 -- No schema changes for this version 
 

 INSERT INTO [exp_common].[ExplorerVersion] 
            ([Version] 
            ,[Major] 
            ,[Minor] 
            ,[Patch] 
            ,[Compile] 
            ,[TimeStamp]) 
      VALUES 
            ('2016.2.0.3',2016,2,0,3,getdate()) 
 GO 

-----------------------------------------------------------------------------------
--explorer_2016_2_0_4.sql
-----------------------------------------------------------------------------------

 -- 2016.2.0.4 
 -- Update Tile table to have GUIDs as IDs 
 

 IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2016.2.0.3') is null) 
     raiserror('Cannot upgrade to 2016.2.0.4 before database is upgraded to 2016.2.0.3', 20, -1) with log 
 ELSE IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2016.2.0.4') is not null) 
     raiserror('Cannot upgrade to 2016.2.0.4 as database is already at this version or greater', 20, -1) with log 
 GO 
 

 SET ANSI_NULLS ON 
 GO 
 

 SET QUOTED_IDENTIFIER ON 
 GO 
 

 ALTER TABLE exp_view.Tile DROP CONSTRAINT PK_Tile; 
 

 ALTER TABLE exp_view.Tile ALTER COLUMN Id nvarchar(36) NOT NULL; 
 

 ALTER TABLE exp_view.Tile ADD CONSTRAINT PK_Tile PRIMARY KEY CLUSTERED ([Id] ASC); 
 

 DELETE FROM exp_view.NextHigh WHERE exp_view.NextHigh.EntityName='Tile'; 
 

 INSERT INTO [exp_common].[ExplorerVersion] 
            ([Version] 
            ,[Major] 
            ,[Minor] 
            ,[Patch] 
            ,[Compile] 
            ,[TimeStamp]) 
      VALUES 
            ('2016.2.0.4',2016,2,0,4,getdate()) 
 GO 
