using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Mobile.Enums;
using Brady.ScrapRunner.Mobile.Helpers;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using Brady.ScrapRunner.Mobile.Resources;
using MvvmCross.Core.ViewModels;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class ModifyReturnToYardViewModel : BaseViewModel
    {
        private readonly ITerminalService _terminalService;
        private readonly ITripService _tripService;

        public ModifyReturnToYardViewModel(ITerminalService terminalService, ITripService tripService)
        {
            _terminalService = terminalService;
            _tripService = tripService;
        }

        public void Init(TerminalChangeEnum changeType, string tripNumber)
        {
            Title = changeType == TerminalChangeEnum.Add ? AppResources.ReturnToYardAdd : AppResources.ReturnToYardEdit;
            ChangeType = changeType;
            CurrentTripNumber = tripNumber;
        }

        public override async void Start()
        {
            var terminals = await _terminalService.FindAllTerminalChanges();
            var terminalsGrouped = terminals.GroupBy(ts => ts.CustCity)
                .Select(g => new {Key = g.Key, Values = g});

            TerminalList = new ObservableCollection<Grouping<string, TerminalChangeModel>>();

            foreach (var grouping in terminalsGrouped.Select(terminal => new Grouping<string, TerminalChangeModel>(terminal.Key, terminal.Values)))
                TerminalList.Add(grouping);
        }

        private IMvxAsyncCommand _terminalSelectedCommand;
        public IMvxAsyncCommand TerminalSelectedCommand => _terminalSelectedCommand ?? (_terminalSelectedCommand = new MvxAsyncCommand<Grouping<string, TerminalChangeModel>>(ExecuteTerminalSelectedCommand));

        private async Task ExecuteTerminalSelectedCommand(Grouping<string, TerminalChangeModel> grouping)
        {
            if (grouping.Count > 1)
            {
                var selectYardAsync =
                await
                    UserDialogs.Instance.ActionSheetAsync(AppResources.SelectYard, "", AppResources.Cancel,
                        grouping.Select(gp => gp.CustName).ToArray());

                if (selectYardAsync != AppResources.Cancel)
                {
                    await ConfirmReturnToYard(grouping.FirstOrDefault(gp => gp.CustName == selectYardAsync));
                }
            }
            else
            {
                await ConfirmReturnToYard(grouping.FirstOrDefault());
            }
        }

        private async Task ConfirmReturnToYard(TerminalChangeModel terminalChange)
        {

            var tripSegments = await _tripService.FindAllSegmentsForTripAsync(CurrentTripNumber);

            if (ChangeType == TerminalChangeEnum.Edit)
            {
                var rty = tripSegments.FirstOrDefault(ts => ts.TripSegType == BasicTripTypeConstants.ReturnYard || ts.TripSegType == BasicTripTypeConstants.ReturnYardNC);

                var message = string.Format(AppResources.ConfirmReturnToYardEdit, 
                    "\n\n", 
                    "\n", 
                    rty.TripSegDestCustName,
                    rty.TripSegDestCustAddress1, 
                    rty.DestCustCityStateZip, 
                    terminalChange.CustName,
                    terminalChange.CustAddress1, 
                    terminalChange.CityStateZipFormatted);

                var confirm = await UserDialogs.Instance.ConfirmAsync(message, AppResources.ConfirmLabel, AppResources.Yes, AppResources.No);

                if (confirm)
                {
                    rty.TripSegDestCustName = terminalChange.CustName;
                    rty.TripSegDestCustAddress1 = terminalChange.CustAddress1;
                    rty.TripSegDestCustAddress2 = terminalChange.CustAddress2;
                    rty.TripSegDestCustCity = terminalChange.CustCity;
                    rty.TripSegDestCustState = terminalChange.CustState;
                    rty.TripSegDestCustZip = terminalChange.CustZip;
                    rty.TripSegDestCustHostCode = terminalChange.CustHostCode;

                    await _tripService.UpdateTripSegmentAsync(rty);

                    UserDialogs.Instance.InfoToast(AppResources.SegmentUpdated);

                    Close(this);
                }

            }
            else // TerminalChangeEnum.Add
            {
                var message = string.Format(AppResources.ConfirmReturnToYardAdd,
                    "\n\n",
                    "\n",
                    terminalChange.CustName,
                    terminalChange.CustAddress1,
                    terminalChange.CityStateZipFormatted);

                var confirm = await UserDialogs.Instance.ConfirmAsync(message, AppResources.ConfirmLabel, AppResources.Yes, AppResources.No);

                if (confirm)
                {
                    var lastSegment = tripSegments.Last();
                    var tripSegType = _tripService.IsContainerDropped(lastSegment)
                        ? BasicTripTypeConstants.ReturnYardNC
                        : BasicTripTypeConstants.ReturnYard;
                    
                    var newTripSegNumber = (int.Parse(tripSegments.Last().TripSegNumber) + 1).ToString("D2");

                    // @TODO : pull down TripTypeBasic table to get descriptions from DB
                    var tripSegDesc = (tripSegType == BasicTripTypeConstants.ReturnYardNC) ? "RTN NC" : "RTN TO YARD";

                    var tripSegment = new TripSegmentModel
                    {
                        TripNumber = CurrentTripNumber,
                        TripSegNumber = newTripSegNumber,
                        TripSegStatus = TripSegStatusConstants.Pending,
                        TripSegType = tripSegType,
                        TripSegTypeDesc = tripSegDesc,
                        TripSegOrigCustName = lastSegment.TripSegOrigCustName,
                        TripSegOrigCustHostCode = lastSegment.TripSegOrigCustHostCode,
                        TripSegDestCustType = "W",
                        TripSegDestCustName = terminalChange.CustName,
                        TripSegDestCustHostCode = terminalChange.CustHostCode,
                        TripSegDestCustAddress1 = terminalChange.CustAddress1,
                        TripSegDestCustAddress2 = terminalChange.CustAddress2,
                        TripSegDestCustCity = terminalChange.CustCity,
                        TripSegDestCustState = terminalChange.CustState,
                        TripSegDestCustZip = terminalChange.CustZip
                    };

                    // Create new trip segment
                    await _tripService.CreateTripSegmentAsync(tripSegment);

                    // If segment has containers, add them
                    if (tripSegment.TripSegType.Equals(BasicTripTypeConstants.ReturnYard))
                    {
                        var containers = await _tripService.FindAllContainersForTripSegmentAsync(CurrentTripNumber,
                            lastSegment.TripSegNumber);

                        foreach (var container in containers)
                        {
                            container.TripSegNumber = newTripSegNumber;
                            await _tripService.CreateTripSegmentContainerAsync(container);
                        }
                    }

                    UserDialogs.Instance.InfoToast(AppResources.NewSegmentCreated);
                    Close(this);
                }
            }
        }

        private string _currentTripNumber;
        public string CurrentTripNumber
        {
            get { return _currentTripNumber; }
            set { SetProperty(ref _currentTripNumber, value); }
        }

        private TerminalChangeEnum _changeType;
        public TerminalChangeEnum ChangeType
        {
            get { return _changeType; }
            set { SetProperty(ref _changeType, value); }
        }

        private ObservableCollection<Grouping<string, TerminalChangeModel>> _terminalList;
        public ObservableCollection<Grouping<string, TerminalChangeModel>> TerminalList
        {
            get { return _terminalList; }
            set { SetProperty(ref _terminalList, value); }
        }
    }
}
