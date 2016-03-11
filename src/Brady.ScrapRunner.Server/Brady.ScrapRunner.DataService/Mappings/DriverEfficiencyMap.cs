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
    /// <summary>
    /// An DriverEfficiency mapping to NHibernate.
    /// </summary>
    public class DriverEfficiencyMap : ClassMapping<DriverEfficiency>
    {
        public DriverEfficiencyMap()
        {

            Table("DriverEfficiency");

            ComposedId(map =>
            {
                map.Property(y => y.TripDriverId, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.TripNumber, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Id, m =>
            {
                m.Formula("concat(TripDriverId, ';', TripNumber)");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.TripDriverName);
            Property(x => x.TripType);
            Property(x => x.TripTypeDesc);
            Property(x => x.TripCustHostCode);
            Property(x => x.TripCustName);
            Property(x => x.TripCustAddress1);
            Property(x => x.TripCustAddress2);
            Property(x => x.TripCustCity);
            Property(x => x.TripCustState);
            Property(x => x.TripCustZip);
            Property(x => x.TripCustCountry);
            Property(x => x.TripTerminalId);
            Property(x => x.TripTerminalName);
            Property(x => x.TripRegionId);
            Property(x => x.TripRegionName);
            Property(x => x.TripReferenceNumber);
            Property(x => x.TripCompletedDateTime);
            Property(x => x.TripActualDriveMinutes);
            Property(x => x.TripStandardDriveMinutes);
            Property(x => x.TripActualStopMinutes);
            Property(x => x.TripStandardStopMinutes);
            Property(x => x.TripActualYardMinutes);
            Property(x => x.TripStandardYardMinutes);
            Property(x => x.TripActualTotalMinutes);
            Property(x => x.TripStandardTotalMinutes);
            Property(x => x.TripDelayMinutes);
            Property(x => x.TripPowerId);
            Property(x => x.TripOdometerStart);
            Property(x => x.TripOdometerEnd);
            Property(x => x.TripYardDelayMinutes);
            Property(x => x.TripCustDelayMinutes);
            Property(x => x.TripLunchBreakDelayMinutes);

        }
    }
}
