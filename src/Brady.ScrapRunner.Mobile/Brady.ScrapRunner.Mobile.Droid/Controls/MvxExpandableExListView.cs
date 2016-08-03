using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.Views;

namespace Brady.ScrapRunner.Mobile.Droid.Controls
{
    /// <summary>
    /// I'm not fond of this implementation, but having some serious issues with trying to get a custom
    /// ExpandableListViewAdapter to work with MvvmCross.
    /// 
    /// The premise of this impl is to allow our ExpandableListView to uniformally scroll with other sibiling elements
    /// in a parent ScrollView. Since android ListViews, et. al., have their own scrolling mechanism, typically you 
    /// wouldn't place one within a scrollview to begin with. However, we have the need to do that within our RouteDetail
    /// view. What it basically does is force a ListView height depending on what's shown within the ListView, thus never letting
    /// the internal scrolling mechanism to kick in. 
    /// 
    /// An alternative to using the below would maybe be to put together a custom control that mocks a ListView UI, just with LinearLayouts
    /// </summary>
    public class MvxExpandableExListView : MvxExpandableListView
    {
        public MvxExpandableExListView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public MvxExpandableExListView(Context context, IAttributeSet attrs, MvxExpandableListAdapter adapter) : base(context, attrs, adapter)
        {
        }

        public MvxExpandableExListView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        //public override bool ExpandGroup(int groupPos, bool animate)
        //{
        //    base.ExpandGroup(groupPos, animate);
        //    SetListViewHeight();
        //    return true;
        //}

        //public override bool CollapseGroup(int groupPos)
        //{
        //    base.CollapseGroup(groupPos);
        //    SetListViewHeight();
        //    return true;
        //}

        protected override void OnDraw(Canvas canvas)
        {
            SetListViewHeight();
        }
        
        private void SetListViewHeight()
        {
            var adapter = Adapter;
            if (adapter == null) return;

            var desiredWidth = MeasureSpec.MakeMeasureSpec(Width, MeasureSpecMode.AtMost);
            var totalHeight = 0;
            View view = null;
            for (var i = 0; i < adapter.Count; i++)
            {
                view = adapter.GetView(i, view, this);
                if (i == 0)
                    view.LayoutParameters = new ViewGroup.LayoutParams(desiredWidth, ViewGroup.LayoutParams.WrapContent);

                view.Measure(desiredWidth, 0);
                totalHeight += view.MeasuredHeight;
            }

            var parameters = LayoutParameters;
            parameters.Height = totalHeight + (DividerHeight * (adapter.Count - 1));
            LayoutParameters = parameters;
            RequestLayout();
        }

        private void ExpandAll()
        {
            var adapter = ExpandableListAdapter;
            if (adapter == null) return;

            for (var i = 0; i < adapter.GroupCount; i++)
                ExpandGroup(i);
        }
    }
}