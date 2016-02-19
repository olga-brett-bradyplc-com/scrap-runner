using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
     public class DriverEfficiencyMetadata : TypeMetadataProvider<DriverEfficiency>
    {
        public DriverEfficiencyMetadata()
        {
            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.TripDriverId)
               .IsId()
               .DisplayName(" Driver Id");

            StringProperty(x => x.TripNumber)
                .IsId()
                .DisplayName("Trip Number");

            StringProperty(x => x.TripDriverName);
            StringProperty(x => x.TripType);
            StringProperty(x => x.TripTypeDesc);
            StringProperty(x => x.TripCustHostCode);
            StringProperty(x => x.TripCustName);
            StringProperty(x => x.TripCustAddress1);
            StringProperty(x => x.TripCustAddress2);
            StringProperty(x => x.TripCustCity);
            StringProperty(x => x.TripCustState);
            StringProperty(x => x.TripCustZip);
            StringProperty(x => x.TripCustCountry);
            StringProperty(x => x.TripTerminalId);
            StringProperty(x => x.TripTerminalName);
            StringProperty(x => x.TripRegionId);
            StringProperty(x => x.TripRegionName);
            StringProperty(x => x.TripReferenceNumber);
            DateProperty(x => x.TripCompletedDateTime);
            IntegerProperty(x => x.TripActualDriveMinutes);
            IntegerProperty(x => x.TripStandardDriveMinutes);
            IntegerProperty(x => x.TripActualStopMinutes);
            IntegerProperty(x => x.TripStandardStopMinutes);
            IntegerProperty(x => x.TripActualYardMinutes);
            IntegerProperty(x => x.TripStandardYardMinutes);
            IntegerProperty(x => x.TripActualTotalMinutes);
            IntegerProperty(x => x.TripStandardTotalMinutes);
            IntegerProperty(x => x.TripDelayMinutes);
            StringProperty(x => x.TripPowerId);
            IntegerProperty(x => x.TripOdometerStart);
            IntegerProperty(x => x.TripOdometerEnd);
            IntegerProperty(x => x.TripYardDelayMinutes);
            IntegerProperty(x => x.TripCustDelayMinutes);
            IntegerProperty(x => x.TripLunchBreakDelayMinutes);


            ViewDefaults()
                .Property(x => x.TripDriverId)
                .Property(x => x.TripNumber)
                .Property(x => x.TripDriverName)
                .Property(x => x.TripType)
                .Property(x => x.TripTypeDesc)
                .Property(x => x.TripCustHostCode)
                .Property(x => x.TripCustName)
                .Property(x => x.TripCustAddress1)
                .Property(x => x.TripCustAddress2)
                .Property(x => x.TripCustCity)
                .Property(x => x.TripCustState)
                .Property(x => x.TripCustZip)
                .Property(x => x.TripCustCountry)
                .Property(x => x.TripTerminalId)
                .Property(x => x.TripTerminalName)
                .Property(x => x.TripRegionId)
                .Property(x => x.TripRegionName)
                .Property(x => x.TripReferenceNumber)
                .Property(x => x.TripCompletedDateTime)
                .Property(x => x.TripActualDriveMinutes)
                .Property(x => x.TripStandardDriveMinutes)
                .Property(x => x.TripActualStopMinutes)
                .Property(x => x.TripStandardStopMinutes)
                .Property(x => x.TripActualYardMinutes)
                .Property(x => x.TripStandardYardMinutes)
                .Property(x => x.TripActualTotalMinutes)
                .Property(x => x.TripStandardTotalMinutes)
                .Property(x => x.TripDelayMinutes)
                .Property(x => x.TripPowerId)
                .Property(x => x.TripOdometerStart)
                .Property(x => x.TripOdometerEnd)
                .Property(x => x.TripYardDelayMinutes)
                .Property(x => x.TripCustDelayMinutes)
                .Property(x => x.TripLunchBreakDelayMinutes)

                .OrderBy(x => x.TripDriverId)
                .OrderBy(x => x.TripNumber);

        }
    }
}
