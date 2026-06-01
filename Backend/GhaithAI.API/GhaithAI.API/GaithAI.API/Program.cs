using GhaithAI.API.Configurations;
using GhaithAI.API.Extensions;
using GhaithAI.API.GaithAI.API.Configurations;
using GhaithAI.API.GaithAI.Application.Helpers;
using GhaithAI.API.GaithAI.Application.Services.Class;
using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

// Register all infrastructure services (DbContext, Unit of Work, Repositories)
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddRepositories();
builder.Services.AddServices();
builder.Services.AddMapping();

builder.Services.Configure<LangflowSettings>(builder.Configuration.GetSection("Langflow"));

builder.Services.AddHttpClient<ILangflowService, LangflowService>((serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<LangflowSettings>>().Value;

    if (string.IsNullOrEmpty(settings.BaseUrl))
    {
        throw new InvalidOperationException("🚨 خطأ كارثي: لم يتم العثور على BaseUrl الخاص بـ Langflow في ملف appsettings.json!");
    }

    client.BaseAddress = new Uri(settings.BaseUrl.EndsWith("/") ? settings.BaseUrl : settings.BaseUrl + "/");
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "GhaithAI API v1");
        options.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseAuthorization();

app.MapControllers();

app.Run();
