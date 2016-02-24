using Brady.ScrapRunner.Mobile.Resources;
using Microsoft.VisualBasic;
using MvvmCross.Platform.IoC;

namespace Brady.ScrapRunner.Mobile
{
    using Acr.UserDialogs;
    using AutoMapper;
    using Interfaces;
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

            // @TODO: Implement a factory to avoid this (http://stackoverflow.com/a/20691906)
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

            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<ScrapRunnerMapperProfile>();
            });

            RegisterAppStart<ViewModels.SignInViewModel>();
        }
    }
}
