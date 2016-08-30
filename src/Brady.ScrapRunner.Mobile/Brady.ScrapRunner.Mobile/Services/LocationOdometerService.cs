namespace Brady.ScrapRunner.Mobile.Services
{
    using System;
    using Domain;
    using Interfaces;
    using Messages;
    using Models;
    using MvvmCross.Platform;
    using MvvmCross.Plugins.Messenger;
    
    public class LocationOdometerService : ILocationOdometerService
    {
        private readonly IMvxMessenger _mvxMessenger;
        private MvxSubscriptionToken _mvxSubscriptionToken;
        private LocationModel _previousLocation;
        private double? _tripOdometer;
        private const double EarthRadiusInMiles = 3960;

        public LocationOdometerService(IMvxMessenger mvxMessenger)
        {
            _mvxMessenger = mvxMessenger;
        }

        public void Start(double startingOdometer)
        {
            if (_mvxSubscriptionToken != null) return;
            _mvxSubscriptionToken = _mvxMessenger.Subscribe<LocationModelMessage>(OnLocationModelMessage);
            _previousLocation = null;
            _tripOdometer = startingOdometer;
            Mvx.TaggedTrace(Constants.ScrapRunner, $"Location odometer service started {startingOdometer}");
        }

        public void Stop()
        {
            if (_mvxSubscriptionToken == null) return;
            _mvxMessenger.Unsubscribe<LocationModelMessage>(_mvxSubscriptionToken);
            Mvx.TaggedTrace(Constants.ScrapRunner, "Location odometer service stopped.");
        }

        public double? CurrentOdometer => _tripOdometer;

        private void OnLocationModelMessage(LocationModelMessage obj)
        {
            if (_previousLocation == null)
            {
                _previousLocation = obj.Location;
                return;
            }
            if (obj.Location.Accuracy.HasValue && obj.Location.Speed > 0.0f)
            {
                var distance = GetDistance(obj.Location, _previousLocation);
                Mvx.TaggedTrace(Constants.ScrapRunner,
                    $"Distance between {obj.Location.Latitude:F4},{obj.Location.Longitude:F4} and {_previousLocation.Latitude:F4},{_previousLocation.Longitude:F4} is {distance:F4}");
                _tripOdometer += distance;
            }
            _previousLocation = obj.Location;
        }

        private static double ConvertToRadians(double input)
        {
            return input * (Math.PI / 180);
        }

        public static double GetDistance(LocationModel location1, LocationModel location2)
        {
            // https://en.wikipedia.org/wiki/Haversine_formula
            var dLat = ConvertToRadians(location2.Latitude - location1.Latitude);
            var dLon = ConvertToRadians(location2.Longitude - location1.Longitude);
            var a = Math.Pow(Math.Sin(dLat / 2), 2) +
                Math.Cos(ConvertToRadians(location1.Latitude)) * Math.Cos(ConvertToRadians(location2.Latitude)) *
                Math.Pow(Math.Sin(dLon / 2), 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = EarthRadiusInMiles * c;
            return distance;
        }
    }
}