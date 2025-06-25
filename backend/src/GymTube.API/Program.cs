using System.Data;
using Microsoft.Data.SqlClient;
using GymTube.API.Application.Commands;
using GymTube.API.Application.Queries;
using GymTube.API.Repositories;
using GymTube.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Stripe;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Konfiguriraj Stripe globalno
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Add services to the container.

// Add CORS
var frontendDomain = builder.Configuration["FrontendDomain"] ?? "http://localhost:3000";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(frontendDomain, "http://localhost:3000", "https://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// HttpClient Factory
builder.Services.AddHttpClient();

// Configure the database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddScoped<IDbConnection>(c => new SqlConnection(connectionString));
builder.Services.AddScoped<IUserRepository, UserRepository>(sp => 
    new UserRepository(sp.GetRequiredService<IDbConnection>(), connectionString!));
builder.Services.AddScoped<IVideoRepository, VideoRepository>();

// Services
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<GoogleAuthService>();
builder.Services.AddScoped<MuxService>();
builder.Services.AddScoped<ImageService>();

// Command and Query Handlers
builder.Services.AddScoped<RegisterUserCommandHandler>();
builder.Services.AddScoped<LoginUserQueryHandler>();
builder.Services.AddScoped<UpdateUserNameCommandHandler>();
builder.Services.AddScoped<UpdateProfileImageCommandHandler>();

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = jwtSettings["Key"];
if (string.IsNullOrEmpty(key))
{
    throw new ArgumentNullException("JWT Key is not configured.");
}
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddControllers();
// Više informacija o konfiguraciji Swagger/OpenAPI na https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Konfiguriraj HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication(); // Provjeri je li ovo potrebno/konfigurirano
app.UseAuthorization();

app.MapControllers();

app.Run();
