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
    public class ErrorLogMap : ClassMapping<ErrorLog>
    {
        public ErrorLogMap()
        {
            Table("ErrorLog");

            Property(x => x.Id, m =>
            {
                m.Formula("CONCAT(CONVERT(VARCHAR(33), ErrorDateTime, 126), ';', ErrorSeqNo)");
                m.Insert(false);
                m.Update(false);
            });

            ComposedId(map =>
            {
                map.Property(y => y.ErrorDateTime, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.ErrorSeqNo, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.ErrorType);
            Property(x => x.ErrorDescription);
            Property(x => x.ErrorTerminalId);
            Property(x => x.ErrorRegionId);
            Property(x => x.ErrorFlag);
            Property(x => x.ErrorContainerNumber);
            Property(x => x.ErrorTripNumber);
        }
    }
}