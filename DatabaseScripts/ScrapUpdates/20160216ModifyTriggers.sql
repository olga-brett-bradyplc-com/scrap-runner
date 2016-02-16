
/****** Object:  Trigger [dbo].[trig_UpdateContainerChangeTable]    Script Date: 2/16/2016 4:54:08 PM ******/
/* Added SET NOCOUNT ON/SET NOCOUNT OFF. Removed PRINT statements */

ALTER         TRIGGER [dbo].[trig_UpdateContainerChangeTable]
ON [dbo].[ContainerMaster] 
FOR INSERT, UPDATE, DELETE 
AS
DECLARE @bDeleted  AS BIT
DECLARE @bInserted AS BIT
DECLARE @bUseInsert AS BIT
DECLARE @bUseDelete AS BIT
DECLARE @bUseInsertDelete AS BIT
DECLARE @insContNum AS VARCHAR(15), @delContNum AS VARCHAR(15)
DECLARE @insTermId AS VARCHAR (10), @delTermId AS VARCHAR (10)
DECLARE @insContType AS VARCHAR(5), @delContType AS VARCHAR(5)
DECLARE @insContSize AS VARCHAR(5), @delContSize AS VARCHAR(5)
DECLARE @insContBarCode AS VARCHAR(30), @delContBarCode AS VARCHAR(30)
DECLARE @insRegionId AS VARCHAR(10), @delRegionId AS VARCHAR(10)
DECLARE @sql AS VARCHAR(600)
DECLARE insertedCursor CURSOR LOCAL FOR SELECT ContainerNumber, ContainerTerminalId, ContainerType, ContainerSize, ContainerBarCodeNo,ContainerRegionId FROM INSERTED
DECLARE deletedCursor CURSOR LOCAL FOR SELECT ContainerNumber, ContainerTerminalId, ContainerType, ContainerSize, ContainerBarCodeNo,ContainerRegionId FROM DELETED

SET NOCOUNT ON
SET @bDeleted = 0
SET @bInserted = 0
SET @bUseInsert = 0
SET @bUseDelete = 0
SET @bUseInsertDelete = 0

OPEN insertedCursor
FETCH insertedCursor INTO @insContNum, @insTermId, @insContType, @insContSize,@insContBarCode,@insRegionId
IF @@FETCH_STATUS = 0 BEGIN
	--PRINT 'insContNum ' + @insContNum

	IF @insTermId IS NULL BEGIN
		SET @insTermId = ''
	END
	--PRINT 'insTermId ' + @insTermId

	IF @insContType IS NULL BEGIN
		SET @insContType = ''
	END
	--PRINT 'InsContType ' + @insContType

	IF @insContSize IS NULL BEGIN
		SET @insContSize = ''
	END
	--PRINT 'insContSize ' + @insContSize

	IF @insContBarCode IS NULL BEGIN
		SET @insContBarCode = ''
	END
	--PRINT 'insContBarCode ' + @insContBarCode

	IF @insRegionId IS NULL BEGIN
		SET @insRegionId = ''
	END
	--PRINT 'insRegionId ' + @insRegionId

	--PRINT 'INSERTED FETCH_STATUS = 0'
	SET @bInserted = 1
END

OPEN deletedCursor
FETCH deletedCursor INTO @delContNum, @delTermId, @delContType, @delContSize, @delContBarCode,@delRegionId

IF @@FETCH_STATUS = 0 BEGIN
	--PRINT 'delContNum ' + @delContNum

	IF @delTermId IS NULL BEGIN
		SET @delTermId = ''
	END
	--PRINT 'delTermId ' + @delTermId

	IF @delContType IS NULL BEGIN
		SET @delContType = ''
	END
	--PRINT 'delContType ' + @delContType

	IF @delContSize IS NULL BEGIN
		SET @delContSize = ''
	END
	--PRINT 'delContSize ' + @delContSize

	IF @delContBarCode IS NULL BEGIN
		SET @delContBarCode = ''
	END
	--PRINT 'delContBarCode ' + @delContBarCode

	IF @delRegionId IS NULL BEGIN
		SET @delRegionId = ''
	END
	--PRINT 'delRegionId ' + @delRegionId

	--PRINT 'deleted FETCH_STATUS = 0'
	SET @bDeleted = 1

END

-- THIS IS AN UPDATE SO SEE IF ANY OF THE FIELDS HAVE CHANGED
IF (@bDeleted = 1 AND @bInserted = 1) BEGIN
	IF (@insContNum <> @delContNum) 
	BEGIN
		--PRINT 'DELETE OLD AND INSERT NEW NUMBER'
		SET @bUseInsertDelete = 1
	END
	IF ((@insContType <> @delContType) OR
	    (@delTermId <> @insTermId) OR
	    (@insContSize <> @delContSize) OR
	    (@insContBarCode <> @delContBarCode) OR
	    (@insRegionId <> @delRegionId))
	BEGIN
		--PRINT 'UPDATE WITH RELEVENT CHANGES'
		SET @bUseInsert = 1
	END
	--ELSE BEGIN
		--PRINT 'UPDATE WITH NO RELEVENT CHANGES'
	--END
END
ELSE IF (@bInserted = 1) BEGIN
	--PRINT 'bUseInsert = 1'
	SET @bUseInsert = 1
END
ELSE IF (@bDeleted = 1) BEGIN
	--PRINT 'bUseDelete = 1'
	SET @bUseDelete = 1
END		

IF (@bUseInsertDelete = 1) BEGIN
	SET @sql = 'DELETE FROM ContainerChange WHERE ContainerNumber = ''' + @insContNum + ''''
	--PRINT @sql
	EXEC (@sql)
	SET @sql = 'DELETE FROM ContainerChange WHERE ContainerNumber = ''' + @delContNum + ''''
	--PRINT @sql
	EXEC (@sql)
	SET @sql = 'INSERT INTO ContainerChange(ContainerNumber,ContainerType,ContainerSize,ActionDate,ActionFlag,TerminalId,ContainerBarCodeNo,RegionId) VALUES (''' + @insContNum + ''', ''' + @insContType + ''', ''' + @insContSize + ''', GETDATE() , ''M'', ''' + @insTermId + ''', ''' + @insContBarCode + ''', ''' + @insRegionId + ''')'
	--PRINT (@sql)
	EXEC(@sql)
	SET @sql = 'INSERT INTO ContainerChange(ContainerNumber,ContainerType,ContainerSize,ActionDate,ActionFlag,TerminalId,ContainerBarCodeNo,RegionId) VALUES (''' + @delContNum + ''', ''' + @delContType + ''', ''' + @delContSize + ''', GETDATE() , ''D'', ''' + @delTermId + ''', ''' + @delContBarCode + ''', ''' + @delRegionId + ''')'
	--PRINT (@sql)
	EXEC(@sql)
END
IF (@bUseInsert = 1) BEGIN
	SET @sql = 'DELETE FROM ContainerChange WHERE ContainerNumber = ''' + @insContNum + ''''
	--PRINT @sql
	EXEC (@sql)
	--PRINT 'DELETE STATEMENT EXECUTED(insert block)'
	SET @sql = 'INSERT INTO ContainerChange(ContainerNumber,ContainerType,ContainerSize,ActionDate,ActionFlag,TerminalId,ContainerBarCodeNo,RegionId) VALUES (''' + @insContNum + ''', ''' + @insContType + ''', ''' + @insContSize + ''', GETDATE() , ''M'', ''' + @insTermId + ''', ''' + @insContBarCode + ''', ''' + @insRegionId + ''')'
	--PRINT (@sql)
	EXEC(@sql)
END
ELSE IF (@bUseDelete = 1) BEGIN
	SET @sql = 'DELETE FROM ContainerChange WHERE ContainerNumber = ''' + @delContNum + ''''
	--PRINT @sql
	EXEC (@sql)
	--PRINT 'DELETE STATEMENT EXECUTED(delete block)'
	SET @sql = 'INSERT INTO ContainerChange(ContainerNumber,ContainerType,ContainerSize,ActionDate,ActionFlag,TerminalId,ContainerBarCodeNo,RegionId) VALUES (''' + @delContNum + ''', ''' + @delContType + ''', ''' + @delContSize + ''', GETDATE() , ''D'', ''' + @delTermId + ''', ''' + @delContBarCode + ''', ''' + @delRegionId + ''')'
	--PRINT (@sql)
	EXEC(@sql)
END

DEALLOCATE insertedCursor
DEALLOCATE deletedCursor
SET NOCOUNT OFF
GO


/****** Object:  Trigger [dbo].[trig_UpdateTerminalChangeTable]    Script Date: 2/16/2016 11:46:50 AM ******/
/* Added SET NOCOUNT ON/SET NOCOUNT OFF. Removed PRINT statements */

ALTER         TRIGGER [dbo].[trig_UpdateTerminalChangeTable]
ON [dbo].[CustomerMaster] 
FOR INSERT, UPDATE, DELETE 
AS
DECLARE @bDeleted  AS BIT
DECLARE @bInserted AS BIT
DECLARE @bUseInsert AS BIT
DECLARE @bUseDelete AS BIT

DECLARE @insTerminalId AS VARCHAR(10), @delTerminalId AS VARCHAR(10)
DECLARE @insCustRegionId AS VARCHAR (10), @delCustRegionid AS VARCHAR (10)
DECLARE @insCustType AS CHAR(1), @delCustType AS CHAR(1)
DECLARE @insCustHostCode AS VARCHAR(15), @delCustHostCode AS VARCHAR(15)
DECLARE @insCustCode4_4 AS VARCHAR(8), @delCustCode4_4 AS VARCHAR(8)
DECLARE @insCustName AS VARCHAR(30), @delCustName AS VARCHAR(30)
DECLARE @insCustAddress1 AS VARCHAR(38), @delCustAddress1 AS VARCHAR(38)
DECLARE @insCustAddress2 AS VARCHAR(38), @delCustAddress2 AS VARCHAR(38)
DECLARE @insCustCity AS VARCHAR(30), @delCustCity AS VARCHAR(30)
DECLARE @insCustState AS CHAR(2), @delCustState AS CHAR(2)
DECLARE @insCustZip AS VARCHAR(10), @delCustZip AS VARCHAR(10)
DECLARE @insCustCountry AS CHAR(3), @delCustCountry AS CHAR(3)
DECLARE @insCustPhone1 AS VARCHAR(20), @delCustPhone1 AS VARCHAR(20)
DECLARE @insCustContact1 AS VARCHAR(30), @delCustContact1 AS VARCHAR(30)
DECLARE @insCustOpenTime AS VARCHAR(20), @delCustOpenTime AS VARCHAR(20)
DECLARE @insCustCloseTime AS VARCHAR(20), @delCustCloseTime AS VARCHAR(20)
DECLARE @insCustLatitude AS VARCHAR(20), @delCustLatitude AS VARCHAR(20)
DECLARE @insCustLongitude AS VARCHAR(20), @delCustLongitude AS VARCHAR(20)
DECLARE @insCustRadius AS VARCHAR(20), @delCustRadius AS VARCHAR(20)
DECLARE @insCustDriverInstructions AS VARCHAR(300), @delCustDriverInstructions AS VARCHAR(300)
DECLARE @insComChgDateTime AS VARCHAR(20), @delComChgDateTime AS VARCHAR(20)
DECLARE @insLocChgDateTime AS VARCHAR(20), @delLocChgDateTime AS VARCHAR(20)

DECLARE @sql AS VARCHAR(3000)
DECLARE insertedCursor CURSOR LOCAL FOR 
  SELECT ServingTerminalId, CustRegionId, CustType, CustHostCode, CustCode4_4,CustName,CustAddress1,
         CustAddress2,CustCity,CustState,CustZip,CustCountry,CustPhone1,CustContact1,CustOpenTime,
         CustCloseTime,CustLatitude,CustLongitude,CustRadius,CustDriverInstructions,
         ComChgDateTime,LocChgDateTime
     FROM INSERTED where CustType = 'Y'
DECLARE deletedCursor CURSOR LOCAL FOR 
    SELECT ServingTerminalId, CustRegionId, CustType, CustHostCode, CustCode4_4,CustName,
         CustAddress1,CustAddress2,CustCity,CustState,CustZip,CustCountry,CustPhone1,
         CustContact1,CustOpenTime,CustCloseTime,CustLatitude,CustLongitude,CustRadius,CustDriverInstructions,
         ComChgDateTime,LocChgDateTime
    FROM DELETED where CustType = 'Y'

SET NOCOUNT ON
SET @bDeleted = 0
SET @bInserted = 0
SET @bUseInsert = 0
SET @bUseDelete = 0

OPEN insertedCursor
FETCH insertedCursor INTO 
	@insTerminalId, @insCustRegionId, @insCustType, @insCustHostCode, @insCustCode4_4,@insCustName,
	@insCustAddress1,@insCustAddress2,@insCustCity,@insCustState,@insCustZip,@insCustCountry,@insCustPhone1,
	@insCustContact1,@insCustOpenTime,@insCustCloseTime,@insCustLatitude,@insCustLongitude,@insCustRadius,
	@insCustDriverInstructions,@insComChgDateTime,@insLocChgDateTime
	
	IF @@FETCH_STATUS = 0 BEGIN
	--PRINT 'insTerminalId ' + @insTerminalId
	
	--PRINT 'INSERTED FETCH_STATUS = 0'
	SET @bInserted = 1
END

OPEN deletedCursor
FETCH deletedCursor INTO 
	@delTerminalId, @delCustRegionId, @delCustType, @delCustHostCode, @delCustCode4_4,@delCustName,
	@delCustAddress1,@delCustAddress2,@delCustCity,@delCustState,@delCustZip,@delCustCountry,@delCustPhone1,
	@delCustContact1,@delCustOpenTime,@delCustCloseTime,@delCustLatitude,@delCustLongitude,@delCustRadius,
	@delCustDriverInstructions,@delComChgDateTime,@delLocChgDateTime

IF @@FETCH_STATUS = 0 BEGIN
	--PRINT 'delTerminalId ' + @delTerminalId

	--PRINT 'deleted FETCH_STATUS = 0'
	SET @bDeleted = 1

END

-- THIS IS AN UPDATE SO SEE IF ANY OF THE FIELDS HAVE CHANGED
IF (@bDeleted = 1 AND @bInserted = 1) BEGIN
	IF ((@insTerminalId <> @delTerminalId)         OR (@insTerminalId IS NOT NULL AND @delTerminalId IS NULL)         OR (@insTerminalId IS NULL AND @delTerminalId IS NOT NULL) OR
	    (@insCustRegionId <> @delCustRegionId)     OR (@insCustRegionId IS NOT NULL AND @delCustRegionId IS NULL)     OR (@insCustRegionId IS NULL AND @delCustRegionId IS NOT NULL) OR
	    (@insCustType <> @delCustType)             OR (@insCustType IS NOT NULL AND @delCustType IS NULL)             OR (@insCustType IS NULL AND @delCustType IS NOT NULL) OR
	    (@insCustHostCode <> @delCustHostCode)     OR (@insCustHostCode IS NOT NULL AND @delCustHostCode IS NULL)     OR (@insCustHostCode IS NULL AND @delCustHostCode IS NOT NULL) OR
	    (@insCustCode4_4 <> @delCustCode4_4)       OR (@insCustCode4_4 IS NOT NULL AND @delCustCode4_4 IS NULL)       OR (@insCustCode4_4 IS NULL AND @delCustCode4_4 IS NOT NULL) OR
	    (@insCustName <> @delCustName)             OR (@insCustName IS NOT NULL AND @delCustName IS NULL)             OR (@insCustName IS NULL AND @delCustName IS NOT NULL) OR
	    (@insCustAddress1 <> @delCustAddress1)     OR (@insCustAddress1 IS NOT NULL AND @delCustAddress1 IS NULL)     OR (@insCustAddress1 IS NULL AND @delCustAddress1 IS NOT NULL) OR 
	    (@insCustAddress2 <> @delCustAddress2)     OR (@insCustAddress2 IS NOT NULL AND @delCustAddress2 IS NULL)     OR (@insCustAddress2 IS NULL AND @delCustAddress2 IS NOT NULL) OR 
	    (@insCustCity <> @delCustCity)             OR (@insCustCity IS NOT NULL AND @delCustCity IS NULL)             OR (@insCustCity IS NULL AND @delCustCity IS NOT NULL) OR
	    (@insCustState <> @delCustState)           OR (@insCustState IS NOT NULL AND @delCustState IS NULL)           OR (@insCustState IS NULL AND @delCustState IS NOT NULL) OR
	    (@insCustZip <> @delCustZip)               OR (@insCustZip IS NOT NULL AND @delCustZip IS NULL)               OR (@insCustZip IS NULL AND @delCustZip IS NOT NULL) OR
	    (@insCustCountry <> @delCustCountry)       OR (@insCustCountry IS NOT NULL AND @delCustCountry IS NULL)       OR (@insCustCountry IS NULL AND @delCustCountry IS NOT NULL) OR
	    (@insCustPhone1 <> @delCustPhone1)         OR (@insCustPhone1 IS NOT NULL AND @delCustPhone1 IS NULL)         OR (@insCustPhone1 IS NULL AND @delCustPhone1 IS NOT NULL) OR
	    (@insCustContact1 <> @delCustContact1)     OR (@insCustContact1 IS NOT NULL AND @delCustContact1 IS NULL)     OR (@insCustContact1 IS NULL AND @delCustContact1 IS NOT NULL) OR
	    (@insCustOpenTime <> @delCustOpenTime)     OR (@insCustOpenTime IS NOT NULL AND @delCustOpenTime IS NULL)     OR (@insCustOpenTime IS NULL AND @delCustOpenTime IS NOT NULL) OR
	    (@insCustCloseTime <> @delCustCloseTime)   OR (@insCustCloseTime IS NOT NULL AND @delCustCloseTime IS NULL)   OR (@insCustCloseTime IS NULL AND @delCustCloseTime IS NOT NULL) OR
	    (@insCustLatitude <> @delCustLatitude)     OR (@insCustLatitude IS NOT NULL AND @delCustLatitude IS NULL)     OR (@insCustLatitude IS NULL AND @delCustLatitude IS NOT NULL) OR
	    (@insCustLongitude <> @delCustLongitude)   OR (@insCustLongitude IS NOT NULL AND @delCustLongitude IS NULL)   OR (@insCustLongitude IS NULL AND @delCustLongitude IS NOT NULL) OR
	    (@insCustDriverInstructions <> @delCustDriverInstructions) OR (@insCustDriverInstructions IS NOT NULL AND @delCustDriverInstructions IS NULL) OR (@insCustDriverInstructions IS NULL AND @delCustDriverInstructions IS NOT NULL) OR
	    (@insComChgDateTime <> @delComChgDateTime) OR (@insComChgDateTime IS NOT NULL AND @delComChgDateTime IS NULL) OR (@insComChgDateTime IS NULL AND @delComChgDateTime IS NOT NULL) OR
	    (@insLocChgDateTime <> @delLocChgDateTime) OR (@insLocChgDateTime IS NOT NULL AND @delLocChgDateTime IS NULL) OR (@insLocChgDateTime IS NULL AND @delLocChgDateTime IS NOT NULL) OR
	    (@insCustRadius <> @delCustRadius)         OR (@insCustRadius IS NOT NULL AND @delCustRadius IS NULL)         OR (@insCustRadius IS NULL AND @delCustRadius IS NOT NULL))
	BEGIN
		--PRINT 'UPDATE WITH RELEVENT CHANGES'
		SET @bUseInsert = 1
	END
	--ELSE BEGIN
		--PRINT 'UPDATE WITH NO RELEVENT CHANGES'
	--END
END
ELSE IF (@bInserted = 1) BEGIN
	--PRINT 'bUseInsert = 1'
	SET @bUseInsert = 1
END
ELSE IF (@bDeleted = 1) BEGIN
	--PRINT 'bUseDelete = 1'
	SET @bUseDelete = 1
END	
	
--check for apostrophes within the data for every field
--replace  ' with ''
SET @insTerminalId = REPLACE(@insTerminalId,'''','''''')
SET @insCustRegionId = REPLACE(@insCustRegionId,'''','''''')
SET @insCustType = REPLACE(@insCustType,'''','''''')
SET @insCustHostCode = REPLACE(@insCustHostCode,'''','''''')
SET @insCustCode4_4 = REPLACE(@insCustCode4_4,'''','''''')
SET @insCustName = REPLACE(@insCustName,'''','''''')
SET @insCustAddress1 = REPLACE(@insCustAddress1,'''','''''')
SET @insCustAddress2 = REPLACE(@insCustAddress2,'''','''''')
SET @insCustCity = REPLACE(@insCustCity,'''','''''')
SET @insCustState = REPLACE(@insCustState,'''','''''')
SET @insCustZip = REPLACE(@insCustZip,'''','''''')
SET @insCustCountry = REPLACE(@insCustCountry,'''','''''')
SET @insCustPhone1 = REPLACE(@insCustPhone1,'''','''''')
SET @insCustContact1 = REPLACE(@insCustContact1,'''','''''')
SET @insCustOpenTime = REPLACE(@insCustOpenTime,'''','''''')
SET @insCustCloseTime = REPLACE(@insCustCloseTime,'''','''''')
SET @insCustLatitude = REPLACE(@insCustLatitude,'''','''''')
SET @insCustLongitude = REPLACE(@insCustLongitude,'''','''''')
SET @insCustRadius = REPLACE(@insCustRadius,'''','''''')
SET @insCustDriverInstructions = REPLACE(@insCustDriverInstructions,'''','''''')
SET @insComChgDateTime = REPLACE(@insComChgDateTime,'''','''''')
SET @insLocChgDateTime = REPLACE(@insLocChgDateTime,'''','''''')

SET @delTerminalId = REPLACE(@delTerminalId,'''','''''')
SET @delCustRegionId = REPLACE(@delCustRegionId,'''','''''')
SET @delCustType = REPLACE(@delCustType,'''','''''')
SET @delCustHostCode = REPLACE(@delCustHostCode,'''','''''')
SET @delCustCode4_4 = REPLACE(@delCustCode4_4,'''','''''')
SET @delCustName = REPLACE(@delCustName,'''','''''')
SET @delCustAddress1 = REPLACE(@delCustAddress1,'''','''''')
SET @delCustAddress2 = REPLACE(@delCustAddress2,'''','''''')
SET @delCustCity = REPLACE(@delCustCity,'''','''''')
SET @delCustState = REPLACE(@delCustState,'''','''''')
SET @delCustZip = REPLACE(@delCustZip,'''','''''')
SET @delCustCountry = REPLACE(@delCustCountry,'''','''''')
SET @delCustPhone1 = REPLACE(@delCustPhone1,'''','''''')
SET @delCustContact1 = REPLACE(@delCustContact1,'''','''''')
SET @delCustOpenTime = REPLACE(@delCustOpenTime,'''','''''')
SET @delCustCloseTime = REPLACE(@delCustCloseTime,'''','''''')
SET @delCustLatitude = REPLACE(@delCustLatitude,'''','''''')
SET @delCustLongitude = REPLACE(@delCustLongitude,'''','''''')
SET @delCustRadius = REPLACE(@delCustRadius,'''','''''')
SET @delCustDriverInstructions = REPLACE(@delCustDriverInstructions,'''','''''')
SET @delComChgDateTime = REPLACE(@delComChgDateTime,'''','''''')
SET @delLocChgDateTime = REPLACE(@delLocChgDateTime,'''','''''')

IF (@bUseInsert = 1) BEGIN
	SET @sql = 'DELETE FROM TerminalChange WHERE TerminalId = ''' + @insTerminalId + ''''
	EXEC (@sql)
	SET @sql = 'INSERT INTO TerminalChange(TerminalId) VALUES (''' + @insTerminalId + ''')' 
	EXEC(@sql)
	SET @sql = 'IF '''+@insCustRegionId+''' IS NOT NULL UPDATE TerminalChange SET RegionId = ''' + @insCustRegionId + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @insTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'IF '''+@insCustType+''' IS NOT NULL UPDATE TerminalChange SET CustType = ''' + @insCustType + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @insTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'IF '''+@insCustHostCode+''' IS NOT NULL UPDATE TerminalChange SET CustHostCode = ''' + @insCustHostCode + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @insTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'IF '''+@insCustCode4_4+''' IS NOT NULL UPDATE TerminalChange SET CustCode4_4 = ''' + @insCustCode4_4 + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @insTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'IF '''+@insCustName+''' IS NOT NULL UPDATE TerminalChange SET CustName = ''' + @insCustName + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @insTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'IF '''+@insCustAddress1+''' IS NOT NULL UPDATE TerminalChange SET CustAddress1 = ''' + @insCustAddress1 + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @insTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'IF '''+@insCustAddress2+''' IS NOT NULL UPDATE TerminalChange SET CustAddress2 = ''' + @insCustAddress2 + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @insTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'IF '''+@insCustCity+''' IS NOT NULL UPDATE TerminalChange SET CustCity = ''' + @insCustCity + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @insTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'IF '''+@insCustState+''' IS NOT NULL UPDATE TerminalChange SET CustState = ''' + @insCustState + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @insTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'IF '''+@insCustZip+''' IS NOT NULL UPDATE TerminalChange SET CustZip = ''' + @insCustZip + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @insTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'IF '''+@insCustCountry+''' IS NOT NULL UPDATE TerminalChange SET CustCountry = ''' + @insCustCountry + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @insTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'IF '''+@insCustPhone1+''' IS NOT NULL UPDATE TerminalChange SET CustPhone1 = ''' + @insCustPhone1 + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @insTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'IF '''+@insCustContact1+''' IS NOT NULL UPDATE TerminalChange SET CustContact1 = ''' + @insCustContact1 + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @insTerminalId + ''''
	EXEC(@sql)	
	SET @sql = 'IF '''+@insCustDriverInstructions+''' IS NOT NULL UPDATE TerminalChange SET CustDriverInstructions = ''' + @insCustDriverInstructions + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @insTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'UPDATE TerminalChange SET CustOpenTime = CONVERT(datetime,'+''''+@insCustOpenTime+''''+')'
	SET @sql = @sql + ' WHERE TerminalId = ''' + @insTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'UPDATE TerminalChange SET CustCloseTime = CONVERT(datetime,'+''''+@insCustCloseTime+''''+')'
	SET @sql = @sql + ' WHERE TerminalId = ''' + @insTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'UPDATE TerminalChange SET CustLatitude = CONVERT(int,'+@insCustLatitude+')'
	SET @sql = @sql + ' WHERE TerminalId = ''' + @insTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'UPDATE TerminalChange SET CustLongitude = CONVERT(int,'+@insCustLongitude+')'
	SET @sql = @sql + ' WHERE TerminalId = ''' + @insTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'UPDATE TerminalChange SET CustRadius = CONVERT(int,'+@insCustRadius+')'
	SET @sql = @sql + ' WHERE TerminalId = ''' + @insTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'UPDATE TerminalChange SET ChgDateTime = GETDATE(),ChgActionFlag = ''M'''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @insTerminalId + ''''
	EXEC(@sql)
END
ELSE IF (@bUseDelete = 1) BEGIN
	SET @sql = 'DELETE FROM TerminalChange WHERE TerminalId = ''' + @delTerminalId + ''''
	EXEC (@sql)
	SET @sql = 'INSERT INTO TerminalChange(TerminalId) VALUES (''' + @delTerminalId + ''')' 
	EXEC(@sql)
	SET @sql = 'IF '''+@delCustRegionId+''' IS NOT NULL UPDATE TerminalChange SET RegionId = ''' + @delCustRegionId + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @delTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'IF '''+@delCustType+''' IS NOT NULL UPDATE TerminalChange SET CustType = ''' + @delCustType + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @delTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'IF '''+@delCustHostCode+''' IS NOT NULL UPDATE TerminalChange SET CustHostCode = ''' + @delCustHostCode + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @delTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'IF '''+@delCustCode4_4+''' IS NOT NULL UPDATE TerminalChange SET CustCode4_4 = ''' + @delCustCode4_4 + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @delTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'IF '''+@delCustName+''' IS NOT NULL UPDATE TerminalChange SET CustName = ''' + @delCustName + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @delTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'IF '''+@delCustAddress1+''' IS NOT NULL UPDATE TerminalChange SET CustAddress1 = ''' + @delCustAddress1 + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @delTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'IF '''+@delCustAddress2+''' IS NOT NULL UPDATE TerminalChange SET CustAddress2 = ''' + @delCustAddress2 + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @delTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'IF '''+@delCustCity+''' IS NOT NULL UPDATE TerminalChange SET CustCity = ''' + @delCustCity + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @delTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'IF '''+@delCustState+''' IS NOT NULL UPDATE TerminalChange SET CustState = ''' + @delCustState + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @delTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'IF '''+@delCustZip+''' IS NOT NULL UPDATE TerminalChange SET CustZip = ''' + @delCustZip + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @delTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'IF '''+@delCustCountry+''' IS NOT NULL UPDATE TerminalChange SET CustCountry = ''' + @delCustCountry + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @delTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'IF '''+@delCustPhone1+''' IS NOT NULL UPDATE TerminalChange SET CustPhone1 = ''' + @delCustPhone1 + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @delTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'IF '''+@delCustContact1+''' IS NOT NULL UPDATE TerminalChange SET CustContact1 = ''' + @delCustContact1 + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @delTerminalId + ''''
	EXEC(@sql)
	
	SET @sql = 'IF '''+@delCustDriverInstructions+''' IS NOT NULL UPDATE TerminalChange SET CustDriverInstructions = ''' + @delCustDriverInstructions + ''''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @delTerminalId + ''''
	EXEC(@sql)

	SET @sql = 'UPDATE TerminalChange SET CustOpenTime = CONVERT(datetime,'+''''+@delCustOpenTime+''''+')'
	SET @sql = @sql + ' WHERE TerminalId = ''' + @delTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'UPDATE TerminalChange SET CustCloseTime = CONVERT(datetime,'+''''+@delCustCloseTime+''''+')'
	SET @sql = @sql + ' WHERE TerminalId = ''' + @delTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'UPDATE TerminalChange SET CustLatitude = CONVERT(int,'+@delCustLatitude+')'
	SET @sql = @sql + ' WHERE TerminalId = ''' + @delTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'UPDATE TerminalChange SET CustLongitude = CONVERT(int,'+@delCustLongitude+')'
	SET @sql = @sql + ' WHERE TerminalId = ''' + @delTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'UPDATE TerminalChange SET CustRadius = CONVERT(int,'+@delCustRadius+')'
	SET @sql = @sql + ' WHERE TerminalId = ''' + @delTerminalId + ''''
	EXEC(@sql)
	SET @sql = 'UPDATE TerminalChange SET ChgDateTime = GETDATE(),ChgActionFlag = ''D'''
	SET @sql = @sql + ' WHERE TerminalId = ''' + @delTerminalId + ''''
	EXEC(@sql)
END

DEALLOCATE insertedCursor
DEALLOCATE deletedCursor
SET NOCOUNT OFF
GO


/****** Object:  Trigger [dbo].[trig_UpdateEmployeeChangeTable]    Script Date: 2/16/2016 12:28:49 PM ******/
/* Added SET NOCOUNT ON/SET NOCOUNT OFF. Removed PRINT statements */

ALTER         TRIGGER [dbo].[trig_UpdateEmployeeChangeTable]
ON [dbo].[EmployeeMaster] 
FOR INSERT, UPDATE, DELETE 
AS
DECLARE @bDeleted  AS BIT
DECLARE @bInserted AS BIT
DECLARE @ActionFlag AS CHAR(1)
DECLARE @insEmployeeId AS VARCHAR(10), @delEmployeeId AS VARCHAR(10), @useEmployeeId AS VARCHAR(10)
DECLARE @insLoginId AS VARCHAR(20), @delLoginId AS VARCHAR(20), @useLoginId AS VARCHAR(20)
DECLARE @insPassword AS VARCHAR (20), @delPassword AS VARCHAR (20), @usePassword AS VARCHAR(20)
DECLARE @insRegionId AS VARCHAR(10), @delRegionId AS VARCHAR(10), @useRegionId AS VARCHAR(10)
DECLARE @insActionFlag AS CHAR(1), @delActionFlag AS CHAR(1)
DECLARE @insAllowMapsAccess AS CHAR(1), @delAllowMapsAccess AS CHAR(1)
DECLARE @sql AS VARCHAR(600)
DECLARE insertedCursor CURSOR LOCAL FOR SELECT EmployeeId,LoginId,Password,RegionId,AllowMapsAccess,ActionFlag FROM INSERTED
DECLARE deletedCursor CURSOR LOCAL FOR SELECT EmployeeId,LoginId,Password,RegionId,AllowMapsAccess,ActionFlag FROM DELETED

SET NOCOUNT ON

OPEN insertedCursor
FETCH insertedCursor INTO @insEmployeeId, @insLoginId, @insPassword, @insRegionId,@insAllowMapsAccess,@insActionFlag
IF @@FETCH_STATUS = 0 BEGIN
	--PRINT 'insEmployeeId ' + @insEmployeeId

	IF @insLoginId IS NULL BEGIN
		SET @insLoginId = @insEmployeeId
	END
	--PRINT 'insLoginId ' + @insLoginId

	IF @insPassword IS NULL BEGIN
		SET @insPassword = ''
	END
	--PRINT 'insPassword ' + @insPassword

	IF @insRegionId IS NULL BEGIN
		SET @insRegionId = ''
	END
	--PRINT 'insRegionId ' + @insRegionId

	IF @insAllowMapsAccess IS NULL BEGIN
		SET @insAllowMapsAccess = ''
	END
	--PRINT 'insAllowMapsAccess ' + @insAllowMapsAccess

	IF @insActionFlag IS NULL BEGIN
		SET @insActionFlag = ''
	END
	--PRINT '@insActionFlag ' + @insActionFlag

	--PRINT 'INSERTED FETCH_STATUS = 0'
	SET @bInserted = 1
END

OPEN deletedCursor
FETCH deletedCursor INTO @delEmployeeId, @delLoginId, @delPassword, @delRegionId,@delAllowMapsAccess,@delActionFlag

IF @@FETCH_STATUS = 0 BEGIN
	--PRINT 'delEmployeeId ' + @delEmployeeId

	IF @delLoginId IS NULL BEGIN
		SET @delLoginId = @delEmployeeId
	END
	--PRINT 'delLoginId ' + @delLoginId

	IF @delPassword IS NULL BEGIN
		SET @delPassword = ''
	END
	--PRINT 'delPassword ' + @delPassword

	IF @delRegionId IS NULL BEGIN
		SET @delRegionId = ''
	END
	--PRINT 'delRegionId ' + @delRegionId

	IF @delAllowMapsAccess IS NULL BEGIN
		SET @delAllowMapsAccess = ''
	END
	--PRINT 'delAllowMapsAccess ' + @delAllowMapsAccess

	IF @delActionFlag IS NULL BEGIN
		SET @delActionFlag = ''
	END
	--PRINT '@delActionFlag ' + @delActionFlag

	--PRINT 'deleted FETCH_STATUS = 0'
	SET @bDeleted = 1

END

-- THIS IS AN UPDATE SO SEE IF ANY OF THE FIELDS HAVE CHANGED
IF (@bDeleted = 1 AND @bInserted = 1) 
    BEGIN
	    IF ((@insEmployeeId <> @delEmployeeId) OR
	        (@insLoginId <> @delLoginId) OR
	        (@insPassword <> @delPassword) OR
	        (@insRegionId <> @delRegionId) OR
	        (@insAllowMapsAccess <> @delAllowMapsAccess))
	    BEGIN
		   IF (@insAllowMapsAccess <> @delAllowMapsAccess AND @insAllowMapsAccess = 'Y')
			  SET @ActionFlag = 'A'
		   ELSE IF (@insAllowMapsAccess <> @delAllowMapsAccess AND @insAllowMapsAccess = 'N')
			  SET @ActionFlag = 'D'
		   ELSE
			  SET @ActionFlag = 'M'
	    END
	    ELSE
	    BEGIN
	     IF(@insActionFlag <> @delActionFlag)
		     IF (@insActionFlag = 'Y')
		       SET @ActionFlag = 'A'
	     END     
    END
ELSE IF (@bInserted = 1 AND @insAllowMapsAccess = 'Y') BEGIN
	SET @ActionFlag = 'A'
END
ELSE IF (@bDeleted = 1 AND @delAllowMapsAccess = 'Y') BEGIN
	SET  @ActionFlag = 'D'
END		

IF (@ActionFlag = 'A')
    BEGIN
	SET @sql = 'DELETE FROM EmployeeChange WHERE EmployeeId = ''' + @insEmployeeId + ''' AND LoginId = ''' + @insLoginId  
	           + ''' AND RegionId = ''' + @insRegionId  + ''' AND Password = ''' + @insPassword  + ''' AND ActionFlag = ''' + @ActionFlag + ''''
	--PRINT @sql
	EXEC (@sql)
	--PRINT 'DELETE STATEMENT EXECUTED(insert block)'
	SET @sql = 'INSERT INTO EmployeeChange(EmployeeId,LoginId,Password,ActionFlag,ChangeDateTime,RegionId) VALUES (''' 
	            + @insEmployeeId + ''', ''' + @insLoginId + ''', '''+ @insPassword + ''', ''' 
	            + @ActionFlag + ''', GETDATE(),''' + @insRegionId + ''')'
	--PRINT (@sql)
	EXEC(@sql)
END
IF (@ActionFlag = 'D')
    BEGIN
	SET @sql = 'DELETE FROM EmployeeChange WHERE EmployeeId = ''' + @delEmployeeId + ''' AND LoginId = ''' + @delLoginId  
	           + ''' AND RegionId = ''' + @delRegionId  + ''' AND Password = ''' + @delPassword  + ''' AND ActionFlag = ''' + @ActionFlag + ''''
	--PRINT @sql
	EXEC (@sql)
	--PRINT 'DELETE STATEMENT EXECUTED(insert block)'
	SET @sql = 'INSERT INTO EmployeeChange(EmployeeId,LoginId,Password,ActionFlag,ChangeDateTime,RegionId) VALUES (''' 
	            + @delEmployeeId + ''', ''' + @delLoginId + ''', '''+ @delPassword + ''', ''' 
	            + @ActionFlag + ''', GETDATE(),''' + @delRegionId + ''')'
	--PRINT (@sql)
	EXEC(@sql)
END
ELSE IF (@ActionFlag = 'M') BEGIN

	SET @ActionFlag = 'D'
	
	if (@insEmployeeId <> @delEmployeeId)
	  SET @useEmployeeId = @delEmployeeId
	else
	  SET @useEmployeeId = @insEmployeeId
	if (@insLoginId <> @delLoginId)
	  SET @useLoginId = @delLoginId
	else
	  SET @useLoginId = @insLoginId
	if (@insPassword <> @delPassword)
	  SET @usePassword = @delPassword
	else
	  SET @usePassword = @insPassword
	if (@insRegionId <> @delRegionId)
	  SET @useRegionId = @delRegionId
	else
	  SET @useRegionId = @insRegionId
	  
	SET @sql = 'DELETE FROM EmployeeChange WHERE EmployeeId = ''' + @useEmployeeId + ''' AND LoginId = ''' + @useLoginId  
	           + ''' AND RegionId = ''' + @useRegionId  + ''' AND Password = ''' + @usePassword  + ''' AND ActionFlag = ''' + @ActionFlag + ''''
	--PRINT @sql
	EXEC (@sql)
	--PRINT 'DELETE STATEMENT EXECUTED(insert block)'
	SET @sql = 'INSERT INTO EmployeeChange(EmployeeId,LoginId,Password,ActionFlag,ChangeDateTime,RegionId) VALUES (''' 
	            + @useEmployeeId + ''', ''' + @useLoginId + ''', '''+ @usePassword + ''', ''' 
	            + @ActionFlag + ''', GETDATE(),''' + @useRegionId + ''')'
	--PRINT (@sql)
	EXEC(@sql)
	WAITFOR DELAY '00:00:01';
	
	SET @ActionFlag = 'A'
		
	SET @sql = 'DELETE FROM EmployeeChange WHERE EmployeeId = ''' + @insEmployeeId + ''' AND LoginId = ''' + @insLoginId  
	           + ''' AND RegionId = ''' + @insRegionId  + ''' AND Password = ''' + @insPassword  + ''' AND ActionFlag = ''' + @ActionFlag + ''''
	--PRINT @sql
	EXEC (@sql)
	--PRINT 'DELETE STATEMENT EXECUTED(insert block)'
	SET @sql = 'INSERT INTO EmployeeChange(EmployeeId,LoginId,Password,ActionFlag,ChangeDateTime,RegionId) VALUES (''' 
	            + @insEmployeeId + ''', ''' + @insLoginId + ''', '''+ @insPassword + ''', ''' 
	            + @ActionFlag + ''', GETDATE(),''' + @insRegionId + ''')'
	--PRINT (@sql)
	EXEC(@sql)
END

DEALLOCATE insertedCursor
DEALLOCATE deletedCursor
SET NOCOUNT OFF
GO

/****** Object:  Trigger [dbo].[trig_IncrementMDTCodeTableVersion]    Script Date: 2/16/2016 12:34:24 PM ******/
/* Added SET NOCOUNT ON/SET NOCOUNT OFF. Removed PRINT statements */

ALTER TRIGGER [dbo].[trig_IncrementMDTCodeTableVersion] ON [dbo].[CodeTable] 
FOR INSERT, UPDATE, DELETE 
AS

DECLARE @INCREMENTCOUNTER INT
DECLARE @CURRENTCOUNT INT

SET NOCOUNT ON

IF EXISTS (SELECT * FROM Inserted WHERE CodeName = 'EXCEPTIONCODES' OR CodeName = 'DELAYCODES' OR CodeName = 'REASONCODES')
BEGIN
	SET @INCREMENTCOUNTER = 1
	--PRINT 'NEED TO UPDATE COUNTER'
END

IF EXISTS (SELECT * FROM Deleted WHERE CodeName = 'EXCEPTIONCODES' OR CodeName = 'DELAYCODES' OR CodeName = 'REASONCODES')
BEGIN
	SET @INCREMENTCOUNTER = 1
	--PRINT 'NEED TO UPDATE COUNTER'
END


IF @INCREMENTCOUNTER = 1
BEGIN
	SELECT @CURRENTCOUNT = CodeDisp1 FROM CodeTable WHERE CodeName = 'VERSION' and CodeValue = 'MDTCODETABLEVERSION'
	SET @CURRENTCOUNT = @CURRENTCOUNT + 1
	UPDATE CodeTable SET CodeDisp1 = @CURRENTCOUNT WHERE CodeName = 'VERSION' AND CodeValue = 'MDTCODETABLEVERSION'
	
END
SET NOCOUNT OFF
GO


