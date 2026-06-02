using GhaithAI.API.Configurations;
using GhaithAI.API.Interfaces.InterfaceService;
using GhaithAI.API.Models;
using GhaithAI.API.Presistance;
using GhaithAI.API.Seeders;
using GhaithAI.API.Services;
using GhaithAI.API.Services.Class;
using GhaithAI.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
//builder.Services.AddSwaggerDocumentation();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<ILangflowService, LangflowService>();

builder.Services.Configure<LangflowSettings>(builder.Configuration.GetSection("Langflow"));
builder.Services
    .AddIdentity<ApplicationUser,
        IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services
    .AddScoped<IAuthService,
        AuthService>();
builder.Services
    .AddScoped<IUserService,
        UserService>();
builder.Services.AddAuthorization();
builder.Services
.AddAuthentication(
JwtBearerDefaults.AuthenticationScheme)

.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,

            ValidateAudience = true,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,

            ValidIssuer =
                builder.Configuration["JWT:Issuer"],

            ValidAudience =
                builder.Configuration["JWT:Audience"],

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        builder.Configuration["JWT:SecretKey"]))
        };
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithOrigins(
                "http://localhost:4200");
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseCors("AllowAngular");
app.UseAuthentication();

app.UseAuthorization();
app.MapControllers();

using (
    var scope =
    app.Services.CreateScope())
{
    var roleManager =
        scope.ServiceProvider
            .GetRequiredService<
                RoleManager<IdentityRole>>();

    await RoleSeeder
        .SeedAsync(roleManager);
}
app.Run();
