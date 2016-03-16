using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.ResourceHelpers;
using MvvmCross.Binding.Droid.Views;

namespace Brady.ScrapRunner.Mobile.Droid.Controls.GroupListView
{
    /// <summary>
    /// Taken from https://github.com/deapsquatter/MvvmCross.DeapExtensions
    /// </summary>
    public class BindableGroupListView : MvxListView
    {
        public BindableGroupListView(Context context, IAttributeSet attrs)
                    : this(context, attrs, new BindableGroupListAdapter(context))
        {
        }

        public BindableGroupListView(Context context, IAttributeSet attrs, BindableGroupListAdapter adapter)
            : base(context, attrs, adapter)
        {
            var groupTemplateId = MvxAttributeHelpers.ReadAttributeValue(context, attrs,
                                                                               MvxAndroidBindingResource.Instance
                                                                                 .ListViewStylableGroupId,
                                                                               AndroidBindingResource.Instance
                                                                               .BindableListGroupItemTemplateId);
            adapter.GroupTemplateId = groupTemplateId;
        }

        public ICommand GroupClick { get; set; }

        protected override void ExecuteCommandOnItem(ICommand command, int position)
        {
            var item = Adapter.GetRawItem(position);
            if (item == null)
                return;
            var flatItem = (BindableGroupListAdapter.FlatItem)item;

            if (flatItem.IsGroup)
                command = GroupClick;

            if (command == null)
                return;

            if (!command.CanExecute(flatItem.Item))
                return;

            command.Execute(flatItem.Item);
        }
    }
}