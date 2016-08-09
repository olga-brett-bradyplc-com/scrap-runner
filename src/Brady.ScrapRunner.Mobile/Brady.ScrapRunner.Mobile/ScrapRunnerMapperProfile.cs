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
            CreateMap<ContainerChange, ContainerMasterModel>()
                .ForSourceMember(cc => cc.ActionDate, o => o.Ignore())
                .ForSourceMember(cc => cc.ActionFlag, o => o.Ignore());
            CreateMap<DriverStatus, DriverStatusModel>();
            CreateMap<Trip, TripModel>();
            CreateMap<TerminalMaster, TerminalMasterModel>()
                .ForMember(m => m.CustHostCode, o => o.Ignore())
                .ForMember(m => m.CustType, o => o.Ignore());
            CreateMap<TerminalChange, TerminalMasterModel>()
                .ForMember(m => m.Region, o => o.MapFrom(src => src.RegionId))
                .ForMember(m => m.Address1, o => o.MapFrom(src => src.CustAddress1))
                .ForMember(m => m.Address2, o => o.MapFrom(src => src.CustAddress2))
                .ForMember(m => m.City, o => o.MapFrom(src => src.CustCity))
                .ForMember(m => m.State, o => o.MapFrom(src => src.CustState))
                .ForMember(m => m.Zip, o => o.MapFrom(src => src.CustZip))
                .ForMember(m => m.Country, o => o.MapFrom(src => src.CustCountry))
                .ForMember(m => m.Phone, o => o.MapFrom(src => src.CustPhone1))
                .ForMember(m => m.Latitude, o => o.MapFrom(src => src.CustLatitude))
                .ForMember(m => m.Longitude, o => o.MapFrom(src => src.CustLongitude))
                .ForMember(m => m.TerminalName, o => o.MapFrom(src => src.CustName));
            CreateMap<TripSegment, TripSegmentModel>()
                .ForMember(m => m.CompositeKey, o => o.Ignore());
            CreateMap<CustomerLocation, CustomerLocationModel>()
                .ForMember(m => m.CompositeKey, o => o.Ignore());
            CreateMap<CustomerCommodity, CustomerCommodityModel>()
                .ForMember(m => m.CompositeKey, o => o.Ignore());
            CreateMap<CustomerDirections, CustomerDirectionsModel>()
                .ForMember(m => m.CompositeId, o => o.Ignore());
            // @TODO: TripSegComments cannot be mapped. We are probably using the wrong property here.
            CreateMap<TripSegmentContainer, TripSegmentContainerModel>()
                .ForMember(tsc => tsc.TripSegComments, o => o.Ignore())
                .ForMember(tsc => tsc.CompositeKey, o => o.Ignore())
                .ForMember(tsc => tsc.TripSegContainerReivewReasonDesc, o => o.Ignore());
            CreateMap<CodeTable, CodeTableModel>();
            CreateMap<Domain.Models.Messages, MessagesModel>()
                .ForMember(m => m.MessageThread, o => o.Ignore());
            CreateMap<MessagesModel, Domain.Models.Messages>();
            CreateMap<CustomerMaster, CustomerMasterModel>();
        }
    }
}
