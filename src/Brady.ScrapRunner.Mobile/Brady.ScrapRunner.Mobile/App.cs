namespace Brady.ScrapRunner.Mobile
{
    using AutoMapper;
    using Interfaces;
    using Resources;
    using Xamarin.Forms;
    using MvvmCross.Platform.IoC;
    using Acr.UserDialogs;
    using Models;
    using MvvmCross.Platform;
    using Services;

    public class App : MvvmCross.Core.ViewModels.MvxApplication
    {
        public override void Initialize()
        {
            CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

            Mvx.RegisterSingleton(() => UserDialogs.Instance);

            Mvx.RegisterType<IRepository<ContainerMasterModel>, 
                SqliteRepository<ContainerMasterModel>>();
            Mvx.RegisterType<IRepository<CustomerDirectionModel>,
                SqliteRepository<CustomerDirectionModel>>();
            Mvx.RegisterType<IRepository<EmployeeMasterModel>,
                SqliteRepository<EmployeeMasterModel>>();
            Mvx.RegisterType<IRepository<TripModel>,
                SqliteRepository<TripModel>>();
            Mvx.RegisterType<IRepository<TripSegmentModel>,
                SqliteRepository<TripSegmentModel>>();
            Mvx.RegisterType<IRepository<TripSegmentContainerModel>,
                SqliteRepository<TripSegmentContainerModel>>();

            Mvx.RegisterSingleton(Mvx.IocConstruct<DemoDataGenerator>);

            if (Device.OS != TargetPlatform.WinPhone)
                AppResources.Culture = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();

            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<ScrapRunnerMapperProfile>();
            });

            RegisterAppStart<ViewModels.SignInViewModel>();
        }
    }
}