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

        public FuelEntryViewModel(IConnectionService<DataServiceClient> connection,
                                  IDriverService driverService,
                                  IPreferenceService preferenceService,
                                  ICodeTableService codeTableService)
        {
            _driverService = driverService;
            _codeTableService = codeTableService;
            _preferenceService = preferenceService;

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
        private float _fuelAmount;
        public float FuelAmount
        {
            get { return _fuelAmount; }
            set
            {
                SetProperty(ref _fuelAmount, value);
                SaveFuelEntryCommand.RaiseCanExecuteChanged();
            }
        }
        private string _selectedState;
        public string SelectedState
        {
            get { return _selectedState; }
            set
            {
                _selectedState = value; RaisePropertyChanged(() => SelectedState);
                SaveFuelEntryCommand.RaiseCanExecuteChanged();
            }
        }
        private ObservableCollection<CodeTableModel> _statesList;
        public ObservableCollection<CodeTableModel> StatesList
        {
            get { return _statesList; }
            set { SetProperty(ref _statesList, value); }
        }

        //public MvxCommand<CodeTableModel> SelectStateCommand { get; private set; }
        //private void ExecuteSelectStateCommand(CodeTableModel stateInfo)
        //{
        //    SelectedState = stateInfo.CodeDisp1;
        //}

        private MvxCommand _saveFuelEntryCommand;
        public MvxCommand SaveFuelEntryCommand => _saveFuelEntryCommand ??
            (_saveFuelEntryCommand = new MvxCommand(ExecuteSaveFuelEntryCommand, CanExecuteSaveFuelEntryCommand));

        protected bool CanExecuteSaveFuelEntryCommand()
        {
            return !string.IsNullOrWhiteSpace(OdometerReading.ToString())
                   && !string.IsNullOrWhiteSpace(FuelAmount.ToString())
                   && !string.IsNullOrWhiteSpace(SelectedState);
        }
        protected async void ExecuteSaveFuelEntryCommand()
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
                var currentUser = await _driverService.GetCurrentDriverStatus();

                var fuelEntry = await _driverService.SetFuelEntry(new DriverFuelEntryProcess
                {
                    EmployeeId = currentUser.EmployeeId,
                    Odometer = OdometerReading ?? default(int),
                    ActionDateTime = DateTime.Now,
                    PowerId = currentUser.PowerId,
                    State = SelectedState,
                    TripNumber = currentUser.TripNumber,
                    TripSegNumber = currentUser.TripSegNumber,
                    FuelAmount = FuelAmount
                });

                if (fuelEntry.WasSuccessful) return true;

                await UserDialogs.Instance.AlertAsync(fuelEntry.Failure.Summary,
                    AppResources.Error, AppResources.OK);
                return false;
            }
        }
    }
}
