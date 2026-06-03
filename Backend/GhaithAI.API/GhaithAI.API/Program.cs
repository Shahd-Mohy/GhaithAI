using GhaithAI.API.Configurations;
using GhaithAI.API.Extensions;
using GhaithAI.API.GaithAI.API.Configurations;
using GhaithAI.API.GaithAI.Application.Helpers;
using GhaithAI.API.Interfaces.InterfaceService;
using GhaithAI.API.Seeders;
using GhaithAI.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

// تسجيل الطبقات والـ Extensions من ملف الهيلبر
builder.Services.AddRepositories();
builder.Services.AddServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddMapping();

// إعدادات الـ Langflow والـ HttpClient الخاص به
builder.Services.Configure<LangflowSettings>(builder.Configuration.GetSection("Langflow"));
builder.Services.AddHttpClient<ILangflowService, LangflowService>((serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<LangflowSettings>>().Value;

    if (string.IsNullOrEmpty(settings.BaseUrl))
        throw new InvalidOperationException("Langflow BaseUrl is missing from appsettings.json");

    client.BaseAddress = new Uri(settings.BaseUrl.EndsWith("/") ? settings.BaseUrl : settings.BaseUrl + "/");
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// الـ Seeder لتجهيز الـ Roles أوتوماتيك أول ما المشروع يقوم
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await RoleSeeder.SeedAsync(roleManager);
}

app.Run();