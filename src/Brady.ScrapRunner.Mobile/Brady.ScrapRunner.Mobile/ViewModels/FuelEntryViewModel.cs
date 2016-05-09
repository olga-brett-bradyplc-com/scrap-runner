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
        private readonly ITripService _tripService;
        private readonly IDbService _dbService;
        private readonly IConnectionService<DataServiceClient> _connection;

        public FuelEntryViewModel(IConnectionService<DataServiceClient> connection, 
                                  IDbService dbService,
                                  ICodeTableService codeTableService, 
                                  ITripService tripService)
        {
            _connection = connection;
            _dbService = dbService;
            _codeTableService = codeTableService;
            _tripService = tripService;

            Title = AppResources.FuelEntry;
        }
        public override async void Start()
        {
            var states = await _codeTableService.FindCountryStatesAsync("STATESUSA");
            StatesList = new ObservableCollection<CodeTableModel>(states);

            base.Start();
        }

        private string _odometerReading;
        public string OdometerReading
        {
            get { return _odometerReading; }
            set
            {
                SetProperty(ref _odometerReading, value);
                SaveFuelEntryCommand.RaiseCanExecuteChanged();
            }
        }
        private string _fuelAmount;
        public string FuelAmount
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
            return !string.IsNullOrWhiteSpace(OdometerReading)
                   && !string.IsNullOrWhiteSpace(FuelAmount)
                   && !string.IsNullOrWhiteSpace(SelectedState);
        }
        protected async void ExecuteSaveFuelEntryCommand()
        {
            //TODO: add update db tables fuel entry routine

            var odometerResults = Validate<OdometerRangeValidator, int?>(Int32.Parse(OdometerReading));
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
                // Delete/Create necesscary SQLite tables
                await _dbService.RefreshAll();

                var tripObj = new TripInfoProcess
                {
                    EmployeeId = "930"
                };

                var tripProcess = await _connection.GetConnection().UpdateAsync(tripObj, requeryUpdated: false);

                if (tripProcess.WasSuccessful)
                {
                    if (tripProcess.Item?.Trips?.Count > 0)
                        await _tripService.UpdateTrips(tripProcess.Item.Trips);

                    if (tripProcess.Item?.TripSegments?.Count > 0)
                        await _tripService.UpdateTripSegments(tripProcess.Item.TripSegments);
                }
                else
                {
                    await UserDialogs.Instance.AlertAsync(tripProcess.Failure.Summary,
                        AppResources.Error, AppResources.OK);
                    return false;
                }
            }
            return true;
        }
    }
}
