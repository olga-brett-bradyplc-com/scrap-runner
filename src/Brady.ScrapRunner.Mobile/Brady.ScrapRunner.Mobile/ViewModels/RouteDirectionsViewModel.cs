﻿namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Collections.ObjectModel;
    using Interfaces;
    using Models;
    using MvvmCross.Core.ViewModels;

    public class RouteDirectionsViewModel : BaseViewModel
    {
        private string _custHostCode;
        private readonly IRepository<CustomerDirectionModel> _repository; 

        public RouteDirectionsViewModel(
            IRepository<CustomerDirectionModel> repository
            )
        {
            _repository = repository;
            BackCommand = new MvxCommand(ExecuteBackCommand);
        }

        public void Init(string custHostCode)
        {
            _custHostCode = custHostCode;
        }

        public override async void Start()
        {
            base.Start();
            var directions = await _repository.ToListAsync(cd => cd.CustHostCode == _custHostCode);
            Directions = new ObservableCollection<CustomerDirectionModel>(directions);
        }

        private ObservableCollection<CustomerDirectionModel> _directions = new ObservableCollection<CustomerDirectionModel>(); 
        public ObservableCollection<CustomerDirectionModel> Directions
        {
            get { return _directions; }
            set { SetProperty(ref _directions, value); }
        }

        public MvxCommand BackCommand { get; private set; }

        private void ExecuteBackCommand()
        {
            Close(this);
        }
    }
}