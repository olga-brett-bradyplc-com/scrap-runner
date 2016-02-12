namespace Brady.ScrapRunner.Mobile.Droid.Views
{
    using Android.Support.V7.Widget;
    using MvvmCross.Core.ViewModels;
    using MvvmCross.Droid.Support.V7.AppCompat;

    public abstract class BaseActivity<T> : MvxAppCompatActivity<T> where T : class, IMvxViewModel
    {
        public override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            if (toolbar != null)
            {
                SetSupportActionBar(toolbar);
            }
        }
    }
}