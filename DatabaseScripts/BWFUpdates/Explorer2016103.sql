USE scraptest 
GO 


IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2016.1.0.2') is null) 
    raiserror('Cannot upgrade to 2016.1.0.3 before database is upgraded to 2016.1.0.2', 20, -1) with log 
ELSE IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2016.1.0.3') is not null) 
    raiserror('Cannot upgrade to 2016.1.0.3 as database is already at this version or greater', 20, -1) with log 
GO 


 SET ANSI_NULLS ON 
 GO 
 

 SET QUOTED_IDENTIFIER ON 
 GO 
 

 ALTER TABLE [exp_view].[a_GridPresentation] ADD [DisableGridSorting] [bit] NOT NULL DEFAULT (0); 
 GO 
 

 ALTER TABLE [exp_view].[GridPresentation] ADD [DisableGridSorting] [bit] NOT NULL DEFAULT (0); 
 GO 
 

 INSERT INTO [exp_common].[ExplorerVersion] 
            ([Version] 
            ,[Major] 
            ,[Minor] 
            ,[Patch] 
            ,[Compile] 
            ,[TimeStamp]) 
      VALUES 
            ('2016.1.0.3',2016,1,0,3,getdate()) 
 GO 
