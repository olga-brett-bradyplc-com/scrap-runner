namespace Brady.ScrapRunner.Mobile.Droid.Views
{
    using MvvmCross.Core.ViewModels;
    using MvvmCross.Droid.Support.V7.AppCompat;

    public abstract class BaseActivity<T> : MvxAppCompatActivity<T> where T : class, IMvxViewModel
    {
    }
}