using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Brady.ScrapRunner.Mobile.Renderers
{
    class CollapsibleListView : ListView
    {
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create<CollapsibleListView, string>(p => p.Title, "");

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly BindableProperty ExpandedProperty =
            BindableProperty.Create<CollapsibleListView, bool>(p => p.Expanded, false);

        public bool Expanded
        {
            get { return (bool) GetValue(ExpandedProperty);  }
            set {  SetValue(ExpandedProperty, value); }
        }

        public static readonly BindableProperty CollapsedCellCountProperty =
            BindableProperty.Create<CollapsibleListView, int>(p => p.CollapsedCellCount, 1);

        public int CollapsedCellCount
        {
            get { return (int) GetValue(CollapsedCellCountProperty); }
            set { SetValue(CollapsedCellCountProperty, value); }
        }
    }
}
