﻿using System.Collections.Generic;
using Brady.ScrapRunner.Mobile.Models;

namespace Brady.ScrapRunner.Mobile.Messages
{
    using MvvmCross.Plugins.Messenger;

    public class TripResequencedMessage : MvxMessage
    {
        public TripResequencedMessage(object sender) : base(sender)
        {
        }
    }
}