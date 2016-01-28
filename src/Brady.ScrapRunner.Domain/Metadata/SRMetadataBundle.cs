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
                  , new PreferenceMetadata()
                  , new EmployeeMasterMetadata()
                  , new PowerMasterMetadata()
                  , new DriverStatusMetadata()
                  , new TripSegmentMetadata()
                  , new TripMetadata()
                  , new TerminalMasterMetadata()
                  , new DriverDelayMetadata()
                  , new DriverHistoryMetadata()
                  , new TripSegmentMileageMetadata()
            )
        {
            
        }
    }
}