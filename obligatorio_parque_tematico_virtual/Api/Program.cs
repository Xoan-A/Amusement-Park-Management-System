using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ApiServiceFactory;
using Api.Filters;
using Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var envPaths = new[]
{
    Path.Combine(Directory.GetCurrentDirectory(), ".env"),
    Path.Combine(AppContext.BaseDirectory, ".env"),
    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".env")
};

var envPath = envPaths.FirstOrDefault(File.Exists);
if (envPath != null)
{
    DotNetEnv.Env.Load(envPath);
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(option => { option.Filters.Add<ExceptionFilter>(); });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
var jwtExpiration = Environment.GetEnvironmentVariable("JWT_EXPIRATION_HOURS");


if (string.IsNullOrEmpty(jwtSecretKey))
{
    throw new InvalidOperationException("JWT_SECRET_KEY not configured in .env file or environment variables");
}

var jwtSettings = new JwtSettings
{
    SecretKey = jwtSecretKey,
    Issuer = jwtIssuer ?? throw new InvalidOperationException("JWT_ISSUER not configured"),
    Audience = jwtAudience ?? throw new InvalidOperationException("JWT_AUDIENCE not configured"),
    ExpirationHours = int.Parse(jwtExpiration ?? "1")
};
builder.Services.AddSingleton(Options.Create(jwtSettings));

builder.Services.AddServices(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDevClient", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

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
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

WebApplication app = builder.Build();

using (Microsoft.Extensions.DependencyInjection.IServiceScope scope = app.Services.CreateScope())
{
    DataAccess.Context.AppDbContext context = scope.ServiceProvider.GetRequiredService<DataAccess.Context.AppDbContext>();

    if (context.Database.IsSqlite())
    {
        context.Database.EnsureCreated();
    }
    else
    {
        context.Database.Migrate();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngularDevClient");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}