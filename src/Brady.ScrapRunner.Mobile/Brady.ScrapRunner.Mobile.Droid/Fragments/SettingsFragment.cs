using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Shared.Attributes;

namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof(MainViewModel), Resource.Id.content_frame)]
    [Register("brady.scraprunner.mobile.droid.fragments.SettingsFragment")]
    public class SettingsFragment : BaseFragment<SettingsViewModel>
    {
        protected override int FragmentId => Resource.Layout.fragment_settings;
        // @TODO : Determine if logged in, show menu, otherwise, hide menu?
        protected override bool NavMenuEnabled => false;
    }
}