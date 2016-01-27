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
    /// A PowerMaster mapping to NHibernate.  FIXME:  Remove the Id workaround for NHibernate 
    /// </summary>
    public class PowerMasterMap : ClassMapping<PowerMaster>
    {
        public PowerMasterMap()
        {

            Table("PowerMaster");

            // This alone causes NHibernate to also select (non-existant) Id.
            //Property(x => x.PowerId);

            // THis alone causes a HiLo generateion failure
            //Id(x => x.PowerId);

            // Id mapping example from 
            // https://github.com/jagregory/fluent-nhibernate/wiki/Fluent-mapping
            //   Id(x => x.Id)
            //      .Column("PowerId")
            //      .GeneratedBy.Assigned();

            // But this doesn;t work as the Id() is a void 
            //Id(x => x.PowerId).GeneratedBy.Assigned();

            ComposedId(map =>
            {
                map.Property(y => y.PowerId, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.Id, m =>
            {
                m.Formula("PowerId");
                m.Insert(false);
                m.Update(false);
            });

            Property(x => x.PowerType);
            Property(x => x.PowerDesc);
            Property(x => x.PowerSize);
            Property(x => x.PowerLength);
            Property(x => x.PowerTareWeight);
            Property(x => x.PowerCustHostCode);
            Property(x => x.PowerCustType);
            Property(x => x.PowerTerminalId);
            Property(x => x.PowerRegionId);
            Property(x => x.PowerLocation);
            Property(x => x.PowerStatus);
            Property(x => x.PowerDateOutOfService);
            Property(x => x.PowerDateInService);
            Property(x => x.PowerDriverId);
            Property(x => x.PowerOdometer);
            Property(x => x.PowerComments);
            Property(x => x.MdtId);
            Property(x => x.PrimaryContainerType);
            Property(x => x.OrigTerminalId);
            Property(x => x.PowerLastActionDateTime);
            Property(x => x.PowerCurrentTripNumber);
            Property(x => x.PowerCurrentTripSegNumber);
            Property(x => x.PowerCurrentTripSegType);
            Property(x => x.PowerAssetNumber);
            Property(x => x.PowerIdHost);
        }
    }
}