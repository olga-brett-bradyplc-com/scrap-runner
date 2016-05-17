namespace Brady.ScrapRunner.Mobile
{
    using AutoMapper;
    using Domain.Models;
    using Models;

    public class ScrapRunnerMapperProfile : Profile
    {
        protected override void Configure()
        {
            base.Configure();
            CreateMap<DriverStatus, DriverStatusModel>();
            CreateMap<EmployeeMaster, EmployeeMasterModel>();
            CreateMap<Preference, PreferenceModel>();
            CreateMap<PowerMaster, PowerMasterModel>();
            CreateMap<ContainerMaster, ContainerMasterModel>();
            CreateMap<DriverStatus, DriverStatusModel>();
            CreateMap<Trip, TripModel>();
            CreateMap<TripSegment, TripSegmentModel>();
            CreateMap<CustomerLocation, CustomerLocationModel>();
            CreateMap<CustomerCommodity, CustomerCommodityModel>();
            CreateMap<CustomerDirections, CustomerDirectionsModel>();
            // @TODO: TripSegComments cannot be mapped. We are probably using the wrong property here.
            CreateMap<TripSegmentContainer, TripSegmentContainerModel>()
                .ForMember(tsc => tsc.TripSegComments, opt => opt.Ignore());
            CreateMap<CodeTable, CodeTableModel>();
        }
    }
}
