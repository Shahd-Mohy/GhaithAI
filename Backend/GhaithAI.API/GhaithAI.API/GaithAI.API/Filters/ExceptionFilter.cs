using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GhaithAI.API.Filters
{
    public class ExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var (statusCode, message) = context.Exception switch
            {
                KeyNotFoundException ex => (404, ex.Message),
                ArgumentException ex => (400, ex.Message),
                UnauthorizedAccessException ex => (403, ex.Message),
                InvalidOperationException ex => (422, ex.Message),
                _ => (500, "An unexpected error occurred.")
            };

            context.Result = new ObjectResult(new { message }) { StatusCode = statusCode };
            context.ExceptionHandled = true;
        }
    }
}