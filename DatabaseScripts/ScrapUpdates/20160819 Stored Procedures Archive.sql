--dbo.pc_ArchiveTripTable
--dbo.pc_ArchiveTripSegmentTable
--dbo.pc_ArchiveTripSegmentContainerTable
--dbo.pc_ArchiveTripSegmentMileageTable
--dbo.pc_ArchiveTripReferenceNumbersTable
--dbo.pc_ArchiveTripSegmentContainerTimeTable
--dbo.pc_ArchiveTripSegmentTimeTable
--dbo.pc_DeleteTripInfo
--dbo.pc_ArchiveTripsDone
--dbo.pc_ArchiveTripsCancelled
--dbo.pc_ArchiveDriverDelayTable
--dbo.pc_DeleteDriverDelayInfo
--dbo.pc_ArchiveDriverDelays
--dbo.pc_ArchiveContainerHistoryTable
--dbo.pc_DeleteContainerHistoryInfo
--dbo.pc_ArchiveContainerHistory
--dbo.pc_ArchiveDriverHistoryTable
--dbo.pc_DeleteDriverHistoryInfo
--dbo.pc_ArchiveDriverHistory
--dbo.pc_DeleteDriverHistoryInfo
--dbo.pc_ArchiveDriverHistory
--dbo.pc_ArchivePowerHistoryTable
--dbo.pc_DeletePowerHistoryInfo
--dbo.pc_ArchivePowerHistory
--dbo.pc_ArchivePowerFuelTable
--dbo.pc_DeletePowerFuelInfo
--dbo.pc_ArchivePowerFuel

/*************************************************************
 10. pc_ArchiveTripTable
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_ArchiveTripTable')
            and   type = 'P')
   drop procedure pc_ArchiveTripTable
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_ArchiveTripTable

@pTripNumber varchar(10)= NULL,
@pErrorSave  smallint  OUTPUT

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

SET @pErrorSave = 0

INSERT INTO ArcTrip
(
	ArchiveDateTime,
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

SELECT  GetDate(),
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
11. pc_ArchiveTripSegmentTable
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_ArchiveTripSegmentTable')
            and   type = 'P')
   drop procedure pc_ArchiveTripSegmentTable
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_ArchiveTripSegmentTable

@pTripNumber varchar(10)= NULL,
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
          INSERT INTO ArcTripSegment
		(
                ArchiveDateTime,
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

               SELECT GetDate(),
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
12. pc_ArchiveTripSegmentContainerTable
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_ArchiveTripSegmentContainerTable')
            and   type = 'P')
   drop procedure pc_ArchiveTripSegmentContainerTable
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_ArchiveTripSegmentContainerTable

@pTripNumber varchar(10)= NULL,
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
          INSERT INTO ArcTripSegmentContainer
           (
		ArchiveDateTime,
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
        GetDate(),
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
13. pc_ArchiveTripSegmentMileageTable
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_ArchiveTripSegmentMileageTable')
            and   type = 'P')
   drop procedure pc_ArchiveTripSegmentMileageTable
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_ArchiveTripSegmentMileageTable

@pTripNumber varchar(10)= NULL,
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
          INSERT INTO ArcTripSegmentMileage
		(
                ArchiveDateTime,
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
                GetDate(),
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
13b. pc_ArchiveTripSegmentContainerTimeTable
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_ArchiveTripSegmentContainerTimeTable')
            and   type = 'P')
   drop procedure pc_ArchiveTripSegmentContainerTimeTable
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_ArchiveTripSegmentContainerTimeTable

@pTripNumber varchar(10)= NULL,
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
          INSERT INTO ArcTripSegmentContainerTime
		  (
            ArchiveDateTime,
		    TripNumber,
		    TripSegNumber,
		    TripSegContainerSeqNumber,
		    SeqNumber,
		    TimeType,
		    TimeDesc,
		    ContainerTime
		  )
          SELECT 
            GetDate(),
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
13c. pc_ArchiveTripSegmentTimeTable
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_ArchiveTripSegmentTimeTable')
            and   type = 'P')
   drop procedure pc_ArchiveTripSegmentTimeTable
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_ArchiveTripSegmentTimeTable

@pTripNumber varchar(10)= NULL,
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
          INSERT INTO ArcTripSegmentTime
		  (
            ArchiveDateTime,
		    TripNumber,
		    TripSegNumber,
		    SeqNumber,
		    TimeType,
		    TimeDesc,
			SegmentTime
		  )
          SELECT 
            GetDate(),
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
14. pc_ArchiveTripReferenceNumbersTable
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_ArchiveTripReferenceNumbersTable')
            and   type = 'P')
   drop procedure pc_ArchiveTripReferenceNumbersTable
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_ArchiveTripReferenceNumbersTable

@pTripNumber varchar(10)= NULL,
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
          INSERT INTO ArcTripReferenceNumbers
		(
                ArchiveDateTime,
		TripNumber,
		TripSeqNumber,
		TripRefNumberDesc,
		TripRefNumber
		)
               SELECT 
                GetDate(),
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
 18. pc_DeleteTripInfo
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_DeleteTripInfo')
            and   type = 'P')
   drop procedure pc_DeleteTripInfo
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_DeleteTripInfo

@pTripNumber varchar(10)= NULL,
@pErrorSave  smallint  OUTPUT

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

DELETE FROM TripReferenceNumbers 
       WHERE TripReferenceNumbers.TripNumber = @pTripNumber

DELETE FROM TripSegmentMileage 
       WHERE TripSegmentMileage.TripNumber = @pTripNumber

DELETE FROM TripSegmentContainer 
       WHERE TripSegmentContainer.TripNumber  = @pTripNumber

DELETE FROM TripSegment 
       WHERE TripSegment.TripNumber  = @pTripNumber

DELETE FROM Trip 
       WHERE Trip.TripNumber = @pTripNumber
GO

/*************************************************************
19. pc_ArchiveTripsDone
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_ArchiveTripsDone')
            and   type = 'P')
   drop procedure pc_ArchiveTripsDone
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_ArchiveTripsDone

@pTerminal varchar(10)= NULL,
@pTypeDesc varchar(50) = NULL,
@pType     int = -1,
@pStatus   char(1)     = NULL,
@pParam1    varchar(20) = NULL,
@Style     int = 101

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

DECLARE @lStartDateTime DATETIME
DECLARE @lEndDateTime DATETIME
DECLARE @lNumberProcessed int
DECLARE @iSuccessful smallint

DECLARE @TripCursor CURSOR

DECLARE @TripNumber varchar(10);
DECLARE @TripSegNumber varchar(2);
DECLARE @ErrorSave smallint;

SET @lStartDateTime = NULL
SET @lEndDateTime = NULL
SET @lNumberProcessed = 0
SET @ErrorSave = 0


--Archive Done trips over x days
BEGIN
   SET @lStartDateTime = GETDATE()

   IF (@pType = 0) -- DAYS
   BEGIN
     SELECT @lNumberProcessed = COUNT(Trip.TripNumber) FROM Trip 
                    WHERE Trip.TripTerminalId = @pTerminal AND
                          Trip.TripStatus = @pStatus AND 
                          (Trip.TripSendFlag = 8 OR Trip.TripSendFlag = 10) AND 
                          DATEDIFF(DAY,  Trip.TripReadyDateTime, GETDATE()) > CONVERT(INT,@pParam1) AND
                          DATEDIFF(DAY,  Trip.TripCompletedDateTime, GETDATE()) > CONVERT(INT,@pParam1) 


     SET @TripCursor = CURSOR FORWARD_ONLY STATIC FOR 
       SELECT Trip.TripNumber FROM Trip 
                     WHERE Trip.TripTerminalId = @pTerminal AND
                           Trip.TripStatus = @pStatus AND 
                          (Trip.TripSendFlag = 8 OR Trip.TripSendFlag = 10) AND 
                           DATEDIFF(DAY,  Trip.TripReadyDateTime, GETDATE()) > CONVERT(INT,@pParam1) AND
                           DATEDIFF(DAY, Trip.TripCompletedDateTime, GETDATE()) > CONVERT(INT,@pParam1)
   END
   ELSE -- DATE
   BEGIN
     SELECT @lNumberProcessed = COUNT(Trip.TripNumber) FROM Trip 
                    WHERE Trip.TripTerminalId = @pTerminal AND
                          Trip.TripStatus = @pStatus AND 
                          (Trip.TripSendFlag = 8 OR Trip.TripSendFlag = 10) AND 
                          Trip.TripReadyDateTime < CONVERT(datetime,@pParam1, @Style) AND
                          Trip.TripCompletedDateTime < CONVERT(datetime,@pParam1, @Style)


     SET @TripCursor = CURSOR FORWARD_ONLY STATIC FOR 
       SELECT Trip.TripNumber FROM Trip 
                     WHERE Trip.TripTerminalId = @pTerminal AND
                           Trip.TripStatus = @pStatus AND 
                          (Trip.TripSendFlag = 8 OR Trip.TripSendFlag = 10) AND 
                           Trip.TripReadyDateTime < CONVERT(datetime,@pParam1, @Style) AND
                           Trip.TripCompletedDateTime < CONVERT(datetime,@pParam1, @Style)
 END

   OPEN @TripCursor
   FETCH NEXT FROM @TripCursor INTO @TripNumber

   WHILE(@@fetch_status = 0)
      BEGIN
        if ( @ErrorSave = 0)
	  exec pc_ArchiveTripSegmentTimeTable @TripNumber,@ErrorSave
	else
           PRINT 'ERROR pc_ArchiveTripSegmentTimeTable ' + CONVERT(varchar(5),@ErrorSave)

        if ( @ErrorSave = 0)
	  exec pc_ArchiveTripSegmentContainerTimeTable @TripNumber,@ErrorSave
	else
           PRINT 'ERROR pc_ArchiveTripSegmentContainerTimeTable ' + CONVERT(varchar(5),@ErrorSave)

        if ( @ErrorSave = 0)
	  exec pc_ArchiveTripReferenceNumbersTable @TripNumber,@ErrorSave
	else
           PRINT 'ERROR pc_ArchiveTripReferenceNumbersTable ' + CONVERT(varchar(5),@ErrorSave)

        if ( @ErrorSave = 0)
	  exec pc_ArchiveTripSegmentMileageTable @TripNumber,@ErrorSave
	else
           PRINT 'ERROR pc_ArchiveTripSegmentMileageTable ' + CONVERT(varchar(5),@ErrorSave)

        if ( @ErrorSave = 0)
	   exec pc_ArchiveTripSegmentContainerTable @TripNumber,@ErrorSave 
        else
           PRINT 'ERROR pc_ArchiveTripSegmentContainerTable ' + CONVERT(varchar(5),@ErrorSave)

        if ( @ErrorSave = 0)
	   exec pc_ArchiveTripSegmentTable @TripNumber,@ErrorSave 
        else
           PRINT 'ERROR pc_ArchiveTripSegmentTable ' + CONVERT(varchar(5),@ErrorSave)

        if ( @ErrorSave = 0)
 	   exec pc_ArchiveTripTable @TripNumber,@ErrorSave
	else
           PRINT 'ERROR pc_ArchiveTripSegmentTable ' + CONVERT(varchar(5),@ErrorSave)

        if ( @ErrorSave = 0)
 	    exec pc_DeleteTripInfo @TripNumber,@ErrorSave
	else
            PRINT 'ERROR pc_ArchiveTripTable ' + CONVERT(varchar(5),@ErrorSave)

        FETCH NEXT FROM @TripCursor INTO @TripNumber
      END

   CLOSE @TripCursor
END --of BEGIN
DEALLOCATE @TripCursor
SET @lEndDateTime = GETDATE()

IF @lNumberProcessed > 0
  BEGIN
      IF EXISTS( SELECT ProcessType FROM ProcessLog 
           WHERE TerminalId = @pTerminal AND 
                 ProcessType = @pTypeDesc)

           UPDATE ProcessLog 
                  SET TerminalId = @pTerminal,
                      ProcessType = @pTypeDesc,
                      ProcessParam1 = @pParam1,
                      LastActionStartDateTime = @lStartDateTime,
                      LastActionEndDateTime = @lEndDateTime,
                      NumberRecordsProcessed = @lNumberProcessed
                  WHERE TerminalId = @pTerminal AND 
                        ProcessType = @pTypeDesc
      ELSE
          INSERT INTO ProcessLog
           (TerminalId,ProcessType,ProcessParam1,
            LastActionStartDateTime,LastActionEndDateTime,
            NumberRecordsProcessed)
                 VALUES(@pTerminal,@pTypeDesc,@pParam1,
                        @lStartDateTime,@lEndDateTime,
                        @lNumberProcessed)
   END

GO
/*************************************************************
19b. pc_ArchiveTripsCancelled
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('dbo.pc_ArchiveTripsCancelled')
            and   type = 'P')
   drop procedure dbo.pc_ArchiveTripsCancelled
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_ArchiveTripsCancelled

@pTerminal varchar(10)= NULL,
@pTypeDesc varchar(50) = NULL,
@pType     int = -1,
@pStatus   char(1)     = NULL,
@pParam1    varchar(20) = NULL,
@Style     int = 101

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

DECLARE @lStartDateTime DATETIME
DECLARE @lEndDateTime DATETIME
DECLARE @lNumberProcessed int
DECLARE @iSuccessful smallint

DECLARE @TripCursor CURSOR

DECLARE @TripNumber varchar(10);
DECLARE @TripSegNumber varchar(2);
DECLARE @ErrorSave smallint;

SET @lStartDateTime = NULL
SET @lEndDateTime = NULL
SET @lNumberProcessed = 0
SET @ErrorSave = 0


--Archive cancelled trips over x days
BEGIN
   SET @lStartDateTime = GETDATE()

   IF (@pType = 0) -- DAYS
   BEGIN
     SELECT @lNumberProcessed = COUNT(Trip.TripNumber) FROM Trip 
                    WHERE Trip.TripTerminalId = @pTerminal AND
                          Trip.TripStatus = @pStatus AND 
                          (Trip.TripSendFlag = 4 OR Trip.TripSendFlag = 5) AND 
                          DATEDIFF(DAY,  Trip.TripReadyDateTime, GETDATE()) > CONVERT(INT,@pParam1)


     SET @TripCursor = CURSOR FORWARD_ONLY STATIC FOR 
       SELECT Trip.TripNumber FROM Trip 
                     WHERE Trip.TripTerminalId = @pTerminal AND
                           Trip.TripStatus = @pStatus AND 
                          (Trip.TripSendFlag = 4 OR Trip.TripSendFlag = 5) AND 
                           DATEDIFF(DAY,  Trip.TripReadyDateTime, GETDATE()) > CONVERT(INT,@pParam1)
   END
   ELSE -- DATE
   BEGIN
     SELECT @lNumberProcessed = COUNT(Trip.TripNumber) FROM Trip 
                    WHERE Trip.TripTerminalId = @pTerminal AND
                          Trip.TripStatus = @pStatus AND 
                          (Trip.TripSendFlag = 4 OR Trip.TripSendFlag = 5) AND 
                          Trip.TripReadyDateTime < CONVERT(datetime,@pParam1,@Style) 


     SET @TripCursor = CURSOR FORWARD_ONLY STATIC FOR 
       SELECT Trip.TripNumber FROM Trip 
                     WHERE Trip.TripTerminalId = @pTerminal AND
                           Trip.TripStatus = @pStatus AND 
                          (Trip.TripSendFlag = 4 OR Trip.TripSendFlag = 5) AND 
                           Trip.TripReadyDateTime < CONVERT(datetime,@pParam1, @Style) 
 END

   OPEN @TripCursor
   FETCH NEXT FROM @TripCursor INTO @TripNumber

   WHILE(@@fetch_status = 0)
      BEGIN

          if ( @ErrorSave = 0)
	  exec pc_ArchiveTripSegmentTimeTable @TripNumber,@ErrorSave
	else
           PRINT 'ERROR pc_ArchiveTripSegmentTimeTable ' + CONVERT(varchar(5),@ErrorSave)

        if ( @ErrorSave = 0)
	  exec pc_ArchiveTripSegmentContainerTimeTable @TripNumber,@ErrorSave
	else
           PRINT 'ERROR pc_ArchiveTripSegmentContainerTimeTable ' + CONVERT(varchar(5),@ErrorSave)

       if ( @ErrorSave = 0)
	  exec pc_ArchiveTripReferenceNumbersTable @TripNumber,@ErrorSave
	else
           PRINT 'ERROR pc_ArchiveTripReferenceNumbersTable ' + CONVERT(varchar(5),@ErrorSave)

        if ( @ErrorSave = 0)
	  exec pc_ArchiveTripSegmentMileageTable @TripNumber,@ErrorSave
	else
           PRINT 'ERROR pc_ArchiveTripSegmentMileageTable ' + CONVERT(varchar(5),@ErrorSave)

        if ( @ErrorSave = 0)
	   exec pc_ArchiveTripSegmentContainerTable @TripNumber,@ErrorSave 
        else
           PRINT 'ERROR pc_ArchiveTripSegmentContainerTable ' + CONVERT(varchar(5),@ErrorSave)

        if ( @ErrorSave = 0)
	   exec pc_ArchiveTripSegmentTable @TripNumber,@ErrorSave 
        else
           PRINT 'ERROR pc_ArchiveTripSegmentTable ' + CONVERT(varchar(5),@ErrorSave)

        if ( @ErrorSave = 0)
 	   exec pc_ArchiveTripTable @TripNumber,@ErrorSave
	else
           PRINT 'ERROR pc_ArchiveTripSegmentTable ' + CONVERT(varchar(5),@ErrorSave)

        if ( @ErrorSave = 0)
 	    exec pc_DeleteTripInfo @TripNumber,@ErrorSave
	else
            PRINT 'ERROR pc_ArchiveTripTable ' + CONVERT(varchar(5),@ErrorSave)

        FETCH NEXT FROM @TripCursor INTO @TripNumber
      END

   CLOSE @TripCursor
END --of BEGIN
DEALLOCATE @TripCursor
SET @lEndDateTime = GETDATE()

IF @lNumberProcessed > 0
  BEGIN
      IF EXISTS( SELECT ProcessType FROM ProcessLog 
           WHERE TerminalId = @pTerminal AND 
                 ProcessType = @pTypeDesc)

           UPDATE ProcessLog 
                  SET TerminalId = @pTerminal,
                      ProcessType = @pTypeDesc,
                      ProcessParam1 = @pParam1,
                      LastActionStartDateTime = @lStartDateTime,
                      LastActionEndDateTime = @lEndDateTime,
                      NumberRecordsProcessed = @lNumberProcessed
                  WHERE TerminalId = @pTerminal AND 
                        ProcessType = @pTypeDesc
      ELSE
          INSERT INTO ProcessLog
           (TerminalId,ProcessType,ProcessParam1,
            LastActionStartDateTime,LastActionEndDateTime,
            NumberRecordsProcessed)
                 VALUES(@pTerminal,@pTypeDesc,@pParam1,
                        @lStartDateTime,@lEndDateTime,
                        @lNumberProcessed)
   END


GO
/*************************************************************
20. pc_ArchiveDriverDelayTable
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_ArchiveDriverDelayTable')
            and   type = 'P')
   drop procedure pc_ArchiveDriverDelayTable
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_ArchiveDriverDelayTable

@pDriverId varchar(10)= NULL,
@pDelaySeqNumber smallint,
@pTripNumber varchar(10)= NULL,
@pErrorSave  smallint  OUTPUT

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

SET @pErrorSave = 0

	INSERT INTO ArcDriverDelay
	(        
                ArchiveDateTime,
		DriverId,
		DelaySeqNumber,
		TripNumber,
		TripSegNumber,
		DriverName,
		DelayCode,
		DelayReason,
		DelayStartDateTime,
		DelayEndDateTime,
		DelayLatitude,
		DelayLongitude,
		TerminalId,
		TerminalName,
		RegionId, 
	        RegionName 
         )

	SELECT  GetDate(),
		DriverId,
		DelaySeqNumber,
		TripNumber,
		TripSegNumber,
		DriverName,
		DelayCode,
		DelayReason,
		DelayStartDateTime,
		DelayEndDateTime,
		DelayLatitude,
		DelayLongitude,
		TerminalId,
		TerminalName,
		RegionId, 
	        RegionName 

 	FROM DriverDelay 
        WHERE DriverDelay.DriverId = @pDriverId AND
              DriverDelay.DelaySeqNumber = @pDelaySeqNumber AND
              DriverDelay.TripNumber = @pTripNumber


IF (@@ERROR <> 0)
    SET @pErrorSave = @@ERROR


GO
/*************************************************************
21. pc_DeleteDriverDelayInfo
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_DeleteDriverDelayInfo')
            and   type = 'P')
   drop procedure pc_DeleteDriverDelayInfo
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_DeleteDriverDelayInfo

@pDriverId varchar(10)= NULL,
@pDelaySeqNumber smallint,
@pTripNumber varchar(10)= NULL,
@pErrorSave  smallint  OUTPUT

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

DELETE FROM DriverDelay 
       WHERE DriverDelay.DriverId = @pDriverId AND
             DriverDelay.DelaySeqNumber = @pDelaySeqNumber AND 
             DriverDelay.TripNumber = @pTripNumber 

GO
/*************************************************************
22. pc_ArchiveDriverDelays
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_ArchiveDriverDelays')
            and   type = 'P')
   drop procedure pc_ArchiveDriverDelays
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_ArchiveDriverDelays

@pTerminal varchar(10)= NULL,
@pTypeDesc varchar(50) = NULL,
@pType     int = -1,
@pStatus   char(1)     = NULL,
@pParam1    varchar(20) = NULL,
@Style     int = 101

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

DECLARE @lStartDateTime DATETIME
DECLARE @lEndDateTime DATETIME
DECLARE @lNumberProcessed int
DECLARE @iSuccessful smallint
DECLARE @DriverDelayCursor CURSOR

DECLARE @lDriverId varchar(10)
DECLARE @lDelaySeqNumber smallint
DECLARE @lTripNumber varchar(10)

DECLARE @ErrorSave smallint;

SET @lStartDateTime = NULL
SET @lEndDateTime = NULL
SET @lNumberProcessed = 0
SET @ErrorSave = 0


--Archive DRIVER DAYS over x days
BEGIN
   SET @lStartDateTime = GETDATE()

   IF (@pType = 0) -- DAYS
     BEGIN
       SELECT @lNumberProcessed = COUNT(DriverDelay.DriverId) FROM DriverDelay 
                    WHERE DriverDelay.TerminalId = @pTerminal AND 
                          DATEDIFF(DAY,  DriverDelay.DelayStartDateTime, GETDATE()) > CONVERT(INT,@pParam1) AND
                          DATEDIFF(DAY,  DriverDelay.DelayEndDateTime, GETDATE()) > CONVERT(INT,@pParam1)

       SET @DriverDelayCursor = CURSOR FORWARD_ONLY STATIC FOR 
         SELECT DriverDelay.DriverId,DriverDelay.DelaySeqNumber,DriverDelay.TripNumber FROM DriverDelay 
                    WHERE DriverDelay.TerminalId = @pTerminal AND 
                          DATEDIFF(DAY,  DriverDelay.DelayStartDateTime, GETDATE()) > CONVERT(INT,@pParam1) AND
                          DATEDIFF(DAY,  DriverDelay.DelayEndDateTime, GETDATE()) > CONVERT(INT,@pParam1)
     END
   ELSE
      BEGIN
       SELECT @lNumberProcessed = COUNT(DriverDelay.DriverId) FROM DriverDelay 
                    WHERE DriverDelay.TerminalId = @pTerminal AND 
                          DriverDelay.DelayStartDateTime < CONVERT(datetime,@pParam1, @Style) AND
                          DriverDelay.DelayEndDateTime < CONVERT(datetime,@pParam1, @Style)

       SET @DriverDelayCursor = CURSOR FORWARD_ONLY STATIC FOR 
         SELECT DriverDelay.DriverId,DriverDelay.DelaySeqNumber,DriverDelay.TripNumber FROM DriverDelay 
                    WHERE DriverDelay.TerminalId = @pTerminal AND 
                          DriverDelay.DelayStartDateTime < CONVERT(datetime,@pParam1, @Style) AND
                          DriverDelay.DelayEndDateTime < CONVERT(datetime,@pParam1, @Style)
       END

   OPEN @DriverDelayCursor
   FETCH NEXT FROM @DriverDelayCursor INTO @lDriverId,@lDelaySeqNumber,@lTripNumber

   WHILE(@@fetch_status = 0)
      BEGIN
        if ( @ErrorSave = 0)
	  exec pc_ArchiveDriverDelayTable @lDriverId,@lDelaySeqNumber,@lTripNumber,@ErrorSave
	else
           PRINT 'ERROR pc_ArchiveDriverDelayTable ' + CONVERT(varchar(5),@ErrorSave)

        if ( @ErrorSave = 0)
 	    exec pc_DeleteDriverDelayInfo @lDriverId,@lDelaySeqNumber,@lTripNumber,@ErrorSave
	else
            PRINT 'ERROR pc_DeleteDriverDelayInfo ' + CONVERT(varchar(5),@ErrorSave)
      FETCH NEXT FROM @DriverDelayCursor INTO @lDriverId,@lDelaySeqNumber,@lTripNumber
      END
CLOSE @DriverDelayCursor
END --of BEGIN
DEALLOCATE @DriverDelayCursor
 
SET @lEndDateTime = GETDATE()

IF @lNumberProcessed > 0
  BEGIN
      IF EXISTS( SELECT ProcessType FROM ProcessLog 
           WHERE TerminalId = @pTerminal AND 
                 ProcessType = @pTypeDesc)

           UPDATE ProcessLog 
                  SET TerminalId = @pTerminal,
                      ProcessType = @pTypeDesc,
                      ProcessParam1 = @pParam1,
                      LastActionStartDateTime = @lStartDateTime,
                      LastActionEndDateTime = @lEndDateTime,
                      NumberRecordsProcessed = @lNumberProcessed
                  WHERE TerminalId = @pTerminal AND 
                        ProcessType = @pTypeDesc
      ELSE
          INSERT INTO ProcessLog
           (TerminalId,ProcessType,ProcessParam1,
            LastActionStartDateTime,LastActionEndDateTime,
            NumberRecordsProcessed)
                 VALUES(@pTerminal,@pTypeDesc,@pParam1,
                        @lStartDateTime,@lEndDateTime,
                        @lNumberProcessed)
   END

GO

/*************************************************************
30. pc_ArchiveContainerHistoryTable
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_ArchiveContainerHistoryTable')
            and   type = 'P')
   drop procedure pc_ArchiveContainerHistoryTable
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_ArchiveContainerHistoryTable

@pContainerNumber varchar(15)= NULL,
@pContainerSeqNumber smallint,

@pErrorSave  smallint  OUTPUT

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

SET @pErrorSave = 0

	INSERT INTO ArcContainerHistory
	(        
		ArchiveDateTime,
		ContainerNumber,
		ContainerSeqNumber,
		ContainerType,
		ContainerSize,
		ContainerUnits,
		ContainerLength,
		ContainerCustHostCode,
		ContainerCustType,
		ContainerCustTypeDesc,
		ContainerTerminalId,
		ContainerTerminalName,
		ContainerRegionId,
		ContainerRegionName,
		ContainerLocation,
		ContainerLastActionDateTime,
		ContainerDaysAtSite,
		ContainerPendingMoveDateTime,
		ContainerTripNumber,
		ContainerTripSegNumber,
		ContainerTripSegType,
		ContainerTripSegTypeDesc,
		ContainerContents,
		ContainerContentsDesc,
		ContainerStatus,
		ContainerStatusDesc,
		ContainerCommodityCode,
		ContainerCommodityDesc,
		ContainerComments,
		ContainerPowerId,
		ContainerShortTerm,
		ContainerCustName,
		ContainerCustAddress1,
		ContainerCustAddress2,
		ContainerCustCity,
		ContainerCustState,
		ContainerCustZip,
		ContainerCustCountry,
		ContainerCustCounty,
		ContainerCustTownship,
		ContainerCustPhone1,
        ContainerLevel,
        ContainerLatitude,
        ContainerLongitude,
		ContainerNotes,
        ContainerCurrentTerminalId,
        ContainerCurrentTerminalName,
        ContainerWidth,
        ContainerHeight
         )

	SELECT
                GetDate(), 
		ContainerNumber,
		ContainerSeqNumber,
		ContainerType,
		ContainerSize,
		ContainerUnits,
		ContainerLength,
		ContainerCustHostCode,
		ContainerCustType,
		ContainerCustTypeDesc,
		ContainerTerminalId,
		ContainerTerminalName,
		ContainerRegionId,
		ContainerRegionName,
		ContainerLocation,
		ContainerLastActionDateTime,
		ContainerDaysAtSite,
		ContainerPendingMoveDateTime,
		ContainerTripNumber,
		ContainerTripSegNumber,
		ContainerTripSegType,
		ContainerTripSegTypeDesc,
		ContainerContents,
		ContainerContentsDesc,
		ContainerStatus,
		ContainerStatusDesc,
		ContainerCommodityCode,
		ContainerCommodityDesc,
		ContainerComments,
		ContainerPowerId,
		ContainerShortTerm,
		ContainerCustName,
		ContainerCustAddress1,
		ContainerCustAddress2,
		ContainerCustCity,
		ContainerCustState,
		ContainerCustZip,
		ContainerCustCountry,
		ContainerCustCounty,
		ContainerCustTownship,
		ContainerCustPhone1,
        ContainerLevel,
        ContainerLatitude,
        ContainerLongitude,
		ContainerNotes,
        ContainerCurrentTerminalId,
        ContainerCurrentTerminalName,
        ContainerWidth,
        ContainerHeight
 	FROM ContainerHistory 
        WHERE ContainerHistory.ContainerNumber = @pContainerNumber AND
              ContainerHistory.ContainerSeqNumber = @pContainerSeqNumber


IF (@@ERROR <> 0)
    SET @pErrorSave = @@ERROR


GO
/*************************************************************
31. pc_DeleteContainerHistoryInfo
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_DeleteContainerHistoryInfo')
            and   type = 'P')
   drop procedure pc_DeleteContainerHistoryInfo
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_DeleteContainerHistoryInfo

@pContainerNumber varchar(15)= NULL,
@pContainerSeqNumber smallint,
@pErrorSave  smallint  OUTPUT

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

DELETE FROM ContainerHistory 
        WHERE ContainerHistory.ContainerNumber = @pContainerNumber AND
              ContainerHistory.ContainerSeqNumber = @pContainerSeqNumber

GO
/*************************************************************
32. pc_ArchiveContainerHistory
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_ArchiveContainerHistory')
            and   type = 'P')
   drop procedure pc_ArchiveContainerHistory
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_ArchiveContainerHistory

@pTerminal varchar(10)= NULL,
@pTypeDesc varchar(50) = NULL,
@pType     int = -1,
@pStatus   char(1) = NULL,
@pParam1    varchar(20) = NULL,
@Style     int = 101

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

DECLARE @lStartDateTime DATETIME
DECLARE @lEndDateTime DATETIME
DECLARE @lNumberProcessed int
DECLARE @iSuccessful smallint
DECLARE @ContainerHistoryCursor CURSOR

DECLARE @pContainerNumber varchar(15)
DECLARE @pContainerSeqNumber smallint
DECLARE @ErrorSave smallint;

SET @lStartDateTime = NULL
SET @lEndDateTime = NULL
SET @lNumberProcessed = 0
SET @ErrorSave = 0


--Archive ContainerHistory over x days
BEGIN
   SET @lStartDateTime = GETDATE()

   IF (@pType = 0) -- DAYS
     BEGIN
       SELECT @lNumberProcessed = COUNT(ContainerHistory.ContainerNumber) FROM ContainerHistory 
              WHERE ContainerHistory.ContainerTerminalId = @pTerminal AND
		    DATEDIFF(DAY, ContainerHistory.ContainerLastActionDateTime, GETDATE()) > CONVERT(INT,@pParam1)

       SET @ContainerHistoryCursor = CURSOR FORWARD_ONLY STATIC FOR 
         SELECT ContainerHistory.ContainerNumber,ContainerHistory.ContainerSeqNumber FROM ContainerHistory 
              WHERE ContainerHistory.ContainerTerminalId = @pTerminal AND
		    DATEDIFF(DAY, ContainerHistory.ContainerLastActionDateTime, GETDATE()) > CONVERT(INT,@pParam1)
     END
   ELSE
      BEGIN
       SELECT @lNumberProcessed = COUNT(ContainerHistory.ContainerNumber) FROM ContainerHistory 
              WHERE ContainerHistory.ContainerTerminalId = @pTerminal AND
		    ContainerHistory.ContainerLastActionDateTime < CONVERT(datetime,@pParam1, @Style)

       SET @ContainerHistoryCursor = CURSOR FORWARD_ONLY STATIC FOR 
         SELECT ContainerHistory.ContainerNumber,ContainerHistory.ContainerSeqNumber FROM ContainerHistory 
              WHERE ContainerHistory.ContainerTerminalId = @pTerminal AND
		    ContainerHistory.ContainerLastActionDateTime < CONVERT(datetime,@pParam1, @Style)
       END

   OPEN @ContainerHistoryCursor
   FETCH NEXT FROM @ContainerHistoryCursor INTO @pContainerNumber,@pContainerSeqNumber

   WHILE(@@fetch_status = 0)
      BEGIN
        if ( @ErrorSave = 0)
	  exec pc_ArchiveContainerHistoryTable @pContainerNumber,@pContainerSeqNumber,@ErrorSave
	else
           PRINT 'ERROR pc_ArchiveContainerHistoryTable ' + CONVERT(varchar(5),@ErrorSave)

        if ( @ErrorSave = 0)
 	    exec pc_DeleteContainerHistoryInfo @pContainerNumber,@pContainerSeqNumber,@ErrorSave
	else
            PRINT 'ERROR pc_DeleteContainerHistoryInfo ' + CONVERT(varchar(5),@ErrorSave)
      FETCH NEXT FROM @ContainerHistoryCursor INTO @pContainerNumber,@pContainerSeqNumber

      END
CLOSE @ContainerHistoryCursor
END --of BEGIN
DEALLOCATE @ContainerHistoryCursor
 
SET @lEndDateTime = GETDATE()

IF @lNumberProcessed > 0
  BEGIN
      IF EXISTS( SELECT ProcessType FROM ProcessLog 
           WHERE TerminalId = @pTerminal AND 
                 ProcessType = @pTypeDesc)

           UPDATE ProcessLog 
                  SET TerminalId = @pTerminal,
                      ProcessType = @pTypeDesc,
                      ProcessParam1 = @pParam1,
                      LastActionStartDateTime = @lStartDateTime,
                      LastActionEndDateTime = @lEndDateTime,
                      NumberRecordsProcessed = @lNumberProcessed
                  WHERE TerminalId = @pTerminal AND 
                        ProcessType = @pTypeDesc
      ELSE
          INSERT INTO ProcessLog
           (TerminalId,ProcessType,ProcessParam1,
            LastActionStartDateTime,LastActionEndDateTime,
            NumberRecordsProcessed)
                 VALUES(@pTerminal,@pTypeDesc,@pParam1,
                        @lStartDateTime,@lEndDateTime,
                        @lNumberProcessed)
   END

GO

/*************************************************************
40. pc_ArchiveDriverHistoryTable
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_ArchiveDriverHistoryTable')
            and   type = 'P')
   drop procedure pc_ArchiveDriverHistoryTable
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_ArchiveDriverHistoryTable

@pEmployeeId varchar(10)= NULL,
@pTripNumber varchar(10)= NULL,
@pDriverSeqNumber smallint,

@pErrorSave  smallint  OUTPUT

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

SET @pErrorSave = 0

	INSERT INTO ArcDriverHistory
	(
		ArchiveDateTime,
		EmployeeId,
		TripNumber,
		DriverSeqNumber,
		TripSegNumber,
		TripSegType,
		TripSegTypeDesc,
		TripAssignStatus,
		TripAssignStatusDesc,
		TripStatus,
		TripStatusDesc,
		TripSegStatus,
		TripSegStatusDesc,
		DriverStatus,
		DriverStatusDesc,
		DriverName,
		TerminalId,
		TerminalName,
		RegionId,
		RegionName,
		PowerId,
		DriverArea,
		MDTId,
		LoginDateTime,
		ActionDateTime,
		DriverCumMinutes,
		Odometer,
		DestCustType,
		DestCustTypeDesc,
		DestCustHostCode,
		DestCustName,
		DestCustAddress1,
		DestCustAddress2,
		DestCustCity,
		DestCustState,
		DestCustZip,
		DestCustCountry,
		GPSAutoGeneratedFlag,
		GPSXmitFlag,
		MdtVersion,
		ServicesFlag

        )

	SELECT 
		GetDate(),
		EmployeeId,
		TripNumber,
		DriverSeqNumber,
		TripSegNumber,
		TripSegType,
		TripSegTypeDesc,
		TripAssignStatus,
		TripAssignStatusDesc,
		TripStatus,
		TripStatusDesc,
		TripSegStatus,
		TripSegStatusDesc,
		DriverStatus,
		DriverStatusDesc,
		DriverName,
		TerminalId,
		TerminalName,
		RegionId,
		RegionName,
		PowerId,
		DriverArea,
		MDTId,
		LoginDateTime,
		ActionDateTime,
		DriverCumMinutes,
		Odometer,
		DestCustType,
		DestCustTypeDesc,
		DestCustHostCode,
		DestCustName,
		DestCustAddress1,
		DestCustAddress2,
		DestCustCity,
		DestCustState,
		DestCustZip,
		DestCustCountry,
		GPSAutoGeneratedFlag,
		GPSXmitFlag,
		MdtVersion,
		ServicesFlag

 	FROM DriverHistory 
        WHERE DriverHistory.EmployeeId = @pEmployeeId AND
              DriverHistory.TripNumber = @pTripNumber AND
              DriverHistory.DriverSeqNumber = @pDriverSeqNumber

IF (@@ERROR <> 0)
    SET @pErrorSave = @@ERROR


GO
/*************************************************************
41. pc_DeleteDriverHistoryInfo
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_DeleteDriverHistoryInfo')
            and   type = 'P')
   drop procedure pc_DeleteDriverHistoryInfo
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_DeleteDriverHistoryInfo

@pEmployeeId varchar(10)= NULL,
@pTripNumber varchar(10)= NULL,
@pDriverSeqNumber smallint,
@pErrorSave  smallint  OUTPUT

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

DELETE FROM DriverHistory 
        WHERE DriverHistory.EmployeeId = @pEmployeeId AND
              DriverHistory.TripNumber = @pTripNumber AND
              DriverHistory.DriverSeqNumber = @pDriverSeqNumber

GO
/*************************************************************
42. pc_ArchiveDriverHistory
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_ArchiveDriverHistory')
            and   type = 'P')
   drop procedure pc_ArchiveDriverHistory
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_ArchiveDriverHistory

@pTerminal varchar(10)= NULL,
@pTypeDesc varchar(50) = NULL,
@pType     int = -1,
@pStatus   char(1)     = NULL,
@pParam1    varchar(20) = NULL,
@Style     int = 101

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

DECLARE @lStartDateTime DATETIME
DECLARE @lEndDateTime DATETIME
DECLARE @lNumberProcessed int
DECLARE @iSuccessful smallint
DECLARE @DriverHistoryCursor CURSOR

DECLARE @pEmployeeId varchar(10)
DECLARE @pTripNumber varchar(10)
DECLARE @pDriverSeqNumber smallint
DECLARE @ErrorSave smallint;

SET @lStartDateTime = NULL
SET @lEndDateTime = NULL
SET @lNumberProcessed = 0
SET @ErrorSave = 0


--Archive DriverHistory over x days
BEGIN
   SET @lStartDateTime = GETDATE()

   IF (@pType = 0) -- DAYS
     BEGIN
       SELECT @lNumberProcessed = COUNT(DriverHistory.EmployeeId) FROM DriverHistory 
              WHERE DriverHistory.TerminalId = @pTerminal AND
		    DATEDIFF(DAY, DriverHistory.ActionDateTime, GETDATE()) > CONVERT(INT,@pParam1)

       SET @DriverHistoryCursor = CURSOR FORWARD_ONLY STATIC FOR 
         SELECT DriverHistory.EmployeeId,DriverHistory.TripNumber,DriverHistory.DriverSeqNumber FROM DriverHistory 
              WHERE DriverHistory.TerminalId = @pTerminal AND
		    DATEDIFF(DAY, DriverHistory.ActionDateTime, GETDATE()) > CONVERT(INT,@pParam1)
     END
   ELSE
      BEGIN
       SELECT @lNumberProcessed = COUNT(DriverHistory.EmployeeId) FROM DriverHistory 
              WHERE DriverHistory.TerminalId = @pTerminal AND
		    DriverHistory.ActionDateTime < CONVERT(datetime,@pParam1, @Style)

       SET @DriverHistoryCursor = CURSOR FORWARD_ONLY STATIC FOR 
         SELECT DriverHistory.EmployeeId,DriverHistory.TripNumber,DriverHistory.DriverSeqNumber FROM DriverHistory 
              WHERE DriverHistory.TerminalId = @pTerminal AND
		    DriverHistory.ActionDateTime < CONVERT(datetime,@pParam1, @Style)
       END

   OPEN @DriverHistoryCursor
   FETCH NEXT FROM @DriverHistoryCursor INTO @pEmployeeId,@pTripNumber,@pDriverSeqNumber

   WHILE(@@fetch_status = 0)
      BEGIN
        if ( @ErrorSave = 0)
	  exec pc_ArchiveDriverHistoryTable @pEmployeeId,@pTripNumber,@pDriverSeqNumber,@ErrorSave
	else
           PRINT 'ERROR pc_ArchiveDriverHistoryTable ' + CONVERT(varchar(5),@ErrorSave)

        if ( @ErrorSave = 0)
 	    exec pc_DeleteDriverHistoryInfo @pEmployeeId,@pTripNumber,@pDriverSeqNumber,@ErrorSave
	else
            PRINT 'ERROR pc_DeleteDriverHistoryInfo ' + CONVERT(varchar(5),@ErrorSave)
   FETCH NEXT FROM @DriverHistoryCursor INTO @pEmployeeId,@pTripNumber,@pDriverSeqNumber

      END
CLOSE @DriverHistoryCursor
END --of BEGIN
DEALLOCATE @DriverHistoryCursor
 
SET @lEndDateTime = GETDATE()

IF @lNumberProcessed > 0
  BEGIN
      IF EXISTS( SELECT ProcessType FROM ProcessLog 
           WHERE TerminalId = @pTerminal AND 
                 ProcessType = @pTypeDesc)

           UPDATE ProcessLog 
                  SET TerminalId = @pTerminal,
                      ProcessType = @pTypeDesc,
                      ProcessParam1 = @pParam1,
                      LastActionStartDateTime = @lStartDateTime,
                      LastActionEndDateTime = @lEndDateTime,
                      NumberRecordsProcessed = @lNumberProcessed
                  WHERE TerminalId = @pTerminal AND 
                        ProcessType = @pTypeDesc
      ELSE
          INSERT INTO ProcessLog
           (TerminalId,ProcessType,ProcessParam1,
            LastActionStartDateTime,LastActionEndDateTime,
            NumberRecordsProcessed)
                 VALUES(@pTerminal,@pTypeDesc,@pParam1,
                        @lStartDateTime,@lEndDateTime,
                        @lNumberProcessed)
   END

GO

/*************************************************************
50. pc_ArchivePowerHistoryTable
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_ArchivePowerHistoryTable')
            and   type = 'P')
   drop procedure pc_ArchivePowerHistoryTable
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_ArchivePowerHistoryTable

@pPowerId varchar(16)= NULL,
@pPowerSeqNumber smallint,
@pErrorSave  smallint  OUTPUT

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

SET @pErrorSave = 0

	INSERT INTO ArcPowerHistory
	(       ArchiveDateTime,
		PowerId,
		PowerSeqNumber,
		PowerType,
		PowerDesc,
		PowerSize,
		PowerLength,
		PowerTareWeight,
		PowerCustType,
		PowerCustTypeDesc,
		PowerTerminalId,
		PowerTerminalName,
		PowerRegionId,
		PowerRegionName,
		PowerLocation,
		PowerStatus,
		PowerStatusDesc,
		PowerDateOutOfService,
		PowerDateInService,
		PowerDriverId,
		PowerOdometer,
		PowerComments,
		MdtId,
		PrimaryPowerType,
		PowerCustHostCode,
		PowerCustName,
		PowerCustAddress1,
		PowerCustAddress2,
		PowerCustCity,
		PowerCustState,
		PowerCustZip,
		PowerCustCountry,
		PowerCustCounty,
		PowerCustTownship,
		PowerCustPhone1,
		PowerLastActionDateTime,
		PowerCurrentTripNumber,
		PowerCurrentTripSegNumber,
		PowerCurrentTripSegType,
		PowerCurrentTripSegTypeDesc
        )

	SELECT  GetDate(),
		PowerId,
		PowerSeqNumber,
		PowerType,
		PowerDesc,
		PowerSize,
		PowerLength,
		PowerTareWeight,
		PowerCustType,
		PowerCustTypeDesc,
		PowerTerminalId,
		PowerTerminalName,
		PowerRegionId,
		PowerRegionName,
		PowerLocation,
		PowerStatus,
		PowerStatusDesc,
		PowerDateOutOfService,
		PowerDateInService,
		PowerDriverId,
		PowerOdometer,
		PowerComments,
		MdtId,
		PrimaryPowerType,
		PowerCustHostCode,
		PowerCustName,
		PowerCustAddress1,
		PowerCustAddress2,
		PowerCustCity,
		PowerCustState,
		PowerCustZip,
		PowerCustCountry,
		PowerCustCounty,
		PowerCustTownship,
		PowerCustPhone1,
		PowerLastActionDateTime,
		PowerCurrentTripNumber,
		PowerCurrentTripSegNumber,
		PowerCurrentTripSegType,
		PowerCurrentTripSegTypeDesc

 	FROM PowerHistory 
        WHERE PowerHistory.PowerId = @pPowerId AND
              PowerHistory.PowerSeqNumber = @pPowerSeqNumber

IF (@@ERROR <> 0)
    SET @pErrorSave = @@ERROR



GO

/*************************************************************
41. pc_DeletePowerHistoryInfo
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_DeletePowerHistoryInfo')
            and   type = 'P')
   drop procedure pc_DeletePowerHistoryInfo
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_DeletePowerHistoryInfo

@pPowerId varchar(16)= NULL,
@pPowerSeqNumber smallint,
@pErrorSave  smallint  OUTPUT

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

DELETE FROM PowerHistory 
        WHERE PowerHistory.PowerId = @pPowerId AND
              PowerHistory.PowerSeqNumber = @pPowerSeqNumber

GO
/*************************************************************
52. pc_ArchivePowerHistory
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_ArchivePowerHistory')
            and   type = 'P')
   drop procedure pc_ArchivePowerHistory
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_ArchivePowerHistory

@pTerminal varchar(10)= NULL,
@pTypeDesc varchar(50) = NULL,
@pType     int = -1,
@pStatus   char(1)     = NULL,
@pParam1    varchar(20) = NULL,
@Style     int = 101

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

DECLARE @lStartDateTime DATETIME
DECLARE @lEndDateTime DATETIME
DECLARE @lNumberProcessed int
DECLARE @iSuccessful smallint
DECLARE @PowerHistoryCursor CURSOR

DECLARE @pPowerId varchar(16)
DECLARE @pPowerSeqNumber smallint
DECLARE @ErrorSave smallint;

SET @lStartDateTime = NULL
SET @lEndDateTime = NULL
SET @lNumberProcessed = 0
SET @ErrorSave = 0


--Archive PowerHistory over x days
BEGIN
   SET @lStartDateTime = GETDATE()

   IF (@pType = 0) -- DAYS
     BEGIN
       SELECT @lNumberProcessed = COUNT(PowerHistory.PowerId) FROM PowerHistory 
              WHERE PowerHistory.PowerTerminalId = @pTerminal AND
		    DATEDIFF(DAY, PowerHistory.PowerLastActionDateTime, GETDATE()) > CONVERT(INT,@pParam1)

       SET @PowerHistoryCursor = CURSOR FORWARD_ONLY STATIC FOR 
         SELECT PowerHistory.PowerId,PowerHistory.PowerSeqNumber FROM PowerHistory 
              WHERE PowerHistory.PowerTerminalId = @pTerminal AND
		    DATEDIFF(DAY, PowerHistory.PowerLastActionDateTime, GETDATE()) > CONVERT(INT,@pParam1)
     END
   ELSE
      BEGIN
       SELECT @lNumberProcessed = COUNT(PowerHistory.PowerId) FROM PowerHistory 
              WHERE PowerHistory.PowerTerminalId = @pTerminal AND
		    PowerHistory.PowerLastActionDateTime < CONVERT(datetime,@pParam1, @Style)

       SET @PowerHistoryCursor = CURSOR FORWARD_ONLY STATIC FOR 
         SELECT PowerHistory.PowerId,PowerHistory.PowerSeqNumber FROM PowerHistory 
              WHERE PowerHistory.PowerTerminalId = @pTerminal AND
		    PowerHistory.PowerLastActionDateTime < CONVERT(datetime,@pParam1, @Style)
       END

   OPEN @PowerHistoryCursor
   FETCH NEXT FROM @PowerHistoryCursor INTO @pPowerId,@pPowerSeqNumber

   WHILE(@@fetch_status = 0)
      BEGIN
        if ( @ErrorSave = 0)
	  exec pc_ArchivePowerHistoryTable @pPowerId,@pPowerSeqNumber,@ErrorSave
	else
           PRINT 'ERROR pc_ArchivePowerHistoryTable ' + CONVERT(varchar(5),@ErrorSave)

        if ( @ErrorSave = 0)
 	    exec pc_DeletePowerHistoryInfo @pPowerId,@pPowerSeqNumber,@ErrorSave
	else
            PRINT 'ERROR pc_DeletePowerHistoryInfo ' + CONVERT(varchar(5),@ErrorSave)
   FETCH NEXT FROM @PowerHistoryCursor INTO @pPowerId,@pPowerSeqNumber

      END
CLOSE @PowerHistoryCursor
END --of BEGIN
DEALLOCATE @PowerHistoryCursor
 
SET @lEndDateTime = GETDATE()

IF @lNumberProcessed > 0
  BEGIN
      IF EXISTS( SELECT ProcessType FROM ProcessLog 
           WHERE TerminalId = @pTerminal AND 
                 ProcessType = @pTypeDesc)

           UPDATE ProcessLog 
                  SET TerminalId = @pTerminal,
                      ProcessType = @pTypeDesc,
                      ProcessParam1 = @pParam1,
                      LastActionStartDateTime = @lStartDateTime,
                      LastActionEndDateTime = @lEndDateTime,
                      NumberRecordsProcessed = @lNumberProcessed
                  WHERE TerminalId = @pTerminal AND 
                        ProcessType = @pTypeDesc
      ELSE
          INSERT INTO ProcessLog
           (TerminalId,ProcessType,ProcessParam1,
            LastActionStartDateTime,LastActionEndDateTime,
            NumberRecordsProcessed)
                 VALUES(@pTerminal,@pTypeDesc,@pParam1,
                        @lStartDateTime,@lEndDateTime,
                        @lNumberProcessed)
   END

GO

/*************************************************************
60. pc_ArchivePowerFuelTable
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_ArchivePowerFuelTable')
            and   type = 'P')
   drop procedure pc_ArchivePowerFuelTable
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_ArchivePowerFuelTable

@pPowerId varchar(16)= NULL,
@pTripNumber varchar(10)= NULL,
@pPowerFuelSeqNumber smallint,
@pErrorSave  smallint  OUTPUT

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

SET @pErrorSave = 0

	INSERT INTO ArcPowerFuel
	(       ArchiveDateTime,
		PowerId,
		PowerFuelSeqNumber,
		TripNumber,
		TripSegNumber,
		TripTerminalId,
		TripRegionId,
		TripDriverId,
		TripDriverName,
		PowerDateOfFuel,
		PowerState,
		PowerCountry,
		PowerOdometer,
		PowerGallons
        )

	SELECT  GetDate(),
		PowerId,
		PowerFuelSeqNumber,
		TripNumber,
		TripSegNumber,
		TripTerminalId,
		TripRegionId,
		TripDriverId,
		TripDriverName,
		PowerDateOfFuel,
		PowerState,
		PowerCountry,
		PowerOdometer,
		PowerGallons
 	FROM PowerFuel 
        WHERE PowerFuel.PowerId = @pPowerId AND
              PowerFuel.TripNumber = @pTripNumber AND
              PowerFuel.PowerFuelSeqNumber = @pPowerFuelSeqNumber

IF (@@ERROR <> 0)
    SET @pErrorSave = @@ERROR


GO
/*************************************************************
61. pc_DeletePowerFuelInfo
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_DeletePowerFuelInfo')
            and   type = 'P')
   drop procedure pc_DeletePowerFuelInfo
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_DeletePowerFuelInfo

@pPowerId varchar(16)= NULL,
@pTripNumber varchar(10)= NULL,
@pPowerFuelSeqNumber smallint,
@pErrorSave  smallint  OUTPUT

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

DELETE FROM PowerFuel 
        WHERE PowerFuel.PowerId = @pPowerId AND
              PowerFuel.TripNumber = @pTripNumber AND
              PowerFuel.PowerFuelSeqNumber = @pPowerFuelSeqNumber

GO
/*************************************************************
62. pc_ArchivePowerFuel
**************************************************************/
if exists (select 1
            from  sysobjects
           where  id = object_id('pc_ArchivePowerFuel')
            and   type = 'P')
   drop procedure pc_ArchivePowerFuel
GO
/**************************************************************/
CREATE PROCEDURE dbo.pc_ArchivePowerFuel

@pTerminal varchar(10)= NULL,
@pTypeDesc varchar(50) = NULL,
@pType     int = -1,
@pStatus   char(1)     = NULL,
@pParam1    varchar(20) = NULL,
@Style     int = 101

AS
SET NOCOUNT ON
SET ANSI_WARNINGS OFF

DECLARE @lStartDateTime DATETIME
DECLARE @lEndDateTime DATETIME
DECLARE @lNumberProcessed int
DECLARE @iSuccessful smallint
DECLARE @PowerFuelCursor CURSOR

DECLARE @pPowerId varchar(16)
DECLARE @pTripNumber varchar(10)
DECLARE @pPowerFuelSeqNumber smallint
DECLARE @ErrorSave smallint;

SET @lStartDateTime = NULL
SET @lEndDateTime = NULL
SET @lNumberProcessed = 0
SET @ErrorSave = 0


--Archive PowerHistory over x days
BEGIN
   SET @lStartDateTime = GETDATE()

   IF (@pType = 0) -- DAYS
     BEGIN
       SELECT @lNumberProcessed = COUNT(PowerFuel.PowerId) FROM PowerFuel 
              WHERE PowerFuel.TripTerminalId = @pTerminal AND
		    DATEDIFF(DAY, PowerFuel.PowerDateOfFuel, GETDATE()) > CONVERT(INT,@pParam1)

       SET @PowerFuelCursor = CURSOR FORWARD_ONLY STATIC FOR 
         SELECT PowerFuel.PowerId,PowerFuel.TripNumber,PowerFuel.PowerFuelSeqNumber FROM PowerFuel 
              WHERE PowerFuel.TripTerminalId = @pTerminal AND
		    DATEDIFF(DAY, PowerFuel.PowerDateOfFuel, GETDATE()) > CONVERT(INT,@pParam1)
     END
   ELSE
      BEGIN
       SELECT @lNumberProcessed = COUNT(PowerFuel.PowerId) FROM PowerFuel 
              WHERE PowerFuel.TripTerminalId = @pTerminal AND
		    PowerFuel.PowerDateOfFuel < CONVERT(datetime,@pParam1, @Style)

       SET @PowerFuelCursor = CURSOR FORWARD_ONLY STATIC FOR 
         SELECT PowerFuel.PowerId,PowerFuel.TripNumber,PowerFuel.PowerFuelSeqNumber FROM PowerFuel 
              WHERE PowerFuel.TripTerminalId = @pTerminal AND
		   PowerFuel.PowerDateOfFuel < CONVERT(datetime,@pParam1, @Style)
       END

   OPEN @PowerFuelCursor
   FETCH NEXT FROM @PowerFuelCursor INTO @pPowerId,@pTripNumber,@pPowerFuelSeqNumber

   WHILE(@@fetch_status = 0)
      BEGIN
        if ( @ErrorSave = 0)
	  exec pc_ArchivePowerFuelTable @pPowerId,@pTripNumber,@pPowerFuelSeqNumber,@ErrorSave
	else
           PRINT 'ERROR pc_ArchivePowerFuelTable ' + CONVERT(varchar(5),@ErrorSave)

        if ( @ErrorSave = 0)
 	    exec pc_DeletePowerFuelInfo @pPowerId,@pTripNumber,@pPowerFuelSeqNumber,@ErrorSave
	else
            PRINT 'ERROR pc_DeletePowerFuelInfo ' + CONVERT(varchar(5),@ErrorSave)
    FETCH NEXT FROM @PowerFuelCursor INTO @pPowerId,@pTripNumber,@pPowerFuelSeqNumber

      END
CLOSE @PowerFuelCursor
END --of BEGIN
DEALLOCATE @PowerFuelCursor
 
SET @lEndDateTime = GETDATE()

IF @lNumberProcessed > 0
  BEGIN
      IF EXISTS( SELECT ProcessType FROM ProcessLog 
           WHERE TerminalId = @pTerminal AND 
                 ProcessType = @pTypeDesc)

           UPDATE ProcessLog 
                  SET TerminalId = @pTerminal,
                      ProcessType = @pTypeDesc,
                      ProcessParam1 = @pParam1,
                      LastActionStartDateTime = @lStartDateTime,
                      LastActionEndDateTime = @lEndDateTime,
                      NumberRecordsProcessed = @lNumberProcessed
                  WHERE TerminalId = @pTerminal AND 
                        ProcessType = @pTypeDesc
      ELSE
          INSERT INTO ProcessLog
           (TerminalId,ProcessType,ProcessParam1,
            LastActionStartDateTime,LastActionEndDateTime,
            NumberRecordsProcessed)
                 VALUES(@pTerminal,@pTypeDesc,@pParam1,
                        @lStartDateTime,@lEndDateTime,
                        @lNumberProcessed)
   END

GO