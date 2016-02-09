namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using Helpers;
    using Models;
    using MvvmCross.Core.ViewModels;

    public class TransactionSummaryViewModel : BaseViewModel
    {
        public TransactionSummaryViewModel()
        {
            Title = "Transactions";

            TransactionDetailList = new ObservableCollection<TransactionDetail>();
            CreateDummyData();

            var grouped = from details in TransactionDetailList
                          orderby details.Order
                          group details by details.Type
                          into detailsGroup
                          select new Grouping<string, TransactionDetail>(detailsGroup.Key, detailsGroup);

            TransactionList = new ObservableCollection<Grouping<string, TransactionDetail>>(grouped);
            TransactionSelectedCommand = new MvxCommand(ExecuteTransactionSelectedCommand);
        }

        // Listview bindings
        public ObservableCollection<Grouping<string, TransactionDetail>> TransactionList { get; private set; }
        public ObservableCollection<TransactionDetail> TransactionDetailList { get; private set; }

        // Command bindings
        public MvxCommand TransactionSelectedCommand { get; private set; }
        public MvxCommand TransactionScannedCommand { get; private set; }

        // Field bindings
        private TransactionDetail _transactionSelected;
        public TransactionDetail TransactionSelected
        {
            get { return _transactionSelected; }
            set { SetProperty(ref _transactionSelected, value); }
        }

        // Command impl
        public void ExecuteTransactionSelectedCommand()
        {
            ShowViewModel<TransactionDetailViewModel>();
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