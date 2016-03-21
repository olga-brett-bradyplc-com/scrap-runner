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
    public class ContainerChangeMap : ClassMapping<ContainerChange>
    {
        public ContainerChangeMap()
        {
            Table("ContainerChange");

            Id(x => x.ContainerNumber, m =>
            {
                m.Generator(Generators.Assigned);
            });

            Property(x => x.Id, m =>
            {
                m.Formula("ContainerNumber");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.ContainerType);
            Property(x => x.ContainerSize);
            Property(x => x.ActionDate);
            Property(x => x.ActionFlag);
            Property(x => x.TerminalId);
            Property(x => x.RegionId);
            Property(x => x.ContainerBarCodeNo);
        }
    }
}