using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BWF.DataServices.Metadata.Interfaces;

namespace Brady.ScrapRunner.Domain.Models
{
    /// <summary>
    /// A CustomerMaster record.
    /// </summary>
    public class CustomerMaster : IHaveId<string>, IEquatable<CustomerMaster>
    {
        public virtual string CustHostCode { get; set; }
        public virtual string CustType { get; set; }
        public virtual string ServingTerminalId { get; set; }
        public virtual string CustCode4_4 { get; set; }
        public virtual string CustCreditType { get; set; }
        public virtual string CustName { get; set; }
        public virtual string CustAddress1 { get; set; }
        public virtual string CustAddress2 { get; set; }
        public virtual string CustAttention { get; set; }
        public virtual string CustCity { get; set; }
        public virtual string CustState { get; set; }
        public virtual string CustZip { get; set; }
        public virtual string CustCountry { get; set; }
        public virtual string CustCounty { get; set; }
        public virtual string CustTownship { get; set; }
        public virtual string CustPhone1 { get; set; }
        public virtual string CustPhone2 { get; set; }
        public virtual string CustPhone3 { get; set; }
        public virtual string CustFaxPhone { get; set; }
        public virtual string CustPhoneExt1 { get; set; }
        public virtual string CustPhoneExt2 { get; set; }
        public virtual string CustPhoneExt3 { get; set; }
        public virtual string CustEMailAddress { get; set; }
        public virtual string CustAutoReceiptFlag { get; set; }
        public virtual string CustContact1 { get; set; }
        public virtual string CustContact2 { get; set; }
        public virtual DateTime? CustOpenTime { get; set; }
        public virtual DateTime? CustCloseTime { get; set; }
        public virtual int? CustEtakLatitude { get; set; }
        public virtual int? CustEtakLongitude { get; set; }
        public virtual string CustDispatchZone { get; set; }
        public virtual int? CustLatitude { get; set; }
        public virtual int? CustLongitude { get; set; }
        public virtual string CustGeoCoded { get; set; }
        public virtual int? CustRadius { get; set; }
        public virtual string CustSpecInstructions { get; set; }
        public virtual string CustDriverId { get; set; }
        public virtual string CustShortTerm { get; set; }
        public virtual string CustSalesman { get; set; }
        public virtual DateTime? CustInactiveDate { get; set; }
        public virtual string CustOverrideTripType { get; set; }
        public virtual int? CustTimeFactor { get; set; }
        public virtual DateTime? CustLastPUDate { get; set; }
        public virtual DateTime? CustAddDate { get; set; }
        public virtual DateTime? ChgDateTime { get; set; }
        public virtual string ChgEmployeeId { get; set; }
        public virtual string AddEmployeeId { get; set; }
        public virtual string CustTempFlag { get; set; }
        public virtual string CustAutoRcptSettings { get; set; }
        public virtual string CustAutoGPSFlag { get; set; }
        public virtual string GPSChgEmployeeId { get; set; }
        public virtual DateTime? GPSChgDateTime { get; set; }
        public virtual string GPSChgSource { get; set; }
        public virtual int? CustSendLatLonReqFlag { get; set; }
        public virtual string RTYardHostCode { get; set; }
        public virtual string ParentHostCode { get; set; }
        public virtual string CustContainerType { get; set; }
        public virtual string CustContainerSize { get; set; }
        public virtual string CustDriverInstructions { get; set; }
        public virtual string CustDispatcherInstructions { get; set; }
        public virtual string CustNightRunFlag { get; set; }
        public virtual string CustRegionId { get; set; }
        public virtual DateTime? ComChgDateTime { get; set; }
        public virtual DateTime? LocChgDateTime { get; set; }
        public virtual string CustExpediteFlag { get; set; }
        public virtual string HasForkLift { get; set; }
        public virtual string CustSignatureRequired { get; set; }
        public virtual string CustCarriersLicense { get; set; }
        public virtual string CustVehicleRegistration { get; set; }
        public virtual string CustAutoReceiptAllFlag { get; set; }
        public virtual string CustomerComments { get; set; }
        public virtual string CustEMailAddress2 { get; set; }
   
        public virtual string Id
        {
            get
            {
                return CustHostCode;
            }
            set
            {

            }
        }

        public virtual bool Equals(CustomerMaster other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(CustHostCode, other.CustHostCode) ;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CustomerMaster) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (CustHostCode != null ? CustHostCode.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
