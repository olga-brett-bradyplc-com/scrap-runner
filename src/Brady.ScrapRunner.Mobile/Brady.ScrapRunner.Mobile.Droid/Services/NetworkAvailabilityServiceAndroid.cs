namespace Brady.ScrapRunner.Mobile.Droid.Services
{
    using Android.Content;
    using Android.Net;
    using Interfaces;
    using MvvmCross.Platform;
    using MvvmCross.Platform.Droid;

    public class NetworkAvailabilityServiceAndroid : INetworkAvailabilityService
    {
        public bool IsNetworkConnectionAvailable()
        {
            var androidGlobals = Mvx.Resolve<IMvxAndroidGlobals>();
            if (androidGlobals == null)
            {
                Mvx.Warning("Cannnot determine if a network connection is available. Unable to Resolve type IMvxAndroidGlobals.");
                return false;
            }
            var context = androidGlobals.ApplicationContext;
            var cm = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
            var info = cm.ActiveNetworkInfo; // Requires manifest permission ACCESS_NETWORK_STATE.
            if (info == null) return false;
            var infoState = info.GetState();
            return infoState == NetworkInfo.State.Connected;
        }
    }
}