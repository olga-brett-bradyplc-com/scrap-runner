using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Models;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Brady.ScrapRunner.DataService.Mappings
{
    /// <summary>
    /// An EmployeePreferences mapping to NHibernate.
    /// </summary>
    public class EmployeePreferencesMap : ClassMapping<EmployeePreferences>
    {
        public EmployeePreferencesMap()
        {
            Table("EmployeePreferences");

            ComposedId(map =>
            {
                map.Property(y => y.RegionId, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.TerminalId, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.EmployeeId, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.Parameter, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Id, m =>
            {
                m.Formula("concat(RegionId, ';', TerminalId, ';', EmployeeId, ';', Parameter)");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.ParameterValue);
            Property(x => x.Description);
            Property(x => x.PreferenceType);
            Property(x => x.PreferenceSeqNo);
        }
    }
}
