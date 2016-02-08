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
            CreateMap<EmployeeMaster, EmployeeMasterModel>();
            CreateMap<Trip, TripModel>();
        }
    }
}
