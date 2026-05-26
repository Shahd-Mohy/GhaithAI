namespace GhaithAI.API.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }

        public static string FormatDate(DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }

        public static string FormatDateTime(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}