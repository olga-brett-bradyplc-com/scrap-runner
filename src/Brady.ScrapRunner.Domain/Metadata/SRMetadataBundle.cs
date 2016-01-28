using BWF.DataServices.Metadata.Fluent.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Domain.Metadata
{
    public class SRMetadataBundle : TypeMetadataBundle
    {
        public SRMetadataBundle()
            : base(Constants.ScrapRunner
                  , new CodeTableMetadata()
                  , new CodeTableHdrMetadata()
                  , new CustomerMasterMetadata()
                  , new DriverDelayMetadata()
                  , new DriverHistoryMetadata()
                  , new DriverStatusMetadata()
                  , new EmployeeMasterMetadata()
                  , new PowerMasterMetadata()
                  , new PreferenceMetadata()
                  , new RegionMasterMetadata()
                  , new TerminalMasterMetadata()
                  , new TripMetadata()
                  , new TripSegmentMetadata()
                  , new TripSegmentMileageMetadata()
            )
        {
            
        }
    }
}