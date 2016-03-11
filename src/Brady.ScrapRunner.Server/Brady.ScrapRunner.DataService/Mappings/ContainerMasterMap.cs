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
    public class ContainerMasterMap : ClassMapping<ContainerMaster>
    {
        public ContainerMasterMap()
        {
            Table("ContainerMaster");

            Property(x => x.Id, m =>
            {
                m.Formula("ContainerNumber");
                m.Insert(false);
                m.Update(false);
            });

            ComposedId(map =>
            {
                map.Property(y => y.ContainerNumber, m => m.Generated(PropertyGeneration.Never));
            });

            Property(x => x.ContainerType);
            Property(x => x.ContainerSize);
            Property(x => x.ContainerUnits);
            Property(x => x.ContainerLength);
            Property(x => x.ContainerWeightTare);
            Property(x => x.ContainerShortTerm);
            Property(x => x.ContainerCustHostCode);
            Property(x => x.ContainerCustType);
            Property(x => x.ContainerTerminalId);
            Property(x => x.ContainerRegionId);
            Property(x => x.ContainerLocation);
            Property(x => x.ContainerLastActionDateTime);
            Property(x => x.ContainerPendingMoveDateTime);
            Property(x => x.ContainerCurrentTripNumber);
            Property(x => x.ContainerCurrentTripSegNumber);
            Property(x => x.ContainerCurrentTripSegType);
            Property(x => x.ContainerContents);
            Property(x => x.ContainerStatus);
            Property(x => x.ContainerCommodityCode);
            Property(x => x.ContainerCommodityDesc);
            Property(x => x.ContainerComments);
            Property(x => x.ContainerPowerId);
            Property(x => x.ContainerBarCodeFlag);
            Property(x => x.ContainerAddDateTime);
            Property(x => x.ContainerAddUserId);
            Property(x => x.ContainerRestrictToHostCode);
            Property(x => x.ContainerPrevCustHostCode);
            Property(x => x.ContainerPrevTripNumber);
            Property(x => x.ContainerLevel);
            Property(x => x.ContainerLatitude);
            Property(x => x.ContainerLongitude);
            Property(x => x.ContainerNotes);
            Property(x => x.ContainerCurrentTerminalId);
            Property(x => x.ContainerManufacturer);
            Property(x => x.ContainerCost);
            Property(x => x.ContainerSerialNumber);
            Property(x => x.ContainerOrigin);
            Property(x => x.ContainerPurchaseDate);
            Property(x => x.LocationWarningFlag);
            Property(x => x.ContainerWidth);
            Property(x => x.ContainerHeight);
            Property(x => x.ContainerBarCodeNo);
            Property(x => x.ContainerInboundTerminalId);
            Property(x => x.ContainerQtyInIDFlag);
        }
    }
}