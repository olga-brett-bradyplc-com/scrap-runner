using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using Brady.ScrapRunner.Mobile.Models;
using Brady.ScrapRunner.Mobile.ViewModels;
using Java.Lang;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.Droid.Binders;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Binding.Droid.Views;

namespace Brady.ScrapRunner.Mobile.Droid.Controls
{
    public class TripPagerAdapter : PagerAdapter
    {
        private readonly Context _context;
        private readonly ICollection<TripLegWrapper> _tripLegs;
        private readonly CustomerDirectionsWrapper _directions;
        private readonly IMvxBindingContext _bindingContext;

        public TripPagerAdapter(Context context, IMvxBindingContext bindingContext, ICollection<TripLegWrapper> tripLegs, CustomerDirectionsWrapper directions)
        {
            _context = context;
            _tripLegs = tripLegs;
            _directions = directions;
            _bindingContext = bindingContext;
        }

        public override Java.Lang.Object InstantiateItem(ViewGroup collection, int position)
        {
            using (new MvxBindingContextStackRegistration<IMvxAndroidBindingContext>((IMvxAndroidBindingContext) _bindingContext))
            {
                var inflater = LayoutInflater.From(_context);
                var layout = inflater.Inflate(Resource.Layout.item_tripleg_page, collection, false);

                if (position == (Count - 1) && _directions != null)
                {
                    layout = inflater.Inflate(Resource.Layout.item_directions_page, collection, false);
                    collection.AddView(SetDirectionsInformation(layout, _directions));
                }
                else
                {
                    collection.AddView(SetLayoutInformation(layout, _tripLegs.ElementAt(position)));
                }

                return layout;
            }
        }

        public override bool IsViewFromObject(View view, Java.Lang.Object objectValue)
        {
            return view == objectValue;
        }

        public override void DestroyItem(ViewGroup container, int position, Java.Lang.Object view)
        {
            container.RemoveView((View) view);
        }
        
        public override int Count => _tripLegs.Count + (_directions != null ? 1 : 0);

        // Replace with BindingInflate?
        private View SetLayoutInformation(View viewGroup, TripLegWrapper tripLeg)
        {
            var companyName = viewGroup.FindViewById<TextView>(Resource.Id.trip_companyname);
            companyName.Text = tripLeg.TripCustName;

            var tripAddress = viewGroup.FindViewById<TextView>(Resource.Id.trip_address);
            tripAddress.Text = tripLeg.TripCustAddress;

            var tripcitystatezip = viewGroup.FindViewById<TextView>(Resource.Id.trip_citystatezip);
            tripcitystatezip.Text = tripLeg.TripCustCityStateZip;

            var notes = viewGroup.FindViewById<TextView>(Resource.Id.detail_notes_content);
            notes.Text = tripLeg.Notes;

            var tripSegments = viewGroup.FindViewById<MvxExpandableExListView>(Resource.Id.TripSegmentContainerList);
            tripSegments.ItemsSource = tripLeg.TripSegments;

            return viewGroup;
        }

        private View SetDirectionsInformation(View viewGroup, CustomerDirectionsWrapper directions)
        {
            var companyName = viewGroup.FindViewById<TextView>(Resource.Id.trip_companyname);
            companyName.Text = directions.TripCustName;

            var tripAddress = viewGroup.FindViewById<TextView>(Resource.Id.trip_address);
            tripAddress.Text = directions.TripCustAddress;

            var tripcitystatezip = viewGroup.FindViewById<TextView>(Resource.Id.trip_citystatezip);
            tripcitystatezip.Text = directions.TripCustCityStateZip;

            var list = viewGroup.FindViewById<MvxListView>(Resource.Id.TripDirections);
            list.ItemsSource = directions.Directions;

            return viewGroup;
        }
    }
}