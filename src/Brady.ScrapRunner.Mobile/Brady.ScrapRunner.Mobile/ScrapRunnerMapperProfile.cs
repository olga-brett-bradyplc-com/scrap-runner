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
            CreateMap<TerminalMaster, YardModel>();
            CreateMap<TerminalChange, TerminalChangeModel>();
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
                .ForMember(tsc => tsc.TripSegComments, opt => opt.Ignore())
                .ForMember(tsc => tsc.CompositeKey, opt => opt.Ignore());
            CreateMap<CodeTable, CodeTableModel>();
            CreateMap<Domain.Models.Messages, MessagesModel>()
                .ForMember(m => m.MessageThread, o => o.Ignore());
            CreateMap<MessagesModel, Domain.Models.Messages>();
            // Mapping ContainerChange's into ContainerMaster
            CreateMap<ContainerChange, ContainerMasterModel>()
                .ForSourceMember(cc => cc.ActionDate, o => o.Ignore())
                .ForSourceMember(cc => cc.ActionFlag, o => o.Ignore());
            CreateMap<CustomerMaster, CustomerMasterModel>();
        }
    }
}
