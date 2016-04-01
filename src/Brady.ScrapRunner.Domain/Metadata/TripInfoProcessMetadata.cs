using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain.Process;
using BWF.DataServices.Metadata.Fluent.Abstract;

namespace Brady.ScrapRunner.Domain.Metadata
{
   public class TripInfoProcessMetadata : TypeMetadataProvider<TripInfoProcess>
    {
        public TripInfoProcessMetadata()
        {

            AutoUpdatesByDefault();

            StringProperty(x => x.Id)
                .IsHiddenInEditor()
                .IsNotEditableInGrid();

            StringProperty(x => x.EmployeeId)
                .IsId()
                .DisplayName("Employee Id");

            StringProperty(x => x.TripNumber);

            ViewDefaults()
                .Property(x => x.EmployeeId)
                .Property(x => x.TripNumber);
 
        }
    }
}
