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
    public class PreferencesDefaultMap : ClassMapping<PreferencesDefault>
    {
        public PreferencesDefaultMap()
        {
            Table("PreferencesDefault");

            Property(x => x.Id, m =>
            {
                m.Formula("Parameter");
                m.Insert(false);
                m.Update(false);
            });

            Id(x => x.Parameter, m =>
            {
                m.Generator(Generators.Assigned);
            });

            Property(x => x.ParameterValue);
            Property(x => x.Description);
            Property(x => x.PreferenceType);
            Property(x => x.PreferenceSeqNo);
        }
    }
}