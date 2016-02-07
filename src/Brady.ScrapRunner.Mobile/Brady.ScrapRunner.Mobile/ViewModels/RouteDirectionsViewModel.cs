namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Views;
    using Interfaces;
    using Models;

    public class RouteDirectionsViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IRepository<CustomerDirectionModel> _repository; 

        public RouteDirectionsViewModel(
            INavigationService navigationService, 
            IRepository<CustomerDirectionModel> repository
            )
        {
            _navigationService = navigationService;
            _repository = repository;
            BackCommand = new RelayCommand(ExecuteBackCommand);
        }

        public async Task LoadAsync(string customerHostCode)
        {
            var directions = await _repository.ToListAsync(cd => cd.CustHostCode == customerHostCode);
            Directions = new ObservableCollection<CustomerDirectionModel>(directions);
        }

        private ObservableCollection<CustomerDirectionModel> _directions = new ObservableCollection<CustomerDirectionModel>(); 
        public ObservableCollection<CustomerDirectionModel> Directions
        {
            get { return _directions; }
            set { Set(ref _directions, value); }
        }

        public RelayCommand BackCommand { get; private set; }

        private void ExecuteBackCommand()
        {
            _navigationService.GoBack();
        }
    }
}