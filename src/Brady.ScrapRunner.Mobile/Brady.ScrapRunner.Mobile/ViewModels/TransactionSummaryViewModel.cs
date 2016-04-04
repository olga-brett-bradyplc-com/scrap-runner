using System;
using System.Collections.Specialized;
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

        public TransactionSummaryViewModel(ITripService tripService)
        {
            _tripService = tripService;
            Title = AppResources.Transactions;
        }

        // Initialize parameter passed from Route Detail Screen
        public void Init(string tripNumber)
        {
            TripNumber = tripNumber;
            SubTitle = TripNumber;
            TransactionScannedCommand = new MvxCommand<string>(ExecuteTransactionScannedCommand);
            ConfirmationSelectedCommand = new MvxCommand(ExecuteConfirmationSelectedCommand);
            TransactionSelectedCommand = new MvxCommand<TripSegmentContainerModel>(ExecuteTransactionSelectedCommand);
        }

        // Grab all relevant data
        public override async void Start()
        {
            FinishLabel = AppResources.FinishLabel;

            var segments = await _tripService.FindNextTripSegmentsAsync(TripNumber);
            Containers = new ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>>();

            foreach (var tsm in segments)
            {
                var containers =
                    await _tripService.FindNextTripSegmentContainersAsync(TripNumber, tsm.TripSegNumber);
                var grouping = new Grouping<TripSegmentModel, TripSegmentContainerModel>(tsm, containers);
                Containers.Add(grouping);
            }

            if (Containers.Any())
            {
                // Set the very first trip segment container as the default current transaction
                CurrentTransaction = Containers.FirstOrDefault().FirstOrDefault();
            }

            base.Start();
        }

        // Listview bindings
        private ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> _containers;
        public ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> Containers
        {
            get { return _containers; }
            set { SetProperty(ref _containers, value); }
        }

        // Command bindings
        public MvxCommand<TripSegmentContainerModel> TransactionSelectedCommand { get; private set; }
        public MvxCommand<string> TransactionScannedCommand { get; private set; }
        public MvxCommand ConfirmationSelectedCommand { get; private set; }

        // Field bindings
        private TripSegmentContainerModel _currentTransaction;
        public TripSegmentContainerModel CurrentTransaction
        {
            get {  return _currentTransaction; }
            set { SetProperty(ref _currentTransaction, value); }
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

        // Command impl
        private void ExecuteTransactionScannedCommand(string scannedNumber)
        {

            // If current transaction tripsegcontainernum == scannedNumber, mark it
            if (CurrentTransaction.TripSegContainerNumber.Equals(scannedNumber))
            {
                CurrentTransaction.TripSegContainerActionDateTime = DateTime.Now;
                return;
            }

            //Check to see if scanned number exists elsewhere in the current set of containers.
            //If so, set it, and move on
            var container =
                    Containers.Select(
                        grouping => grouping.Where(tscm => tscm.TripSegContainerNumber.Equals(scannedNumber))).FirstOrDefault().FirstOrDefault();
            if (container != null)
            {
                var previousTransaction = CurrentTransaction;
                CurrentTransaction = container;
                CurrentTransaction.TripSegContainerActionDateTime = DateTime.Now;

                CurrentTransaction = previousTransaction;
                return;
            }

            // Otherwise, if scanned number isn't found anywhere else, and current transaction.tripsegcontainernum === null
            // Set it, and move on.
            // @TODO : Should we validate that this container is within the container master, etc., etc.?
            if (CurrentTransaction.TripSegContainerNumber == null)
            {
                CurrentTransaction.TripSegContainerNumber = scannedNumber;
                CurrentTransaction.TripSegContainerActionDateTime = DateTime.Now;
            }
        }

        private void ExecuteTransactionSelectedCommand(TripSegmentContainerModel tripContainer)
        {
            ShowViewModel<TransactionDetailViewModel>(
                new
                {
                    tripNumber = tripContainer.TripNumber,
                    tripSegmentNumber = tripContainer.TripSegNumber,
                    tripSegmentContainerNumber = tripContainer.TripSegContainerNumber
                });
        }

        private void ExecuteConfirmationSelectedCommand()
        {
            Close(this);
            ShowViewModel<TransactionConfirmationViewModel>(new {tripNumber = TripNumber});
        }
        
    }
}