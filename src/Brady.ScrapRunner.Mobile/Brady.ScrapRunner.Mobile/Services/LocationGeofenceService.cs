﻿namespace Brady.ScrapRunner.Mobile.Services
{
    using System;
    using Domain;
    using Interfaces;
    using Messages;
    using Models;
    using MvvmCross.Platform;
    using MvvmCross.Plugins.Messenger;

    public class LocationGeofenceService : ILocationGeofenceService
    {
        private const short TriggerDistance = 12;  // hundredths of a mile away for trigger
        private const short TriggerSpeed = 5;   // must be below this speed (mph)
        private const short TriggerSeconds = 60;  // for this many seconds
        private const int TriggerDepart = 8;   // 8 mph triggers depart

        private int _triggerFlag;
        private GeofenceContext _geofenceContext;
        private MvxSubscriptionToken _mvxSubscriptionToken;

        private readonly IMvxMessenger _mvxMessenger;

        public LocationGeofenceService(IMvxMessenger mvxMessenger)
        {
            _mvxMessenger = mvxMessenger;
        }

        public void Start()
        {
            if (_mvxSubscriptionToken == null)
            {
                _mvxSubscriptionToken = _mvxMessenger.Subscribe<LocationModelMessage>(OnLocationModelMessage);
            }
        }

        public void Stop()
        {
            if (_mvxSubscriptionToken == null) return;
            _mvxSubscriptionToken.Dispose();
            _mvxSubscriptionToken = null;
        }

        public void StartAutoArrive(string key, int synergyLatitude, int synergyLongitude, short radius)
        {
            if ((_geofenceContext.State == GeofenceState.EnRoute) &&
                ((_geofenceContext.Latitude != synergyLatitude) || (_geofenceContext.Longitude != synergyLongitude)))
            {
                Mvx.TaggedTrace(Constants.ScrapRunner, string.Format("Geofence changed from {0} {1} {2} to {3} {4}.",
                    _geofenceContext.Latitude, _geofenceContext.Longitude, _geofenceContext.TriggerDistance, synergyLatitude, synergyLongitude));
            }
            _geofenceContext = new GeofenceContext
            {
                Id = key,
                State = GeofenceState.EnRoute,
                Latitude = synergyLatitude,
                Longitude = synergyLongitude,
                TriggerDistance = radius + TriggerDistance,
                Distance = radius
            };
            _triggerFlag = 0;
        }

        public void StartAutoDepart(string key)
        {
            _geofenceContext = new GeofenceContext
            {
                Id = key,
                State = GeofenceState.Arrive,
                Latitude = 0, // synergyLatitude ?
                Longitude = 0, // synergyLongitude ?
                ArriveLatitude = 0, // synergyLatitude ?
                ArriveLongitude = 0, // synergyLongitude ?
                TriggerDistance = 0,  // dist + trigger_distance
                Distance = 0 // dist
            };
        }

        private void OnLocationModelMessage(LocationModelMessage obj)
        {
            switch (_geofenceContext.State)
            {
                case GeofenceState.Unknown:
                    break;
                case GeofenceState.Arrive:
                    CheckForDepart(obj.Location);
                    break;
                case GeofenceState.Depart:
                    break;
                case GeofenceState.EnRoute:
                    CheckForArrive(obj.Location);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CheckForArrive(LocationModel currentLocation)
        {
            var close = true;
            var synergyLatitude = Convert.ToInt32(currentLocation.Latitude*600000);
            var synergyLongitude = Convert.ToInt32(currentLocation.Longitude*600000);
            var distanceLatitude = synergyLatitude - _geofenceContext.Latitude;
            if (distanceLatitude < 0) distanceLatitude = -distanceLatitude;
            if (distanceLatitude > _geofenceContext.TriggerDistance) close = false;
            var distanceLongitude = synergyLongitude - _geofenceContext.Longitude;
            if (distanceLongitude < 0) distanceLongitude = -distanceLongitude;
            if (distanceLongitude > _geofenceContext.TriggerDistance) close = false;
            _geofenceContext.DistanceFrom = distanceLatitude + distanceLongitude;

            if (currentLocation.Speed > TriggerSpeed)
            {
                _triggerFlag = 0;
            }
            else
            {
                if (close)
                {
                    if (_triggerFlag != 0)
                    {
                        if (!((currentLocation.Timestamp - _geofenceContext.Arrive).TotalSeconds > TriggerSeconds)) return;
                        _geofenceContext.State = GeofenceState.Arrive;
                        _geofenceContext.ArriveLatitude = synergyLatitude;
                        _geofenceContext.ArriveLongitude = synergyLongitude;
                        Mvx.TaggedTrace(Constants.ScrapRunner, $"Arrived in {_geofenceContext.Id} geofence.");
                        _mvxMessenger.Publish(new GeofenceArriveMessage(this));
                    }
                    else
                    {
                        _triggerFlag = 1;
                        _geofenceContext.Arrive = currentLocation.Timestamp;
                        Mvx.TaggedTrace(Constants.ScrapRunner, $"Geofence {_geofenceContext.Id} detected. Waiting {TriggerSeconds} seconds before arriving...");
                    }
                }
                else
                {
                    _triggerFlag = 0;
                }
            }
        }

        private void CheckForDepart(LocationModel currentLocation)
        {
            var synergyLatitude = Convert.ToInt32(currentLocation.Latitude*600000);
            var synergyLongitude = Convert.ToInt32(currentLocation.Longitude*600000);
            var distanceLatitude = synergyLatitude - _geofenceContext.ArriveLatitude;
            if (distanceLatitude < 0) distanceLatitude = -distanceLatitude;
            var distanceLongitude = synergyLongitude - _geofenceContext.ArriveLongitude;
            if (distanceLongitude < 0) distanceLongitude = -distanceLongitude;

            if (currentLocation.Speed > TriggerDepart && ((_geofenceContext.Distance == -1) || (distanceLatitude + distanceLongitude > TriggerDistance)))
            {
                _geofenceContext.State = GeofenceState.Depart;
                _geofenceContext.Depart = currentLocation.Timestamp;
                Mvx.TaggedTrace(Constants.ScrapRunner, $"Departed geofence {_geofenceContext.Id}.");
                _mvxMessenger.Publish(new GeofenceDepartMessage(this));
            }
        }
    }

    enum GeofenceState
    {
        Unknown,
        Arrive,
        Depart,
        EnRoute
    }

    class GeofenceContext
    {
        public string Id { get; set; }
        public int Latitude { get; set; }
        public int Longitude { get; set; }
        public int DistanceFrom { get; set; }
        public int MinimumDistanceFrom { get; set; }
        public int TriggerDistance { get; set; }

        public GeofenceState State { get; set; }
        public DateTimeOffset Arrive { get; set; }
        public DateTimeOffset Depart { get; set; }
        public int Distance { get; set; }
        public int ArriveLatitude { get; set; }
        public int ArriveLongitude { get; set; }
    }
}