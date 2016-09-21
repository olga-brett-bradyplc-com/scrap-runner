using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Mobile.Helpers;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Messages;
using Brady.ScrapRunner.Mobile.Resources;
using MvvmCross.Plugins.Messenger;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Collections.ObjectModel;
    using Models;
    using MvvmCross.Core.ViewModels;

    public class PublicScaleSummaryViewModel : BaseViewModel
    {
        private readonly ITripService _tripService;
        private readonly ICodeTableService _codeTableService;
        private MvxSubscriptionToken _mvxSubscriptionToken;
        private readonly IMvxMessenger _mvxMessenger;

        public PublicScaleSummaryViewModel(ITripService tripService, ICodeTableService codeTableService, IMvxMessenger mvxMessenger)
        {
            _tripService = tripService;
            _codeTableService = codeTableService;
            _mvxMessenger = mvxMessenger;
            _mvxSubscriptionToken = mvxMessenger.SubscribeOnMainThread<TripNotificationMessage>(OnTripNotification);

            Title = AppResources.PublicScaleSummary;
            ContainerSelectedCommand = new MvxCommand<TripSegmentContainerModel>(ExecuteContainerSelectedCommand);
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
        public void Init(string tripNumber, string methodOfEntry)
        {
            TripNumber = tripNumber;
            MethodOfEntry = methodOfEntry;
            SubTitle = $"{AppResources.Trip} {TripNumber}";
        }
        private string _methodOfEntry;

        public string MethodOfEntry
        {
            get { return _methodOfEntry; }
            set { SetProperty(ref _methodOfEntry, value); }
        }
        private List<CodeTableModel> _contTypeList;
        public List<CodeTableModel> ContTypesList
        {
            get { return _contTypeList; }
            set { SetProperty(ref _contTypeList, value); }
        }

        private async void LoadData()
        {
            using (var tripDataLoad = UserDialogs.Instance.Loading(AppResources.LoadingTripData, maskType: MaskType.Clear))
            {
                var segments = await _tripService.FindNextTripLegSegmentsAsync(TripNumber);
                var list = new ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>>();
                ContTypesList = await _codeTableService.FindCodeTableList(CodeTableNameConstants.ContainerType);

                foreach (var tsm in segments)
                {
                    var containers =
                        await _tripService.FindNextTripSegmentContainersAsync(TripNumber, tsm.TripSegNumber);
                    foreach (var cont in containers)
                    {
                        var contType = ContTypesList.FirstOrDefault(ct => ct.CodeValue == cont.TripSegContainerType?.TrimEnd());
                        cont.TripSegContainerTypeDesc = contType != null ? contType.CodeDisp1?.TrimEnd() : cont.TripSegContainerType;
                    }
                    var grouping = new Grouping<TripSegmentModel, TripSegmentContainerModel>(tsm, containers);
                    list.Add(grouping);
                }

                if (list.Any())
                    Containers = list;
            }
            
        }
        public override void Start()
        {
            LoadData();

            base.Start();
        }
        private ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> _containers;
        public ObservableCollection<Grouping<TripSegmentModel, TripSegmentContainerModel>> Containers
        {
            get { return _containers; }
            set { SetProperty(ref _containers, value); }
        }

        // Command bindings
        public MvxCommand<TripSegmentContainerModel> ContainerSelectedCommand { get; private set; }

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

        // Command impl
        public void ExecuteContainerSelectedCommand(TripSegmentContainerModel selectedSegment)
        {
            Close(this);
            ShowViewModel<PublicScaleDetailViewModel>(new
            {
                tripNumber = selectedSegment.TripNumber,
                tripSegNumber = selectedSegment.TripSegNumber,
                tripSegContainerSeqNumber = selectedSegment.TripSegContainerSeqNumber,
                tripSegContainerNumber = selectedSegment.TripSegContainerNumber,
                methodOfEntry = MethodOfEntry
            });
        }
    }
}
