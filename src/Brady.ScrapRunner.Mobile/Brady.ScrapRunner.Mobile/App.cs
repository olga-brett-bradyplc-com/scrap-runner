using Brady.ScrapRunner.Mobile.Renderers;

namespace Brady.ScrapRunner.Mobile
{
    using AutoMapper;
    using GalaSoft.MvvmLight.Ioc;
    using GalaSoft.MvvmLight.Views;
    using Interfaces;
    using Models;
    using Resources;
    using Services;
    using Views;
    using Xamarin.Forms;

    public class App : Application
    {
        private static Locator _locator;
        public static Locator Locator => _locator ?? (_locator = new Locator());

        public App()
        {
            if (Device.OS != TargetPlatform.WinPhone)
                AppResources.Culture = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();
            var isRegistered = SimpleIoc.Default.IsRegistered<INavigationService>();
            var nav = isRegistered ?
                (NavigationService)SimpleIoc.Default.GetInstance(typeof(INavigationService)) :
                new NavigationService();
            if (!isRegistered)
            {
                ConfigureViews(nav);
                SimpleIoc.Default.Register<INavigationService>(() => nav);
                Mapper.Initialize(cfg => {
                    cfg.AddProfile<ScrapRunnerMapperProfile>();
                });
            }
            var firstPage = new NavigationPage(new SignInView());
            nav.Initialize(firstPage);
            MainPage = firstPage;
        }

        protected void ConfigureViews(NavigationService nav)
        {
            nav.Configure(Locator.SettingsView, typeof(SettingsView));
            nav.Configure(Locator.ChangeLanguageView, typeof(ChangeLanguageView));
            nav.Configure(Locator.SignInView, typeof(SignInView));
            nav.Configure(Locator.PowerUnitView, typeof(PowerUnitView));
            nav.Configure(Locator.RouteSummaryView, typeof(RouteSummaryView));
            nav.Configure(Locator.RouteDetailView, typeof(RouteDetailView));
            nav.Configure(Locator.RouteDirectionsView, typeof(RouteDirectionsView));
            nav.Configure(Locator.TransactionSummaryView, typeof(TransactionSummaryView));
            nav.Configure(Locator.TransactionDetailView, typeof(TransactionDetailView));
            nav.Configure(Locator.ScaleSummaryView, typeof(ScaleSummaryView));
            nav.Configure(Locator.ScaleDetailView, typeof(ScaleDetailView));
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
