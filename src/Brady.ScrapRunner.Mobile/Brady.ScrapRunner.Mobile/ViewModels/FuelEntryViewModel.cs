using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using Brady.ScrapRunner.Mobile.Services;
using Brady.ScrapRunner.Mobile.Validators;
using BWF.DataServices.PortableClients;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.ExtensionMethods;
using MvvmCross.Localization;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Globalization;
    using MvvmCross.Core.ViewModels;
    using Resources;

    public class FuelEntryViewModel : BaseViewModel
    {
        private readonly ICodeTableService _codeTableService;
        private readonly IPreferenceService _preferenceService;
        private readonly IDriverService _driverService;
        private readonly ITripService _tripService;
        private readonly ITerminalService _terminalService;

        public FuelEntryViewModel(IDriverService driverService,
                                  IPreferenceService preferenceService,
                                  ICodeTableService codeTableService,
                                  ITripService tripService,
                                  ITerminalService terminalService)
        {
            _driverService = driverService;
            _codeTableService = codeTableService;
            _preferenceService = preferenceService;
            _tripService = tripService;
            _terminalService = terminalService;

            Title = AppResources.FuelEntry;
        }

        public override async void Start()
        {
            var currentCountryPreference = await _preferenceService.FindPreferenceValueAsync(PrefDriverConstants.DEFCountry);

            // If not Canada or Mexico, default to grabbing US states
            var currentStateList =
                currentCountryPreference == "CAN"
                    ? CodeTableNameConstants.StatesCanada
                    : currentCountryPreference == "MEX"
                        ? CodeTableNameConstants.StatesMexico
                        : CodeTableNameConstants.StatesUSA;
            
            var states = await _codeTableService.FindCountryStatesAsync(currentStateList);
            StatesList = new ObservableCollection<CodeTableModel>(states);

            /* default selected state to:  the destination state of the last segment of his last trip. 
             * If that is not possible (first trip of day) then, the origin state of the first segment of the first trip that he has. 
             * If that is not known, (no trips) then perhaps the state for the driver's terminal.*/

            var currentDriver = await _driverService.GetCurrentDriverStatusAsync();

            var currentSegment = await _tripService.FindTripSegmentInfoAsync(currentDriver.TripNumber, currentDriver.TripSegNumber);

            if (currentSegment != null)
            {
                string state;

                if (currentDriver.Status == "E" || currentDriver.Status == "A")
                    state = currentSegment.TripSegDestCustState;
                else
                    state = currentSegment.TripSegOrigCustState;
            
                if (currentCountryPreference == "CAN")
                    SelectedState = await _codeTableService.FindCodeTableObject(CodeTableNameConstants.StatesCanada,
                        state);
                else if (currentCountryPreference == "MEX")
                    SelectedState = await _codeTableService.FindCodeTableObject(CodeTableNameConstants.StatesMexico,
                        state);
                else
                    SelectedState = await _codeTableService.FindCodeTableObject(CodeTableNameConstants.StatesUSA,
                        state);
            }
            else
            {
                var drvrTerminal = await _terminalService.FindTerminalMasterAsync(currentDriver.TerminalId);

                if (drvrTerminal != null)
                {
                    if (currentCountryPreference == "CAN")
                        SelectedState = await _codeTableService.FindCodeTableObject(CodeTableNameConstants.StatesCanada,
                            drvrTerminal.State);
                    else if (currentCountryPreference == "MEX")
                        SelectedState = await _codeTableService.FindCodeTableObject(CodeTableNameConstants.StatesMexico,
                            drvrTerminal.State);
                    else
                        SelectedState = await _codeTableService.FindCodeTableObject(CodeTableNameConstants.StatesUSA,
                           drvrTerminal.State);
                }
            }
            
            base.Start();
        }

        private int? _odometerReading;
        public int? OdometerReading
        {
            get { return _odometerReading; }
            set
            {
                SetProperty(ref _odometerReading, value);
                SaveFuelEntryCommand.RaiseCanExecuteChanged();
            }
        }

        private float? _fuelAmount;
        public float? FuelAmount
        {
            get { return _fuelAmount; }
            set
            {
                SetProperty(ref _fuelAmount, value);
                SaveFuelEntryCommand.RaiseCanExecuteChanged();
            }
        }

        private CodeTableModel _selectedState;
        public CodeTableModel SelectedState
        {
            get { return _selectedState; }
            set
            {
                SetProperty(ref _selectedState, value);
                SaveFuelEntryCommand.RaiseCanExecuteChanged();
            }
        }

        private ObservableCollection<CodeTableModel> _statesList;
        public ObservableCollection<CodeTableModel> StatesList
        {
            get { return _statesList; }
            set { SetProperty(ref _statesList, value); }
        }

        private IMvxAsyncCommand _saveFuelEntryCommand;
        public IMvxAsyncCommand SaveFuelEntryCommand => _saveFuelEntryCommand ??
            (_saveFuelEntryCommand = new MvxAsyncCommand(ExecuteSaveFuelEntryCommandAsync, CanExecuteSaveFuelEntryCommand));

        protected bool CanExecuteSaveFuelEntryCommand()
        {
            return OdometerReading.HasValue
                   && FuelAmount.HasValue
                   && !string.IsNullOrWhiteSpace(SelectedState?.CodeValue);
        }

        protected async Task ExecuteSaveFuelEntryCommandAsync()
        {
            //TODO: add update db tables fuel entry routine

            var odometerResults = Validate<OdometerRangeValidator, int?>(OdometerReading);
            if (!odometerResults.IsValid)
            {
                UserDialogs.Instance.Alert(odometerResults.Errors.First().ErrorMessage);
                return;
            }

            try
            {
                var saveFuelEntryResult = await SaveFuelEntryAsync();
                if (!saveFuelEntryResult)
                    return;

                Close(this);
            }
            catch (Exception exception)
            {
                var message = exception?.InnerException?.Message ?? exception.Message;
                await UserDialogs.Instance.AlertAsync(
                    message, AppResources.Error, AppResources.OK);
            }
        }

        private async Task<bool> SaveFuelEntryAsync()
        {
            using (var loginData = UserDialogs.Instance.Loading(AppResources.SavingData, maskType: MaskType.Black))
            {
                var currentUser = await _driverService.GetCurrentDriverStatusAsync();

                var fuelEntry = await _driverService.ProcessDriverFuelEntryAsync(new DriverFuelEntryProcess
                {
                    EmployeeId = currentUser.EmployeeId,
                    Odometer = OdometerReading ?? default(int),
                    ActionDateTime = DateTime.Now,
                    PowerId = currentUser.PowerId,
                    State = SelectedState.CodeValue,
                    Country = SelectedState.CodeDisp2,
                    TripNumber = currentUser.TripNumber,
                    TripSegNumber = currentUser.TripSegNumber,
                    FuelAmount = FuelAmount ?? default(float)
                });

                if (fuelEntry.WasSuccessful) return true;

                await UserDialogs.Instance.AlertAsync(fuelEntry.Failure.Summary,
                    AppResources.Error, AppResources.OK);
                return false;
            }
        }
    }
}
