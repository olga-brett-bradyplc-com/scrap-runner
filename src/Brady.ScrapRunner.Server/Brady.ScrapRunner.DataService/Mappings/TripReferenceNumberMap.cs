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
    public class TripReferenceNumberMap : ClassMapping<TripReferenceNumber>
    {
        public TripReferenceNumberMap()
        {

            Table("TripReferenceNumbers");

            ComposedId(map =>
            {
                map.Property(y => y.TripNumber, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.TripSeqNumber, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Id, m =>
            {
                m.Formula("concat(TripNumber, ';', TripSeqNumber)");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.TripRefNumberDesc);
            Property(x => x.TripRefNumber);
        }
    }
}