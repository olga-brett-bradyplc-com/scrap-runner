using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class CustomerMasterMetadata : TypeMetadataProvider<CustomerMaster>
    {
        public CustomerMasterMetadata()
        {

            AutoUpdatesByDefault();
            
            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.CustHostCode)
                .IsId()
                .DisplayName("Cust Host Code");

            StringProperty(x => x.CustType);
            StringProperty(x => x.ServingTerminalId);
            StringProperty(x => x.CustCode4_4);
            StringProperty(x => x.CustCreditType);
            StringProperty(x => x.CustName);
            StringProperty(x => x.CustAddress1);
            StringProperty(x => x.CustAddress2);
            StringProperty(x => x.CustAttention);
            StringProperty(x => x.CustCity);
            StringProperty(x => x.CustState);
            StringProperty(x => x.CustZip);
            StringProperty(x => x.CustCountry);
            StringProperty(x => x.CustCounty);
            StringProperty(x => x.CustTownship);
            StringProperty(x => x.CustPhone1);
            StringProperty(x => x.CustPhone2);
            StringProperty(x => x.CustPhone3);
            StringProperty(x => x.CustFaxPhone);
            StringProperty(x => x.CustPhoneExt1);
            StringProperty(x => x.CustPhoneExt2);
            StringProperty(x => x.CustPhoneExt3);
            StringProperty(x => x.CustEMailAddress);
            StringProperty(x => x.CustAutoReceiptFlag);
            StringProperty(x => x.CustContact1);
            StringProperty(x => x.CustContact2);
            DateProperty(x => x.CustOpenTime);
            DateProperty(x => x.CustCloseTime);
            IntegerProperty(x => x.CustEtakLatitude);
            IntegerProperty(x => x.CustEtakLongitude);
            StringProperty(x => x.CustDispatchZone);
            IntegerProperty(x => x.CustLatitude);
            IntegerProperty(x => x.CustLongitude);
            StringProperty(x => x.CustGeoCoded);
            IntegerProperty(x => x.CustRadius);
            StringProperty(x => x.CustSpecInstructions);
            StringProperty(x => x.CustDriverId);
            StringProperty(x => x.CustShortTerm);
            StringProperty(x => x.CustSalesman);
            DateProperty(x => x.CustInactiveDate);
            StringProperty(x => x.CustOverrideTripType);
            IntegerProperty(x => x.CustTimeFactor);
            DateProperty(x => x.CustLastPUDate);
            DateProperty(x => x.CustAddDate);
            DateProperty(x => x.ChgDateTime);
            StringProperty(x => x.ChgEmployeeId);
            StringProperty(x => x.AddEmployeeId);
            StringProperty(x => x.CustTempFlag);
            StringProperty(x => x.CustAutoRcptSettings);
            StringProperty(x => x.CustAutoGPSFlag);
            StringProperty(x => x.GPSChgEmployeeId);
            DateProperty(x => x.GPSChgDateTime);
            StringProperty(x => x.GPSChgSource);
            IntegerProperty(x => x.CustSendLatLonReqFlag);
            StringProperty(x => x.RTYardHostCode);
            StringProperty(x => x.ParentHostCode);
            StringProperty(x => x.CustContainerType);
            StringProperty(x => x.CustContainerSize);
            StringProperty(x => x.CustDriverInstructions);
            StringProperty(x => x.CustDispatcherInstructions);
            StringProperty(x => x.CustNightRunFlag);
            StringProperty(x => x.CustRegionId);
            DateProperty(x => x.ComChgDateTime);
            DateProperty(x => x.LocChgDateTime);
            StringProperty(x => x.CustExpediteFlag);
            StringProperty(x => x.HasForkLift);
            StringProperty(x => x.CustSignatureRequired);
            StringProperty(x => x.CustCarriersLicense);
            StringProperty(x => x.CustVehicleRegistration);
            StringProperty(x => x.CustAutoReceiptAllFlag);
            StringProperty(x => x.CustomerComments);
            StringProperty(x => x.CustEMailAddress2);

            ViewDefaults()
                .Property(x => x.CustHostCode)
                .Property(x => x.CustType)
                .Property(x => x.ServingTerminalId)
                .Property(x => x.CustCode4_4)
                .Property(x => x.CustCreditType)
                .Property(x => x.CustName)
                .Property(x => x.CustAddress1)
                .Property(x => x.CustAddress2)
                .Property(x => x.CustAttention)
                .Property(x => x.CustCity)
                .Property(x => x.CustState)
                .Property(x => x.CustZip)
                .Property(x => x.CustCountry)
                .Property(x => x.CustCounty)
                .Property(x => x.CustTownship)
                .Property(x => x.CustPhone1)
                .Property(x => x.CustPhone2)
                .Property(x => x.CustPhone3)
                .Property(x => x.CustFaxPhone)
                .Property(x => x.CustPhoneExt1)
                .Property(x => x.CustPhoneExt2)
                .Property(x => x.CustPhoneExt3)
                .Property(x => x.CustEMailAddress)
                .Property(x => x.CustAutoReceiptFlag)
                .Property(x => x.CustContact1)
                .Property(x => x.CustContact2)
                .Property(x => x.CustOpenTime)
                .Property(x => x.CustCloseTime)
                .Property(x => x.CustEtakLatitude)
                .Property(x => x.CustEtakLongitude)
                .Property(x => x.CustDispatchZone)
                .Property(x => x.CustLatitude)
                .Property(x => x.CustLongitude)
                .Property(x => x.CustGeoCoded)
                .Property(x => x.CustRadius)
                .Property(x => x.CustSpecInstructions)
                .Property(x => x.CustDriverId)
                .Property(x => x.CustShortTerm)
                .Property(x => x.CustSalesman)
                .Property(x => x.CustInactiveDate)
                .Property(x => x.CustOverrideTripType)
                .Property(x => x.CustTimeFactor)
                .Property(x => x.CustLastPUDate)
                .Property(x => x.CustAddDate)
                .Property(x => x.ChgDateTime)
                .Property(x => x.ChgEmployeeId)
                .Property(x => x.AddEmployeeId)
                .Property(x => x.CustTempFlag)
                .Property(x => x.CustAutoRcptSettings)
                .Property(x => x.CustAutoGPSFlag)
                .Property(x => x.GPSChgEmployeeId)
                .Property(x => x.GPSChgDateTime)
                .Property(x => x.GPSChgSource)
                .Property(x => x.CustSendLatLonReqFlag)
                .Property(x => x.RTYardHostCode)
                .Property(x => x.ParentHostCode)
                .Property(x => x.CustContainerType)
                .Property(x => x.CustContainerSize)
                .Property(x => x.CustDriverInstructions)
                .Property(x => x.CustDispatcherInstructions)
                .Property(x => x.CustNightRunFlag)
                .Property(x => x.CustRegionId)
                .Property(x => x.ComChgDateTime)
                .Property(x => x.LocChgDateTime)
                .Property(x => x.CustExpediteFlag)
                .Property(x => x.HasForkLift)
                .Property(x => x.CustSignatureRequired)
                .Property(x => x.CustCarriersLicense)
                .Property(x => x.CustVehicleRegistration)
                .Property(x => x.CustAutoReceiptAllFlag)
                .Property(x => x.CustomerComments)
                .Property(x => x.CustEMailAddress2)

                .OrderBy(x => x.CustHostCode);
        }
    }
}
