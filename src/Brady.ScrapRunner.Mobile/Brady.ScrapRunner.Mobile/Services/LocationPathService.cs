namespace Brady.ScrapRunner.Mobile.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Domain;
    using Interfaces;
    using Messages;
    using Models;
    using MvvmCross.Platform;
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
        private readonly double[] _averageSpeeds = { 0.0f, 0.0f, 0.0f, 0.0f };
        private double _averageSpeed = 0.0f;
        private double _maxSpeed = 0.0f;
        private MovementState _movementState = MovementState.Stationary;
        private DateTimeOffset _movementStateDateTimeOffset;
        private const double AccelerationSpeedLimit = 2.0f;
        private const double DecelerationSpeedLimit = 4.0f;
        private const int AccelerationDurationSeconds = 3;
        private const int DecelerationDurationSeconds = 3;

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
            _movementState = MovementState.Stationary;
            _movementStateDateTimeOffset = DateTimeOffset.Now;
            Mvx.TaggedTrace(Constants.ScrapRunner, "Location path service started");
        }

        public void Stop()
        {
            if (!_started) return;
            _mvxMessenger.Unsubscribe<LocationModelMessage>(_locationToken);
            _locationToken = null;
            _started = false;
            Mvx.TaggedTrace(Constants.ScrapRunner, "Location path service stopped");
        }

        public async void OnLocationModelMessage(LocationModelMessage locationMessage)
        {
            if (!locationMessage.Location.Accuracy.HasValue)
            {
                Mvx.TaggedWarning(Constants.ScrapRunner, "Location accuracy is invalid.");
                return;
            }
            UpdateSpeedAverage(locationMessage.Location.Speed);
            UpdateMovementState(locationMessage.Location);
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

        private TimeSpan TimeSinceAccelerationChange()
        {
            return DateTimeOffset.Now - _movementStateDateTimeOffset;
        }

        private void UpdateMovementState(LocationModel locationModel)
        {
            var speed = locationModel.Speed.GetValueOrDefault(0.0f);
            var now = DateTimeOffset.Now;
            switch (_movementState)
            {
                case MovementState.Stationary:
                    if (speed > AccelerationSpeedLimit)
                    {
                        Mvx.TaggedTrace(Constants.ScrapRunner, "Acceleration detected.");
                        _movementStateDateTimeOffset = now;
                        _movementState = MovementState.Accelerating;
                    }
                    break;
                case MovementState.Accelerating:
                    if (speed < AccelerationSpeedLimit)
                    {
                        Mvx.TaggedTrace(Constants.ScrapRunner, "Acceleration aborted.");
                        _movementStateDateTimeOffset = now;
                        _movementState = MovementState.Stationary;
                        return;
                    }
                    if (TimeSinceAccelerationChange().TotalSeconds > AccelerationDurationSeconds)
                    {
                        Mvx.TaggedTrace(Constants.ScrapRunner, "Movement detected.");
                        _movementStateDateTimeOffset = now;
                        _movementState = MovementState.Moving;
                    }
                    break;
                case MovementState.Moving:
                    if (speed < DecelerationSpeedLimit)
                    {
                        Mvx.TaggedTrace(Constants.ScrapRunner, "Deceleration detected.");
                        _movementStateDateTimeOffset = now;
                        _movementState = MovementState.Decelerating;
                    }
                    break;
                case MovementState.Decelerating:
                    if (speed > DecelerationSpeedLimit)
                    {
                        Mvx.TaggedTrace(Constants.ScrapRunner, "Deceleration aborted.");
                        _movementStateDateTimeOffset = now;
                        _movementState = MovementState.Moving;
                        return;
                    }
                    if (TimeSinceAccelerationChange().TotalSeconds > DecelerationDurationSeconds)
                    {
                        Mvx.TaggedTrace(Constants.ScrapRunner, "Stationary detected.");
                        _movementStateDateTimeOffset = now;
                        _movementState = MovementState.Stationary;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        enum MovementState
        {
            Stationary,
            Accelerating,
            Moving,
            Decelerating
        }

        private void UpdateSpeedAverage(double? speed)
        {
            if (!speed.HasValue || speed == 0)
            {
                _averageSpeeds[0] = 0.0f;
                _averageSpeeds[1] = 0.0f;
                _averageSpeeds[2] = 0.0f;
                _averageSpeeds[3] = 0.0f;
                _averageSpeed = 0.0f;
            }
            else
            {
                _averageSpeeds[3] = _averageSpeeds[2];
                _averageSpeeds[2] = _averageSpeeds[1];
                _averageSpeeds[1] = _averageSpeeds[0];
                _averageSpeeds[0] = speed.Value;
                _averageSpeed = (_averageSpeeds[0] 
                    + _averageSpeeds[1] 
                    + _averageSpeeds[2] 
                    + _averageSpeeds[3]) / 4;
            }
            if (_averageSpeed > _maxSpeed)
                _maxSpeed = _averageSpeed;
        }

        private void AddPath(LocationModel location)
        {
            var newLocation = new LocationModel
            {
                Accuracy = location.Accuracy,
                Heading = location.Heading,
                HeadingAccuracy = location.HeadingAccuracy,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Speed = _maxSpeed,
                Timestamp = location.Timestamp
            };
            _locationPath.Add(newLocation);
            Mvx.TaggedTrace(Constants.ScrapRunner, $"Add point {_locationPath.Count} to location path.");
            _nextPointAdded = location.Timestamp.AddMinutes(AddPointMinutes);
            _maxSpeed = 0.0f;
        }

        private void ClearPath()
        {
            _locationPath.Clear();
            _nextPointAdded = null;
            _nextPathTransmit = null;
            Mvx.TaggedTrace(Constants.ScrapRunner, "Location path cleared.");
        }

        private Task SendPathAsync()
        {
            // @TODO: Implement using _connectionService.
            Mvx.TaggedTrace(Constants.ScrapRunner, $"Sent {_locationPath.Count} points in location path to server.");
            return Task.FromResult(default(object));
        }
    }
}