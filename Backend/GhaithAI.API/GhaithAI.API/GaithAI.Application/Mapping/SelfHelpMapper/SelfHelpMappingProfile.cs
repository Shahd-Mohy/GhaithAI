global using GhaithAI.API.GaithAI.Application.DTOs.SelfHelp;
global using GhaithAI.API.Models;

namespace GhaithAI.API.GaithAI.Application.Mapping.SelfHelpMapper
{
    public class SelfHelpMappingProfile :Profile
    {
        public SelfHelpMappingProfile()
        {
            CreateMap<AdminSelfHelpSaveDto, SelfHelpContent>();
            CreateMap<SelfHelpContent, AdminSelfHelpResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            CreateMap<SelfHelpContent, UserSelfHelpResponseDto>();

        }
    }
}
