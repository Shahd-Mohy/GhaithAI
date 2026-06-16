namespace GhaithAI.GaithAI.Application.DTOs.Admin
{
    public class AdminDashboardStatsDto
    {

        public int PendingDoctors { get; set; }    
        public int ApprovedDoctors { get; set; }   

        public int TotalBookings { get; set; }

        public int TotalSelfHelpContents { get; set; } 
        public int TotalAIChatSessions { get; set; }
        public int TotalAIChatMassage { get; set; }
    }
}
