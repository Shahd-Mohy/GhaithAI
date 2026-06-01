using AutoMapper;
using GhaithAI.API.GaithAI.Application.DTOs.Chat;
using GhaithAI.API.Models;

namespace GhaithAI.API.GaithAI.Application.Mapping.ChatMapper
{
    public class ChatMappingProfile : Profile
    {
        public ChatMappingProfile()
        {
            CreateMap<RiskDetailsDto, RiskEvent>()
                .ForMember(dest => dest.RiskId, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.RiskType, opt => opt.MapFrom(src => src.RiskType))
                .ForMember(dest => dest.DetectedMarkers, opt => opt.MapFrom(src => src.DetectedMarkers))
                .ForMember(dest => dest.SupportingContext, opt => opt.MapFrom(src => src.SupportingContext))
                .ForMember(dest => dest.ConfidenceScore, opt => opt.MapFrom(src => (decimal?)src.ConfidenceScore))
                .ForMember(dest => dest.AIActionTaken, opt => opt.MapFrom(src => src.SuggestedAction))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "open"))
                .ForMember(dest => dest.IsDeletable, opt => opt.MapFrom(_ => false))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        }
    }
}
