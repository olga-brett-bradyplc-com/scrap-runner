﻿using BWF.DataServices.Metadata.Fluent.Abstract;
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
                  , new CodeTableProcessMetadata()
                  , new CodeTableHdrMetadata()
                  , new CommodityMasterMetadata()
                  , new CommodityMasterProcessMetadata()
                  , new CommodityMasterDestMetadata()
                  , new ContainerChangeMetadata()
                  , new ContainerChangeProcessMetadata()
                  , new ContainerHistoryMetadata()
                  , new ContainerMasterMetadata()
                  , new ContainerQuantityMetadata()
                  , new ContainerQuantityHistoryMetadata()
                  , new ControlNumberMasterMetadata()
                  , new CustomerCommodityMetadata()
                  , new CustomerDirectionsMetadata()
                  , new CustomerLocationMetadata()
                  , new CustomerMasterMetadata()
                  , new DriverDelayMetadata()
                  , new DriverEfficiencyMetadata()
                  , new DriverArriveProcessMetadata()
                  , new DriverContainerActionProcessMetadata()
                  , new DriverEnrouteProcessMetadata()
                  , new DriverHistoryMetadata()
                  , new DriverLoginProcessMetadata()
                  , new DriverMasterMetadata()
                  , new DriverSegmentDoneProcessMetadata()
                  , new DriverStatusMetadata()
                  , new DriverTripAckProcessMetadata()
                  , new EmployeeAreaMetadata()
                  , new EmployeeChangeMetadata()
                  , new EmployeeMasterMetadata()
                  , new ErrorLogMetadata()
                  , new EventLogMetadata()
                  , new HistTripMetadata()
                  , new HistTripReferenceNumberMetadata()
                  , new HistTripSegmentMetadata()
                  , new HistTripSegmentContainerMetadata()
                  , new HistTripSegmentMileageMetadata()
                  , new MessagesMetadata()
                  , new PowerFuelMetadata()
                  , new PowerHistoryMetadata()
                  , new PowerLimitsMetadata()
                  , new PowerMasterMetadata()
                  , new PreferenceMetadata()
                  , new PreferencesProcessMetadata()
                  , new RegionMasterMetadata()
                  , new SecurityMasterMetadata()
                  , new TerminalChangeMetadata()
                  , new TerminalChangeProcessMetadata()
                  , new TerminalMasterMetadata()
                  , new TripInfoProcessMetadata()
                  , new TripMetadata()
                  , new TripPointsMetadata()
                  , new TripReferenceNumberMetadata()
                  , new TripSegmentMetadata()
                  , new TripSegmentContainerMetadata()
                  , new TripSegmentContainerTimeMetadata()
                  , new TripSegmentImageMetadata()
                  , new TripSegmentMileageMetadata()
                  , new TripSegmentTimeMetadata()
                  , new TripTypeBasicMetadata()
                  , new TripTypeBasicDetailsMetadata()
                  , new TripTypeMasterMetadata()
                  , new TripTypeAllowCustTypeMetadata()
                  , new TripTypeMasterDescMetadata()
                  , new TripTypeMasterDetailsMetadata()
           )
        {
            
        }
    }
}