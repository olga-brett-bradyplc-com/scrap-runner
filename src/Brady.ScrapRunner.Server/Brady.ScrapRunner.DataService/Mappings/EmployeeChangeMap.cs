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
    /// An EmployeeChange mapping to NHibernate.  
    /// </summary>
    public class EmployeeChangeMap : ClassMapping<EmployeeChange>
    {
        public EmployeeChangeMap()
        {

            Table("EmployeeChange");

            ComposedId(map =>
            {
                map.Property(y => y.ActionFlag, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.EmployeeId, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.LoginId, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.Password, m => m.Generated(PropertyGeneration.Never));
                map.Property(y => y.RegionId, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Id, m =>
            {
                m.Formula("concat(ActionFlag, ';', EmployeeId, ';', LoginId, ';', Password, ';', RegionId)");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.ChangeDateTime);
 
        }
    }
}
