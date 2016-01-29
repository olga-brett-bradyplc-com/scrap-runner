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
    public class TripSegmentMap : ClassMapping<TripSegment>
    {
        public TripSegmentMap()
        {

            Table("TripSegment");

            ComposedId(map =>
            {
                map.Property(y => y.TripNumber, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.TripSegNumber, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Id, m =>
            {
                m.Formula("concat(TripNumber, ';', TripSegNumber)");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.TripSegStatus);
            Property(x => x.TripSegStatusDesc);
            Property(x => x.TripSegType);
            Property(x => x.TripSegTypeDesc);
            Property(x => x.TripSegPowerId);
            Property(x => x.TripSegDriverId);
            Property(x => x.TripSegDriverName);
            Property(x => x.TripSegStartDateTime);
            Property(x => x.TripSegEndDateTime);
            Property(x => x.TripSegStandardDriveMinutes);
            Property(x => x.TripSegStandardStopMinutes);
            Property(x => x.TripSegActualDriveMinutes);
            Property(x => x.TripSegActualStopMinutes);
            Property(x => x.TripSegOdometerStart);
            Property(x => x.TripSegOdometerEnd);
            Property(x => x.TripSegComments);
            Property(x => x.TripSegOrigCustType);
            Property(x => x.TripSegOrigCustTypeDesc);
            Property(x => x.TripSegOrigCustHostCode);
            Property(x => x.TripSegOrigCustCode4_4);
            Property(x => x.TripSegOrigCustName);
            Property(x => x.TripSegOrigCustAddress1);
            Property(x => x.TripSegOrigCustAddress2);
            Property(x => x.TripSegOrigCustCity);
            Property(x => x.TripSegOrigCustState);
            Property(x => x.TripSegOrigCustZip);
            Property(x => x.TripSegOrigCustCountry);
            Property(x => x.TripSegOrigCustPhone1);
            Property(x => x.TripSegOrigCustTimeFactor);
            Property(x => x.TripSegDestCustType);
            Property(x => x.TripSegDestCustTypeDesc);
            Property(x => x.TripSegDestCustHostCode);
            Property(x => x.TripSegDestCustCode4_4);
            Property(x => x.TripSegDestCustName);
            Property(x => x.TripSegDestCustAddress1);
            Property(x => x.TripSegDestCustAddress2);
            Property(x => x.TripSegDestCustCity);
            Property(x => x.TripSegDestCustState);
            Property(x => x.TripSegDestCustZip);
            Property(x => x.TripSegDestCustCountry);
            Property(x => x.TripSegDestCustPhone1);
            Property(x => x.TripSegDestCustTimeFactor);
            Property(x => x.TripSegPrimaryContainerNumber);
            Property(x => x.TripSegPrimaryContainerType);
            Property(x => x.TripSegPrimaryContainerSize);
            Property(x => x.TripSegPrimaryContainerCommodityCode);
            Property(x => x.TripSegPrimaryContainerCommodityDesc);
            Property(x => x.TripSegPrimaryContainerLocation);
            Property(x => x.TripSegActualDriveStartDateTime);
            Property(x => x.TripSegActualDriveEndDateTime);
            Property(x => x.TripSegActualStopStartDateTime);
            Property(x => x.TripSegActualStopEndDateTime);
            Property(x => x.TripSegStartLatitude);
            Property(x => x.TripSegStartLongitude);
            Property(x => x.TripSegEndLatitude);
            Property(x => x.TripSegEndLongitude);
            Property(x => x.TripSegStandardMiles);
            Property(x => x.TripSegErrorDesc);
            Property(x => x.TripSegContainerQty);
            Property(x => x.TripSegDriverGenerated);
            Property(x => x.TripSegDriverModified);
            Property(x => x.TripSegPowerAssetNumber);
            Property(x => x.TripSegExtendedFlag);
            Property(x => x.TripSegSendReceiptFlag);

        }
    }
}