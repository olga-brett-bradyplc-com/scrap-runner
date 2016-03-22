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
            Property(x => x.CustCity);
            Property(x => x.CustState);
            Property(x => x.CustZip);
            Property(x => x.CustCountry);
            Property(x => x.CustPhone1);
            Property(x => x.CustContact1);
            Property(x => x.CustOpenTime);
            Property(x => x.CustCloseTime);
            Property(x => x.CustLatitude);
            Property(x => x.CustLongitude);
            Property(x => x.CustRadius);
            Property(x => x.ChgDateTime);
            Property(x => x.ChgActionFlag);
            Property(x => x.CustDriverInstructions);
        }
    }
}