using System;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Brady.ScrapRunner.Mobile.Droid.Controls.ImageGridViewAdapter;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Binding.Droid.Views;
using MvvmCross.Droid.Shared.Attributes;

namespace Brady.ScrapRunner.Mobile.Droid.Fragments
{
    [MvxFragment(typeof(MainViewModel), Resource.Id.content_frame, true)]
    [Register("brady.scraprunner.mobile.droid.fragments.PhotosFragment")]
    public class PhotosFragment : BaseFragment<PhotosViewModel>
    {
        protected override int FragmentId => Resource.Layout.fragment_photos;
        protected override bool NavMenuEnabled => false;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            HasOptionsMenu = true;
            var gridView = View.FindViewById<MvxGridView>(Resource.Id.PhotosGridView);
            gridView.Adapter = new ImageGridViewAdapter(Activity, (MvxAndroidBindingContext)BindingContext);
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.addphoto_menu, menu);
            base.OnCreateOptionsMenu(menu, inflater);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.add_another_photo_nav:
                    ViewModel.AddAnotherPhotoCommand.Execute();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
    }
}