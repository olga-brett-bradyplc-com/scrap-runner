namespace Brady.ScrapRunner.Mobile.Models
{
    using GalaSoft.MvvmLight.Ioc;
    using Interfaces;
    using Microsoft.Practices.ServiceLocation;
    using ViewModels;

    public class Locator
    {
        public Locator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<IRepository<EmployeeMasterModel>,
                SqliteRepository<EmployeeMasterModel>>();
            SimpleIoc.Default.Register<IRepository<CustomerDirectionModel>,
                SqliteRepository<CustomerDirectionModel>>();

            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<ChangeLanguageViewModel>();
            SimpleIoc.Default.Register<SignInViewModel>();
            SimpleIoc.Default.Register<PowerUnitViewModel>();
            SimpleIoc.Default.Register<RouteSummaryViewModel>();
            SimpleIoc.Default.Register<RouteDetailViewModel>();
            SimpleIoc.Default.Register<RouteDirectionsViewModel>();
            SimpleIoc.Default.Register<TransactionsViewModel>();
        }

        public const string SettingsView = "SettingsView";
        public const string ChangeLanguageView = "ChangeLanguageView";
        public const string SignInView = "SignInView";
        public const string PowerUnitView = "PowerUnitView";
        public const string RouteSummaryView = "RouteSummaryView";
        public const string RouteDetailView = "RouteDetailView";
        public const string RouteDirectionsView = "RouteDirectionsView";
        public const string TransactionsView = "TransactionsView";

        public ChangeLanguageViewModel ChangeLanguage => ServiceLocator.Current.GetInstance<ChangeLanguageViewModel>();
        public SettingsViewModel Settings => ServiceLocator.Current.GetInstance<SettingsViewModel>();
        public SignInViewModel SignIn => ServiceLocator.Current.GetInstance<SignInViewModel>();
        public PowerUnitViewModel PowerUnit => ServiceLocator.Current.GetInstance<PowerUnitViewModel>();
        public RouteSummaryViewModel RouteSummary => ServiceLocator.Current.GetInstance<RouteSummaryViewModel>();
        public RouteDetailViewModel RouteDetail => ServiceLocator.Current.GetInstance<RouteDetailViewModel>();
        public RouteDirectionsViewModel RouteDirections => ServiceLocator.Current.GetInstance<RouteDirectionsViewModel>();
        public TransactionsViewModel Transactions => ServiceLocator.Current.GetInstance<TransactionsViewModel>();
    }
}
