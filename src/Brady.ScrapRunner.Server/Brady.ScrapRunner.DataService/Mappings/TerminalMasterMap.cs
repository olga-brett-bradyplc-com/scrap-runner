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
    public class TerminalMasterMap : ClassMapping<TerminalMaster>
    {
        public TerminalMasterMap()
        {
                        
            Table("TerminalMaster");

            Id(x => x.TerminalId, m =>
            {
                m.Generator(Generators.Assigned);
            });

            Property(x => x.Id, m =>
            {
                m.Formula("TerminalId");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.Region);
            Property(x => x.TerminalName);
            Property(x => x.Address1);
            Property(x => x.Address2);
            Property(x => x.City);
            Property(x => x.State);
            Property(x => x.Zip);
            Property(x => x.Country);
            Property(x => x.Phone);
            Property(x => x.EtakLatitude);
            Property(x => x.EtakLongitude);
            Property(x => x.Latitude);
            Property(x => x.Longitude);
            Property(x => x.DispatchZone);
            Property(x => x.Geocoded);
            Property(x => x.RegionIndex);
            Property(x => x.TerminalIdNumber);
            Property(x => x.MasterTerminal);
            Property(x => x.ChgDateTime);
            Property(x => x.ChgEmployeeId);
            Property(x => x.TerminalType);
            Property(x => x.TimeZoneFactor);
            Property(x => x.DaylightSavings);
            Property(x => x.TerminalIdHost);
            Property(x => x.FileNameHost);
        }
    }
}
