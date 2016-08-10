namespace Brady.ScrapRunner.Mobile.Droid.Activities
{
    using Android.OS;
    using Android.Support.V7.Widget;
    using MvvmCross.Droid.Support.V7.AppCompat;
    using ViewModels;

    public abstract class BaseActivity<TViewModel> : MvxAppCompatActivity<TViewModel> where TViewModel : BaseViewModel
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(ActivityId);
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            if (toolbar != null)
            {
                SetSupportActionBar(toolbar);
                if (!string.IsNullOrEmpty(ViewModel.Title)) SupportActionBar.Title = ViewModel.Title;
                if (!string.IsNullOrEmpty(ViewModel.SubTitle)) SupportActionBar.Subtitle = ViewModel.SubTitle;
            }
        }

        protected abstract int ActivityId { get; }
    }
}