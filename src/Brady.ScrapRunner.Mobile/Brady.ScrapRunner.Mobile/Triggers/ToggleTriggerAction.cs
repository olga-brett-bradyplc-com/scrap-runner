using Brady.ScrapRunner.Mobile.Renderers;
using Xamarin.Forms;

namespace Brady.ScrapRunner.Mobile.Triggers
{
    class ToggleTriggerAction : TriggerAction<StatusButton>
    {
        public ToggleTriggerAction() {}

        public int MoveX { get; set; }
        public int MoveY { get; set; }
        public int Duration { get; set; }

        protected override async void Invoke(StatusButton sender)
        {
            await sender.TranslateTo(MoveX, MoveY, (uint)Duration, Easing.Linear); //Easing.BounceOut);
        }
    }
}
