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
    public class TerminalChangeMap : ClassMapping<TerminalChange>
    {
        public TerminalChangeMap()
        {
            Table("TerminalChange");

            Property(x => x.Id, m =>
            {
                m.Formula("TerminalId");
                m.Insert(false);
                m.Update(false);
            });

            Id(x => x.TerminalId, m =>
            {
                m.Generator(Generators.Assigned);
            });

            Property(x => x.RegionId);
            Property(x => x.CustType);
            Property(x => x.CustHostCode);
            Property(x => x.CustCode4_4);
            Property(x => x.CustName);
            Property(x => x.CustAddress1);
            Property(x => x.CustAddress2);
        }
    }
}