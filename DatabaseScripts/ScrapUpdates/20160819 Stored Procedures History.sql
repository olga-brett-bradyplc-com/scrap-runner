--dbo.pc_HistoryTripTable
--dbo.pc_HistoryTripSegmentTable
--dbo.pc_HistoryTripSegmentContainerTable
--dbo.pc_HistoryTripSegmentMileageTable
--dbo.pc_HistoryTripReferenceNumbersTable
--dbo.pc_HistoryTripSegmentContainerTimeTable
--dbo.pc_HistoryTripSegmentTimeTable
--dbo.pc_HistorySaveTrip
/*************************************************************
 10. pc_HistoryTripTable
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_HistoryTripTable')
            and   type = 'P')
   drop procedure pc_HistoryTripTable
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_HistoryTripTable

@pTripNumber  varchar(10)= NULL,
@pHistSeqNo   smallint,
@pHistAction  varchar(25),
@pErrorSave   smallint  OUTPUT

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

SET @pErrorSave = 0

INSERT INTO HistTrip
(
	HistSeqNo,
	HistAction,
	TripNumber,
	TripStatus,
	TripStatusDesc,
	TripAssignStatus,
	TripAssignStatusDesc,
	TripType,
	TripTypeDesc,
	TripSequenceNumber,
	TripSendFlag,
	TripDriverId,
	TripDriverName,
	TripCustHostCode,
	TripCustCode4_4,
	TripCustName,
	TripCustAddress1,
	TripCustAddress2,
	TripCustCity,
	TripCustState,
	TripCustZip,
	TripCustCountry,
	TripCustPhone1,
	TripTerminalId,
	TripTerminalName,
	TripRegionId,
	TripRegionName,
	TripContactName,
	TripSalesman,
	TripCustOpenTime,
	TripCustCloseTime,
	TripReadyDateTime,
	TripEnteredUserId,
	TripEnteredUserName,
	TripEnteredDateTime,
	TripStandardDriveMinutes,
	TripStandardStopMinutes,
	TripActualDriveMinutes,
	TripActualStopMinutes,
	TripContractNumber,
	TripShipmentNumber,
	TripCommodityPurchase,
	TripCommoditySale, 
	TripSpecInstructions,
	TripExpediteFlag,
	TripPrimaryContainerNumber,
	TripPrimaryContainerType,
	TripPrimaryContainerSize,
	TripPrimaryCommodityCode,
	TripPrimaryCommodityDesc,
	TripPrimaryContainerLocation,
	TripPowerId,
	TripCommodityScaleMsg,
	TripDriverIdPrev,
	TripCompletedDateTime,
	TripSendScaleNotificationFlag,
	TripSendReceiptFlag,
	TripExtendedFlag,
	TripExtendedReason,
	TripInProgressFlag,
	TripNightRunFlag,
	TripDoneMethod,
	TripCompletedUserId,
	TripCompletedUserName,
	TripReferenceNumber,
	TripChangedUserId,
	TripChangedUserName,
	TripChangedDateTime,
	TripDirectSegNumber,
	TripDirectDriveMinutes,
	TripDirectTotalMinutes,
	TripStartedDateTime,
	TripActualTotalMinutes,
	TripErrorDesc,
	TripDriverInstructions,
    TripDispatcherInstructions,
    TripScaleReferenceNumber,
    TripMultContainerFlag,
	TripSendReseqFlag,
	TripPowerAssetNumber,
	TripHaulerHostCode, --OIB added Hauler 01/30/12
	TripHaulerName,
	TripHaulerAddress1,
	TripHaulerCity,
	TripHaulerState,
	TripHaulerZip,
	TripHaulerCountry,
	TripResolvedFlag,
	TripSendScaleDateTime,
	TripResendScaleNotificationFlag,
	TripResendScaleDateTime,
	TripImageFlag,
	TripScaleTerminalId,
	TripScaleTerminalName,
 	TripSendScaleTerminalId,
	TripSendScaleTerminalName,
 	TripResendScaleTerminalId,
	TripResendScaleTerminalName

 )

SELECT  @pHistSeqNo,
	@pHistAction,
	TripNumber,
	TripStatus,
	TripStatusDesc,
	TripAssignStatus,
	TripAssignStatusDesc,
	TripType,
	TripTypeDesc,
	TripSequenceNumber,
	TripSendFlag,
	TripDriverId,
	TripDriverName,
	TripCustHostCode,
	TripCustCode4_4,
	TripCustName,
	TripCustAddress1,
	TripCustAddress2,
	TripCustCity,
	TripCustState,
	TripCustZip,
	TripCustCountry,
	TripCustPhone1,
	TripTerminalId,
	TripTerminalName,
	TripRegionId,
	TripRegionName,
	TripContactName,
	TripSalesman,
	TripCustOpenTime,
	TripCustCloseTime,
	TripReadyDateTime,
	TripEnteredUserId,
	TripEnteredUserName,
	TripEnteredDateTime,
	TripStandardDriveMinutes,
	TripStandardStopMinutes,
	TripActualDriveMinutes,
	TripActualStopMinutes,
	TripContractNumber,
	TripShipmentNumber,
	TripCommodityPurchase,
	TripCommoditySale, 
	TripSpecInstructions,
	TripExpediteFlag,
	TripPrimaryContainerNumber,
	TripPrimaryContainerType,
	TripPrimaryContainerSize,
	TripPrimaryCommodityCode,
	TripPrimaryCommodityDesc,
	TripPrimaryContainerLocation,
	TripPowerId,
	TripCommodityScaleMsg,
	TripDriverIdPrev,
	TripCompletedDateTime,
	TripSendScaleNotificationFlag,
	TripSendReceiptFlag,
	TripExtendedFlag,
	TripExtendedReason,
	TripInProgressFlag,
	TripNightRunFlag,
	TripDoneMethod,
	TripCompletedUserId,
	TripCompletedUserName,
	TripReferenceNumber,
	TripChangedUserId,
	TripChangedUserName,
	TripChangedDateTime,
	TripDirectSegNumber,
	TripDirectDriveMinutes,
	TripDirectTotalMinutes,
	TripStartedDateTime,
	TripActualTotalMinutes,
	TripErrorDesc,
	TripDriverInstructions,
    TripDispatcherInstructions,
    TripScaleReferenceNumber,
    TripMultContainerFlag,
	TripSendReseqFlag,
	TripPowerAssetNumber,
	TripHaulerHostCode, --OIB added Hauler 01/30/12
	TripHaulerName,
	TripHaulerAddress1,
	TripHaulerCity,
	TripHaulerState,
	TripHaulerZip,
	TripHaulerCountry,
	TripResolvedFlag,
	TripSendScaleDateTime,
	TripResendScaleNotificationFlag,
	TripResendScaleDateTime,
	TripImageFlag,
	TripScaleTerminalId,
	TripScaleTerminalName,
 	TripSendScaleTerminalId,
	TripSendScaleTerminalName,
 	TripResendScaleTerminalId,
	TripResendScaleTerminalName

 FROM Trip WHERE Trip.TripNumber = @pTripNumber 

IF (@@ERROR <> 0)
    SET @pErrorSave = @@ERROR


GO





/*************************************************************
11. pc_HistoryTripSegmentTable
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_HistoryTripSegmentTable')
            and   type = 'P')
   drop procedure pc_HistoryTripSegmentTable
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_HistoryTripSegmentTable

@pTripNumber varchar(10)= NULL,
@pHistSeqNo      smallint,
@pErrorSave  smallint  OUTPUT

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

DECLARE @TripSegCursor CURSOR
DECLARE @TripSegNumber varchar(2);

SET @pErrorSave = 0

SET @TripSegCursor = CURSOR FORWARD_ONLY STATIC FOR 
          SELECT TripSegment.TripNumber,TripSegment.TripSegNumber FROM TripSegment 
                 WHERE TripSegment.TripNumber =@pTripNumber
OPEN @TripSegCursor
FETCH NEXT FROM @TripSegCursor INTO @pTripNumber,@TripSegNumber

WHILE(@@fetch_status = 0)
      BEGIN
          INSERT INTO HistTripSegment
		(
                HistSeqNo,
		TripNumber,
		TripSegNumber,
		TripSegStatus,
		TripSegStatusDesc,
		TripSegType,
		TripSegTypeDesc,
		TripSegPowerId,
		TripSegDriverId,
		TripSegDriverName,
		TripSegStartDateTime,
		TripSegEndDateTime,
		TripSegStandardDriveMinutes,
		TripSegStandardStopMinutes,
		TripSegActualDriveMinutes,
		TripSegActualStopMinutes,
		TripSegOdometerStart,
		TripSegOdometerEnd,
		TripSegComments,
		TripSegOrigCustType,
		TripSegOrigCustTypeDesc,
		TripSegOrigCustHostCode,
		TripSegOrigCustCode4_4 ,
		TripSegOrigCustName,
		TripSegOrigCustAddress1,
		TripSegOrigCustAddress2,
		TripSegOrigCustCity,
		TripSegOrigCustState,
		TripSegOrigCustZip,
		TripSegOrigCustCountry,
		TripSegOrigCustPhone1,
		TripSegOrigCustTimeFactor,
		TripSegDestCustType,
		TripSegDestCustTypeDesc,
		TripSegDestCustHostCode,
		TripSegDestCustCode4_4,
		TripSegDestCustName,
		TripSegDestCustAddress1,
		TripSegDestCustAddress2,
		TripSegDestCustCity,
		TripSegDestCustState,
		TripSegDestCustZip,
		TripSegDestCustCountry,
		TripSegDestCustPhone1,
		TripSegDestCustTimeFactor,
		TripSegPrimaryContainerNumber,
		TripSegPrimaryContainerType,
		TripSegPrimaryContainerSize,
		TripSegPrimaryContainerCommodityCode,
		TripSegPrimaryContainerCommodityDesc,
		TripSegPrimaryContainerLocation,
		TripSegActualDriveStartDateTime,
		TripSegActualDriveEndDateTime,
		TripSegActualStopStartDateTime,
		TripSegActualStopEndDateTime,
		TripSegStartLatitude,
		TripSegStartLongitude,
		TripSegEndLatitude,
		TripSegEndLongitude,
		TripSegStandardMiles,
        TripSegErrorDesc,
        TripSegContainerQty,
        TripSegDriverGenerated,
        TripSegDriverModified,
        TripSegPowerAssetNumber,
        TripSegExtendedFlag,
        TripSegSendReceiptFlag 
		)

               SELECT @pHistSeqNo,
		TripNumber,
		TripSegNumber,
		TripSegStatus,
		TripSegStatusDesc,
		TripSegType,
		TripSegTypeDesc,
		TripSegPowerId,
		TripSegDriverId,
		TripSegDriverName,
		TripSegStartDateTime,
		TripSegEndDateTime,
		TripSegStandardDriveMinutes,
		TripSegStandardStopMinutes,
		TripSegActualDriveMinutes,
		TripSegActualStopMinutes,
		TripSegOdometerStart,
		TripSegOdometerEnd,
		TripSegComments,
		TripSegOrigCustType,
		TripSegOrigCustTypeDesc,
		TripSegOrigCustHostCode,
		TripSegOrigCustCode4_4 ,
		TripSegOrigCustName,
		TripSegOrigCustAddress1,
		TripSegOrigCustAddress2,
		TripSegOrigCustCity,
		TripSegOrigCustState,
		TripSegOrigCustZip,
		TripSegOrigCustCountry,
		TripSegOrigCustPhone1,
		TripSegOrigCustTimeFactor,
		TripSegDestCustType,
		TripSegDestCustTypeDesc,
		TripSegDestCustHostCode,
		TripSegDestCustCode4_4,
		TripSegDestCustName,
		TripSegDestCustAddress1,
		TripSegDestCustAddress2,
		TripSegDestCustCity,
		TripSegDestCustState,
		TripSegDestCustZip,
		TripSegDestCustCountry,
		TripSegDestCustPhone1,
		TripSegDestCustTimeFactor,
		TripSegPrimaryContainerNumber,
		TripSegPrimaryContainerType,
		TripSegPrimaryContainerSize,
		TripSegPrimaryContainerCommodityCode,
		TripSegPrimaryContainerCommodityDesc,
		TripSegPrimaryContainerLocation,
		TripSegActualDriveStartDateTime,
		TripSegActualDriveEndDateTime,
		TripSegActualStopStartDateTime,
		TripSegActualStopEndDateTime,
		TripSegStartLatitude,
		TripSegStartLongitude,
		TripSegEndLatitude,
		TripSegEndLongitude,
		TripSegStandardMiles,
        TripSegErrorDesc,
        TripSegContainerQty,
        TripSegDriverGenerated,
        TripSegDriverModified,
        TripSegPowerAssetNumber,
        TripSegExtendedFlag,
        TripSegSendReceiptFlag 
	 FROM TripSegment 
         WHERE TripSegment.TripNumber = @pTripNumber
                                      AND TripSegment.TripSegNumber = @TripSegNumber 
          IF (@@ERROR <> 0)
              SET @pErrorSave = @@ERROR
 
          FETCH NEXT FROM @TripSegCursor INTO @pTripNumber,@TripSegNumber

      END
CLOSE @TripSegCursor

DEALLOCATE @TripSegCursor
GO

/*************************************************************
12. pc_HistoryTripSegmentContainerTable
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_HistoryTripSegmentContainerTable')
            and   type = 'P')
   drop procedure pc_HistoryTripSegmentContainerTable
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_HistoryTripSegmentContainerTable

@pTripNumber varchar(10)= NULL,
@pHistSeqNo      smallint,
@pErrorSave  smallint  OUTPUT

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

DECLARE @TripSegContainerCursor CURSOR
DECLARE @TripSegNumber varchar(2);
DECLARE @TripSegContainerSeqNumber smallint;

SET @pErrorSave = 0

SET @TripSegContainerCursor = CURSOR FORWARD_ONLY STATIC FOR 
          SELECT TripSegmentContainer.TripNumber,
                 TripSegmentContainer.TripSegNumber,
                 TripSegmentContainer.TripSegContainerSeqNumber 
                 FROM TripSegmentContainer 
                 WHERE TripSegmentContainer.TripNumber =@pTripNumber
OPEN @TripSegContainerCursor
FETCH NEXT FROM @TripSegContainerCursor INTO @pTripNumber,@TripSegNumber,@TripSegContainerSeqNumber

WHILE(@@fetch_status = 0)
      BEGIN
          INSERT INTO HistTripSegmentContainer
           (
		HistSeqNo,
		TripNumber,
		TripSegNumber,
		TripSegContainerSeqNumber,
		TripSegContainerNumber,
		TripSegContainerType,
		TripSegContainerSize,
		TripSegContainerCommodityCode,
		TripSegContainerCommodityDesc,
		TripSegContainerLocation,
		TripSegContainerShortTerm,
		TripSegContainerWeightGross,
		TripSegContainerWeightTare,
		TripSegContainerReviewFlag,
		TripSegContainerReviewReason,
		TripSegContainerActionDateTime,
		TripSegContainerEntryMethod,
		TripSegContainerWeightGross2nd,
		Commod1Code,
		Commod1Desc,
		Commod1Pct,
		Commod2Code,
		Commod2Desc,
		Commod2Pct,
		Commod3Code,
		Commod3Desc,
		Commod3Pct,
		Commod4Code,
		Commod4Desc,
		Commod4Pct,
		ContamCode,
		ContamWeight,
        ContamDesc,
        WeightGrossDateTime,
        WeightGross2ndDateTime,
        WeightTareDateTime,
		TripSegContainerLevel,
        TripSegContainerLatitude,
        TripSegContainerLongitude,
        TripSegContainerLoaded,
        TripSegContainerOnTruck,
        TripScaleReferenceNumber,
        TripSegContainerSubReason,
        TripSegContainerComment,
        TripSegContainerComplete,
		TripSegContainerDriverNotes
        )
        SELECT 
         @pHistSeqNo,
		TripNumber,
		TripSegNumber,
		TripSegContainerSeqNumber,
		TripSegContainerNumber,
		TripSegContainerType,
		TripSegContainerSize,
		TripSegContainerCommodityCode,
		TripSegContainerCommodityDesc,
		TripSegContainerLocation,
		TripSegContainerShortTerm,
		TripSegContainerWeightGross,
		TripSegContainerWeightTare,
		TripSegContainerReviewFlag,
		TripSegContainerReviewReason,
		TripSegContainerActionDateTime,
		TripSegContainerEntryMethod,
		TripSegContainerWeightGross2nd,
		Commod1Code,
		Commod1Desc,
		Commod1Pct,
		Commod2Code,
		Commod2Desc,
		Commod2Pct,
		Commod3Code,
		Commod3Desc,
		Commod3Pct,
		Commod4Code,
		Commod4Desc,
		Commod4Pct,
		ContamCode,
		ContamWeight,
        ContamDesc,
        WeightGrossDateTime,
        WeightGross2ndDateTime,
        WeightTareDateTime,
		TripSegContainerLevel,
        TripSegContainerLatitude,
        TripSegContainerLongitude,
        TripSegContainerLoaded,
        TripSegContainerOnTruck,
        TripScaleReferenceNumber,
        TripSegContainerSubReason,
        TripSegContainerComment,
        TripSegContainerComplete,
		TripSegContainerDriverNotes
        FROM TripSegmentContainer WHERE TripSegmentContainer.TripNumber = @pTripNumber
                                      AND TripSegmentContainer.TripSegNumber = @TripSegNumber 
                                      AND TripSegmentContainer.TripSegContainerSeqNumber = @TripSegContainerSeqNumber
          IF (@@ERROR <> 0)
              SET @pErrorSave = @@ERROR

          FETCH NEXT FROM @TripSegContainerCursor INTO @pTripNumber,@TripSegNumber,@TripSegContainerSeqNumber

      END
CLOSE @TripSegContainerCursor

DEALLOCATE @TripSegContainerCursor
GO

/*************************************************************
13. pc_HistoryTripSegmentMileageTable
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_HistoryTripSegmentMileageTable')
            and   type = 'P')
   drop procedure pc_HistoryTripSegmentMileageTable
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_HistoryTripSegmentMileageTable

@pTripNumber varchar(10)= NULL,
@pHistSeqNo      smallint,
@pErrorSave  smallint  OUTPUT

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

DECLARE @TripSegMileageCursor CURSOR
DECLARE @TripSegNumber varchar(2);
DECLARE @TripSegMileageSeqNumber smallint;

SET @pErrorSave = 0

SET @TripSegMileageCursor = CURSOR FORWARD_ONLY STATIC FOR 
          SELECT TripSegmentMileage.TripNumber,
                 TripSegmentMileage.TripSegNumber,
                 TripSegmentMileage.TripSegMileageSeqNumber 
                 FROM TripSegmentMileage 
                 WHERE TripSegmentMileage.TripNumber =@pTripNumber
OPEN @TripSegMileageCursor
FETCH NEXT FROM @TripSegMileageCursor INTO @pTripNumber,@TripSegNumber,@TripSegMileageSeqNumber

WHILE(@@fetch_status = 0)
      BEGIN
          INSERT INTO HistTripSegmentMileage
		(
                HistSeqNo,
		TripNumber,
		TripSegNumber,
		TripSegMileageSeqNumber,
		TripSegMileageState,
		TripSegMileageCountry,
		TripSegMileageOdometerStart,
		TripSegMileageOdometerEnd,
		TripSegLoadedFlag,
        TripSegMileagePowerId,
        TripSegMileageDriverId,
        TripSegMileageDriverName
		)
               SELECT 
                @pHistSeqNo,
		TripNumber,
		TripSegNumber,
		TripSegMileageSeqNumber,
		TripSegMileageState,
		TripSegMileageCountry,
		TripSegMileageOdometerStart,
		TripSegMileageOdometerEnd,
		TripSegLoadedFlag,
        TripSegMileagePowerId,
        TripSegMileageDriverId,
        TripSegMileageDriverName

	       FROM TripSegmentMileage WHERE TripSegmentMileage.TripNumber = @pTripNumber
                                      AND TripSegmentMileage.TripSegNumber = @TripSegNumber 
                                      AND TripSegmentMileage.TripSegMileageSeqNumber = @TripSegMileageSeqNumber
          IF (@@ERROR <> 0)
              SET @pErrorSave = @@ERROR

          FETCH NEXT FROM @TripSegMileageCursor INTO @pTripNumber,@TripSegNumber,@TripSegMileageSeqNumber

      END
CLOSE @TripSegMileageCursor

DEALLOCATE @TripSegMileageCursor
GO

/*************************************************************
14. pc_HistoryTripReferenceNumbersTable
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_HistoryTripReferenceNumbersTable')
            and   type = 'P')
   drop procedure pc_HistoryTripReferenceNumbersTable
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_HistoryTripReferenceNumbersTable

@pTripNumber varchar(10)= NULL,
@pHistSeqNo      smallint,
@pErrorSave  smallint  OUTPUT

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

DECLARE @TripReferenceNumbersCursor CURSOR
DECLARE @TripSeqNumber smallint;

SET @pErrorSave = 0

SET @TripReferenceNumbersCursor = CURSOR FORWARD_ONLY STATIC FOR 
          SELECT TripReferenceNumbers.TripNumber,
                 TripReferenceNumbers.TripSeqNumber
                 FROM TripReferenceNumbers 
                 WHERE TripReferenceNumbers.TripNumber =@pTripNumber

OPEN @TripReferenceNumbersCursor
FETCH NEXT FROM @TripReferenceNumbersCursor INTO @pTripNumber,@TripSeqNumber

WHILE(@@fetch_status = 0)
      BEGIN
          INSERT INTO HistTripReferenceNumbers
		(
                HistSeqNo,
		TripNumber,
		TripSeqNumber,
		TripRefNumberDesc,
		TripRefNumber
		)
               SELECT 
                @pHistSeqNo,
		TripNumber,
		TripSeqNumber,
		TripRefNumberDesc,
		TripRefNumber
	       FROM TripReferenceNumbers WHERE TripReferenceNumbers.TripNumber = @pTripNumber
                                      AND TripReferenceNumbers.TripSeqNumber = @TripSeqNumber

          IF (@@ERROR <> 0)
              SET @pErrorSave = @@ERROR

          FETCH NEXT FROM @TripReferenceNumbersCursor INTO @pTripNumber,@TripSeqNumber

      END
CLOSE @TripReferenceNumbersCursor

DEALLOCATE @TripReferenceNumbersCursor
GO

/*************************************************************
 10b. pc_HistoryTripSegmentContainerTimeTable
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_HistoryTripSegmentContainerTimeTable')
            and   type = 'P')
   drop procedure pc_HistoryTripSegmentContainerTimeTable
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_HistoryTripSegmentContainerTimeTable

@pTripNumber varchar(10)= NULL,
@pHistSeqNo      smallint,
@pErrorSave  smallint  OUTPUT

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

DECLARE @TripSegmentContainerTimeCursor CURSOR
DECLARE @TripSegNumber varchar(2);
DECLARE @TripSegContainerSeqNumber smallint;
DECLARE @SeqNumber smallint;

SET @pErrorSave = 0

SET @TripSegmentContainerTimeCursor = CURSOR FORWARD_ONLY STATIC FOR 
          SELECT TripSegmentContainerTime.TripNumber,
                 TripSegmentContainerTime.TripSegNumber,
                 TripSegmentContainerTime.TripSegContainerSeqNumber,
                 TripSegmentContainerTime.SeqNumber
                 FROM TripSegmentContainerTime 
                 WHERE TripSegmentContainerTime.TripNumber =@pTripNumber

OPEN @TripSegmentContainerTimeCursor
FETCH NEXT FROM @TripSegmentContainerTimeCursor INTO @pTripNumber,@TripSegNumber,@TripSegContainerSeqNumber,@SeqNumber

WHILE(@@fetch_status = 0)
      BEGIN
          INSERT INTO HistTripSegmentContainerTime
		  (
            HistSeqNo,
		    TripNumber,
		    TripSegNumber,
		    TripSegContainerSeqNumber,
		    SeqNumber,
		    TimeType,
		    TimeDesc,
		    ContainerTime
		)
         SELECT 
            @pHistSeqNo,
		    TripNumber,
		    TripSegNumber,
		    TripSegContainerSeqNumber,
		    SeqNumber,
		    TimeType,
		    TimeDesc,
		    ContainerTime
	        FROM TripSegmentContainerTime WHERE TripSegmentContainerTime.TripNumber = @pTripNumber
                                           AND TripSegmentContainerTime.TripSegNumber = @TripSegNumber
                                           AND TripSegmentContainerTime.TripSegContainerSeqNumber = @TripSegContainerSeqNumber
                                           AND TripSegmentContainerTime.SeqNumber = @SeqNumber

          IF (@@ERROR <> 0)
              SET @pErrorSave = @@ERROR

          FETCH NEXT FROM @TripSegmentContainerTimeCursor INTO @pTripNumber,@TripSegNumber,@TripSegContainerSeqNumber,@SeqNumber

      END
CLOSE @TripSegmentContainerTimeCursor

DEALLOCATE @TripSegmentContainerTimeCursor
GO
/*************************************************************
 10c. pc_HistoryTripSegmentTimeTable
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_HistoryTripSegmentTimeTable')
            and   type = 'P')
   drop procedure pc_HistoryTripSegmentTimeTable
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_HistoryTripSegmentTimeTable

@pTripNumber varchar(10)= NULL,
@pHistSeqNo      smallint,
@pErrorSave  smallint  OUTPUT

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

DECLARE @TripSegmentTimeCursor CURSOR
DECLARE @TripSegNumber varchar(2);
DECLARE @SeqNumber smallint;

SET @pErrorSave = 0

SET @TripSegmentTimeCursor = CURSOR FORWARD_ONLY STATIC FOR 
          SELECT TripSegmentTime.TripNumber,
                 TripSegmentTime.TripSegNumber,
                 TripSegmentTime.SeqNumber
                 FROM TripSegmentTime 
                 WHERE TripSegmentTime.TripNumber =@pTripNumber

OPEN @TripSegmentTimeCursor
FETCH NEXT FROM @TripSegmentTimeCursor INTO @pTripNumber,@TripSegNumber,@SeqNumber

WHILE(@@fetch_status = 0)
      BEGIN
          INSERT INTO HistTripSegmentTime
		  (
            HistSeqNo,
		    TripNumber,
		    TripSegNumber,
		    SeqNumber,
		    TimeType,
		    TimeDesc,
		    SegmentTime
		)
         SELECT 
            @pHistSeqNo,
		    TripNumber,
		    TripSegNumber,
		    SeqNumber,
		    TimeType,
		    TimeDesc,
		    SegmentTime
	        FROM TripSegmentTime WHERE TripSegmentTime.TripNumber = @pTripNumber
                                           AND TripSegmentTime.TripSegNumber = @TripSegNumber
                                           AND TripSegmentTime.SeqNumber = @SeqNumber

          IF (@@ERROR <> 0)
              SET @pErrorSave = @@ERROR

          FETCH NEXT FROM @TripSegmentTimeCursor INTO @pTripNumber,@TripSegNumber,@SeqNumber

      END
CLOSE @TripSegmentTimeCursor

DEALLOCATE @TripSegmentTimeCursor
GO

/*************************************************************
 10. pc_HistorySaveTrip
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_HistorySaveTrip')
            and   type = 'P')
   drop procedure pc_HistorySaveTrip
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_HistorySaveTrip

@pTripNumber varchar(10)= NULL,
@pHistSeqNo smallint,
@pHistAction varchar(25)

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

DECLARE @iSuccessful smallint
DECLARE @ErrorSave smallint;

SET @ErrorSave = 0


--Save Trip to History Table
  BEGIN
        if ( @ErrorSave = 0)
	  exec pc_HistoryTripSegmentTimeTable @pTripNumber,@pHistSeqNo,@ErrorSave
	else
           PRINT 'ERROR pc_HistoryTripSegmentTimeTable ' + CONVERT(varchar(5),@ErrorSave)
        if ( @ErrorSave = 0)
	  exec pc_HistoryTripSegmentContainerTimeTable @pTripNumber,@pHistSeqNo,@ErrorSave
	else
           PRINT 'ERROR pc_HistoryTripSegmentContainerTimeTable ' + CONVERT(varchar(5),@ErrorSave)

        if ( @ErrorSave = 0)
	  exec pc_HistoryTripReferenceNumbersTable @pTripNumber,@pHistSeqNo,@ErrorSave
	else
           PRINT 'ERROR pc_HistoryTripReferenceNumbersTable ' + CONVERT(varchar(5),@ErrorSave)

        if ( @ErrorSave = 0)
	  exec pc_HistoryTripSegmentMileageTable @pTripNumber,@pHistSeqNo,@ErrorSave
	else
           PRINT 'ERROR pc_HistoryTripSegmentMileageTable ' + CONVERT(varchar(5),@ErrorSave)

        if ( @ErrorSave = 0)
	   exec pc_HistoryTripSegmentContainerTable @pTripNumber,@pHistSeqNo,@ErrorSave 
        else
           PRINT 'ERROR pc_HistoryTripSegmentContainerTable ' + CONVERT(varchar(5),@ErrorSave)

        if ( @ErrorSave = 0)
	   exec pc_HistoryTripSegmentTable @pTripNumber,@pHistSeqNo,@ErrorSave 
        else
           PRINT 'ERROR pc_HistoryTripSegmentTable ' + CONVERT(varchar(5),@ErrorSave)

        if ( @ErrorSave = 0)
 	   exec pc_HistoryTripTable @pTripNumber,@pHistSeqNo,@pHistAction,@ErrorSave
	else
           PRINT 'ERROR pc_HistoryTripTable ' + CONVERT(varchar(5),@ErrorSave)

END --of BEGIN


GO
