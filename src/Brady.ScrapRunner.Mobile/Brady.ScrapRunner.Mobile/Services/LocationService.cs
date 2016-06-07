namespace Brady.ScrapRunner.Mobile.Services
{
    using System;
    using Interfaces;
    using Messages;
    using Models;
    using MvvmCross.Platform;
    using MvvmCross.Platform.Core;
    using MvvmCross.Plugins.Location;
    using MvvmCross.Plugins.Messenger;

    public class LocationService : ILocationService
    {
        private readonly IMvxLocationWatcher _locationWatcher;
        private readonly IMvxMessenger _mvxMessenger;

        public LocationService(IMvxLocationWatcher locationWatcher, IMvxMessenger mvxMessenger)
        {
            _locationWatcher = locationWatcher;
            _mvxMessenger = mvxMessenger;
            _locationWatcher.OnPermissionChanged += OnPermissionChange;
        }

        public void Start()
        {
            if (_locationWatcher.Started) return;
            var options = new MvxLocationOptions
            {
                Accuracy = MvxLocationAccuracy.Fine,
                TimeBetweenUpdates = new TimeSpan(0, 0, 0, 5),
                TrackingMode = MvxLocationTrackingMode.Background
            };

            Mvx.Resolve<IMvxMainThreadDispatcher>().RequestMainThreadAction(() =>
            {
                _locationWatcher.Start(options, OnLocationChange, OnLocationError);
            });
        }

        public void Stop()
        {
            if (!_locationWatcher.Started) return;
            _locationWatcher.Stop();
        }

        public LocationModel CurrentLocation => _locationWatcher.CurrentLocation == null ? 
                null : 
                ConvertMvxGeoLocation(_locationWatcher.CurrentLocation);

        private LocationModel ConvertMvxGeoLocation(MvxGeoLocation location)
        {
            return new LocationModel
            {
                Accuracy = location.Coordinates.Accuracy,
                Heading = location.Coordinates.Heading,
                HeadingAccuracy = location.Coordinates.HeadingAccuracy,
                Latitude = location.Coordinates.Latitude,
                Longitude = location.Coordinates.Longitude,
                Speed = location.Coordinates.Speed,
                Timestamp = location.Timestamp
            };
        }

        private void OnLocationChange(MvxGeoLocation location)
        {
            var locationModel = ConvertMvxGeoLocation(location);
            _mvxMessenger.Publish(new LocationModelMessage(this)
            {
                Location = locationModel
            });
        }

        private void OnLocationError(MvxLocationError locationError)
        {
            Mvx.Error($"Location error {locationError.Code}.");
        }

        private void OnPermissionChange(object sender, MvxValueEventArgs<MvxLocationPermission> mvxValueEventArgs)
        {
            Mvx.Warning($"Location permission {mvxValueEventArgs.Value}.");
        }
    }
}
