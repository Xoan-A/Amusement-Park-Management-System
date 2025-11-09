using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ApiServiceFactory;
using Api.Filters;
using Models;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(option => { option.Filters.Add<ExceptionFilter>(); });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
JwtSettings? jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

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

ServiceFactory.ConfigureObservers(app.Services);

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