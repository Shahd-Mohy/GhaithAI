namespace GhaithAI.GaithAI.Application.DTOs.DoctorProfile
{
    public class CustomScheduleDto
    {
        public Guid Id { get; set; }
        public DateTime CustomDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsOffDay { get; set; }
    }

    public class UpsertCustomScheduleDto
    {
        public DateTime CustomDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsOffDay { get; set; }
    }

    public class AvailableSlotDto
    {
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }

    public class SetPublicListingDto
    {
        public bool IsPublicListed { get; set; }
    }
}
