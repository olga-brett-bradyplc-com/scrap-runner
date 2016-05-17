using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class NbScanViewModel : BaseViewModel
    {
        private readonly IContainerService _containerService;

        public NbScanViewModel(IContainerService containerService)
        {
            _containerService = containerService;
        }

        private string _containerId;
        public string ContainerId
        {
            get { return _containerId; }
            set { SetProperty(ref _containerId, value); }
        }

        private string _barcodeNumber;
        public string BarcodeNumber
        {
            get { return _barcodeNumber; }
            set { SetProperty(ref _barcodeNumber, value); }
        }

        private List<string> _typeList;
        public List<string> TypeList
        {
            get { return _typeList; }
            set { SetProperty(ref _typeList, value); }
        }

        private List<string> _sizeList;
        public List<string> SizeList
        {
            get { return _sizeList; }
            set { SetProperty(ref _sizeList, value); }
        }

        private List<string> _unitList;
        public List<string> UnitList
        {
            get { return _unitList; }
            set { SetProperty(ref _unitList, value); }
        }
    }
}
