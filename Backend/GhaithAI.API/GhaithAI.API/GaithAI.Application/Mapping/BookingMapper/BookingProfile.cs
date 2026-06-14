using GhaithAI.GaithAI.Application.DTOs.Booking;

namespace GhaithAI.GaithAI.Application.Mapping.BookingMapper
{
    public class BookingProfile : Profile
    {
        public BookingProfile()
        {
            CreateMap<Booking, DoctorBookingResponseDto>()
                .ForMember(dest => dest.BookingId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.SessionType, opt => opt.MapFrom(src => src.SessionType.ToString()))
                .ForMember(dest => dest.BookingSource, opt => opt.MapFrom(src => src.BookingSource.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes ?? string.Empty))

                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src =>
                    src.BookingSource == BookingSource.App && src.Patient != null
                        ? src.Patient.FullName
                        : (src.BookingSource == BookingSource.Manual && src.ClinicPatient != null
                            ? src.ClinicPatient.PatientFullName
                            : "Unknown Patient")))

                .ForMember(dest => dest.PatientPhone, opt => opt.MapFrom(src =>
                    src.BookingSource == BookingSource.App && src.Patient != null
                        ? src.Patient.PhoneNumber ?? "N/A"
                        : (src.BookingSource == BookingSource.Manual && src.ClinicPatient != null
                            ? src.ClinicPatient.PatientPhone ?? "N/A"
                            : "N/A")));
        }
    }
}
        