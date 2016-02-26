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
    public class SecurityMasterMap : ClassMapping<SecurityMaster>
    {
        public SecurityMasterMap()
        {

            Table("SecurityMaster");

            ComposedId(map =>
            {
                map.Property(y => y.SecurityFunction, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.SecurityLevel, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.SecurityProgram, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.SecurityType, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Id, m =>
            {
                m.Formula("concat(SecurityFunction, ';', SecurityLevel, ';', SecurityProgram, ';', SecurityType)");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.SecurityDescription1);
            Property(x => x.SecurityDescription2);
            Property(x => x.SecurityDescription3);
            Property(x => x.SecurityDescription4);
            Property(x => x.SecurityAccess);
        }
    }
}
