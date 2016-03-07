using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;


namespace Brady.ScrapRunner.Domain.Metadata
{
    public class TripMetadata : TypeMetadataProvider<Trip>
    {
        public TripMetadata()
        {
            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.TripNumber)
                .IsId()
                .DisplayName("Trip Number");

            StringProperty(x => x.TripStatus);
            StringProperty(x => x.TripStatusDesc);
            StringProperty(x => x.TripAssignStatus);
            StringProperty(x => x.TripAssignStatusDesc);
            StringProperty(x => x.TripType);
            StringProperty(x => x.TripTypeDesc);
            IntegerProperty(x => x.TripSequenceNumber);
            EnumProperty(x => x.TripSendFlag);
            StringProperty(x => x.TripDriverId);
            StringProperty(x => x.TripDriverName);
            StringProperty(x => x.TripCustHostCode);
            StringProperty(x => x.TripCustCode4_4);
            StringProperty(x => x.TripCustName);
            StringProperty(x => x.TripCustAddress1);
            StringProperty(x => x.TripCustAddress2);
            StringProperty(x => x.TripCustCity);
            StringProperty(x => x.TripCustState);
            StringProperty(x => x.TripCustZip);
            StringProperty(x => x.TripCustCountry);
            StringProperty(x => x.TripCustPhone1);
            StringProperty(x => x.TripTerminalId);
            StringProperty(x => x.TripTerminalName);
            StringProperty(x => x.TripRegionId);
            StringProperty(x => x.TripRegionName);
            StringProperty(x => x.TripContactName);
            StringProperty(x => x.TripSalesman);
            DateProperty(x => x.TripCustOpenTime);
            DateProperty(x => x.TripCustCloseTime);
            DateProperty(x => x.TripReadyDateTime);
            StringProperty(x => x.TripEnteredUserId);
            StringProperty(x => x.TripEnteredUserName);
            DateProperty(x => x.TripEnteredDateTime);
            IntegerProperty(x => x.TripStandardDriveMinutes);
            IntegerProperty(x => x.TripStandardStopMinutes);
            IntegerProperty(x => x.TripActualDriveMinutes);
            IntegerProperty(x => x.TripActualStopMinutes);
            StringProperty(x => x.TripContractNumber);
            StringProperty(x => x.TripShipmentNumber);
            StringProperty(x => x.TripCommodityPurchase);
            StringProperty(x => x.TripCommoditySale);
            StringProperty(x => x.TripSpecInstructions);
            StringProperty(x => x.TripExpediteFlag);
            StringProperty(x => x.TripPrimaryContainerNumber);
            StringProperty(x => x.TripPrimaryContainerType);
            StringProperty(x => x.TripPrimaryContainerSize);
            StringProperty(x => x.TripPrimaryCommodityCode);
            StringProperty(x => x.TripPrimaryCommodityDesc);
            StringProperty(x => x.TripPrimaryContainerLocation);
            StringProperty(x => x.TripPowerId);
            StringProperty(x => x.TripCommodityScaleMsg);
            IntegerProperty(x => x.TripSendReceiptFlag);
            StringProperty(x => x.TripDriverIdPrev);
            DateProperty(x => x.TripCompletedDateTime);
            IntegerProperty(x => x.TripSendScaleNotificationFlag);
            StringProperty(x => x.TripExtendedFlag);
            StringProperty(x => x.TripExtendedReason);
            StringProperty(x => x.TripInProgressFlag);
            StringProperty(x => x.TripNightRunFlag);
            StringProperty(x => x.TripDoneMethod);
            StringProperty(x => x.TripCompletedUserId);
            StringProperty(x => x.TripCompletedUserName);
            StringProperty(x => x.TripReferenceNumber);
            DateProperty(x => x.TripChangedDateTime);
            StringProperty(x => x.TripChangedUserId);
            StringProperty(x => x.TripChangedUserName);
            StringProperty(x => x.TripDirectSegNumber);
            IntegerProperty(x => x.TripDirectDriveMinutes);
            IntegerProperty(x => x.TripDirectTotalMinutes);
            DateProperty(x => x.TripStartedDateTime);
            IntegerProperty(x => x.TripActualTotalMinutes);
            StringProperty(x => x.TripErrorDesc);
            StringProperty(x => x.TripDriverInstructions);
            StringProperty(x => x.TripDispatcherInstructions);
            StringProperty(x => x.TripScaleReferenceNumber);
            StringProperty(x => x.TripMultContainerFlag);
            IntegerProperty(x => x.TripSendReseqFlag);
            StringProperty(x => x.TripServerLocation);
            StringProperty(x => x.TripPowerAssetNumber);
            StringProperty(x => x.TripStatusPrev);
            StringProperty(x => x.TripHaulerHostCode);
            StringProperty(x => x.TripHaulerName);
            StringProperty(x => x.TripHaulerAddress1);
            StringProperty(x => x.TripHaulerCity);
            StringProperty(x => x.TripHaulerState);
            StringProperty(x => x.TripHaulerZip);
            StringProperty(x => x.TripHaulerCountry);
            StringProperty(x => x.TripResolvedFlag);
            DateProperty(x => x.TripSendScaleDateTime);
            IntegerProperty(x => x.TripResendScaleNotificationFlag);
            DateProperty(x => x.TripResendScaleDateTime);
            StringProperty(x => x.TripImageFlag);
            StringProperty(x => x.TripScaleTerminalId);
            StringProperty(x => x.TripScaleTerminalName);
            StringProperty(x => x.TripSendScaleTerminalId);
            StringProperty(x => x.TripSendScaleTerminalName);
            StringProperty(x => x.TripResendScaleTerminalId);
            StringProperty(x => x.TripResendScaleTerminalName);

            ViewDefaults()
                .Property(x => x.TripNumber)
                .Property(x => x.TripStatus)
                .Property(x => x.TripStatusDesc)
                .Property(x => x.TripAssignStatus)
                .Property(x => x.TripAssignStatusDesc)
                .Property(x => x.TripType)
                .Property(x => x.TripTypeDesc)
                .Property(x => x.TripSequenceNumber)
                .Property(x => x.TripSendFlag)
                .Property(x => x.TripDriverId)
                .Property(x => x.TripDriverName)
                .Property(x => x.TripCustHostCode)
                .Property(x => x.TripCustCode4_4)
                .Property(x => x.TripCustName)
                .Property(x => x.TripCustAddress1)
                .Property(x => x.TripCustAddress2)
                .Property(x => x.TripCustCity)
                .Property(x => x.TripCustState)
                .Property(x => x.TripCustZip)
                .Property(x => x.TripCustCountry)
                .Property(x => x.TripCustPhone1)
                .Property(x => x.TripTerminalId)
                .Property(x => x.TripTerminalName)
                .Property(x => x.TripRegionId)
                .Property(x => x.TripRegionName)
                .Property(x => x.TripContactName)
                .Property(x => x.TripSalesman)
                .Property(x => x.TripCustOpenTime)
                .Property(x => x.TripCustCloseTime)
                .Property(x => x.TripReadyDateTime)
                .Property(x => x.TripEnteredUserId)
                .Property(x => x.TripEnteredUserName)
                .Property(x => x.TripEnteredDateTime)
                .Property(x => x.TripStandardDriveMinutes)
                .Property(x => x.TripStandardStopMinutes)
                .Property(x => x.TripActualDriveMinutes)
                .Property(x => x.TripActualStopMinutes)
                .Property(x => x.TripContractNumber)
                .Property(x => x.TripShipmentNumber)
                .Property(x => x.TripCommodityPurchase)
                .Property(x => x.TripCommoditySale)
                .Property(x => x.TripSpecInstructions)
                .Property(x => x.TripExpediteFlag)
                .Property(x => x.TripPrimaryContainerNumber)
                .Property(x => x.TripPrimaryContainerType)
                .Property(x => x.TripPrimaryContainerSize)
                .Property(x => x.TripPrimaryCommodityCode)
                .Property(x => x.TripPrimaryCommodityDesc)
                .Property(x => x.TripPrimaryContainerLocation)
                .Property(x => x.TripPowerId)
                .Property(x => x.TripCommodityScaleMsg)
                .Property(x => x.TripSendReceiptFlag)
                .Property(x => x.TripDriverIdPrev)
                .Property(x => x.TripCompletedDateTime)
                .Property(x => x.TripSendScaleNotificationFlag)
                .Property(x => x.TripExtendedFlag)
                .Property(x => x.TripExtendedReason)
                .Property(x => x.TripInProgressFlag)
                .Property(x => x.TripNightRunFlag)
                .Property(x => x.TripDoneMethod)
                .Property(x => x.TripCompletedUserId)
                .Property(x => x.TripCompletedUserName)
                .Property(x => x.TripReferenceNumber)
                .Property(x => x.TripChangedDateTime)
                .Property(x => x.TripChangedUserId)
                .Property(x => x.TripChangedUserName)
                .Property(x => x.TripDirectSegNumber)
                .Property(x => x.TripDirectDriveMinutes)
                .Property(x => x.TripDirectTotalMinutes)
                .Property(x => x.TripStartedDateTime)
                .Property(x => x.TripActualTotalMinutes)
                .Property(x => x.TripErrorDesc)
                .Property(x => x.TripDriverInstructions)
                .Property(x => x.TripDispatcherInstructions)
                .Property(x => x.TripScaleReferenceNumber)
                .Property(x => x.TripMultContainerFlag)
                .Property(x => x.TripSendReseqFlag)
                .Property(x => x.TripServerLocation)
                .Property(x => x.TripPowerAssetNumber)
                .Property(x => x.TripStatusPrev)
                .Property(x => x.TripHaulerHostCode)
                .Property(x => x.TripHaulerName)
                .Property(x => x.TripHaulerAddress1)
                .Property(x => x.TripHaulerCity)
                .Property(x => x.TripHaulerState)
                .Property(x => x.TripHaulerZip)
                .Property(x => x.TripHaulerCountry)
                .Property(x => x.TripResolvedFlag)
                .Property(x => x.TripSendScaleDateTime)
                .Property(x => x.TripResendScaleNotificationFlag)
                .Property(x => x.TripResendScaleDateTime)
                .Property(x => x.TripImageFlag)
                .Property(x => x.TripScaleTerminalId)
                .Property(x => x.TripScaleTerminalName)
                .Property(x => x.TripSendScaleTerminalId)
                .Property(x => x.TripSendScaleTerminalName)
                .Property(x => x.TripResendScaleTerminalId)
                .Property(x => x.TripResendScaleTerminalName)
                .OrderBy(x => x.TripNumber);

        }
    }
}
