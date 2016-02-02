using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Mobile.ViewModels;
using Xamarin.Forms;
using Brady.ScrapRunner.Mobile.Renderers;

namespace Brady.ScrapRunner.Mobile.Views
{
    public partial class TransactionsView : ContentPage
    {
        public TransactionsView()
        {
            InitializeComponent();
            BindingContext = App.Locator.Transactions;
        }

    }
}
