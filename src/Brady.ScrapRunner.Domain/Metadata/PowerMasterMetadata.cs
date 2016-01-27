using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class PowerMasterMetadata : TypeMetadataProvider<PowerMaster>
    {
        public PowerMasterMetadata()
        {

            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.PowerId)
                .IsId()
                .DisplayName("Power Id");

            StringProperty(x => x.PowerType);
            StringProperty(x => x.PowerDesc);
            StringProperty(x => x.PowerSize);
            IntegerProperty(x => x.PowerLength);
            IntegerProperty(x => x.PowerTareWeight);
            StringProperty(x => x.PowerCustHostCode);
            StringProperty(x => x.PowerCustType);
            StringProperty(x => x.PowerTerminalId);
            StringProperty(x => x.PowerRegionId);
            StringProperty(x => x.PowerLocation);
            StringProperty(x => x.PowerStatus);
            DateProperty(x => x.PowerDateOutOfService);
            DateProperty(x => x.PowerDateInService);
            StringProperty(x => x.PowerDriverId);
            IntegerProperty(x => x.PowerOdometer);
            StringProperty(x => x.PowerComments);
            StringProperty(x => x.MdtId);
            StringProperty(x => x.PrimaryContainerType);
            StringProperty(x => x.OrigTerminalId);
            DateProperty(x => x.PowerLastActionDateTime);
            StringProperty(x => x.PowerCurrentTripNumber);
            StringProperty(x => x.PowerCurrentTripSegNumber);
            StringProperty(x => x.PowerCurrentTripSegType);
            StringProperty(x => x.PowerAssetNumber);
            StringProperty(x => x.PowerIdHost);

            ViewDefaults() 
                .Property(x => x.PowerId)
                .Property(x => x.PowerType)
                .Property(x => x.PowerDesc)
                .Property(x => x.PowerSize)
                .Property(x => x.PowerLength)
                .Property(x => x.PowerTareWeight)
                .Property(x => x.PowerCustHostCode)
                .Property(x => x.PowerCustType)
                .Property(x => x.PowerTerminalId)
                .Property(x => x.PowerRegionId)
                .Property(x => x.PowerLocation)
                .Property(x => x.PowerStatus)
                .Property(x => x.PowerDateOutOfService)
                .Property(x => x.PowerDateInService)
                .Property(x => x.PowerDriverId)
                .Property(x => x.PowerOdometer)
                .Property(x => x.PowerComments)
                .Property(x => x.MdtId)
                .Property(x => x.PrimaryContainerType)
                .Property(x => x.OrigTerminalId)
                .Property(x => x.PowerLastActionDateTime)
                .Property(x => x.PowerCurrentTripNumber)
                .Property(x => x.PowerCurrentTripSegNumber)
                .Property(x => x.PowerCurrentTripSegType)
                .Property(x => x.PowerAssetNumber)
                .Property(x => x.PowerIdHost)
                .OrderBy(x => x.PowerId);
             
        }
    }
}
