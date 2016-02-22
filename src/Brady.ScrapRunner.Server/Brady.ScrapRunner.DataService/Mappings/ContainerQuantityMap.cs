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
    /// An ContainerQuantity mapping to NHibernate.
    /// </summary>
    public class ContainerQuantityMap : ClassMapping<ContainerQuantity>
    {
        public ContainerQuantityMap()
        {
            Table("ContainerQuantity");

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

            Property(x => x.CustTerminalId);
            Property(x => x.ContainerType);
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
            Property(x => x.RemoveFromList);

        }

    }
}
