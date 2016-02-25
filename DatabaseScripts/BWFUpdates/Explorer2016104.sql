USE scraptest 
GO 


IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2016.1.0.3') is null) 
   raiserror('Cannot upgrade to 2016.1.0.4 before database is upgraded to 2016.1.0.3', 20, -1) with log 
ELSE IF ((SELECT [Version] FROM [exp_common].[ExplorerVersion] WHERE [Version]='2016.1.0.4') is not null) 
   raiserror('Cannot upgrade to 2016.1.0.4 as database is already at this version or greater', 20, -1) with log 
GO 


SET ANSI_NULLS ON 
GO 


SET QUOTED_IDENTIFIER ON 
GO 


ALTER TABLE [exp_view].[Property] ALTER COLUMN [Name] NVARCHAR(1024) NOT NULL; 
GO 


ALTER TABLE [exp_view].[a_Property] ALTER COLUMN [Name] NVARCHAR(1024) NOT NULL; 
GO 


INSERT INTO [exp_common].[ExplorerVersion] 
           ([Version] 
           ,[Major] 
           ,[Minor] 
           ,[Patch] 
           ,[Compile] 
           ,[TimeStamp]) 
     VALUES 
           ('2016.1.0.4',2016,1,0,4,getdate()) 
GO 
