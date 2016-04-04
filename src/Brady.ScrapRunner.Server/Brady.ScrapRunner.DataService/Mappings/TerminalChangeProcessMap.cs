using System;
using Brady.ScrapRunner.Domain.Process;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Brady.ScrapRunner.DataService.Mappings
{
    /// <summary>
    /// A (trivial) TerminalChangeProcess mapping via Employee query to NHibernate.
    /// </summary>
    public class TerminalChangeProcessMap : ClassMapping<TerminalChangeProcess>
    {
        public TerminalChangeProcessMap()
        {

            Table("EmployeeMaster");

            Property(x => x.Id, m =>
            {
                m.Formula("EmployeeId");
                m.Insert(false);
                m.Update(false);
            });

            ComposedId(map =>
            {
                map.Property(y => y.EmployeeId, m => m.Generated(PropertyGeneration.Never));
            });

        }
    }
}
