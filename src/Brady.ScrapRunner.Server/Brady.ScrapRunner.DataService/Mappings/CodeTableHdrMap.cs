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
    public class CodeTableHdrMap : ClassMapping<CodeTableHdr>
    {
        public CodeTableHdrMap()
        {
            Table("CodeTableHdr");

            Property(x => x.Id, m =>
            {
                m.Formula("CodeName");
                m.Insert(false);
                m.Update(false);
            });

            ComposedId(map =>
            {
                map.Property(y => y.CodeName, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.CodeDesc);
            Property(x => x.CodeType);
            Property(x => x.AppliesTo);
        }
    }
}