﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brady.ScrapRunner.Mobile.Helpers;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using Acr.UserDialogs;
    using Interfaces;
    using Models;
    using MvvmCross.Core.ViewModels;
    using Resources;

    // This is still a work in progress
    public class RouteDetailViewModel : BaseViewModel
    {
        private readonly ITripService _tripService;
        private string _custHostCode;

        public RouteDetailViewModel(ITripService tripService)
        {
            _tripService = tripService;
            DirectionsCommand = new MvxCommand(ExecuteDrivingDirectionsCommand);
            EnRouteCommand = new MvxCommand(ExecuteEnRouteCommand);
            ArriveCommand = new MvxCommand(ExecuteArriveCommand);
            NextStageCommand = new MvxCommand(ExecuteNextStageCommand);
        }

        public void Init(string tripNumber)
        {
            TripNumber = tripNumber;
        }

        public override async void Start()
        {
            var trip = await _tripService.FindTripAsync(TripNumber);

            if (trip != null)
            {
                using (var tripDataLoad = UserDialogs.Instance.Loading("Loading Trip Data", maskType: MaskType.Clear))
                {
                    var segments = await _tripService.FindNextTripSegmentsAsync(TripNumber);
                    var list = new ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>>();

                    foreach (var tsm in segments)
                    {
                        var containers =
                            await _tripService.FindNextTripSegmentContainersAsync(TripNumber, tsm.TripSegNumber);
                        var grouping = new Grouping<TripSegmentModel, TripSegmentContainerModel>(tsm, containers);
                        list.Add(grouping);
                    }

                    _custHostCode = trip.TripCustHostCode;
                    Title = trip.TripTypeDesc;
                    SubTitle = $"Trip {trip.TripNumber}";
                    TripType = trip.TripType;
                    TripFor = trip.TripCustName;

                    if (list.Any())
                    {
                        Containers = list;
                        TripCustName = list.First().Key.TripSegDestCustName;
                        TripDriverInstructions = trip.TripDriverInstructions;
                        TripCustAddress = list.First().Key.TripSegDestCustAddress1 +
                                          list.First().Key.TripSegDestCustAddress2;
                        TripCustCityStateZip = $"{list.First().Key.TripSegDestCustCity}, {list.First().Key.TripSegDestCustState} {list.First().Key.TripSegDestCustZip}";
                    }
                }
            }

            base.Start();
        }

        // Field bindings
        private string _tripNumber;
        public string TripNumber
        {
            get { return _tripNumber; }
            set { SetProperty(ref _tripNumber, value); }
        }

        private string _tripType;
        public string TripType
        {
            get { return _tripType; }
            set { SetProperty(ref _tripType, value); }
        }

        private string _tripFor;
        public string TripFor
        {
            get { return _tripFor; }
            set { SetProperty(ref _tripFor, value); }
        }

        private string _tripDriverInstructions;
        public string TripDriverInstructions
        {
            get { return _tripDriverInstructions; }
            set { SetProperty(ref _tripDriverInstructions, value); }
        }

        private string _tripCustName;
        public string TripCustName
        {
            get { return _tripCustName; }
            set { SetProperty(ref _tripCustName, value); }
        }

        private string _tripCustAddress;
        public string TripCustAddress
        {
            get { return _tripCustAddress; }
            set { SetProperty(ref _tripCustAddress, value); }
        }

        private string _tripCustCityStateZip;
        public string TripCustCityStateZip
        {
            get { return _tripCustCityStateZip; }
            set { SetProperty(ref _tripCustCityStateZip, value); }
        }

        private string _currentStatus;
        public string CurrentStatus
        {
            get { return _currentStatus; }
            set { SetProperty(ref _currentStatus, value); }
        }

        private ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> _containers; 
        public ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> Containers
        {
            get { return _containers; }
            set { SetProperty(ref _containers, value); }
        }

        // Command bindings
        public MvxCommand DirectionsCommand { get; private set; }
        public MvxCommand EnRouteCommand { get; private set; }
        public MvxCommand ArriveCommand { get; private set; }
        public MvxCommand NextStageCommand { get; private set; }

        // Command impl
        private void ExecuteDrivingDirectionsCommand()
        {
            if (!string.IsNullOrEmpty(_custHostCode))
                ShowViewModel<RouteDirectionsViewModel>(new {custHostCode = _custHostCode});
        }

        private async void ExecuteEnRouteCommand()
        {
            var message = string.Format(AppResources.ConfirmEnRouteMessage,
                "\n\n",
                "\n",
                TripCustName,
                TripCustAddress,
                TripCustCityStateZip);
            var confirm = await UserDialogs.Instance.ConfirmAsync(message, AppResources.ConfirmEnrouteTitle);
            if (confirm)
            {
                CurrentStatus = "EN"; //DriverStatusConstants.EnRoute;
            }
        }

        private async void ExecuteArriveCommand()
        {
            var message = string.Format(AppResources.ConfirmArrivalMessage,
                "\n\n",
                "\n",
                TripCustName,
                TripCustAddress,
                TripCustCityStateZip);
            var confirm = await UserDialogs.Instance.ConfirmAsync(message, AppResources.ConfirmArrivalTitle);
            if (confirm)
            {
                CurrentStatus = "AR"; //DriverStatusConstants.Arrive;
            }
        }

        private void ExecuteNextStageCommand()
        {
            if (Containers.First().Key.TripSegType.Equals("DE"))
            {
                Close(this);
                ShowViewModel<TransactionSummaryViewModel>(new { tripNumber = TripNumber });
            }
            else if (Containers.First().Key.TripSegType.Equals("RT"))
            {
                Close(this);
                ShowViewModel<ScaleSummaryViewModel>(new { tripNumber = TripNumber });
            }
        }
    }
}