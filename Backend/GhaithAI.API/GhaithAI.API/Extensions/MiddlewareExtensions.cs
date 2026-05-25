namespace GhaithAI.API.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseProjectMiddleware(
            this IApplicationBuilder app)
        {
            app.UseAuthentication();

            app.UseAuthorization();

            return app;
        }
    }
}