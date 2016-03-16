using Brady.ScrapRunner.Domain.Models;
using BWF.DataServices.Metadata.Fluent.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Metadata
{

    public class PowerLimitsMetadata : TypeMetadataProvider<PowerLimits>
    {
        public PowerLimitsMetadata()
        {

            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.PowerId)
                .IsId()
                .DisplayName("Power Id");

            StringProperty(x => x.ContainerType)
                .IsId()
                .DisplayName("Container Type");

            IntegerProperty(x => x.PowerSeqNumber)
                .IsId()
                .DisplayName("Power Seq Number");

            StringProperty(x => x.ContainerMinSize);
            StringProperty(x => x.ContainerMaxSize);

            ViewDefaults()
                .Property(x => x.PowerId)
                .Property(x => x.ContainerType)
                .Property(x => x.PowerSeqNumber)
                .Property(x => x.ContainerMinSize)
                .Property(x => x.ContainerMaxSize)

                .OrderBy(x => x.PowerId)
                .OrderBy(x => x.ContainerType)
                .OrderBy(x => x.PowerSeqNumber);
        }
    }
}
