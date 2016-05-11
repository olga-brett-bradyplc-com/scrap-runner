using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Shared.Attributes;


namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof(MainViewModel), Resource.Id.content_frame, true)]
    [Register("brady.scraprunner.mobile.droid.fragments.MessagesFragment")]
    public class MessagesFragment : BaseFragment<MessagesViewModel>
    {
        protected override int FragmentId => Resource.Layout.fragment_messages;
        protected override bool NavMenuEnabled => false;
    }
}