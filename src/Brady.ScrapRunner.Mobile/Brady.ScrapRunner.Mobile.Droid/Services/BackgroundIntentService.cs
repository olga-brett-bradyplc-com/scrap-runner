namespace Brady.ScrapRunner.Mobile.Droid.Services
{
    using System;
    using Android.App;
    using Android.Content;
    using Android.Support.V4.Content;
    using Interfaces;
    using MvvmCross.Droid.Platform;
    using MvvmCross.Platform;
    using MvvmCross.Platform.Exceptions;

    [Service]
    public class BackgroundIntentService : IntentService
    {
        private bool _inProgress;
        private INetworkAvailabilityService _networkAvailabilityService;
        private IQueueService _queueService;
        private IPollingService _pollingService;
        private IDriverService _driverService;

        public override void OnCreate()
        {
            base.OnCreate();
            try
            {
                // The IntentService can actually be called at times when MvvmCross is not yet properly initialized.
                // See https://github.com/MvvmCross/MvvmCross/issues/1245
                var setupSingleton = MvxAndroidSetupSingleton.EnsureSingletonAvailable(ApplicationContext);
                setupSingleton.EnsureInitialized(); // Can rarely throw MvxException.
                _networkAvailabilityService = Mvx.Resolve<INetworkAvailabilityService>();
                _queueService = Mvx.Resolve<IQueueService>();
                _pollingService = Mvx.Resolve<IPollingService>();
                _driverService = Mvx.Resolve<IDriverService>();
            }
            catch (MvxException e)
            {
                Mvx.Warning($"Caught Exception in BackgroundIntentService.OnCreate ({e.Message})");
                // If another request is in progress MvxAndroidSetupSingleton.EnsureInitialized() will throw an MvxException
                // See https://github.com/MvvmCross/MvvmCross/issues/955
            }
        }

        protected override async void OnHandleIntent(Intent intent)
        {
            if (_inProgress)
            {
                Mvx.Warning("BackgroundIntentService.OnHandleIntent: Operation is still in progress.");
                return;
            }
            if (_networkAvailabilityService == null)
            {
                // If MvxException was caught in OnCreate() then MvvmCross IoC won't work. 
                // Just exit and try again next time.
                Mvx.Warning("BackgroundIntentService.OnHandleIntent: Failed to resolve INetworkAvailabilityService");
                return;
            }
            try
            {
                if (!_networkAvailabilityService.IsNetworkConnectionAvailable()) return;
                _inProgress = true;
                await _queueService.ProcessQueueAsync();
                var driverStatus = await _driverService.GetCurrentDriverStatusAsync();
                if (!_networkAvailabilityService.IsNetworkConnectionAvailable()) return;
                await _pollingService.PollForChangesAsync(driverStatus.EmployeeId, driverStatus.TerminalId,
                        driverStatus.RegionId, driverStatus.DriverArea);
            }
            catch (Exception exception)
            {
                Mvx.Error($"Caught exception in BackgroundIntentService.OnHandleIntent {exception}");
            }
            finally
            {
                WakefulBroadcastReceiver.CompleteWakefulIntent(intent);
                _inProgress = false;
            }
        }
    }
}