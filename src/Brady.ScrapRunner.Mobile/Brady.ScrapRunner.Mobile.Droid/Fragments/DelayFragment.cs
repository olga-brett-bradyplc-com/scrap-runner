using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Brady.ScrapRunner.Mobile.Droid.Messages;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Droid.Shared.Attributes;
using MvvmCross.Platform;
using MvvmCross.Plugins.Messenger;

namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof (MainViewModel), Resource.Id.content_frame, true)]
    [Register("brady.scraprunner.mobile.droid.fragments.DelayFragment")]
    public class DelayFragment : BaseFragment<DelayViewModel>
    {
        private IMvxMessenger _mvxMessenger;

        protected override int FragmentId => Resource.Layout.fragment_delay;
        protected override bool NavMenuEnabled => true;
        protected override int NavColor => Resource.Color.delayed;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            var timer = View.FindViewById<Chronometer>(Resource.Id.chronometer);
            timer.Start();

            _mvxMessenger = Mvx.Resolve<IMvxMessenger>();

            _mvxMessenger.Publish(new MenuStateMessage(this) { Context = MenuState.OnDelay });
        }
    }
}