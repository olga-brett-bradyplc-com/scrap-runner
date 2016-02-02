using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brady.ScrapRunner.Mobile.Helpers
{
    public class Grouping<TKey, TItem> : ObservableCollection<TItem>
    {
        public TKey Key { get; private set; }

        public Grouping(TKey key, IEnumerable<TItem> items)
        {
            Key = key;
            foreach (var item in items)
                Items.Add(item);
        }
    }
}
