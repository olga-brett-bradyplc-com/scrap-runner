using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Mobile.Enums;
using BWF.DataServices.Metadata.Models;
using MvvmCross.Binding.ExtensionMethods;
using Splat;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using Helpers;
    using Models;
    using MvvmCross.Core.ViewModels;
    using Brady.ScrapRunner.Mobile.Interfaces;
    using Brady.ScrapRunner.Mobile.Resources;

    public class TransactionSummaryViewModel : BaseViewModel
    {
        private readonly ITripService _tripService;
        private readonly IPreferenceService _preferenceService;

        public TransactionSummaryViewModel(ITripService tripService, IPreferenceService preferenceService)
        {
            _tripService = tripService;
            _preferenceService = preferenceService;
            Title = AppResources.Transactions;
        }

        public void Init(string tripNumber, string methodOfEntry)
        {
            TripNumber = tripNumber;
            MethodOfEntry = methodOfEntry;
            SubTitle = $"{AppResources.Trip} {TripNumber}";
            //SelectNextTransactionCommand = new MvxCommand(ExecuteSelectNextTransactionCommand);
        }

        public override async void Start()
        {
            FinishLabel = AppResources.FinishLabel;

            var segments = await _tripService.FindNextTripSegmentsAsync(TripNumber);
            Containers = new ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>>();

            /*
                A trip leg can contain, for example, trip segments of DE, PF and SC. We don't want to
                process the SC containers on this screen, but navigate them to the scale screens after
                they're done with the DE/PF containers
            */
            foreach (var tsm in segments.Where(s => !_tripService.IsTripLegScale(s)))
            {
                var containers =
                    await _tripService.FindNextTripSegmentContainersAsync(TripNumber, tsm.TripSegNumber);
                var grouping = new Grouping<TripSegmentModel, TripSegmentContainerModel>(tsm, containers);
                Containers.Add(grouping);
            }

            // Set the very first trip segment container as the default current transaction
            if (Containers.Any())
                CurrentTransaction = Containers.FirstOrDefault().FirstOrDefault();

            // Is the user allowed to add a rt/rn segment?
            // rt/rn must not exist as last segment
            var tripSegments = await _tripService.FindAllSegmentsForTripAsync(TripNumber);
            var doesRtnSegExistAtEnd = tripSegments.All(sg => sg.TripSegType != BasicTripTypeConstants.ReturnYard && sg.TripSegType != BasicTripTypeConstants.ReturnYardNC);
            // User preference DEFAllowAddRT must be set to 'Y'
            var defAllowAddRt = await _preferenceService.FindPreferenceValueAsync(PrefDriverConstants.DEFAllowAddRT);

            AllowRtnAdd = doesRtnSegExistAtEnd && Constants.Yes.Equals(defAllowAddRt);

            MenuFilter = MenuFilterEnum.OnTrip;

            base.Start();
        }

        private ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> _containers;
        public ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> Containers
        {
            get { return _containers; }
            set { SetProperty(ref _containers, value); }
        }

        private TripSegmentContainerModel _currentTransaction;
        public TripSegmentContainerModel CurrentTransaction
        {
            get {  return _currentTransaction; }
            set
            {
                SetProperty(ref _currentTransaction, value);
                ConfirmationSelectedCommand.RaiseCanExecuteChanged();
            }
        }

        private bool? _allowRtnAdd;
        public bool? AllowRtnAdd
        {
            get { return _allowRtnAdd; }
            set { SetProperty(ref _allowRtnAdd, value); }
        }

        private string _tripNumber;
        public string TripNumber
        {
            get { return _tripNumber; }
            set { SetProperty(ref _tripNumber, value); }
        }

        private string _cameraViewLabel;
        public string CameraViewLabel
        {
            get { return _cameraViewLabel; }
            set { SetProperty(ref _cameraViewLabel, value); }
        }

        private string _finishLabel;
        public string FinishLabel
        {
            get { return _finishLabel; }
            set { SetProperty(ref _finishLabel, value); }
        }

        private string _methodOfEntry;
        public string MethodOfEntry
        {
            get { return _methodOfEntry; }
            set { SetProperty(ref _methodOfEntry, value); }
        }

        private IMvxCommand _transactionSelectedCommand;
        public IMvxCommand TransactionSelectedCommand => _transactionSelectedCommand ?? (_transactionSelectedCommand = new MvxCommand<TripSegmentContainerModel>(ExecuteTransactionSelectedCommand));

        private IMvxAsyncCommand _transactionScannedCommandAsync;
        public IMvxAsyncCommand TransactionScannedCommandAsync => _transactionScannedCommandAsync ?? (_transactionScannedCommandAsync = new MvxAsyncCommand<string>(ExecuteTransactionScannedCommandAsync));
        //public IMvxCommand SelectNextTransactionCommand { get; private set; }

        private IMvxCommand _confirmationSelectedCommand;
        public IMvxCommand ConfirmationSelectedCommand => _confirmationSelectedCommand ?? (_confirmationSelectedCommand = new MvxCommand(ExecuteConfirmationSelectedCommand, CanExecuteConfirmationSelectedCommand));

        private IMvxCommand _addRtnYardCommand;
        public IMvxCommand AddRtnYardCommand => _addRtnYardCommand ?? (_addRtnYardCommand = new MvxCommand(ExecuteAddRtnYardCommand));

        private void ExecuteAddRtnYardCommand()
        {
            ShowViewModel<ModifyReturnToYardViewModel>(
                new { changeType = TerminalChangeEnum.Add, tripNumber = TripNumber });
        }

        private async Task ExecuteTransactionScannedCommandAsync(string scannedNumber)
        {
            foreach (var currentTripSeg in Containers.Select(container2 => container2.FirstOrDefault(tscm => tscm.TripSegContainerNumber == scannedNumber && string.IsNullOrEmpty(tscm.TripSegContainerComplete))))
                CurrentTransaction = currentTripSeg ?? CurrentTransaction;

            if (string.IsNullOrEmpty(CurrentTransaction.TripSegContainerNumber))
                CurrentTransaction.TripSegContainerNumber = scannedNumber;

            //await
            //    _tripService.ProcessTripSegmentContainerAsync(CurrentTransaction.TripNumber,
            //        CurrentTransaction.TripSegNumber, CurrentTransaction.TripSegContainerSeqNumber,
            //        CurrentTransaction.TripSegContainerNumber, true);

            // Update local copy of container list
            var container = await _tripService.FindTripSegmentContainer(CurrentTransaction.TripNumber,
                CurrentTransaction.TripSegNumber, CurrentTransaction.TripSegContainerSeqNumber);

            MethodOfEntry = TripMethodOfCompletionConstants.Scanned;

            UpdateLocalContainers(container);
        }

        private void ExecuteSelectNextTransactionCommand()
        {
            // Find next trip segment container that hasn't been completed
            foreach (var currentTripSeg in Containers.Select(container2 => container2.FirstOrDefault(tscm => string.IsNullOrEmpty(tscm.TripSegContainerComplete))))
            {
                CurrentTransaction = currentTripSeg;
                break;
            }
        }

        private void ExecuteTransactionSelectedCommand(TripSegmentContainerModel tripContainer)
        {
            ShowViewModel<TransactionDetailViewModel>(
                new
                {
                    tripNumber = tripContainer.TripNumber,
                    tripSegmentNumber = tripContainer.TripSegNumber,
                    tripSegmentSeqNo = tripContainer.TripSegContainerSeqNumber,
                    methodOfEntry = TripMethodOfCompletionConstants.Manual
                });
        }

        private void ExecuteConfirmationSelectedCommand()
        {
            Close(this);
            ShowViewModel<TransactionConfirmationViewModel>(new {tripNumber = TripNumber});
        }

        private bool CanExecuteConfirmationSelectedCommand()
        {
            return Containers.All(container => !container.All(tscm => string.IsNullOrEmpty(tscm.TripSegContainerComplete)));
        }

        private void UpdateLocalContainers(TripSegmentContainerModel tripContainer)
        {
            var cotainerGroupingPos =
                Containers.IndexOf(
                    Containers.First(ts => ts.Key.TripSegNumber == CurrentTransaction.TripSegNumber));

            var containerListPos =
                Containers[cotainerGroupingPos].IndexOf(Containers[cotainerGroupingPos].First(
                    tscm => tscm.TripSegContainerSeqNumber == CurrentTransaction.TripSegContainerSeqNumber));

            Containers[cotainerGroupingPos][containerListPos] = tripContainer;
        }
    }
}