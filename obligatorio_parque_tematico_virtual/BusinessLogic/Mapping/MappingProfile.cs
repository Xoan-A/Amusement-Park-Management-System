using AutoMapper;
using Domain;
using Models.Out;

namespace BusinessLogic.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserResponse>()
            .ForMember(dest => dest.MembershipLevel,
                opt => opt.MapFrom(src => (int?)src.MembershipLevel))
            .ForMember(dest => dest.UserRoles,
                opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role.Name).ToList()));

        CreateMap<Attraction, AttractionResponse>()
            .ForMember(dest => dest.Type,
                opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.IsActive,
                opt => opt.MapFrom(src => src.IsActive));

        CreateMap<Event, EventResponse>()
            .ForMember(dest => dest.Attractions,
                opt => opt.MapFrom(src => src.Attractions.Select(ea => ea.Attraction).ToList()));

        CreateMap<Ticket, TicketResponse>()
            .ForMember(dest => dest.VisitorName,
                opt => opt.MapFrom(src => src.Visitor != null ? src.Visitor.Name : null))
            .ForMember(dest => dest.VisitorLastName,
                opt => opt.MapFrom(src => src.Visitor != null ? src.Visitor.LastName : null))
            .ForMember(dest => dest.Type,
                opt => opt.MapFrom(src => (int)src.Type));

        CreateMap<MaintenanceSchedule, MaintenanceScheduleResponse>()
            .ForMember(dest => dest.AttractionName,
                opt => opt.MapFrom(src => src.Attraction != null ? src.Attraction.Name : "Unknown"))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<RedemptionHistory, RedemptionHistoryModelOut>()
            .ForMember(dest => dest.RewardName,
                opt => opt.MapFrom(src => src.Reward != null ? src.Reward.Name : null))
            .ForMember(dest => dest.VisitorName,
                opt => opt.MapFrom(src => src.Visitor != null ? src.Visitor.Name : null));

        CreateMap<ScoreHistory, ScoreHistoryModelOut>()
            .ForMember(dest => dest.VisitorName,
                opt => opt.MapFrom(src => src.Visitor != null ? src.Visitor.Name : null))
            .ForMember(dest => dest.Origin,
                opt => opt.MapFrom(src => src.Origin.ToString()));

        CreateMap<Reward, RewardModelOut>()
            .ForMember(dest => dest.RequiredMembershipLevel,
                opt => opt.MapFrom(src => src.RequiredMembershipLevel.HasValue ? (int?)src.RequiredMembershipLevel : null))
            .ForMember(dest => dest.IsAvailable,
                opt => opt.MapFrom(src => src.IsAvailable()));
    }
}
