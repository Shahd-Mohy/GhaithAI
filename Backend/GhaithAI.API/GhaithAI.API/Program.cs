using GhaithAI.API.Configurations;
using GhaithAI.API.Extensions;
using GhaithAI.API.GaithAI.API.Configurations;
using GhaithAI.API.GaithAI.API.Hubs;
using GhaithAI.API.GaithAI.Application.Helpers;
using GhaithAI.API.GaithAI.Application.Services.Class;
using GhaithAI.API.Interfaces.InterfaceService;
using GhaithAI.API.Seeders;
using GhaithAI.API.Services;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using GhaithAI.GaithAI.Infrastructure.Seeders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using GhaithAI.API.BackgroundJobs;
using GhaithAI.API.SignalR;

var builder = WebApplication.CreateBuilder(args);

// QuestPDF — Community license (free for projects with revenue < $1M USD)
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

// تسجيل الطبقات والـ Extensions من ملف الهيلبر
builder.Services.AddRepositories();
builder.Services.AddServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddMapping();
builder.Services.AddMailService(builder.Configuration);

//builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddHostedService<NotificationJob>();

// إعدادات الـ Langflow والـ HttpClient الخاص به (Chat)
builder.Services.Configure<LangflowSettings>(builder.Configuration.GetSection("Langflow"));
builder.Services.AddHttpClient<ILangflowService, LangflowService>((serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<LangflowSettings>>().Value;

    if (string.IsNullOrEmpty(settings.BaseUrl))
        throw new InvalidOperationException("Langflow BaseUrl is missing from appsettings.json");

    client.BaseAddress = new Uri(settings.BaseUrl.EndsWith("/") ? settings.BaseUrl : settings.BaseUrl + "/");
});

// إعدادات الـ Langflow Report Flow والـ HttpClient الخاص به
builder.Services.Configure<LangflowReportSettings>(builder.Configuration.GetSection("LangflowReport"));
builder.Services.AddHttpClient<IReportLangflowService, ReportLangflowService>((serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<LangflowReportSettings>>().Value;

    if (string.IsNullOrEmpty(settings.BaseUrl))
        throw new InvalidOperationException("LangflowReport BaseUrl is missing from appsettings.json");

    client.BaseAddress = new Uri(settings.BaseUrl.EndsWith("/") ? settings.BaseUrl : settings.BaseUrl + "/");

    // الـ Report flow ممكن تاخد وقت أطول بسبب الـ 3 LLMs — نزود الـ timeout
    client.Timeout = TimeSpan.FromMinutes(5);
});

// إعدادات الـ Identity
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddAuthorization();

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//.AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = builder.Configuration["JWT:Issuer"],
//        ValidAudience = builder.Configuration["JWT:Audience"],
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"]))
//    };
//});
// 🛡️ تسجيل الـ Authentication مرة واحدة فقط هنا لمنع إيرور الـ Scheme Already Exists
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"]))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/hubs/chat") || path.StartsWithSegments("/hubs/session") || path.StartsWithSegments("/hubs/notifications")))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // ✅ يحل الـ circular reference
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

        // ✅ enum كـ string
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// إعدادات الـ CORS عشان فرونت الـ Angular يربط بسلام
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithOrigins("http://localhost:4200");
    });
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

app.UseCors("AllowAngular");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<SessionHub>("/hubs/session");
app.MapHub<NotificationHub>("/hubs/notifications");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        await CountrySeeder.SeedAsync(context);

        await RoleSeeder.SeedAsync(roleManager);

        await AdminSeeder.SeedAsync(userManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();