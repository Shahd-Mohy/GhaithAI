using GhaithAI.API.Configurations;
using GhaithAI.API.Data;
using GhaithAI.API.GaithAI.API.Configurations;
using GhaithAI.API.GaithAI.Application.Helpers;
using GhaithAI.API.GaithAI.Application.Services.Class;
using GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerDocumentation();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddRepositories();
builder.Services.AddServices();
builder.Services.AddMapping();



builder.Services.Configure<LangflowSettings>(builder.Configuration.GetSection("Langflow"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
