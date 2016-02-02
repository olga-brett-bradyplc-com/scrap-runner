using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Mobile.Helpers;
using Brady.ScrapRunner.Mobile.Models;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Xamarin.Forms;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class TransactionsViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        public TransactionsViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            Title = "Transactions";

            TransactionDetailList = new ObservableCollection<TransactionDetail>();
            CreateDummyData();

            var grouped = from details in TransactionDetailList
                          orderby details.Order
                          group details by details.Type
                          into detailsGroup
                          select new Grouping<string, TransactionDetail>(detailsGroup.Key, detailsGroup);

            TransactionList = new ObservableCollection<Grouping<string, TransactionDetail>>(grouped);
            TransactionSelectedCommand = new RelayCommand(ExecuteTransactionSelectedCommand);
        }

        // Listview bindings
        public ObservableCollection<Grouping<string, TransactionDetail>> TransactionList { get; private set; }
        public ObservableCollection<TransactionDetail> TransactionDetailList { get; private set; }

        // Command bindings
        public RelayCommand TransactionSelectedCommand { get; private set; }
        public RelayCommand TransactionScannedCommand { get; private set; }

        // Field bindings
        private TransactionDetail _transactionSelected;
        public TransactionDetail TransactionSelected
        {
            get { return _transactionSelected; }
            set { Set(ref _transactionSelected, value); }
        }

        // Command impl
        public void ExecuteTransactionSelectedCommand()
        {
            _navigationService.NavigateTo(Locator.TransactionDetailView);
        }

        // @TODO: Refactor using Brady.Domain objects when convenient
        public void CreateDummyData()
        {
            TransactionDetailList.Add(new TransactionDetail
            {
                Order = 1,
                Type = "Drop Empty",
                Id = "<NO NUMBER> LUGGER-10",
                Location = "DOCK DOOR 14",
                MaterialType = "#15 SHEARING IRON"
            });

            TransactionDetailList.Add(new TransactionDetail
            {
                Order = 1,
                Type = "Drop Empty",
                Id = "<NO NUMBER> LUGGER-12",
                Location = "DOCK DOOR 14",
                MaterialType = "#15 SHEARING IRON"
            });

            TransactionDetailList.Add(new TransactionDetail
            {
                Order = 2,
                Type = "Pickup Full",
                Id = "<NO NUMBER> LUGGER-12",
                Location = "DOCK DOOR 14",
                MaterialType = "#15 SHEARING IRON"
            });
        }
    }
}
