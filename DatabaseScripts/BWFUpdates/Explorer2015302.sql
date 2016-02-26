USE ScrapTest 
 GO 
 

 IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2015.3.0.1') is null) 
     raiserror('Cannot upgrade to 2015.3.0.2 before database is upgraded to 2015.3.0.1', 20, -1) with log 
 ELSE IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2015.3.0.2') is not null) 
     raiserror('Cannot upgrade to 2015.3.0.2 as database is already at this version or greater', 20, -1) with log 
 GO 
 
 
 SET ANSI_NULLS ON 
 GO 
 

 SET QUOTED_IDENTIFIER ON 
 GO 
 

 ALTER TABLE [exp_view].[a_GridPresentation] ALTER COLUMN [OrderBy] NVARCHAR(2000) NULL; 
 GO 
 

 ALTER TABLE [exp_view].[GridPresentation] ALTER COLUMN [OrderBy] NVARCHAR(2000) NULL; 
 GO 
 

 INSERT INTO [exp_common].[ExplorerVersion] 
            ([Version] 
            ,[Major] 
            ,[Minor] 
            ,[Patch] 
            ,[Compile] 
            ,[TimeStamp]) 
      VALUES 
            ('2015.3.0.2',2015,3,0,2,getdate()) 
 GO 
