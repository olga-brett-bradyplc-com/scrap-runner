using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class CommodityMasterProcessMetadata : TypeMetadataProvider<CommodityMasterProcess>
    {
        public CommodityMasterProcessMetadata()
        {

            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.EmployeeId)
                .IsId()
                .DisplayName("Employee Id");

            StringProperty(x => x.CommodityCode);
            StringProperty(x => x.CommodityDesc);

            ViewDefaults()
            .Property(x => x.EmployeeId)
            .Property(x => x.CommodityCode)
            .Property(x => x.CommodityDesc)
            .OrderBy(x => x.EmployeeId);
        }
    }
}
