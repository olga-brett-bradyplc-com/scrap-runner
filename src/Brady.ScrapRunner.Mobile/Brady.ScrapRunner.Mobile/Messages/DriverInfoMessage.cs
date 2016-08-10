using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmCross.Plugins.Messenger;

namespace Brady.ScrapRunner.Mobile.Messages
{
    public class DriverInfoMessage : MvxMessage
    {
        public DriverInfoMessage(object sender) : base(sender)
        {
        }

        public string DriverName { get; set; }
        public string DriverYard { get; set; }
        public string DriverVehicle { get; set; }
    }
}
