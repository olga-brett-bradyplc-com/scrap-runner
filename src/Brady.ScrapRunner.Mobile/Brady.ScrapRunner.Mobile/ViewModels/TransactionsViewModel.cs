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
using Xamarin.Forms;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class TransactionsViewModel : BaseViewModel
    {
        public TransactionsViewModel()
        {
            Title = "Transactions";
            SubTitle = "Trip 615112";

            TransactionDetailList = new ObservableCollection<TransactionDetail>();
            CreateDummyData();

            var grouped = from details in TransactionDetailList
                orderby details.Order
                group details by details.Type
                into detailsGroup
                select new Grouping<string, TransactionDetail>(detailsGroup.Key, detailsGroup);

            TransactionList = new ObservableCollection<Grouping<string, TransactionDetail>>(grouped);
        }

        // Our bindings
        public ObservableCollection<Grouping<string, TransactionDetail>> TransactionList { get; set; }
        public ObservableCollection<TransactionDetail> TransactionDetailList { get; set; }

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
