using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmCross.Plugins.File;
using MvvmCross.Plugins.PictureChooser;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class PhotosViewModel : BaseViewModel
    {
        private readonly IMvxPictureChooserTask _pictureChooserTask;
        private readonly IMvxFileStore _fileStore;

        public PhotosViewModel(IMvxPictureChooserTask pictureChooserTask, IMvxFileStore fileStore)
        {
            _pictureChooserTask = pictureChooserTask;
            _fileStore = fileStore;
            Title = "Photos";
        }
    }
}
