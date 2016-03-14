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

            // The way to do composed IDs
            //ComposedId(map =>
            //{
            //    map.Property(y => y.CodeName, 
            //      m => m.Generated(PropertyGeneration.Never));
            //});
  
            // A (presumably) more direct mapping of assigned string ID
            Id(x => x.CodeName, m =>
            {
                m.Generator(Generators.Assigned);
            });
 
            Property(x => x.Id, m =>
            {
                m.Formula("CodeName");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.CodeDesc);
            Property(x => x.CodeType);
            Property(x => x.AppliesTo);
        }
    }
}