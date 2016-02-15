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
    public class PreferenceMap : ClassMapping<Preference>
    {
        public PreferenceMap()
        {
            Table("Preferences");

            Property(x => x.Id, m =>
            {
                m.Formula("concat(Parameter, ';', TerminalId)");
                m.Insert(false);
                m.Update(false);
            });

            ComposedId(map =>
            {
                map.Property(y => y.Parameter, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.TerminalId, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Description);
            Property(x => x.ParameterValue);
        }
    }
}