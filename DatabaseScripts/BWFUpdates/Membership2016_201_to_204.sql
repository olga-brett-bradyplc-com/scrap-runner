--SELECT TOP 1000 *  FROM [ScrapTest].[brady_membership].[dbversion]

USE ScrapTest 
 GO 
 
-----------------------------------------------------------------------------------
--membership_2016_2_0_1.sql
-----------------------------------------------------------------------------------

 BEGIN TRY 
    EXEC [brady_membership].[prc_checkversion] 2015,3,0,3,2016,2,0,1 
 END TRY 
 BEGIN CATCH 
    PRINT ERROR_MESSAGE() 
    SET NOEXEC ON 
 END CATCH 
 GO 
 

 SET ANSI_NULLS ON 
 GO 
 

 SET QUOTED_IDENTIFIER ON 
 GO 
 

 -- Start DB Changes 
 

 -- Update table to use datetime column instead of a computed column 
 

 BEGIN TRANSACTION 
 SET QUOTED_IDENTIFIER ON 
 SET ARITHABORT ON 
 SET NUMERIC_ROUNDABORT OFF 
 SET CONCAT_NULL_YIELDS_NULL ON 
 SET ANSI_NULLS ON 
 SET ANSI_PADDING ON 
 SET ANSI_WARNINGS ON 
 COMMIT 
 BEGIN TRANSACTION 
 GO 
 

 ALTER TABLE [brady_membership].[dbversion] 
 DROP COLUMN [timestamp] 
 GO 
 

 ALTER TABLE [brady_membership].[dbversion] 
 ADD [timestamp] [datetime2](7) NOT NULL CONSTRAINT [MEMBERSHIP_DBVER_DEFAULT] DEFAULT (getdate()) 
 GO 
 COMMIT 
 

 -- Replace updateversion proc 
 IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[prc_updateversion]') AND type in (N'P')) 
    DROP PROCEDURE [brady_membership].[prc_updateversion] 
 GO 
 

 CREATE PROCEDURE brady_membership.prc_updateversion 
     ( @NewMajor    INT 
     , @NewMinor    INT 
     , @NewPatch    INT 
     , @NewCompile  INT 
    ) 
 AS 
 BEGIN 
    DECLARE @version NVARCHAR(43) 
    SET @version = (CAST(@NewMajor AS NVARCHAR(10)) + '.' + CAST(@NewMinor AS NVARCHAR(10)) + '.' + CAST(@NewPatch AS NVARCHAR(10)) + '.' + CAST(@NewCompile AS NVARCHAR(10))) 
     
    INSERT INTO brady_membership.dbversion (version, major, minor, patch, compile, timestamp) 
       VALUES (@version, @NewMajor, @NewMinor, @NewPatch, @NewCompile, getDate()) 
 END 
 GO 
 

 -- End DB Changes 
 

 [brady_membership].[prc_updateversion] 2016, 2, 0, 1 
 GO 
 

 SET NOEXEC OFF 

-----------------------------------------------------------------------------------
--membership_2016_2_0_2.sql
-----------------------------------------------------------------------------------
 
 BEGIN TRY 
    EXEC [brady_membership].[prc_checkversion] 2016,2,0,1,2016,2,0,2 
 END TRY 
 BEGIN CATCH 
    PRINT ERROR_MESSAGE() 
    SET NOEXEC ON 
 END CATCH 
 GO 
 

 SET ANSI_NULLS ON 
 GO 
 

 SET QUOTED_IDENTIFIER ON 
 GO 
 

 -- Start DB Changes 
 

 BEGIN TRANSACTION 
 SET QUOTED_IDENTIFIER ON 
 SET ARITHABORT ON 
 SET NUMERIC_ROUNDABORT OFF 
 SET CONCAT_NULL_YIELDS_NULL ON 
 SET ANSI_NULLS ON 
 SET ANSI_PADDING ON 
 SET ANSI_WARNINGS ON 
 COMMIT 
 

 BEGIN TRANSACTION 
 GO 
 

 CREATE TABLE [brady_membership].[BwfRecordLock]( 
 	[Id] [bigint] NOT NULL, 
 	[EntityType] [nvarchar](256) NOT NULL, 
 	[EntityId] [bigint] NOT NULL, 
 	[Username] [nvarchar](256) NOT NULL, 
 	[Reason] [nvarchar](256) NOT NULL, 
 	[Context] [nvarchar](256) NOT NULL, 
 	[TimeStamp] [datetime2](7) NOT NULL, 
  CONSTRAINT [PK_BwfRecordLock] PRIMARY KEY CLUSTERED  
 ( 
 	[Id] ASC 
 )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] 
 ) ON [PRIMARY] 
 GO 
 

 INSERT INTO [brady_membership].[NextHigh] ([NextHigh], [EntityName]) 
 VALUES (1, 'BwfRecordLock') 
 GO 
 

 CREATE TABLE [brady_membership].[ApiKey]( 
 	[Id] [bigint] NOT NULL, 
 	[Label] [nvarchar](256) NOT NULL, 
 	[Username] [nvarchar](256) NOT NULL, 
 	[CreatedAt] [datetimeoffset](7) NOT NULL, 
 	[LastUsed] [datetimeoffset](7) NOT NULL, 
 	[Key] [nvarchar](256) NOT NULL, 
 	[IsExpired] [bit] NOT NULL 
  CONSTRAINT [PK_ApiKey] PRIMARY KEY CLUSTERED  
 ( 
 	[Id] ASC 
 )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] 
 ) ON [PRIMARY] 
 GO 
 

 INSERT INTO [brady_membership].[NextHigh] ([NextHigh], [EntityName]) 
 VALUES (1, 'ApiKey') 
 GO 
 

 COMMIT 
 GO 
 

 -- End DB Changes 
 

 [brady_membership].[prc_updateversion] 2016, 2, 0, 2 
 GO 
 

 SET NOEXEC OFF 

-----------------------------------------------------------------------------------
--membership_2016_2_0_3.sql
-----------------------------------------------------------------------------------
 
 BEGIN TRY 
    EXEC [brady_membership].[prc_checkversion] 2016,2,0,2,2016,2,0,3 
 END TRY 
 BEGIN CATCH 
    PRINT ERROR_MESSAGE() 
    SET NOEXEC ON 
 END CATCH 
 GO 
 

 SET ANSI_NULLS ON 
 GO 
 

 SET QUOTED_IDENTIFIER ON 
 GO 
 

 -- Start DB Changes 
 

 BEGIN TRANSACTION 
 SET QUOTED_IDENTIFIER ON 
 SET ARITHABORT ON 
 SET NUMERIC_ROUNDABORT OFF 
 SET CONCAT_NULL_YIELDS_NULL ON 
 SET ANSI_NULLS ON 
 SET ANSI_PADDING ON 
 SET ANSI_WARNINGS ON 
 COMMIT  
 GO 
 

 BEGIN TRANSACTION 
 

 -- Increase next high value 
 

 ALTER TABLE [brady_membership].[nexthigh] DROP CONSTRAINT [uk_membershipnexthigh1]; 
 

 ALTER TABLE [brady_membership].[NextHigh] ALTER COLUMN [EntityName] NVARCHAR(255); 
 GO 
 

 ALTER TABLE [brady_membership].[nexthigh] ADD CONSTRAINT [uk_membershipnexthigh1] UNIQUE NONCLUSTERED  
 ( 
 	[entityname] ASC 
 ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) 
 GO 
 

 --------------------- 
 

 CREATE TABLE [brady_membership].[PermissionGroup]( 
 	[Id] [bigint] NOT NULL, 
 	[Name] [nvarchar](500) NOT NULL, 
 	[Description] [nvarchar](1000) NULL, 
  CONSTRAINT [PK_PermissionGroup] PRIMARY KEY CLUSTERED  
 ( 
 	[Id] ASC 
 )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] 
 ) ON [PRIMARY] 
 

 GO 
 

 INSERT INTO [brady_membership].[NextHigh] ([NextHigh], [EntityName]) VALUES (1, 'PermissionGroup') 
 GO 
 

 --------------------- 
 

 CREATE TABLE [brady_membership].[PermGroupDataPermission]( 
 	[Id] [bigint] NOT NULL, 
 	[PermissionGroupId] [bigint] NULL, 
 	[EntityType] [nvarchar](256) NOT NULL, 
 	[PermissionName] [nvarchar](256) NOT NULL, 
 	[EntityDescription] [nvarchar](1000) NOT NULL, 
     [DataService] [nvarchar](256) NOT NULL, 
 	[Description] [nvarchar](1000) NULL, 
  CONSTRAINT [PK_PermGroupDataPermission] PRIMARY KEY CLUSTERED  
 ( 
 	[Id] ASC 
 )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] 
 ) ON [PRIMARY] 
 GO 
 

 ALTER TABLE [brady_membership].[PermGroupDataPermission] WITH CHECK ADD  CONSTRAINT [FK_PermGroupDataPermission_PermissionGroup] FOREIGN KEY([PermissionGroupId]) 
 REFERENCES [brady_membership].[PermissionGroup] ([Id]) 
 ON UPDATE CASCADE 
 ON DELETE CASCADE 
 GO 
 

 ALTER TABLE [brady_membership].[PermGroupDataPermission] CHECK CONSTRAINT [FK_PermGroupDataPermission_PermissionGroup] 
 GO 
 

 INSERT INTO [brady_membership].[NextHigh] ([NextHigh], [EntityName]) VALUES (1, 'PermissionGroupDataLevelPermission') 
 GO 
 

 --------------------- 
 

 CREATE TABLE [brady_membership].[PermGroupServicePermission]( 
 	[Id] [bigint] NOT NULL, 
 	[PermissionGroupId] [bigint] NULL, 
 	[Type] [nvarchar](256) NOT NULL, 
 	[Name] [nvarchar](256) NOT NULL, 
     [DataService] [nvarchar](256) NOT NULL, 
 	[Description] [nvarchar](1000) NULL, 
  CONSTRAINT [PK_PermGroupServicePermission] PRIMARY KEY CLUSTERED  
 ( 
 	[Id] ASC 
 )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] 
 ) ON [PRIMARY] 
 

 GO 
 

 ALTER TABLE [brady_membership].[PermGroupServicePermission]  WITH CHECK ADD  CONSTRAINT [FK_PermGroupServicePermission_PermissionGroup] FOREIGN KEY([PermissionGroupId]) 
 REFERENCES [brady_membership].[PermissionGroup] ([Id]) 
 ON UPDATE CASCADE 
 ON DELETE CASCADE 
 GO 
 

 ALTER TABLE [brady_membership].[PermGroupServicePermission] CHECK CONSTRAINT [FK_PermGroupServicePermission_PermissionGroup] 
 GO 
 

 

 INSERT INTO [brady_membership].[NextHigh] ([NextHigh], [EntityName]) VALUES (1, 'PermissionGroupServiceLevelPermission') 
 GO 
 

 --------------------- 
 

 COMMIT 
 GO 
 

 -- End DB Changes 
 

 [brady_membership].[prc_updateversion] 2016,2,0,3 
 GO 
 

 SET NOEXEC OFF 

-----------------------------------------------------------------------------------
--membership_2016_2_0_4.sql
-----------------------------------------------------------------------------------
 
 BEGIN TRY 
    EXEC [brady_membership].[prc_checkversion] 2016,2,0,3,2016,2,0,4 
 END TRY 
 BEGIN CATCH 
    PRINT ERROR_MESSAGE() 
    SET NOEXEC ON 
 END CATCH 
 GO 
 

 SET ANSI_NULLS ON 
 GO 
 

 SET QUOTED_IDENTIFIER ON 
 GO 
 

 -- NO DB CHANGES IN THIS VERSION 
 

 [brady_membership].[prc_updateversion] 2016, 2, 0, 4 
 GO 
 

 SET NOEXEC OFF 
