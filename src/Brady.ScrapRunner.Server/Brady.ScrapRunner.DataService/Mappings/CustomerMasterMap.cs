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
    public class CustomerMasterMap : ClassMapping<CustomerMaster>
    {
        public CustomerMasterMap()
        {
            Table("CustomerMaster");

            Property(x => x.Id, m =>
            {
                m.Formula("CustHostCode");
                m.Insert(false);
                m.Update(false);
            });
      
            ComposedId(map =>
            {
                map.Property(y => y.CustHostCode, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.CustType);
            Property(x => x.ServingTerminalId);
            Property(x => x.CustCode4_4);
            Property(x => x.CustCreditType);
            Property(x => x.CustName);
            Property(x => x.CustAddress1);
            Property(x => x.CustAddress2);
            Property(x => x.CustAttention);
            Property(x => x.CustCity);
            Property(x => x.CustState);
            Property(x => x.CustZip);
            Property(x => x.CustCountry);
            Property(x => x.CustCounty);
            Property(x => x.CustTownship);
            Property(x => x.CustPhone1);
            Property(x => x.CustPhone2);
            Property(x => x.CustPhone3);
            Property(x => x.CustFaxPhone);
            Property(x => x.CustPhoneExt1);
            Property(x => x.CustPhoneExt2);
            Property(x => x.CustPhoneExt3);
            Property(x => x.CustEMailAddress);
            Property(x => x.CustAutoReceiptFlag);
            Property(x => x.CustContact1);
            Property(x => x.CustContact2);
            Property(x => x.CustOpenTime);
            Property(x => x.CustCloseTime);
            Property(x => x.CustEtakLatitude);
            Property(x => x.CustEtakLongitude);
            Property(x => x.CustDispatchZone);
            Property(x => x.CustLatitude);
            Property(x => x.CustLongitude);
            Property(x => x.CustGeoCoded);
            Property(x => x.CustRadius);
            Property(x => x.CustSpecInstructions);
            Property(x => x.CustDriverId);
            Property(x => x.CustShortTerm);
            Property(x => x.CustSalesman);
            Property(x => x.CustInactiveDate);
            Property(x => x.CustOverrideTripType);
            Property(x => x.CustTimeFactor);
            Property(x => x.CustLastPUDate);
            Property(x => x.CustAddDate);
            Property(x => x.ChgDateTime);
            Property(x => x.ChgEmployeeId);
            Property(x => x.AddEmployeeId);
            Property(x => x.CustTempFlag);
            Property(x => x.CustAutoRcptSettings);
            Property(x => x.CustAutoGPSFlag);
            Property(x => x.GPSChgEmployeeId);
            Property(x => x.GPSChgDateTime);
            Property(x => x.GPSChgSource);
            Property(x => x.CustSendLatLonReqFlag);
            Property(x => x.RTYardHostCode);
            Property(x => x.ParentHostCode);
            Property(x => x.CustContainerType);
            Property(x => x.CustContainerSize);
            Property(x => x.CustDriverInstructions);
            Property(x => x.CustDispatcherInstructions);
            Property(x => x.CustNightRunFlag);
            Property(x => x.CustRegionId);
            Property(x => x.ComChgDateTime);
            Property(x => x.LocChgDateTime);
            Property(x => x.CustExpediteFlag);
            Property(x => x.HasForkLift);
            Property(x => x.CustSignatureRequired);
            Property(x => x.CustCarriersLicense);
            Property(x => x.CustVehicleRegistration);
            Property(x => x.CustAutoReceiptAllFlag);
            Property(x => x.CustomerComments);
            Property(x => x.CustEMailAddress2);
        }
    }
}