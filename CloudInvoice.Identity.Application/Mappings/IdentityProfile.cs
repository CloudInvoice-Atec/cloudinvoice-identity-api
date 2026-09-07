using AutoMapper;
using CloudInvoice.Identity.Application.Dtos.Requests;
using CloudInvoice.Identity.Application.Dtos.Responses;
using CloudInvoice.Identity.Domain.Entities;

namespace CloudInvoice.Identity.Application.Mappings;

public class IdentityProfile : Profile
{
    public IdentityProfile()
    {
        CreateMap<RegisterRequestDto, ApplicationUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));

        CreateMap<UserResponseDto, ApplicationUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Role, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<ApplicationUser, UserResponseDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email ?? string.Empty));

        CreateMap<ApplicationUser, AuthResponseDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim()))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
            .ForMember(dest => dest.IsSuccess, opt => opt.Ignore())
            .ForMember(dest => dest.Message, opt => opt.Ignore())
            .ForMember(dest => dest.Token, opt => opt.Ignore());
    }
}
