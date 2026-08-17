using KeyStone_Identity.API.Middleware;
using KeyStone_Identity.Core.Interfaces;
using KeyStone_Identity.Core.Models;
using KeyStone_Identity.Core.Services;
using KeyStone_Identity.Infrastructure.Data;
using KeyStone_Identity.Infrastructure.Repositories;
using KeyStone_Identity.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<KeyStone_Identity_DbContext>(x => x.UseNpgsql(builder.Configuration["ConnectionStrings:DefaultConnection"]));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddTransient<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IActivationTokenRepository, ActivationTokenRepository>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = false,
        ValidateLifetime = true,
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();

}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
