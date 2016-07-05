using System;
using Brady.ScrapRunner.Domain.Models;
using Brady.ScrapRunner.Mobile.Helpers;
using Brady.ScrapRunner.Mobile.Resources;
using Microsoft.VisualBasic;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using MvvmCross.Platform.IoC;
using Plugin.Settings.Abstractions;
using static MvvmCross.Platform.Mvx;

namespace Brady.ScrapRunner.Mobile
{
    using Acr.UserDialogs;
    using AutoMapper;
    using Interfaces;
    using Models;
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
            Mvx.RegisterType<IRepository<CustomerDirectionsModel>,
                SqliteRepository<CustomerDirectionsModel>>();
            Mvx.RegisterType<IRepository<DriverStatusModel>,
                SqliteRepository<DriverStatusModel>>();
            Mvx.RegisterType<IRepository<EmployeeMasterModel>,
                SqliteRepository<EmployeeMasterModel>>();
            Mvx.RegisterType<IRepository<TripModel>,
                SqliteRepository<TripModel>>();
            Mvx.RegisterType<IRepository<TripSegmentModel>,
                SqliteRepository<TripSegmentModel>>();
            Mvx.RegisterType<IRepository<TripSegmentContainerModel>,
                SqliteRepository<TripSegmentContainerModel>>();
            Mvx.RegisterType<IRepository<PreferenceModel>,
                SqliteRepository<PreferenceModel>>();
            Mvx.RegisterType<IRepository<PowerMasterModel>,
                SqliteRepository<PowerMasterModel>>();
            Mvx.RegisterType<IRepository<CustomerLocationModel>,
                SqliteRepository<CustomerLocationModel>>();
            Mvx.RegisterType<IRepository<CustomerCommodityModel>,
                SqliteRepository<CustomerCommodityModel>>();
            Mvx.RegisterType<IRepository<CodeTableModel>,
               SqliteRepository<CodeTableModel>>();
            Mvx.RegisterType<IRepository<MessagesModel>,
                SqliteRepository<MessagesModel>>();
            Mvx.RegisterType<IRepository<QueueItemModel>,
                SqliteRepository<QueueItemModel>>();
            Mvx.RegisterType<IRepository<YardModel>,
                SqliteRepository<YardModel>>();
            Mvx.RegisterType<IRepository<TerminalChangeModel>, 
                SqliteRepository<TerminalChangeModel>>();
            Mvx.RegisterType<IRepository<ContainerChangeModel>,
                SqliteRepository<ContainerChangeModel>>();


            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<ScrapRunnerMapperProfile>();
            });

            Mvx.ConstructAndRegisterSingleton<IMvxAppStart, AppStart>();
            var appStart = Mvx.Resolve<IMvxAppStart>();

            RegisterAppStart(appStart);
        }
    }
}
