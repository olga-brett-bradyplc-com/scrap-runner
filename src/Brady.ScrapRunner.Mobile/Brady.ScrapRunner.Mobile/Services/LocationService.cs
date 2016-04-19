namespace Brady.ScrapRunner.Mobile.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces;
    using MvvmCross.Platform;
    using MvvmCross.Platform.Core;
    using MvvmCross.Plugins.Location;

    public class LocationService : ILocationService
    {
        private readonly IMvxLocationWatcher _locationWatcher;
        private readonly List<MvxGeoLocation> _locationPath = new List<MvxGeoLocation>();
        private bool _isMoving;

        public LocationService(IMvxLocationWatcher locationWatcher)
        {
            _locationWatcher = locationWatcher;
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

        private void OnLocationChange(MvxGeoLocation location)
        {
            if (_locationPath.Count == 0)
            {
                _locationPath.Add(location);
                return;
            }
            var previousLocation = _locationPath.Last();
            // @TODO: Finish implementing point collecting logic.
            // @TODO: Add support for _isMoving. GPS transmissions to the server will be throttled in this mode.
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
