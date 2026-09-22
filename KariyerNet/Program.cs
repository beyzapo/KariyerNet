using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using KariyerNet.Infrastructure.Persistence;
using KariyerNet.API.Middleware;

using KariyerNet.Application.Interfaces;
using KariyerNet.Infrastructure.Repositories;
using KariyerNet.Infrastructure.Security;
using KariyerNet.Application.Services;
using KariyerNet.Application.Validators;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// dış dünyaya açılacak olan API'yi tanımlıyoruz. Bu sayede dış dünyadan gelen istekleri karşılayabiliriz.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICandidateProfileRepository, CandidateProfileRepository>();
builder.Services.AddScoped<IJobPostingRepository, JobPostingRepository>();
builder.Services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<CandidateProfileService>();
builder.Services.AddScoped<JobPostingService>();
builder.Services.AddScoped<JobApplicationService>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginUserDtoValidator>();

builder.Services.AddOpenApi();

var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
