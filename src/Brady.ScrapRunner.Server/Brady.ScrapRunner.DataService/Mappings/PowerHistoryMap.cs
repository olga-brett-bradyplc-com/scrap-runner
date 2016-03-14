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
    /// A PowerHistory mapping to NHibernate.  
    /// </summary>
    public class PowerHistoryMap : ClassMapping<PowerHistory>
    {
        public PowerHistoryMap()
        {
            Table("PowerHistory");

            ComposedId(map =>
            {
                map.Property(y => y.PowerId, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.PowerSeqNumber, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Id, m =>
            {
                m.Formula("concat(PowerId, ';', PowerSeqNumber)");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.PowerType);
            Property(x => x.PowerDesc);
            Property(x => x.PowerSize);
            Property(x => x.PowerLength);
            Property(x => x.PowerTareWeight);
            Property(x => x.PowerCustType);
            Property(x => x.PowerCustTypeDesc);
            Property(x => x.PowerTerminalId);
            Property(x => x.PowerTerminalName);
            Property(x => x.PowerRegionId);
            Property(x => x.PowerRegionName);
            Property(x => x.PowerLocation);
            Property(x => x.PowerStatus);
            Property(x => x.PowerDateOutOfService);
            Property(x => x.PowerDateInService);
            Property(x => x.PowerDriverId);
            Property(x => x.PowerDriverName);
            Property(x => x.PowerOdometer);
            Property(x => x.PowerComments);
            Property(x => x.MdtId);
            Property(x => x.PrimaryPowerType);
            Property(x => x.PowerCustHostCode);
            Property(x => x.PowerCustName);
            Property(x => x.PowerCustAddress1);
            Property(x => x.PowerCustAddress2);
            Property(x => x.PowerCustCity);
            Property(x => x.PowerCustState);
            Property(x => x.PowerCustZip);
            Property(x => x.PowerCustCountry);
            Property(x => x.PowerCustCounty);
            Property(x => x.PowerCustTownship);
            Property(x => x.PowerCustPhone1);
            Property(x => x.PowerLastActionDateTime);
            Property(x => x.PowerStatusDesc);
            Property(x => x.PowerCurrentTripNumber);
            Property(x => x.PowerCurrentTripSegNumber);
            Property(x => x.PowerCurrentTripSegType);
            Property(x => x.PowerCurrentTripSegTypeDesc);
        }
    }
}