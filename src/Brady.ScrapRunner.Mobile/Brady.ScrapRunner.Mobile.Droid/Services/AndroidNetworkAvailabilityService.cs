namespace Brady.ScrapRunner.Mobile.Droid.Services
{
    using Android.App;
    using Android.Net;
    using Interfaces;

    public class AndroidNetworkAvailabilityService : INetworkAvailabilityService
    {
        public bool IsNetworkConnectionAvailable()
        {
            var cm = ConnectivityManager.FromContext(Application.Context);
            var info = cm.ActiveNetworkInfo; // Requires manifest permission ACCESS_NETWORK_STATE.
            if (info == null) return false;
            var infoState = info.GetState();
            return infoState == NetworkInfo.State.Connected;
        }
    }
}