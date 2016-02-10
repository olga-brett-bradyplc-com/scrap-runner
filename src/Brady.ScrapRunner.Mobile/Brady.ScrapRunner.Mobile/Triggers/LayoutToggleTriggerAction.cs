using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Brady.ScrapRunner.Mobile.Triggers
{
    public class LayoutToggleTriggerAction : TriggerAction<Layout>
    {
        protected override async void Invoke(Layout sender)
        {
            await sender.LayoutTo(new Rectangle(1380, 500, 800, 75), 1000, Easing.SpringOut);
        }
    }
}
