using AuthApp.Api.Configurations;
using AuthApp.Api.Data;
using AuthApp.Api.Services.JWT;
using AuthApp.Api.Services.Mail;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region DATABASE CONFIGURATION
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("AppConnection")));
#endregion

#region AUTH CONFIGURATION

builder.Services.AddIdentity<IdentityUser , IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = true; // Necesita confirmacion de correo 
    options.Lockout.MaxFailedAccessAttempts = 5; // Dspués de 5 intentos se bloquea la cuenta 
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Después de 5 minutos se bloquea

}).AddEntityFrameworkStores<AppDbContext>()
  .AddRoles<IdentityRole>()
  .AddDefaultTokenProviders();

#endregion

#region EMAIL CONFIGURATION
var emailSettings = new EmailSettings();
builder.Configuration.Bind(EmailSettings.SectionName, emailSettings);
builder.Services.AddSingleton(Options.Create(emailSettings));
builder.Services.AddSingleton<IMailService, MailService>();
#endregion

#region JWT CONFIGURATION
var jwtSettings = new JWTSettings();
builder.Configuration.Bind(JWTSettings.SectionName, jwtSettings);

builder.Services.AddSingleton(Options.Create(jwtSettings));

builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(
    options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.Secret))
    });
#endregion

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
