using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using Brady.ScrapRunner.Mobile.Droid.Activities;
using Brady.ScrapRunner.Mobile.Helpers;
using Brady.ScrapRunner.Mobile.Models;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Shared.Attributes;
using MvvmCross.Platform.WeakSubscription;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof(MainViewModel), Resource.Id.content_frame, true)]
    [Register("brady.scraprunner.mobile.droid.fragments.NewMessageFragment")]
    public class NewMessageFragment : BaseFragment<NewMessageViewModel>
    {
        protected override int FragmentId => Resource.Layout.fragment_newmessage;
        protected override bool NavMenuEnabled => false;

    }
}