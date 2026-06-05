namespace GhaithAI.API.Responses
{
    public class ErrorResponse : ApiResponse
    {
        public List<string> Errors { get; set; } = new();

        public static ErrorResponse BadRequest(List<string> errors) =>
            new() { Success = false, Message = "Validation failed", StatusCode = 400, Errors = errors };

        public static ErrorResponse BadRequest(string error) =>
            new() { Success = false, Message = "Validation failed", StatusCode = 400, Errors = new List<string> { error } };

    }
}
