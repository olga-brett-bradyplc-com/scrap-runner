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
    /// A ContainerHistory mapping to NHibernate.  
    /// </summary>
    public class ContainerHistoryMap : ClassMapping<ContainerHistory>
    {
        public ContainerHistoryMap()
        {
            Table("ContainerHistory");

            Property(x => x.Id, m =>
            {
                m.Formula("concat(ContainerNumber, ';', ContainerSeqNumber)");
                m.Insert(false);
                m.Update(false);
            });

            ComposedId(map =>
            {
                map.Property(y => y.ContainerNumber, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.ContainerSeqNumber, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.ContainerType);
            Property(x => x.ContainerSize);
            Property(x => x.ContainerUnits);
            Property(x => x.ContainerLength);
            Property(x => x.ContainerCustHostCode);
            Property(x => x.ContainerCustType);
            Property(x => x.ContainerCustTypeDesc);
            Property(x => x.ContainerTerminalId);
            Property(x => x.ContainerTerminalName);
            Property(x => x.ContainerRegionId);
            Property(x => x.ContainerRegionName);
            Property(x => x.ContainerLocation);
            Property(x => x.ContainerLastActionDateTime);
            Property(x => x.ContainerDaysAtSite);
            Property(x => x.ContainerPendingMoveDateTime);
            Property(x => x.ContainerTripNumber);
            Property(x => x.ContainerTripSegNumber);
            Property(x => x.ContainerTripSegType);
            Property(x => x.ContainerTripSegTypeDesc);
            Property(x => x.ContainerContents);
            Property(x => x.ContainerContentsDesc);
            Property(x => x.ContainerStatus);
            Property(x => x.ContainerStatusDesc);
            Property(x => x.ContainerCommodityCode);
            Property(x => x.ContainerCommodityDesc);
            Property(x => x.ContainerComments);
            Property(x => x.ContainerPowerId);
            Property(x => x.ContainerShortTerm);
            Property(x => x.ContainerCustName);
            Property(x => x.ContainerCustAddress1);
            Property(x => x.ContainerCustAddress2);
            Property(x => x.ContainerCustCity);
            Property(x => x.ContainerCustState);
            Property(x => x.ContainerCustZip);
            Property(x => x.ContainerCustCountry);
            Property(x => x.ContainerCustCounty);
            Property(x => x.ContainerCustTownship);
            Property(x => x.ContainerCustPhone1);
            Property(x => x.ContainerLevel);
            Property(x => x.ContainerLatitude);
            Property(x => x.ContainerLongitude);
            Property(x => x.ContainerNotes);
            Property(x => x.ContainerCurrentTerminalId);
            Property(x => x.ContainerCurrentTerminalName);
            Property(x => x.ContainerWidth);
            Property(x => x.ContainerHeight);
        }
    }
}
