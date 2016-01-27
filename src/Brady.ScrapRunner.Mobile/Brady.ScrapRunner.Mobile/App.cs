namespace Brady.ScrapRunner.Mobile
{
    using Interfaces;
    using Resources;
    using Views;
    using Xamarin.Forms;

    public class App : Application
    {
        public App()
        {
            if (Device.OS != TargetPlatform.WinPhone)
                AppResources.Culture = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();
            MainPage = new NavigationPage(new SignInView());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
