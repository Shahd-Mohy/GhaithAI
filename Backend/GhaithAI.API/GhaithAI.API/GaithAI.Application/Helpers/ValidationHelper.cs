namespace GhaithAI.API.Helpers
{
    public static class ValidationHelper
    {
        public static bool IsNullOrEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static bool IsValidEmail(string email)
        {
            return email.Contains("@");
        }
    }
}