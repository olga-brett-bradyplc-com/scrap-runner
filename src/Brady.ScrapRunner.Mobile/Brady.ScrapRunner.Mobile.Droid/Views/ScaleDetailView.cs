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
using Brady.ScrapRunner.Mobile.ViewModels;

namespace Brady.ScrapRunner.Mobile.Droid.Views
{
    [Activity(Label = "ScaleDetailView")]
    public class ScaleDetailView : BaseActivity<ScaleDetailViewModel>
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_scaledetail);
        }
    }
}