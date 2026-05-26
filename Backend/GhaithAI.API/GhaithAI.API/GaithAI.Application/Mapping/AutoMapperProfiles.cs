using AutoMapper;
using GhaithAI.API.DTOs.Auth;
using GhaithAI.API.DTOs.Chat;
using GhaithAI.API.DTOs.Journal;
using GhaithAI.API.DTOs.Mood;
using GhaithAI.API.DTOs.User;
using GhaithAI.API.Models;

namespace GhaithAI.API.Mapping
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<RegisterDTO, ApplicationUser>();

            CreateMap<CreateMoodLogDTO, MoodLog>();

            CreateMap<CreateJournalDTO, JournalEntry>();

            CreateMap<SendMessageDTO, ChatMessage>();

            CreateMap<ApplicationUser, UserProfileDTO>();
        }
    }
}