using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;

namespace Brady.ScrapRunner.Mobile.Services
{
    public class NavigationService
    {
        private readonly ITripService _tripService;
        private readonly IContainerService _containerService;
        private readonly IDriverService _driverService;

        public NavigationService(ITripService tripService, IContainerService containerService, IDriverService driverService)
        {
            _tripService = tripService;
            _containerService = containerService;
            _driverService = driverService;
        }

        public async Task Navigate(TripSegmentModel currentTripSegment)
        {
            if (_tripService.IsTripLegTransaction(currentTripSegment))
            {
            }
        }
    }
}
