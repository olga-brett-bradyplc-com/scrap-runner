using Brady.ScrapRunner.Domain.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Cfg.MappingSchema;

namespace Brady.ScrapRunner.DataService.Mappings
{
    public class GPSLocationMap : ClassMapping<GPSLocation>
    {
        public GPSLocationMap()
        {
            Table("GPSLocation");

            // Mapping suggested by
            // http://stackoverflow.com/questions/20925197/using-nhibernate-to-insert-a-new-object-to-sql-server-gives-error-about-identity
            // http://stackoverflow.com/questions/7279473/using-nhibernate-mapping-by-code-cannot-insert-explicit-value-for-identity-colu

            Id(x => x.GPSSeqId, m =>
            {
                m.UnsavedValue(0);
                m.Generator(Generators.Identity);
            });

            Property(x => x.Id, m =>
            {
                m.Formula("GPSSeqId");
                m.Insert(false);
                m.Update(false);
                m.Generated(PropertyGeneration.Never);
            });

            Property(x => x.EmployeeId);
            Property(x => x.TerminalId);
            Property(x => x.RegionId);
            Property(x => x.GPSDateTime);
            Property(x => x.GPSID);
            Property(x => x.GPSLatitude);
            Property(x => x.GPSLongitude);
            Property(x => x.GPSSpeed);
            Property(x => x.GPSHeading);
            Property(x => x.GPSSendFlag);
        }
    }
}