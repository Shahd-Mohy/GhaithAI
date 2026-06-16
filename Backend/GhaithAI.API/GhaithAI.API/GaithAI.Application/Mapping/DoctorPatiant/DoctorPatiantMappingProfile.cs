using GhaithAI.GaithAI.Application.DTOs.DoctorProfile;

namespace GhaithAI.GaithAI.Application.Mapping.DoctorPatiant
{
    public class DoctorPatiantMappingProfile : Profile
    {
        public DoctorPatiantMappingProfile()
        {
            CreateMap<ApplicationUser, PatientListItemDto>()
              .ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
              .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
              .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));
        }
    }
}
