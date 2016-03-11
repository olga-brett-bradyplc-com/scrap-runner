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
    /// An ContainerQuantityHistory mapping to NHibernate.
    /// </summary>
    public class ContainerQuantityHistoryMap : ClassMapping<ContainerQuantityHistory>
    {
        public ContainerQuantityHistoryMap()
        {
            Table("ContainerQuantityHistory");

            ComposedId(map =>
            {
                map.Property(y => y.CustHostCode, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.CustSeqNo, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Id, m =>
            {
                m.Formula("concat(CustHostCode, ';', CustSeqNo)");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.ContainerType);
            Property(x => x.ContainerTypeDesc);
            Property(x => x.ContainerSize);
            Property(x => x.LastActionDateTime);
            Property(x => x.LastTripNumber);
            Property(x => x.LastTripSegNumber);
            Property(x => x.LastTripSegType);
            Property(x => x.LastQuantity);
            Property(x => x.CurrentQuantity);
            Property(x => x.ChangedDateTime);
            Property(x => x.ChangedUserId);
            Property(x => x.ChangedUserName);
            Property(x => x.CustTerminalId);
            Property(x => x.CustTerminalName);
            Property(x => x.CustRegionId);
            Property(x => x.CustRegionName);
            Property(x => x.CustType);
            Property(x => x.CustTypeDesc);
            Property(x => x.CustName);
            Property(x => x.CustAddress1);
            Property(x => x.CustAddress2);
            Property(x => x.CustCity);
            Property(x => x.CustState);
            Property(x => x.CustZip);
            Property(x => x.CustCountry);
            Property(x => x.CustCounty);
            Property(x => x.CustTownship);
            Property(x => x.CustPhone1);

        }
    }
}
