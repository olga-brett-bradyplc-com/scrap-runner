
USE ScrapTest 
GO 


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
