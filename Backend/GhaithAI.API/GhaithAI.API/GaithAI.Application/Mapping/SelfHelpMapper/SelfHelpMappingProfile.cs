global using GhaithAI.API.GaithAI.Application.DTOs.SelfHelp;
global using GhaithAI.API.Models;
using GhaithAI.GaithAI.Application.DTOs.SelfHelp;

namespace GhaithAI.API.GaithAI.Application.Mapping.SelfHelpMapper
{
    public class SelfHelpMappingProfile :Profile
    {
        public SelfHelpMappingProfile()
        {
            CreateMap<SelfHelpContent, AdminSelfHelpGetAllDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));



            CreateMap<SelfHelpContent, AdminSelfHelpDetailsDTO>();
            CreateMap<ExerciseTip, ExerciseTipDto>();

            CreateMap<AdminSelfHelpSaveDto, SelfHelpContent>();
            CreateMap<CreateExerciseTipDto, ExerciseTip>();


            CreateMap<SelfHelpContent, UserSelfHelpResponseDto>();
            CreateMap<SelfHelpContent, UserSelfHelpDetailsResponseDto>()
                 .ForMember(dest => dest.ExerciseTips,
               opt => opt.MapFrom(src => src.ExerciseTips.Select(t => t.Text).ToList()));

        }
    }
}
