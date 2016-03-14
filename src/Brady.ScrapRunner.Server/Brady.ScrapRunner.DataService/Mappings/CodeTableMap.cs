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
    public class CodeTableMap : ClassMapping<CodeTable>
    {
        public CodeTableMap()
        {
            Table("CodeTable");

            Property(x => x.Id, m =>
            {
                m.Formula("concat(CodeName, ';', CodeValue)");
                m.Insert(false);
                m.Update(false);
            });

            ComposedId(map =>
            {
                map.Property(y => y.CodeName, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.CodeValue, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.CodeSeq);
            Property(x => x.CodeDisp1);
            Property(x => x.CodeDisp2);
            Property(x => x.CodeDisp3);
            Property(x => x.CodeDisp4);
            Property(x => x.CodeDisp5);
            Property(x => x.CodeDisp6);
        }
    }
}