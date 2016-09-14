using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

using Android.Content;
using Android.Views;
using Android.Widget;
using Brady.ScrapRunner.Mobile.Helpers;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Binding.Droid.Views;

namespace Brady.ScrapRunner.Mobile.Droid.Controls.GroupListView
{
    public class BindableGroupListScaleAdapter : MvxAdapter
    {
        private int _groupTemplateId;
        private IEnumerable _itemsSource;
        private readonly Context _context;

        public BindableGroupListScaleAdapter(Context context) : base(context)
        {
            _context = context;
        }

        public BindableGroupListScaleAdapter(Context context, IMvxAndroidBindingContext bindingContext, int groupTemplateId)
            : base(context, bindingContext)
        {
            GroupTemplateId = groupTemplateId;
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

            if (parentLayout != null)
            {
                var flatItem = (FlatItem) item;
                var containerGroup = (ContainerMasterWithTripContainer) flatItem.Item;
                
                if (icon != null)
                {
                    switch (containerGroup.ContainerMaster.ContainerReviewFlag)
                    {
                        case "D":
                            icon.SetImageResource(Resource.Drawable.check_circle_green_tint_icon);
                            break;
                        case "R":
                            icon.SetImageResource(Resource.Drawable.cancelled_red_tint_icon);
                            break;
                        default:
                            icon.SetImageResource(Resource.Drawable.ic_keyboard_arrow_right_black_24dp);
                            break;
                    }
                }
            }

            return tempView;
        }
    }
}