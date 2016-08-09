using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using Brady.ScrapRunner.Mobile.Interfaces;
using Brady.ScrapRunner.Mobile.Models;
using MvvmCross.Binding.Droid.Views;
using MvvmCross.Platform;


namespace Brady.ScrapRunner.Mobile.Droid.Controls.GroupListView
{    
    /// <summary>
    /// Taken from https://github.com/deapsquatter/MvvmCross.DeapExtensions
    /// </summary>
    public class BindableGroupListAdapter : MvxAdapter
    {
        private int _groupTemplateId;
        private IEnumerable _itemsSource;
        private readonly Context _context;

        public BindableGroupListAdapter(Context context) : base(context)
        {
            _context = context;
        }

        public int GroupTemplateId
        {
            get { return _groupTemplateId; }
            set
            {
                if (_groupTemplateId == value)
                    return;
                _groupTemplateId = value;

                // since the template has changed then let's force the list to redisplay by firing NotifyDataSetChanged()
                if (ItemsSource != null)
                    NotifyDataSetChanged();
            }
        }

        private void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            FlattenAndSetSource(_itemsSource);
        }

        private void FlattenAndSetSource(IEnumerable value)
        {
            var list = value.Cast<object>();
            var flattened = list.SelectMany(FlattenHierachy);
            base.SetItemsSource(flattened.ToList());
        }

        protected override void SetItemsSource(IEnumerable value)
        {
            //if (_itemsSource == value)
            //    return;
            var existingObservable = _itemsSource as INotifyCollectionChanged;
            if (existingObservable != null)
                existingObservable.CollectionChanged -= OnItemsSourceCollectionChanged;

            _itemsSource = value;

            var newObservable = _itemsSource as INotifyCollectionChanged;
            if (newObservable != null)
                newObservable.CollectionChanged += OnItemsSourceCollectionChanged;

            if (value != null)
            {
                FlattenAndSetSource(value);
            }
            else
                base.SetItemsSource(null);
        }

        public class FlatItem
        {
            public bool IsGroup;
            public object Item;
        }

        private IEnumerable<object> FlattenHierachy(object group)
        {
            yield return new FlatItem { IsGroup = true, Item = group };
            var items = group as IEnumerable;
            if (items != null)
                foreach (object d in items)
                    yield return new FlatItem { IsGroup = false, Item = d };
        }

        protected override View GetBindableView(View convertView, object source, int templateId)
        {
            var item = (FlatItem)source;
            return base.GetBindableView(convertView, item.Item, item.IsGroup ? GroupTemplateId : ItemTemplateId);
        }

        protected override View GetView(int position, View convertView, ViewGroup parent, int templateId)
        {
            var tempView = base.GetView(position, convertView, parent, templateId);
            var item = GetRawItem(position);

            var parentLayout = tempView.FindViewById<RelativeLayout>(Resource.Id.TripLayoutParent);
            var icon = tempView.FindViewById<ImageView>(Resource.Id.arrow_image);
            var info = tempView.FindViewById<TextView>(Resource.Id.tripContainerInfo);
            var commodity = tempView.FindViewById<TextView>(Resource.Id.tripContainerCommodityDesc);
            var location = tempView.FindViewById<TextView>(Resource.Id.tripContainerLocation);
            var reviewReason = tempView.FindViewById<TextView>(Resource.Id.tripContainerReviewReason);

            if (parentLayout != null)
            {
                var flatItem = (FlatItem) item;
                var tscm = (TripSegmentContainerModel) flatItem.Item;

                if( tscm.SelectedTransaction )
                    parentLayout.SetBackgroundColor(new Color(ContextCompat.GetColor(_context, Resource.Color.current_transaction_background)));
                else
                    parentLayout.SetBackgroundColor(Color.ParseColor("#ffffff"));

                if (icon != null)
                {
                    switch (tscm.TripSegContainerReviewFlag)
                    {
                        case "N":
                            icon.SetImageResource(Resource.Drawable.check_circle_green_tint_icon);
                            break;
                        case "E":
                            icon.SetImageResource(Resource.Drawable.cancelled_red_tint_icon);
                            break;
                        default:
                            icon.SetImageResource(Resource.Drawable.ic_keyboard_arrow_right_black_24dp);
                            break;
                    }
                }

                if (info != null)
                    info.Text = $"{tscm.DefaultTripSegContainerNumber} {tscm.DefaultTripContainerTypeSize}";

                if (string.IsNullOrEmpty(tscm.TripSegContainerReivewReasonDesc))
                {
                    reviewReason.Visibility = ViewStates.Gone;
                }
                else
                {
                    reviewReason.Visibility = ViewStates.Visible;
                    reviewReason.Text = tscm.TripSegContainerReivewReasonDesc;
                }
            }

            return tempView;
        }
    }
}