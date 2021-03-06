﻿using Brady.ScrapRunner.Mobile.Resources;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Collections.ObjectModel;
    using Domain;
    using Interfaces;
    using Models;
    using MvvmCross.Core.ViewModels;

    public class RouteSummaryViewModel : BaseViewModel
    {
        private readonly ITripService _tripService;

        public RouteSummaryViewModel(ITripService tripService)
        {
            _tripService = tripService;
            Title = AppResources.RouteSummary;
        }

        public override async void Start()
        {
            var trips = await _tripService.FindTripsAsync();
            RouteSummaryList = new ObservableCollection<TripModel>(trips);

            base.Start();
        }

        private ObservableCollection<TripModel> _routeSummaryList;
        public ObservableCollection<TripModel> RouteSummaryList
        {
            get { return _routeSummaryList; }
            set { SetProperty(ref _routeSummaryList, value); }
        }

        private TripModel _selectedTrip;
        public TripModel SelectedTrip
        {
            get { return _selectedTrip; }
            set { SetProperty(ref _selectedTrip, value); }
        }


        private IMvxCommand _routeSelectedCommand;
        public IMvxCommand RouteSelectedCommand => _routeSelectedCommand ?? (_routeSelectedCommand = new MvxCommand<TripModel>(ExecuteRouteSelectedCommand));

        public void ExecuteRouteSelectedCommand(TripModel selectedTrip)
        {
            ShowViewModel<RouteDetailViewModel>(new {tripNumber = selectedTrip.TripNumber});
        }
    }
}