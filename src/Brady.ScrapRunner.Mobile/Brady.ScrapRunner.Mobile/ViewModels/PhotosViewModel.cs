using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Brady.ScrapRunner.Domain;
using Brady.ScrapRunner.Domain.Process;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
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
        private readonly IDriverService _driverService;
        private readonly ITripService _tripService;

        public PhotosViewModel(
            IMvxPictureChooserTask pictureChooserTask, 
            IMvxFileStore fileStore,
            IDriverService driverService,
            ITripService tripService)
        {
            _pictureChooserTask = pictureChooserTask;
            _fileStore = fileStore;
            _driverService = driverService;
            _tripService = tripService;
            Title = AppResources.Photos;
        }

        public override async void Start()
        {
            CurrentDriver = await _driverService.GetCurrentDriverStatusAsync();

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

            var randomFileName = Guid.NewGuid().ToString("N") + "." + ImageExtConstants.Picture;
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
            set
            {
                SetProperty(ref _images, value);
                SendPhotosCommand.RaiseCanExecuteChanged();
            }
        }

        private DriverStatusModel _currentDriver;
        public DriverStatusModel CurrentDriver
        {
            get { return _currentDriver; }
            set { SetProperty(ref _currentDriver, value); }
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

        private IMvxAsyncCommand _sendPhotosCommand;
        public IMvxAsyncCommand SendPhotosCommand
            => _sendPhotosCommand ?? (_sendPhotosCommand = new MvxAsyncCommand(ExecuteSendPhotosCommand, CanExecuteSendPhotosCommand));

        private async Task ExecuteSendPhotosCommand()
        {
            using (var loading = UserDialogs.Instance.Loading(AppResources.Loading, maskType: MaskType.Black))
            {
                var fileList = _fileStore.GetFilesIn(MobileConstants.ImagesDirectory);
                foreach (var file in fileList)
                {
                    byte[] bytes;
                    _fileStore.TryReadBinaryFile(file, out bytes);

                    var imageProcess = await _tripService.ProcessDriverImageAsync(new DriverImageProcess
                    {
                        EmployeeId = CurrentDriver.EmployeeId,
                        TripNumber = CurrentDriver.TripNumber,
                        TripSegNumber = CurrentDriver.TripSegNumber,
                        ActionDateTime = DateTime.Now,
                        ImageType = ImageTypeConstants.Picture,
                        ImageByteArray = bytes
                    });

                    Array.Clear(bytes, 0, bytes.Length);

                    if (!imageProcess.WasSuccessful)
                    {
                        UserDialogs.Instance.Alert(imageProcess.Failure.Summary, AppResources.Error);
                        return;
                    }
                }
            }

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
