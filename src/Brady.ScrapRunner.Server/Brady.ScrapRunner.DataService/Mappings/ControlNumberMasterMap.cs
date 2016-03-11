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
 
    /// <summary>
    /// An ControlNumberMaster mapping to NHibernate.  
    /// </summary>
    public class ControlNumberMasterMap : ClassMapping<ControlNumberMaster>
    {
        public ControlNumberMasterMap()
        {

            Table("ControlNumberMaster");

            ComposedId(map =>
            {
                map.Property(y => y.ControlType, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Id, m =>
            {
                m.Formula("ControlType");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.ControlNumberNext);

        }
    }
}
