
--SELECT is_read_committed_snapshot_on FROM sys.databases WHERE name= 'ScrapTest'

ALTER DATABASE ScrapTest 
SET READ_COMMITTED_SNAPSHOT ON
GO
