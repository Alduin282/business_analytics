using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BusinessAnalytics.API.Data;
using BusinessAnalytics.API.Models;
using BusinessAnalytics.API.Repositories;
using BusinessAnalytics.API.Services.Import.Parsing;
using BusinessAnalytics.API.Services.Import.Validation;
using BusinessAnalytics.API.Services.Import.Pipeline;
using BusinessAnalytics.API.Services.Import.Pipeline.Stages;
using BusinessAnalytics.API.Services.Events;
using BusinessAnalytics.API.Services.Events.Observers;

// на время разработки упрощаем требования ко всему , небезопасно для прода 
// TODO: в проде заменить на норм требования
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<DbSeeder>();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(); // for api ui

// DB Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{

    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

// Repositories
builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Import — Parsing (Strategy + Factory)
builder.Services.AddScoped<IFileParser, CsvFileParser>();
builder.Services.AddScoped<FileParserFactory>();

// Import — Validation (Chain of Responsibility)
builder.Services.AddScoped<HeaderValidator>();
builder.Services.AddScoped<DataTypeValidator>();
builder.Services.AddScoped<BusinessRuleValidator>();

// Import — Pipeline Stages (order matters!)
builder.Services.AddScoped<IImportPipelineStage, HashCheckStage>();
builder.Services.AddScoped<IImportPipelineStage, ParseStage>();
builder.Services.AddScoped<IImportPipelineStage, ValidationStage>();
builder.Services.AddScoped<IImportPipelineStage, TransformStage>();
builder.Services.AddScoped<IImportPipelineStage, PersistStage>();
builder.Services.AddScoped<ImportPipeline>();

// Observer Pattern - Events & Observers
builder.Services.AddScoped<IImportObserver, AuditObserver>();
builder.Services.AddScoped<IImportObserver, PerformanceObserver>();
builder.Services.AddScoped<IImportEventDispatcher, ImportEventDispatcher>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
