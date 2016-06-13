using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Mobile.Resources;
using MvvmCross.Core.ViewModels;
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
            Title = AppResources.Photos;
        }

        public override async void Start()
        {
            Images = new ObservableCollection<string>();

            // Make sure all previous photos have been deleted from previous use
            if ( _fileStore.FolderExists(MobileConstants.ImagesDirectory) )
                _fileStore.DeleteFolder(MobileConstants.ImagesDirectory, true);

            // Open the camera app on first load of viewmodel
            // There's currently an issue ( at least on samsung devices ), that the camera app will 
            // save a photo ( at full image quality settings ) along with our manual save below 
            // ( w/ customized image quality settings )
            _pictureChooserTask.TakePicture(MobileConstants.MaxPixelDimension, MobileConstants.ImageQuality, OnPictureTaken, () => {/*nothing on cancel*/});
        }

        // My thinking here is that it may be better to temporarily store the images on disk
        // rather than in memory to accomodate older devices where memory may be a concern
        private void OnPictureTaken(Stream stream)
        {
            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);

            var randomFileName = Guid.NewGuid().ToString("N") + ".jpg";
            _fileStore.EnsureFolderExists(MobileConstants.ImagesDirectory);
            var path = _fileStore.PathCombine(MobileConstants.ImagesDirectory, randomFileName);
            _fileStore.WriteFile(path, memoryStream.ToArray());

            var fullPath = _fileStore.NativePath(path);
            Images.Add(fullPath);

            memoryStream.Flush(); // This may be redundant
        }

        private ObservableCollection<string> _images;
        public ObservableCollection<string> Images
        {
            get { return _images; }
            set { SetProperty(ref _images, value); }
        }

        private IMvxCommand _addAnotherPhotoCommand;
        public IMvxCommand AddAnotherPhotoCommand
            => _addAnotherPhotoCommand ?? (_addAnotherPhotoCommand = new MvxCommand(ExecuteAddAnotherPhotoCommand));

        private void ExecuteAddAnotherPhotoCommand()
        {
            _pictureChooserTask.TakePicture(MobileConstants.MaxPixelDimension, MobileConstants.ImageQuality, OnPictureTaken, () => {/*nothing on cancel*/});
        }

        private IMvxCommand _cancelPhotosCommand;
        public IMvxCommand CancelPhotosCommand
            => _cancelPhotosCommand ?? (_cancelPhotosCommand = new MvxCommand(ExecuteCancelPhotosCommand));

        private void ExecuteCancelPhotosCommand()
        {
            _fileStore.DeleteFolder(MobileConstants.ImagesDirectory, true);
            Close(this);
        }

        private IMvxCommand _sendPhotosCommand;
        public IMvxCommand SendPhotosCommand
            => _sendPhotosCommand ?? (_sendPhotosCommand = new MvxCommand(ExecuteSendPhotosCommand, CanExecuteSendPhotosCommand));

        private void ExecuteSendPhotosCommand()
        {
            // @TODO : Implement send photos functionality once server-side component complete

            // After files sent, remove them from disk
            _fileStore.DeleteFolder(MobileConstants.ImagesDirectory, true);
            Close(this);
        }

        private bool CanExecuteSendPhotosCommand()
        {
            return Images.Count > 0;
        }
    }
}
