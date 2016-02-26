USE ScrapTest 
 GO 
 

 /****** Check: Preceeding Version Number ******/ 
 IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2015.3.0.2') is null) 
     raiserror('Cannot upgrade to 2016.1.0.1 before database is upgraded to 2015.3.0.2', 20, -1) with log 
 ELSE IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2016.1.0.1') is not null) 
     raiserror('Cannot upgrade to 2016.1.0.1 as database is already at this version or greater', 20, -1) with log 
 GO 
 

 SET ANSI_NULLS ON 
 GO 
 

 SET QUOTED_IDENTIFIER ON 
 GO 
 

 ALTER TABLE [exp_view].[ViewParameter] ADD [Alias] NVARCHAR(512) NULL 
 GO 
 

 ALTER TABLE [exp_view].[a_ViewParameter] ADD [Alias] NVARCHAR(512) NULL 
 GO 
 

 INSERT INTO [exp_common].[ExplorerVersion] 
            ([Version] 
            ,[Major] 
            ,[Minor] 
            ,[Patch] 
            ,[Compile] 
            ,[TimeStamp]) 
      VALUES 
            ('2016.1.0.1',2016,1,0,1,getdate()) 
 GO 
