using System.Globalization;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Binding.Droid.Views;
using MvvmCross.Droid.Shared.Attributes;

namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof(MainViewModel), Resource.Id.content_frame, true)]
    [Register("brady.scraprunner.mobile.droid.fragments.ChangeLanguageFragment")]
    public class ChangeLanguageFragment : BaseFragment<ChangeLanguageViewModel>
    {
        protected override int FragmentId => Resource.Layout.fragment_changelanguage;
        protected override bool NavMenuEnabled => false;

        public override void OnViewCreated(View view, Bundle savedStateInstance)
        {
            var languageList = View.FindViewById<MvxListView>(Resource.Id.LanguageListView);
            languageList.Adapter = new LanguageSelectorAdapter(Activity, (MvxAndroidBindingContext)BindingContext);
        }

        private class LanguageSelectorAdapter : MvxAdapter
        {
            public LanguageSelectorAdapter(Context context, IMvxAndroidBindingContext bindingContext)
                : base(context, bindingContext)
            {
            }

            protected override View GetView(int position, View convertView, ViewGroup parent, int templateId)
            {
                var tempView = base.GetView(position, convertView, parent, templateId);
                var item = GetRawItem(position);
                var checkedView = tempView.FindViewById<ImageView>(Resource.Id.selected_language_image);

                if (checkedView != null)
                {
                    var cul = (CultureInfo)item;
                    if (CultureInfo.CurrentCulture.DisplayName == cul.DisplayName)
                        checkedView.SetImageResource(Resource.Drawable.ic_check_black_24dp);
                }

                return tempView;
            }
        }
    }
}