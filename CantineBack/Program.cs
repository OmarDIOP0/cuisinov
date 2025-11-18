using CantineBack;
using CantineBack.Helpers;
using CantineBack.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Configuration;
using System.Security.Claims;
using System.Text;
using Coravel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnectionString"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(15),
                errorNumbersToAdd: null);
        }));
builder.Services.AddTransient<ITokenService, TokenService>();

// Service Coravel pour la version 6.0.2
builder.Services.AddScheduler();
builder.Services.AddQueue();
builder.Services.AddMemoryCache();
builder.Services.AddTransient<EmailManager>();

Common.ShopID = builder.Configuration.GetValue<int>("ShopID");
Common.EntrepriseID = builder.Configuration.GetValue<int>("EntrepriseID");
Common.FrontEndLink = builder.Configuration.GetValue<string>("FrontEndLink") ?? string.Empty;
Common.EnvironmentMode = builder.Configuration.GetValue<string>("EnvironmentMode");
Common.CreateAccountMessage = builder.Configuration.GetValue<string>("CreateAccountMessage");
Common.PasswordResetMessage = builder.Configuration.GetValue<string>("PasswordResetMessage");
Common.QrCodeEmailMessage = builder.Configuration.GetValue<string>("QrCodeEmailMessage");
Common.QrCodeEmailMessageEn = builder.Configuration.GetValue<string>("QrCodeEmailMessageEn");
Common.SmtpSettings = builder.Configuration.GetSection("Smtp").Get<SmtpSettings>();
var config = builder.Configuration;
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = config["Jwt:Issuer"],
        ValidAudience = config["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(IdentityData.AdminUserPolicyName, p => p.RequireClaim(ClaimTypes.Role, IdentityData.AdminUserClaimName));
    options.AddPolicy(IdentityData.GerantUserPolicyName, p => p.RequireClaim(ClaimTypes.Role, IdentityData.GerantUserClaimName));
});

builder.Services.AddControllers().AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore
);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Auto Mapper 
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(builder.Configuration)
                        .Enrich.FromLogContext()
                        .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || Common.EnvironmentMode == "TEST")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ErrorHandlerMiddleware>();

app.MapControllers();

// Configuration Coravel pour la version 6.0.2
// Pas besoin d'appeler UseQueue() ou UseScheduler() séparément
// Ces services sont déjà disponibles via l'injection de dépendances

using (IServiceScope serviceScope = app.Services.CreateScope())
{
    if (serviceScope.ServiceProvider != null)
    {
        if (app.Environment.IsProduction())
        {
            Console.WriteLine("----> Attempting to apply migrations..");

            try
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context?.Database.Migrate();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"----> Could not apply migrations: {ex.Message}");
            }
        }
    }
}

app.Run();