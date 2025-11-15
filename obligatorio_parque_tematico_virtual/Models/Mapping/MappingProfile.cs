using AutoMapper;
using Domain;
using Models.Out;

namespace Models.Mapping;

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
    }
}
