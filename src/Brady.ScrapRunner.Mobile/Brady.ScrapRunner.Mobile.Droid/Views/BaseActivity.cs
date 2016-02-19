namespace Brady.ScrapRunner.Mobile.Droid.Views
{
    using System;
    using System.ComponentModel;
    using Android.Support.V7.Widget;
    using MvvmCross.Droid.Support.V7.AppCompat;
    using MvvmCross.Platform.WeakSubscription;
    using ViewModels;

    public abstract class BaseActivity<T> : MvxAppCompatActivity<T> where T : BaseViewModel
    {
        private IDisposable _titleToken;
        private IDisposable _subTitleToken;

        public override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            if (toolbar != null)
            {
                SetSupportActionBar(toolbar);
                if (!string.IsNullOrEmpty(ViewModel.Title))
                    SupportActionBar.Title = ViewModel.Title;
                if (!string.IsNullOrEmpty(ViewModel.SubTitle))
                    SupportActionBar.Subtitle = ViewModel.SubTitle;
                _titleToken = ViewModel.WeakSubscribe(() => ViewModel.Title, OnTitleChanged);
                _subTitleToken = ViewModel.WeakSubscribe(() => ViewModel.SubTitle, OnSubTitleChanged);
            }
        }

        public override void OnDetachedFromWindow()
        {
            if (_titleToken != null)
            {
                _titleToken.Dispose();
                _titleToken = null;
            }
            if (_subTitleToken != null)
            {
                _subTitleToken.Dispose();
                _subTitleToken = null;
            }
            base.OnDetachedFromWindow();
        }

        private void OnTitleChanged(object sender, PropertyChangedEventArgs args)
        {
            if (SupportActionBar == null) return;
            SupportActionBar.Title = ViewModel.Title;
        }

        private void OnSubTitleChanged(object sender, PropertyChangedEventArgs args)
        {
            if (SupportActionBar == null) return;
            SupportActionBar.Subtitle = ViewModel.SubTitle;
        }
    }
}