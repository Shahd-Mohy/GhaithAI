using GhaithAI.GaithAI.Application.DTOs.DoctorProfile;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Identity;

namespace GhaithAI.GaithAI.Application.Services.Class
{
    public class DoctorDashboardPatiantService : IDoctorDashboardPatiantService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DoctorDashboardPatiantService> _logger; 

        public DoctorDashboardPatiantService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            ILogger<DoctorDashboardPatiantService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<DoctorPatientsDashboardDto> GetDoctorPatientsDashboardAsync(
            string userId,
            string? search = null,
            string? riskFilter = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("GetDoctorPatientsDashboardAsync called with an empty or null userId.");
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }

            _logger.LogInformation("Starting to fetch doctor patients dashboard for UserId: {UserId} with PageNumber: {PageNumber}, PageSize: {PageSize}.", userId, pageNumber, pageSize);

            try
            {
                var doctorId = await GetDoctorIdByUserIdAsync(userId);

                pageNumber = pageNumber < 1 ? 1 : pageNumber;
                pageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize); 

                var today = DateTime.UtcNow.Date;
                var startOfWeek = today.AddDays(-7);

                var bookingsRepo = _unitOfWork.Booking.GetAllQueryableNoTracking();
                var chatSessionsRepo = _unitOfWork.Session.GetAllQueryableNoTracking();
                var usersQuery = _userManager.Users.AsNoTracking();

                var totalPatients = await bookingsRepo
                     .Where(b => b.DoctorId == doctorId && b.PatientId != null)
                     .Select(b => b.PatientId)
                     .Distinct()
                     .CountAsync();

                var sessionsThisWeek = await bookingsRepo
                       .CountAsync(b => b.DoctorId == doctorId && b.BookingDate >= startOfWeek && b.Status == BookingStatus.Confirmed);

                var patientIdsQuery = bookingsRepo
                     .Where(b => b.DoctorId == doctorId && b.PatientId != null)
                     .Select(b => b.PatientId)
                     .Distinct();

                usersQuery = usersQuery.Where(u => patientIdsQuery.Contains(u.Id));

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim().ToLower();
                    usersQuery = usersQuery.Where(u => u.FullName.ToLower().Contains(search));
                    _logger.LogDebug("Applying search filter: '{Search}' for DoctorId: {DoctorId}.", search, doctorId);
                }

                var patientsProjectedQuery = usersQuery.Select(u => new
                {
                    PatientUserEntity = u,

                    LastChatRisk = chatSessionsRepo
                          .Where(cs => cs.UserId == u.Id)
                          .OrderByDescending(cs => cs.CreatedAt)
                          .Select(cs => cs.RiskLevel)
                          .FirstOrDefault(),

                    TotalBookingsCount = bookingsRepo.Count(b => b.DoctorId == doctorId && b.PatientId == u.Id),

                    LastBookingDate = bookingsRepo
                          .Where(b => b.DoctorId == doctorId && b.PatientId == u.Id)
                          .OrderByDescending(b => b.BookingDate)
                          .Select(b => (DateTime?)b.BookingDate)
                          .FirstOrDefault()
                });

                if (!string.IsNullOrWhiteSpace(riskFilter))
                {
                    riskFilter = riskFilter.Trim().ToLower();

                    if (riskFilter != "high" && riskFilter != "medium" && riskFilter != "low")
                    {
                        _logger.LogWarning("Unexpected Risk Filter value received: '{RiskFilter}' for DoctorId: {DoctorId}.", riskFilter, doctorId);
                    }

                    patientsProjectedQuery = patientsProjectedQuery.Where(p => p.LastChatRisk.ToLower() == riskFilter);
                }

                var totalFilteredPatients = await patientsProjectedQuery.CountAsync();

                var filteredPatientsData = await patientsProjectedQuery
                    .OrderBy(p => p.PatientUserEntity.FullName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var allDoctorChats = await chatSessionsRepo
                    .Where(cs => patientIdsQuery.Contains(cs.UserId))
                    .GroupBy(cs => cs.UserId)
                    .Select(g => g.OrderByDescending(cs => cs.CreatedAt).Select(cs => cs.RiskLevel).FirstOrDefault())
                    .ToListAsync();

                var highRiskCount = allDoctorChats.Count(r => r?.ToLower() == "high");
                var totalRiskAlerts = allDoctorChats.Count(r => r?.ToLower() == "high" || r?.ToLower() == "medium");

                var patientList = filteredPatientsData.Select(p =>
                {
                    var dto = _mapper.Map<PatientListItemDto>(p.PatientUserEntity);
                    dto.RiskLevel = p.LastChatRisk ?? "low";
                    dto.LastBookingDate = p.LastBookingDate;
                    dto.TotalBookingsCount = p.TotalBookingsCount;
                    return dto;
                }).ToList();

                _logger.LogInformation("Successfully generated dashboard data for DoctorId: {DoctorId}. Returning {Count} patients out of {TotalFiltered} filtered.", doctorId, patientList.Count, totalFilteredPatients);

                return new DoctorPatientsDashboardDto
                {
                    TotalPatients = totalPatients,
                    HighRiskCount = highRiskCount,
                    SessionsThisWeek = sessionsThisWeek,
                    TotalRiskAlerts = totalRiskAlerts,
                    TotalFilteredPatients = totalFilteredPatients,
                    Patients = patientList
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting doctor patients dashboard for UserId: {UserId}.", userId);
                throw; 
            }
        }

        private async Task<Guid> GetDoctorIdByUserIdAsync(string userId)
        {
            var doctorProfile = await _unitOfWork.DoctorProfile.GetAllQueryableNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctorProfile == null)
            {
                _logger.LogError("Access Denied: ApplicationUser with ID {UserId} does not have a registered DoctorProfile.", userId);
                throw new KeyNotFoundException($"Doctor profile not found for the user ID: {userId}. Please complete your professional registration.");
            }

            return doctorProfile.Id;
        }
    }
}
