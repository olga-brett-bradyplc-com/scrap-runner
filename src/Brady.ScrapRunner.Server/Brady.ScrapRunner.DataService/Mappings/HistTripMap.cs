using Brady.ScrapRunner.Domain.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.DataService.Mappings
{
    public class HistTripMap : ClassMapping<HistTrip>
    {
        public HistTripMap()
        {

            Table("HistTrip");

            ComposedId(map =>
            {
                map.Property(y => y.HistSeqNo, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.TripNumber, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Id, m =>
            {
                m.Formula("concat(HistSeqNo, ';', TripNumber)");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.HistAction);
            Property(x => x.TripStatus);
            Property(x => x.TripStatusDesc);
            Property(x => x.TripAssignStatus);
            Property(x => x.TripAssignStatusDesc);
            Property(x => x.TripType);
            Property(x => x.TripTypeDesc);
            Property(x => x.TripSequenceNumber);
            Property(x => x.TripSendFlag);
            Property(x => x.TripDriverId);
            Property(x => x.TripDriverName);
            Property(x => x.TripCustHostCode);
            Property(x => x.TripCustCode4_4);
            Property(x => x.TripCustName);
            Property(x => x.TripCustAddress1);
            Property(x => x.TripCustAddress2);
            Property(x => x.TripCustCity);
            Property(x => x.TripCustState);
            Property(x => x.TripCustZip);
            Property(x => x.TripCustCountry);
            Property(x => x.TripCustPhone1);
            Property(x => x.TripTerminalId);
            Property(x => x.TripTerminalName);
            Property(x => x.TripRegionId);
            Property(x => x.TripRegionName);
            Property(x => x.TripContactName);
            Property(x => x.TripSalesman);
            Property(x => x.TripCustOpenTime);
            Property(x => x.TripCustCloseTime);
            Property(x => x.TripReadyDateTime);
            Property(x => x.TripEnteredUserId);
            Property(x => x.TripEnteredUserName);
            Property(x => x.TripEnteredDateTime);
            Property(x => x.TripStandardDriveMinutes);
            Property(x => x.TripStandardStopMinutes);
            Property(x => x.TripActualDriveMinutes);
            Property(x => x.TripActualStopMinutes);
            Property(x => x.TripContractNumber);
            Property(x => x.TripShipmentNumber);
            Property(x => x.TripCommodityPurchase);
            Property(x => x.TripCommoditySale);
            Property(x => x.TripSpecInstructions);
            Property(x => x.TripExpediteFlag);
            Property(x => x.TripPrimaryContainerNumber);
            Property(x => x.TripPrimaryContainerType);
            Property(x => x.TripPrimaryContainerSize);
            Property(x => x.TripPrimaryCommodityCode);
            Property(x => x.TripPrimaryCommodityDesc);
            Property(x => x.TripPrimaryContainerLocation);
            Property(x => x.TripPowerId);
            Property(x => x.TripCommodityScaleMsg);
            Property(x => x.TripSendReceiptFlag);
            Property(x => x.TripDriverIdPrev);
            Property(x => x.TripCompletedDateTime);
            Property(x => x.TripSendScaleNotificationFlag);
            Property(x => x.TripExtendedFlag);
            Property(x => x.TripExtendedReason);
            Property(x => x.TripInProgressFlag);
            Property(x => x.TripNightRunFlag);
            Property(x => x.TripDoneMethod);
            Property(x => x.TripCompletedUserId);
            Property(x => x.TripCompletedUserName);
            Property(x => x.TripReferenceNumber);
            Property(x => x.TripChangedDateTime);
            Property(x => x.TripChangedUserId);
            Property(x => x.TripChangedUserName);
            Property(x => x.TripDirectSegNumber);
            Property(x => x.TripDirectDriveMinutes);
            Property(x => x.TripDirectTotalMinutes);
            Property(x => x.TripStartedDateTime);
            Property(x => x.TripActualTotalMinutes);
            Property(x => x.TripErrorDesc);
            Property(x => x.TripDriverInstructions);
            Property(x => x.TripDispatcherInstructions);
            Property(x => x.TripScaleReferenceNumber);
            Property(x => x.TripMultContainerFlag);
            Property(x => x.TripSendReseqFlag);
            //Property(x => x.TripServerLocation);
            Property(x => x.TripPowerAssetNumber);
            //Property(x => x.TripStatusPrev);
            Property(x => x.TripHaulerHostCode);
            Property(x => x.TripHaulerName);
            Property(x => x.TripHaulerAddress1);
            Property(x => x.TripHaulerCity);
            Property(x => x.TripHaulerState);
            Property(x => x.TripHaulerZip);
            Property(x => x.TripHaulerCountry);
            Property(x => x.TripResolvedFlag);
            Property(x => x.TripSendScaleDateTime);
            Property(x => x.TripResendScaleNotificationFlag);
            Property(x => x.TripResendScaleDateTime);
            Property(x => x.TripImageFlag);
            Property(x => x.TripScaleTerminalId);
            Property(x => x.TripScaleTerminalName);
            Property(x => x.TripSendScaleTerminalId);
            Property(x => x.TripSendScaleTerminalName);
            Property(x => x.TripResendScaleTerminalId);
            Property(x => x.TripResendScaleTerminalName);
        }
    }
}
