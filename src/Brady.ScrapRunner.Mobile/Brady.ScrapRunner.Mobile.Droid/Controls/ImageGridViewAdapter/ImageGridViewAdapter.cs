using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Binding.Droid.Views;
using MvvmCross.Droid.Views;

namespace Brady.ScrapRunner.Mobile.Droid.Controls.ImageGridViewAdapter
{
    public class ImageGridViewAdapter : MvxAdapter
    {
        public ImageGridViewAdapter(Context context, IMvxAndroidBindingContext bindingContext)
            : base(context, bindingContext)
        {
        }

        protected override View GetView(int position, View convertView, ViewGroup parent, int templateId)
        {
            var tempView = base.GetView(position, convertView, parent, templateId);
            var item = GetRawItem(position);
            var imageContainer = tempView.FindViewById<ImageView>(Resource.Id.GridViewImage);

            if (imageContainer != null)
            {
                var imagePath = (string) item;
                imageContainer.SetImageBitmap(BitmapFactory.DecodeFile(imagePath));
            }

            return tempView;
        }
    }
}