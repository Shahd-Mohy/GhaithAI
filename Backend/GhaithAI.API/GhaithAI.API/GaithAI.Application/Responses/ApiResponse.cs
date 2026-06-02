namespace GhaithAI.API.Responses
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public static ApiResponse Ok(string message = "Success") =>

            new() { Success = true, Message = message, StatusCode = 200 };

        public static ApiResponse Fail(string message, int statusCode = 400) =>
            new() { Success = false, Message = message, StatusCode = statusCode };

        public static ApiResponse NotFound(string message = "Not found") =>
            new() { Success = false, Message = message, StatusCode = 404 };
    }
}
