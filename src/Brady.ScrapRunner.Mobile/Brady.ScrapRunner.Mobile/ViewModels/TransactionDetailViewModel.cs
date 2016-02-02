using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class TransactionDetailViewModel : BaseViewModel
    {
        public TransactionDetailViewModel()
        {
            Title = "Pickup Full";
            //Subtitle = "Trip 615112";
        }

        private string _id;

        public string Id
        {
            get { return _id; }
            set { Set(ref _id, value); }
        }

        private string _containerId;
        public string ContainerId
        {
            get { return _containerId; }
            set
            {
                Set(ref _containerId, value);
            }
        }

        private string _location;
        public string Location
        {
            get { return _location; }
            set
            {
                Set(ref _location, value);
            }
        }

        private string _commodity;
        public string Commodity
        {
            get { return _commodity; }
            set
            {
                Set(ref _commodity, value);
            }
        }

        private string _level;
        public string Level
        {
            get { return _level; }
            set
            {
                Set(ref _level, value);
            }
        }

        private string _notes;
        public string Notes
        {
            get { return _notes; }
            set
            {
                Set(ref _notes, value);
            }
        }

        private string _referenceNumber;
        public string ReferenceNumber
        {
            get { return _referenceNumber; }
            set
            {
                Set(ref _referenceNumber, value);
            }
        }

    }
}
