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
using MvvmCross.Plugins.Messenger;

namespace Brady.ScrapRunner.Mobile.Droid.Messages
{
    public class MenuStateMessage : MvxMessage
    {
        public MenuStateMessage(object sender) : base(sender)
        {
        }

        public MenuState Context { get; set; }
    }

    public enum MenuState
    {
        Avaliable,
        OnTrip,
        OnDelay
    }
}