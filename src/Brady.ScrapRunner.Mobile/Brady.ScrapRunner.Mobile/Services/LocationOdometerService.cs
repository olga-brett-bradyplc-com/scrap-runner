namespace Brady.ScrapRunner.Mobile.Services
{
    using System;
    using Interfaces;
    using Messages;
    using MvvmCross.Plugins.Messenger;

    public class LocationOdometerService : ILocationOdometerService
    {
        private readonly IMvxMessenger _mvxMessenger;
        private MvxSubscriptionToken _mvxSubscriptionToken;
        private int _startingOdometer = 0;
        private uint _miles16; // .01 miles * 16
        private uint _offlineGuess;
        private int _latitude = 0;
        private int _longitude = 0;
        private DateTimeOffset _dateTime;
        private DateTimeOffset _startingDateTime;

        public LocationOdometerService(IMvxMessenger mvxMessenger)
        {
            _mvxMessenger = mvxMessenger;
        }

        public void Start(int startingOdometer)
        {
            _startingOdometer = startingOdometer;
            _mvxSubscriptionToken = _mvxMessenger.Subscribe<LocationModelMessage>(OnLocationModelMessage);
        }

        public void Stop()
        {
            _mvxMessenger.Unsubscribe<LocationModelMessage>(_mvxSubscriptionToken);
        }

        public int? CurrentOdometer { get; }

        private void OnLocationModelMessage(LocationModelMessage obj)
        {
            throw new NotImplementedException();
        }
    }
}
