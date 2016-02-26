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
            CreateMap<Trip, TripModel>();
            CreateMap<TripSegment, TripSegmentModel>();
            CreateMap<TripSegmentContainer, TripSegmentContainerModel>();
        }
    }
}
