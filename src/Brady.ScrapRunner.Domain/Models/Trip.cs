using BWF.DataServices.Metadata.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Enums;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A Trip record.
    /// </summary>

    public class Trip : IHaveId<string>, IEquatable<Trip>
    {
        public virtual string TripNumber { get; set; }
        public virtual string TripStatus { get; set; }
        public virtual string TripStatusDesc { get; set; }
        public virtual string TripAssignStatus { get; set; }
        public virtual string TripAssignStatusDesc { get; set; }
        public virtual string TripType { get; set; }
        public virtual string TripTypeDesc { get; set; }
        public virtual int? TripSequenceNumber { get; set; }
        public virtual TripSendFlagValue TripSendFlag { get; set; }
        public virtual string TripDriverId { get; set; }
        public virtual string TripDriverName { get; set; }
        public virtual string TripCustHostCode { get; set; }
        public virtual string TripCustCode4_4 { get; set; }
        public virtual string TripCustName { get; set; }
        public virtual string TripCustAddress1 { get; set; }
        public virtual string TripCustAddress2 { get; set; }
        public virtual string TripCustCity { get; set; }
        public virtual string TripCustState { get; set; }
        public virtual string TripCustZip { get; set; }
        public virtual string TripCustCountry { get; set; }
        public virtual string TripCustPhone1 { get; set; }
        public virtual string TripTerminalId { get; set; }
        public virtual string TripTerminalName { get; set; }
        public virtual string TripRegionId { get; set; }
        public virtual string TripRegionName { get; set; }
        public virtual string TripContactName { get; set; }
        public virtual string TripSalesman { get; set; }
        public virtual DateTime? TripCustOpenTime { get; set; }
        public virtual DateTime? TripCustCloseTime { get; set; }
        public virtual DateTime? TripReadyDateTime { get; set; }
        public virtual string TripEnteredUserId { get; set; }
        public virtual string TripEnteredUserName { get; set; }
        public virtual DateTime? TripEnteredDateTime { get; set; }
        public virtual int? TripStandardDriveMinutes { get; set; }
        public virtual int? TripStandardStopMinutes { get; set; }
        public virtual int? TripActualDriveMinutes { get; set; }
        public virtual int? TripActualStopMinutes { get; set; }
        public virtual string TripContractNumber { get; set; }
        public virtual string TripShipmentNumber { get; set; }
        public virtual string TripCommodityPurchase { get; set; }
        public virtual string TripCommoditySale { get; set; }
        public virtual string TripSpecInstructions { get; set; }
        public virtual string TripExpediteFlag { get; set; }
        public virtual string TripPrimaryContainerNumber { get; set; }
        public virtual string TripPrimaryContainerType { get; set; }
        public virtual string TripPrimaryContainerSize { get; set; }
        public virtual string TripPrimaryCommodityCode { get; set; }
        public virtual string TripPrimaryCommodityDesc { get; set; }
        public virtual string TripPrimaryContainerLocation { get; set; }
        public virtual string TripPowerId { get; set; }
        public virtual string TripCommodityScaleMsg { get; set; }
        public virtual TripSendAutoReceiptValue TripSendReceiptFlag { get; set; }
        public virtual string TripDriverIdPrev { get; set; }
        public virtual DateTime? TripCompletedDateTime { get; set; }
        public virtual int? TripSendScaleNotificationFlag { get; set; }
        public virtual string TripExtendedFlag { get; set; }
        public virtual string TripExtendedReason { get; set; }
        public virtual string TripInProgressFlag { get; set; }
        public virtual string TripNightRunFlag { get; set; }
        public virtual string TripDoneMethod { get; set; }
        public virtual string TripCompletedUserId { get; set; }
        public virtual string TripCompletedUserName { get; set; }
        public virtual string TripReferenceNumber { get; set; }
        public virtual DateTime? TripChangedDateTime { get; set; }
        public virtual string TripChangedUserId { get; set; }
        public virtual string TripChangedUserName { get; set; }
        public virtual string TripDirectSegNumber { get; set; }
        public virtual int? TripDirectDriveMinutes { get; set; }
        public virtual int? TripDirectTotalMinutes { get; set; }
        public virtual DateTime? TripStartedDateTime { get; set; }
        public virtual int? TripActualTotalMinutes { get; set; }
        public virtual string TripErrorDesc { get; set; }
        public virtual string TripDriverInstructions { get; set; }
        public virtual string TripDispatcherInstructions { get; set; }
        public virtual string TripScaleReferenceNumber { get; set; }
        public virtual string TripMultContainerFlag { get; set; }
        public virtual TripSendReseqFlagValue TripSendReseqFlag { get; set; }
        public virtual string TripServerLocation { get; set; }
        public virtual string TripPowerAssetNumber { get; set; }
        public virtual string TripStatusPrev { get; set; }
        public virtual string TripHaulerHostCode { get; set; }
        public virtual string TripHaulerName { get; set; }
        public virtual string TripHaulerAddress1 { get; set; }
        public virtual string TripHaulerCity { get; set; }
        public virtual string TripHaulerState { get; set; }
        public virtual string TripHaulerZip { get; set; }
        public virtual string TripHaulerCountry { get; set; }
        public virtual string TripResolvedFlag { get; set; }
        public virtual DateTime? TripSendScaleDateTime { get; set; }
        public virtual int? TripResendScaleNotificationFlag { get; set; }
        public virtual DateTime? TripResendScaleDateTime { get; set; }
        public virtual string TripImageFlag { get; set; }
        public virtual string TripScaleTerminalId { get; set; }
        public virtual string TripScaleTerminalName { get; set; }
        public virtual string TripSendScaleTerminalId { get; set; }
        public virtual string TripSendScaleTerminalName { get; set; }
        public virtual string TripResendScaleTerminalId { get; set; }
        public virtual string TripResendScaleTerminalName { get; set; }

        public virtual string Id
        {
            get
            {
                return TripNumber;
            }
            set
            {

            }
        }

        public virtual bool Equals(Trip other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(TripNumber, other.TripNumber);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Trip) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (TripNumber != null ? TripNumber.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
