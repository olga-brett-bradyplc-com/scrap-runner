namespace Brady.ScrapRunner.Mobile.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Interfaces;
    using Messages;
    using Models;
    using MvvmCross.Plugins.Messenger;

    public class LocationPathService : ILocationPathService
    {
        private readonly IMvxMessenger _mvxMessenger;
        private readonly IConnectionService _connectionService;
        private readonly List<LocationModel> _locationPath = new List<LocationModel>();
        private const int AddPointMinutes = 1;
        private const double AddPointDegrees = 30.0;
        private const int SendPathMinutes = 10;
        private MvxSubscriptionToken _locationToken;
        private DateTimeOffset? _nextPathTransmit;
        private DateTimeOffset? _nextPointAdded;
        private bool _started;

        public LocationPathService(IMvxMessenger mvxMessenger, IConnectionService connectionService)
        {
            _mvxMessenger = mvxMessenger;
            _connectionService = connectionService;
        }

        public void Start()
        {
            if (_started) return;
            _locationToken = _mvxMessenger.Subscribe<LocationModelMessage>(OnLocationModelMessage);
            _started = true;
        }

        public void Stop()
        {
            if (!_started) return;
            _mvxMessenger.Unsubscribe<LocationModelMessage>(_locationToken);
            _locationToken = null;
            _started = false;
        }

        public async void OnLocationModelMessage(LocationModelMessage locationMessage)
        {
            if (_locationPath.Count == 0)
            {
                _nextPathTransmit = locationMessage.Location.Timestamp.AddMinutes(SendPathMinutes);
                AddPath(locationMessage.Location);
                return;
            }
            var lastLocation = _locationPath.Last();
            if (_nextPointAdded.HasValue && locationMessage.Location.Timestamp > _nextPointAdded)
            {
                AddPath(locationMessage.Location);
                if (_nextPathTransmit.HasValue && locationMessage.Location.Timestamp >= _nextPathTransmit)
                {
                    await SendPathAsync();
                    ClearPath();
                }
            }
            if (lastLocation.Heading.HasValue && lastLocation.Heading > 0.0 &&
                locationMessage.Location.Heading.HasValue && locationMessage.Location.Heading > 0.0)
            {
                if (Math.Abs(locationMessage.Location.Heading.Value - lastLocation.Heading.Value) >= AddPointDegrees)
                {
                    AddPath(locationMessage.Location);
                }
            }
            if (_nextPathTransmit.HasValue && locationMessage.Location.Timestamp >= _nextPathTransmit)
            {
                await SendPathAsync();
                ClearPath();
            }
        }

        private void AddPath(LocationModel location)
        {
            _locationPath.Add(location);
            _nextPointAdded = location.Timestamp.AddMinutes(AddPointMinutes);
        }

        private void ClearPath()
        {
            _locationPath.Clear();
            _nextPointAdded = null;
            _nextPathTransmit = null;
        }

        private Task SendPathAsync()
        {
            // @TODO: Implement using _connectionService.
            return Task.FromResult(default(object));
        }
    }
}
