namespace Brady.ScrapRunner.Mobile.Droid.Activities
{
    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.OS;
    using Android.Support.V7.Widget;
    using Android.Views;
    using MvvmCross.Binding.Droid.BindingContext;
    using MvvmCross.Binding.Droid.Views;
    using MvvmCross.Droid.Support.V7.AppCompat;
    using ViewModels;

    [Activity(
        Label = "Notifications",
        LaunchMode = LaunchMode.SingleTop,
        Name = "brady.scraprunner.mobile.droid.activities.NotificationActivity")]
    public class NotificationActivity : MvxAppCompatActivity<NotificationViewModel>
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_notification);
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = ViewModel.Title;
            var list = FindViewById<MvxListView>(Resource.Id.notification_list);
            list.Adapter = new NotificationAdapter(this, (MvxAndroidBindingContext)BindingContext);
        }
    }

    public class NotificationAdapter : MvxAdapter
    {
        public NotificationAdapter(Context context, IMvxAndroidBindingContext bindingContext)
            : base(context, bindingContext)
        {
        }

        protected override View GetView(int position, View convertView, ViewGroup parent, int templateId)
        {
            return base.GetView(position, convertView, parent, Resource.Layout.item_notification);
        }
    }
}