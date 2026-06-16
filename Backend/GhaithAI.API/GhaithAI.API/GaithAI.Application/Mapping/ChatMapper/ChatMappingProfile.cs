using AutoMapper;
using GhaithAI.API.DTOs.Chat;
using GhaithAI.API.GaithAI.Application.DTOs.Chat;
using GhaithAI.API.Models;

namespace GhaithAI.API.GaithAI.Application.Mapping.ChatMapper
{
    public class ChatMappingProfile : Profile
    {
        public ChatMappingProfile()
        {
            CreateMap<RiskDetailsDto, RiskEvent>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.RiskType, opt => opt.MapFrom(src => src.RiskType))
                .ForMember(dest => dest.DetectedMarkers, opt => opt.MapFrom(src => src.DetectedMarkers))
                .ForMember(dest => dest.SupportingContext, opt => opt.MapFrom(src => src.SupportingContext))
                .ForMember(dest => dest.ConfidenceScore, opt => opt.MapFrom(src => (decimal?)src.ConfidenceScore))
                .ForMember(dest => dest.AIActionTaken, opt => opt.MapFrom(src => src.SuggestedAction))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "open"))
                .ForMember(dest => dest.IsDeletable, opt => opt.MapFrom(_ => false));

            // ChatSession → SessionDTO
            CreateMap<ChatSession, SessionDTO>()
                .ForMember(dest => dest.StartedAt,
                    opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.MessageCount,
                    opt => opt.MapFrom(src =>
                        src.ChatMessages != null ? src.ChatMessages.Count : 0));

            // ChatMessage → ChatMessageDTO
            CreateMap<ChatMessage, ChatMessageDTO>()
                .ForMember(dest => dest.SentAt,
                    opt => opt.MapFrom(src => src.CreatedAt));
        }
    }
}
