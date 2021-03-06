﻿using Brady.ScrapRunner.Domain.Models;
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
    /// An CustomerDirections mapping to NHibernate.
    /// </summary>
    public class CustomerDirectionsMap : ClassMapping<CustomerDirections>
    {
        public CustomerDirectionsMap()
        {
            Table("CustomerDirections");

            ComposedId(map =>
            {
                map.Property(y => y.CustHostCode, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.DirectionsSeqNo, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Id, m =>
            {
                m.Formula("concat(CustHostCode, ';', DirectionsSeqNo)");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.DirectionsDesc);

        }

    }
}
