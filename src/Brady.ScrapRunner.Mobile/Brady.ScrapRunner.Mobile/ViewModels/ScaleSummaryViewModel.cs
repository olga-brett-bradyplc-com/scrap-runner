using System.Collections.Generic;
﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Helpers;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Messages;
using Brady.ScrapRunner.Mobile.Resources;
using MvvmCross.Binding.Combiners;
using MvvmCross.Plugins.Messenger;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Collections.ObjectModel;
    using Models;
    using MvvmCross.Core.ViewModels;

    public class ScaleSummaryViewModel : BaseViewModel
    {
        private readonly ITripService _tripService;
        private readonly IContainerService _containerService;
        private readonly IDriverService _driverService;
        private readonly ICustomerService _customerService;
        private readonly ICodeTableService _codeTableService;
        private MvxSubscriptionToken _mvxSubscriptionToken;
        private readonly IMvxMessenger _mvxMessenger;

        public ScaleSummaryViewModel(ITripService tripService, 
            IContainerService containerService, 
            IDriverService driverService, 
            ICustomerService customerService,
            ICodeTableService codeTableService,
            IMvxMessenger mvxMessenger)
        {
            _tripService = tripService;
            _containerService = containerService;
            _driverService = driverService;
            _customerService = customerService;
            _codeTableService = codeTableService;
            _mvxMessenger = mvxMessenger;
            _mvxSubscriptionToken = mvxMessenger.SubscribeOnMainThread<TripNotificationMessage>(OnTripNotification);

            ContainerSelectedCommand = new MvxCommand<ContainerMasterWithTripContainer>(ExecuteContainerSelectedCommand);
        }
        private List<CodeTableModel> _contTypeList;
        public List<CodeTableModel> ContTypesList
        {
            get { return _contTypeList; }
            set { SetProperty(ref _contTypeList, value); }
        }
        public void Init(string tripNumber)
        {
            TripNumber = tripNumber;
        }
        private void OnTripNotification(TripNotificationMessage msg)
        {
            switch (msg.Context)
            {
                case TripNotificationContext.Canceled:
                case TripNotificationContext.Reassigned:
                case TripNotificationContext.MarkedDone:
                    if (msg.Trip.TripNumber == TripNumber)
                        ShowViewModel<RouteSummaryViewModel>();
                    break;
                case TripNotificationContext.New:
                    break;
                case TripNotificationContext.Modified:
                    LoadData();
                    break;
                case TripNotificationContext.OnHold:
                    break;
                case TripNotificationContext.Future:
                    break;
                case TripNotificationContext.Unassigned:
                    break;
                case TripNotificationContext.Resequenced:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async void LoadData()
        {
            ContTypesList = await _codeTableService.FindCodeTableList(CodeTableNameConstants.ContainerType);

            using (var tripDataLoad = UserDialogs.Instance.Loading(AppResources.LoadingTripData, maskType: MaskType.Clear))
            {
                CurrentDriver = await _driverService.GetCurrentDriverStatusAsync();
                var containersOnPowerId = await _containerService.FindPowerIdContainersAsync(CurrentDriver.PowerId);
                var containersToBeProcessed = containersOnPowerId.Where(c => c.ContainerComplete != Constants.Yes).ToList();
                var powerlist = new ObservableCollection<Grouping<ContainerGroupKey, ContainerMasterWithTripContainer>>();

                var currentTripSegment = await _tripService.FindNextTripSegmentsAsync(TripNumber);
                TripSegNumber = currentTripSegment.FirstOrDefault().TripSegNumber;

                foreach (var container in containersToBeProcessed.Where(container => container.ContainerCurrentTripNumber != null && container.ContainerCustHostCode != null))
                {
                    var contType = ContTypesList.FirstOrDefault(ct => ct.CodeValue == container.ContainerType);
                    container.ContainerTypeDesc = contType != null
                        ? contType.CodeDisp1?.TrimEnd()
                        : container.ContainerType;

                    var tripSegments = await _tripService.FindAllContainersForTripSegmentAsync(container.ContainerCurrentTripNumber, container.ContainerCurrentTripSegNumber);

                    // Existing container grouping
                    if (powerlist.Any(grp => grp.Key.Id == $"{container.ContainerCustHostCode};{container.ContainerCurrentTripNumber}"))
                    {
                        powerlist.SingleOrDefault(grp => grp.Key.Id == $"{container.ContainerCustHostCode};{container.ContainerCurrentTripNumber}").Add(new ContainerMasterWithTripContainer
                        {
                            ContainerMaster = container,
                            TripSegmentContainer = tripSegments.FirstOrDefault(ts => ts.TripSegNumber == container.ContainerCurrentTripSegNumber) // this won't work correctly with tripsegmentcontainers that have the same barcode
                        });

                        continue;
                    }

                    var tempCustomerName = await _customerService.FindCustomerMaster(container.ContainerCustHostCode);

                    // Create new container grouping
                    var tempContainerKey = new ContainerGroupKey
                    {
                        TripNumber = container.ContainerCurrentTripNumber,
                        CustHostCode = container.ContainerCustHostCode,
                        Name = tempCustomerName.CustName
                    };

                    var tempContainerMasterTripSeg = new List<ContainerMasterWithTripContainer>
                    {
                        new ContainerMasterWithTripContainer
                        {
                            ContainerMaster = container,
                            TripSegmentContainer = tripSegments.FirstOrDefault(ts => ts.TripSegNumber == container.ContainerCurrentTripSegNumber) // this won't work correctly with tripsegmentcontainers that have the same barcode
                        }
                    };

                    powerlist.Add(new Grouping<ContainerGroupKey, ContainerMasterWithTripContainer>(tempContainerKey, tempContainerMasterTripSeg));
                }

                // Append "Unused" container grouping to end of power list if any unused containers exist
                foreach (var container in containersToBeProcessed.Where(container => container.ContainerCurrentTripNumber == null || container.ContainerCustHostCode == null))
                {
                    var contType = ContTypesList.FirstOrDefault(ct => ct.CodeValue == container.ContainerType);
                    container.ContainerTypeDesc = contType != null
                        ? contType.CodeDisp1?.TrimEnd()
                        : container.ContainerType;

                    // If unused grouping doesn't already exist, create it and add container to list, then continue to next iterator
                    if (powerlist.Any(grp => grp.Key.CustHostCode == null && grp.Key.TripNumber == null))
                    {
                        powerlist.SingleOrDefault(grp => grp.Key.CustHostCode == null && grp.Key.TripNumber == null).Add(new ContainerMasterWithTripContainer
                        {
                            ContainerMaster = container,
                            TripSegmentContainer = null
                        });

                        continue;
                    }

                    var unusedKey = new ContainerGroupKey
                    {
                        CustHostCode = null,
                        TripNumber = null,
                        Name = "Unused"
                    };

                    var tempContainerList = new List<ContainerMasterWithTripContainer>
                    {
                        new ContainerMasterWithTripContainer
                        {
                            ContainerMaster = container,
                            TripSegmentContainer = null
                        }
                    };

                    powerlist.Add(new Grouping<ContainerGroupKey, ContainerMasterWithTripContainer>(unusedKey, tempContainerList));
                }

                ContainersOnPowerId = powerlist;
            }

        }
        public override void Start()
        {
            Title = AppResources.YardScaleSummary;
            SubTitle = AppResources.Trip + $" {TripNumber}";

            LoadData();

            base.Start();
        }
        private string _containerTypeDesc;

        public string ContainerTypeDesc
        {
            get { return _containerTypeDesc; }
            set { SetProperty(ref _containerTypeDesc, value); }
        }

        // Listview bindings
        private ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> _containers;
        public ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> Containers
        {
            get { return _containers; }
            set { SetProperty(ref _containers, value); }
        }

        private ObservableCollection<Grouping<ContainerGroupKey, ContainerMasterWithTripContainer>> _containersOnPowerId;
        public ObservableCollection<Grouping<ContainerGroupKey, ContainerMasterWithTripContainer>> ContainersOnPowerId
        {
            get { return _containersOnPowerId; }
            set { SetProperty(ref _containersOnPowerId, value); }
        }

        private async Task ExecuteFinishSegmentCommand()
        {
            var confirm = await UserDialogs.Instance.ConfirmAsync(AppResources.CompleteTrip, AppResources.ConfirmLabel,
                AppResources.OK, AppResources.Cancel);

            if (confirm)
            {
                using ( var completeTripSegment = UserDialogs.Instance.Loading(AppResources.CompletingTripSegment, maskType: MaskType.Black))
                {
                    foreach (var container in ContainersOnPowerId.SelectMany(grouping => grouping))
                    {
                        if (container.TripSegmentContainer == null)
                        {
                            var unusedContainerAction =
                                await _tripService.ProcessContainerActionAsync(new DriverContainerActionProcess
                                {
                                    EmployeeId = CurrentDriver.EmployeeId,
                                    PowerId = CurrentDriver.PowerId,
                                    ActionType = ContainerActionTypeConstants.Dropped,
                                    ActionDateTime = DateTime.Now,
                                    ContainerNumber = container.ContainerMaster.ContainerNumber
                                });

                            if (!unusedContainerAction.WasSuccessful)
                            {
                                UserDialogs.Instance.Alert(unusedContainerAction.Failure.Summary, AppResources.Error);
                                return;
                            }
                        }
                        else
                        {
                             var containerAction = await _tripService.ProcessContainerActionAsync(new DriverContainerActionProcess
                                {
                                    EmployeeId = CurrentDriver.EmployeeId,
                                    PowerId = CurrentDriver.PowerId,
                                    ActionType = (container.TripSegmentContainer.TripSegContainerReviewFlag == ContainerActionTypeConstants.Review) ? ContainerActionTypeConstants.Review : ContainerActionTypeConstants.Done,
                                    ActionDateTime = DateTime.Now,
                                    TripNumber = TripNumber,
                                    TripSegNumber = container.TripSegmentContainer.TripSegNumber,
                                    ContainerNumber = container.TripSegmentContainer.TripSegContainerNumber,
                                    Gross1ActionDateTime = container.TripSegmentContainer.WeightGrossDateTime,
                                    TareActionDateTime = container.TripSegmentContainer.WeightTareDateTime,
                                    Gross2ActionDateTime = container.TripSegmentContainer.WeightGross2ndDateTime,
                                    SetInYardFlag = container.ContainerMaster.ContainerToBeUnloaded,
                                    ContainerContents = container.ContainerMaster.ContainerContents,
                                    MethodOfEntry = ContainerMethodOfEntry.Manual,
                                    ActionCode = container.TripSegmentContainer.TripSegContainerReviewReason,
                                    ActionDesc = container.TripSegmentContainer.TripSegContainerReivewReasonDesc,
                                    Gross1Weight = container.TripSegmentContainer.TripSegContainerWeightGross.GetValueOrDefault(),
                                    Gross2Weight = container.TripSegmentContainer.TripSegContainerWeightGross2nd.GetValueOrDefault(),
                                    TareWeight = container.TripSegmentContainer.TripSegContainerWeightTare.GetValueOrDefault()
                                });

                            if (!containerAction.WasSuccessful)
                            {
                                UserDialogs.Instance.Alert(containerAction.Failure.Summary, AppResources.Error);
                                return;
                            }
                        }

                        await _containerService.ResetContainer(container.ContainerMaster);
                    }
                    
                    var tripSegment = await _tripService.FindTripSegmentInfoAsync(TripNumber, TripSegNumber);

                    var reviewContainers = ContainersOnPowerId.SelectMany(grouping => grouping).Any(ts => ts.TripSegmentContainer.TripSegContainerReviewFlag == ContainerActionTypeConstants.Review);

                    var tripSegmentProcess = await _tripService.ProcessTripSegmentDoneAsync(new DriverSegmentDoneProcess
                    {
                        EmployeeId = CurrentDriver.EmployeeId,
                        TripNumber = TripNumber,
                        TripSegNumber = TripSegNumber,
                        ActionDateTime = DateTime.Now,
                        PowerId = CurrentDriver.PowerId,
                        ActionType = TripSegStatusConstants.Done,
                        Latitude = tripSegment.TripSegEndLatitude,
                        Longitude = tripSegment.TripSegEndLongitude
                    });

                    if (tripSegmentProcess.WasSuccessful)
                        await _tripService.CompleteTripSegmentAsync(tripSegment);
                    else
                    {
                        UserDialogs.Instance.Alert(tripSegmentProcess.Failure.Summary, AppResources.Error);
                        return;
                    }

                    await _tripService.CompleteTripAsync(TripNumber);

                    var nextTrip = await _tripService.FindNextTripAsync();
                    var seg = await _tripService.FindNextTripSegmentsAsync(nextTrip?.TripNumber);

                    CurrentDriver.Status = nextTrip == null ? DriverStatusSRConstants.NoWork : DriverStatusSRConstants.Available;
                    CurrentDriver.TripNumber = nextTrip == null ? "" : nextTrip.TripNumber;
                    CurrentDriver.TripSegNumber = seg.Count < 1 ? "" : seg.FirstOrDefault().TripSegNumber;

                    await _driverService.UpdateDriver(CurrentDriver);

                    Close(this);
                    ShowViewModel<RouteSummaryViewModel>();
                }
            }
        }

        private bool CanExecuteFinishSegmentCommand()
        {
            return
                ContainersOnPowerId.SelectMany(grouping => grouping)
                    .All(container => !string.IsNullOrEmpty(container.ContainerMaster?.ContainerComplete));
        }

        // Command bindings
        public MvxCommand<ContainerMasterWithTripContainer> ContainerSelectedCommand { get; private set; }

        // Field bindings
        private string _tripNumber;
        public string TripNumber
        {
            get { return _tripNumber; }
            set { SetProperty(ref _tripNumber, value); }
        }

        private ContainerMasterModel _containerSelected;
        public ContainerMasterModel ContainerSelected
        {
            get { return _containerSelected; }
            set { SetProperty(ref _containerSelected, value); }
        }

        private DriverStatusModel _currentDriver;
        public DriverStatusModel CurrentDriver
        {
            get { return _currentDriver; }
            set { SetProperty(ref _currentDriver, value); }
        }

        private string _tripSegNumber;
        public string TripSegNumber
        {
            get { return _tripSegNumber; }
            set { SetProperty(ref _tripSegNumber, value); }
        }

        // Command impl
        public void ExecuteContainerSelectedCommand(ContainerMasterWithTripContainer selectedSegment)
        {
            Close(this);
            ShowViewModel<ScaleDetailViewModel>(new
            {
                tripNumber = TripNumber, // This is the current trip a user is on, not the trip number of the container, which could be different
                containerNumber = selectedSegment.ContainerMaster.ContainerNumber
            });
        }
    }
}
