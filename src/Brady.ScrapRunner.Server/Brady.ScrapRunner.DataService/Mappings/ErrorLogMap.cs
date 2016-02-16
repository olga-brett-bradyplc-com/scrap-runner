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

            // Mapping suggested by:
            // http://stackoverflow.com/questions/20925197/using-nhibernate-to-insert-a-new-object-to-sql-server-gives-error-about-identity
            // http://stackoverflow.com/questions/7279473/using-nhibernate-mapping-by-code-cannot-insert-explicit-value-for-identity-colu

            Id(x => x.ErrorId, m =>
            {
                m.UnsavedValue(0);
                m.Generator(Generators.Identity);
            });

            Property(x => x.Id, m =>
            {
                m.Formula("ErrorId");
                m.Insert(false);
                m.Update(false);
                m.Generated(PropertyGeneration.Never);
            });

            Property(x => x.ErrorDateTime);
            Property(x => x.ErrorSeqNo);
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