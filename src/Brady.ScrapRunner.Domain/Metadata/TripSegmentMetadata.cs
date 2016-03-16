using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;


namespace Brady.ScrapRunner.Domain.Metadata
{
    public class TripSegmentMetadata : TypeMetadataProvider<TripSegment>
    {
        public TripSegmentMetadata()
        {
            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.TripNumber)
                .IsId()
                .DisplayName("Trip Number");

            StringProperty(x => x.TripSegNumber)
                .IsId()
                .DisplayName("Trip Seg Number");

            StringProperty(x => x.TripSegStatus);
            StringProperty(x => x.TripSegStatusDesc);
            StringProperty(x => x.TripSegType);
            StringProperty(x => x.TripSegTypeDesc);
            StringProperty(x => x.TripSegPowerId);
            StringProperty(x => x.TripSegDriverId);
            StringProperty(x => x.TripSegDriverName);
            TimeProperty(x => x.TripSegStartDateTime);
            TimeProperty(x => x.TripSegEndDateTime);
            IntegerProperty(x => x.TripSegStandardDriveMinutes);
            IntegerProperty(x => x.TripSegStandardStopMinutes);
            IntegerProperty(x => x.TripSegActualDriveMinutes);
            IntegerProperty(x => x.TripSegActualStopMinutes);
            IntegerProperty(x => x.TripSegOdometerStart);
            IntegerProperty(x => x.TripSegOdometerEnd);
            StringProperty(x => x.TripSegComments);
            StringProperty(x => x.TripSegOrigCustType);
            StringProperty(x => x.TripSegOrigCustTypeDesc);
            StringProperty(x => x.TripSegOrigCustHostCode);
            StringProperty(x => x.TripSegOrigCustCode4_4);
            StringProperty(x => x.TripSegOrigCustName);
            StringProperty(x => x.TripSegOrigCustAddress1);
            StringProperty(x => x.TripSegOrigCustAddress2);
            StringProperty(x => x.TripSegOrigCustCity);
            StringProperty(x => x.TripSegOrigCustState);
            StringProperty(x => x.TripSegOrigCustZip);
            StringProperty(x => x.TripSegOrigCustCountry);
            StringProperty(x => x.TripSegOrigCustPhone1);
            IntegerProperty(x => x.TripSegOrigCustTimeFactor);
            StringProperty(x => x.TripSegDestCustType);
            StringProperty(x => x.TripSegDestCustTypeDesc);
            StringProperty(x => x.TripSegDestCustHostCode);
            StringProperty(x => x.TripSegDestCustCode4_4);
            StringProperty(x => x.TripSegDestCustName);
            StringProperty(x => x.TripSegDestCustAddress1);
            StringProperty(x => x.TripSegDestCustAddress2);
            StringProperty(x => x.TripSegDestCustCity);
            StringProperty(x => x.TripSegDestCustState);
            StringProperty(x => x.TripSegDestCustZip);
            StringProperty(x => x.TripSegDestCustCountry);
            StringProperty(x => x.TripSegDestCustPhone1);
            IntegerProperty(x => x.TripSegDestCustTimeFactor);
            StringProperty(x => x.TripSegPrimaryContainerNumber);
            StringProperty(x => x.TripSegPrimaryContainerType);
            StringProperty(x => x.TripSegPrimaryContainerSize);
            StringProperty(x => x.TripSegPrimaryContainerCommodityCode);
            StringProperty(x => x.TripSegPrimaryContainerCommodityDesc);
            StringProperty(x => x.TripSegPrimaryContainerLocation);
            TimeProperty(x => x.TripSegActualDriveStartDateTime);
            TimeProperty(x => x.TripSegActualDriveEndDateTime);
            TimeProperty(x => x.TripSegActualStopStartDateTime);
            TimeProperty(x => x.TripSegActualStopEndDateTime);
            IntegerProperty(x => x.TripSegStartLatitude);
            IntegerProperty(x => x.TripSegStartLongitude);
            IntegerProperty(x => x.TripSegEndLatitude);
            IntegerProperty(x => x.TripSegEndLongitude);
            IntegerProperty(x => x.TripSegStandardMiles);
            StringProperty(x => x.TripSegErrorDesc);
            IntegerProperty(x => x.TripSegContainerQty);
            StringProperty(x => x.TripSegDriverGenerated);
            StringProperty(x => x.TripSegDriverModified);
            StringProperty(x => x.TripSegPowerAssetNumber);
            StringProperty(x => x.TripSegExtendedFlag);
            IntegerProperty(x => x.TripSegSendReceiptFlag);

            ViewDefaults()
                .Property(x => x.TripNumber)
                .Property(x => x.TripSegNumber)
                .Property(x => x.TripSegStatus)
                .Property(x => x.TripSegStatusDesc)
                .Property(x => x.TripSegType)
                .Property(x => x.TripSegTypeDesc)
                .Property(x => x.TripSegPowerId)
                .Property(x => x.TripSegDriverId)
                .Property(x => x.TripSegDriverName)
                .Property(x => x.TripSegStartDateTime)
                .Property(x => x.TripSegEndDateTime)
                .Property(x => x.TripSegStandardDriveMinutes)
                .Property(x => x.TripSegStandardStopMinutes)
                .Property(x => x.TripSegActualDriveMinutes)
                .Property(x => x.TripSegActualStopMinutes)
                .Property(x => x.TripSegOdometerStart)
                .Property(x => x.TripSegOdometerEnd)
                .Property(x => x.TripSegComments)
                .Property(x => x.TripSegOrigCustType)
                .Property(x => x.TripSegOrigCustTypeDesc)
                .Property(x => x.TripSegOrigCustHostCode)
                .Property(x => x.TripSegOrigCustCode4_4)
                .Property(x => x.TripSegOrigCustName)
                .Property(x => x.TripSegOrigCustAddress1)
                .Property(x => x.TripSegOrigCustAddress2)
                .Property(x => x.TripSegOrigCustCity)
                .Property(x => x.TripSegOrigCustState)
                .Property(x => x.TripSegOrigCustZip)
                .Property(x => x.TripSegOrigCustCountry)
                .Property(x => x.TripSegOrigCustPhone1)
                .Property(x => x.TripSegOrigCustTimeFactor)
                .Property(x => x.TripSegDestCustType)
                .Property(x => x.TripSegDestCustTypeDesc)
                .Property(x => x.TripSegDestCustHostCode)
                .Property(x => x.TripSegDestCustCode4_4)
                .Property(x => x.TripSegDestCustName)
                .Property(x => x.TripSegDestCustAddress1)
                .Property(x => x.TripSegDestCustAddress2)
                .Property(x => x.TripSegDestCustCity)
                .Property(x => x.TripSegDestCustState)
                .Property(x => x.TripSegDestCustZip)
                .Property(x => x.TripSegDestCustCountry)
                .Property(x => x.TripSegDestCustPhone1)
                .Property(x => x.TripSegDestCustTimeFactor)
                .Property(x => x.TripSegPrimaryContainerNumber)
                .Property(x => x.TripSegPrimaryContainerType)
                .Property(x => x.TripSegPrimaryContainerSize)
                .Property(x => x.TripSegPrimaryContainerCommodityCode)
                .Property(x => x.TripSegPrimaryContainerCommodityDesc)
                .Property(x => x.TripSegPrimaryContainerLocation)
                .Property(x => x.TripSegActualDriveStartDateTime)
                .Property(x => x.TripSegActualDriveEndDateTime)
                .Property(x => x.TripSegActualStopStartDateTime)
                .Property(x => x.TripSegActualStopEndDateTime)
                .Property(x => x.TripSegStartLatitude)
                .Property(x => x.TripSegStartLongitude)
                .Property(x => x.TripSegEndLatitude)
                .Property(x => x.TripSegEndLongitude)
                .Property(x => x.TripSegStandardMiles)
                .Property(x => x.TripSegErrorDesc)
                .Property(x => x.TripSegContainerQty)
                .Property(x => x.TripSegDriverGenerated)
                .Property(x => x.TripSegDriverModified)
                .Property(x => x.TripSegPowerAssetNumber)
                .Property(x => x.TripSegExtendedFlag)
                .Property(x => x.TripSegSendReceiptFlag)
                .OrderBy(x => x.TripNumber)
                .OrderBy(x => x.TripSegNumber);

        }
    }
}
