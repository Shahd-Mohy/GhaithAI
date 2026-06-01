namespace GhaithAI.API.Responses
{
    public class SuccessResponse<T> : ApiResponse
    {
        public T? Data { get; set; }

        public static SuccessResponse<T> Ok(T data, string message = "Success") =>
            new() { Success = true, Message = message, StatusCode = 200, Data = data };

        public static SuccessResponse<T> Created(T data, string message = "Created") =>
            new() { Success = true, Message = message, StatusCode = 201, Data = data };
    }
}

