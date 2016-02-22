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
                  , new AreaMasterMetadata() 
                  , new CodeTableMetadata()
                  , new CodeTableHdrMetadata()
                  , new CommodityMasterMetadata()
                  , new ContainerChangeMetadata()
                  , new ContainerHistoryMetadata()
                  , new ContainerMasterMetadata()
                  , new ContainerQuantityMetadata()
                  , new ContainerQuantityHistoryMetadata()
                  , new CustomerCommodityMetadata()
                  , new CustomerDirectionsMetadata()
                  , new CustomerLocationMetadata()
                  , new CustomerMasterMetadata()
                  , new DriverDelayMetadata()
                  , new DriverEfficiencyMetadata()
                  , new DriverHistoryMetadata()
                  , new DriverStatusMetadata()
                  , new EmployeeAreaMetadata()
                  , new EmployeeMasterMetadata()
                  , new ErrorLogMetadata()
                  , new EventLogMetadata()
                  , new PowerFuelMetadata()
                  , new PowerHistoryMetadata()
                  , new PowerMasterMetadata()
                  , new PreferenceMetadata()
                  , new RegionMasterMetadata()
                  , new TerminalChangeMetadata()
                  , new TerminalMasterMetadata()
                  , new TripMetadata()
                  , new TripReferenceNumberMetadata()
                  , new TripSegmentMetadata()
                  , new TripSegmentContainerMetadata()
                  , new TripSegmentMileageMetadata() 
                  , new TripTypeBasicMetadata()
                  , new TripTypeMasterDescMetadata()
            )
        {
            
        }
    }
}