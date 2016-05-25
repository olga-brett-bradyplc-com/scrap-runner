using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Brady.ScrapRunner.Mobile.Models;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Binding.Droid.Views;

namespace Brady.ScrapRunner.Mobile.Droid.Controls.ChatAdapter
{
    public class ChatMessageAdapter : MvxAdapter
    {
        public ChatMessageAdapter(Context context, IMvxAndroidBindingContext bindingContext)
            : base(context, bindingContext)
        {
        }

        protected override View GetView(int position, View convertView, ViewGroup parent, int templateId)
        {
            var tempView = base.GetView(position, convertView, parent, templateId);
            var item = GetRawItem(position);
            var msgContainer = tempView.FindViewById<LinearLayout>(Resource.Id.msg_container);

            if (msgContainer != null)
            {
                var msg = (MessagesModel) item;
                var layoutParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.MatchParent);
                layoutParams.SetMargins(30, 7, 0, 7);
                switch (msg.MsgSource)
                {
                    case "R":
                        msgContainer.SetBackgroundResource(Resource.Drawable.chatbubble_remote_new);
                        msgContainer.LayoutParameters = layoutParams;
                        break;
                    case "L":
                    case "B":
                        msgContainer.SetBackgroundResource(Resource.Drawable.chatbubble_local_new);
                        break;
                    default:
                        msgContainer.SetBackgroundResource(Resource.Drawable.chatbubble_local_new);
                        break;
                }
            }

            return tempView;
        }
    }
}