namespace Brady.ScrapRunner.Mobile.Services
{
    using System;
    using Interfaces;
    using MvvmCross.Platform;
    using MvvmCross.Platform.Core;
    using MvvmCross.Plugins.Location;

    public class LocationService : ILocationService
    {
        private readonly IMvxLocationWatcher _locationWatcher;

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
