namespace Brady.ScrapRunner.Mobile.Droid.Views
{
    using Android.Support.V7.Widget;
    using MvvmCross.Droid.Support.V7.AppCompat;
    using ViewModels;

    public abstract class BaseActivity<T> : MvxAppCompatActivity<T> where T : BaseViewModel
    {
        public override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            if (toolbar != null)
            {
                SetSupportActionBar(toolbar);
                SupportActionBar.Title = ViewModel.Title;
                SupportActionBar.Subtitle = ViewModel.SubTitle;
            }
        }
    }
}