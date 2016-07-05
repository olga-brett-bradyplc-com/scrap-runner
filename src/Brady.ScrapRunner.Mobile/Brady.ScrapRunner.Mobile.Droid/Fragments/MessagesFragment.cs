using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Brady.ScrapRunner.Mobile.Droid.Activities;
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

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            HasOptionsMenu = true;
        }
        
        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.messages_menu, menu);
            base.OnCreateOptionsMenu(menu, inflater);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var baseActivity = ((MainActivity)Activity);
            switch (item.ItemId)
            {
                case Resource.Id.send_new_message_nav:
                    baseActivity.CloseOptionsMenu();
                    ViewModel.NewMessageCommand.Execute();
                    return true;
                default:
                    baseActivity.CloseOptionsMenu();
                    return base.OnOptionsItemSelected(item);
            }
        }
    }
}