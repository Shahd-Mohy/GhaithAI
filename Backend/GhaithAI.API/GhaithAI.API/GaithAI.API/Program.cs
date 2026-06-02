
global using GhaithAI.API.Configurations;
global using GhaithAI.API.Extensions;
global using GhaithAI.API.GaithAI.API.Configurations;
global using GhaithAI.API.GaithAI.Application.Helpers;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddRepositories();
builder.Services.AddServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddMapping();

builder.Services.Configure<LangflowSettings>(builder.Configuration.GetSection("Langflow"));

builder.Services.AddHttpClient<ILangflowService, LangflowService>((serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<LangflowSettings>>().Value;

    if (string.IsNullOrEmpty(settings.BaseUrl))
        throw new InvalidOperationException("Langflow BaseUrl is missing from appsettings.json");

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
        options.RoutePrefix = string.Empty;
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
