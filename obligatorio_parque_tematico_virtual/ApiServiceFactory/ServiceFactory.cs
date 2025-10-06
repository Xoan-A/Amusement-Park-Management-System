using IBusinessLogic;
using IDataAccess;
using BusinessLogic;
using DataAccess.Context;
using DataAccess.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ApiServiceFactory;

public static class ServiceFactory
{
    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDateTimeLogic>(provider => DateTimeLogic.Instance);
        services.AddSingleton<IPasswordService, PasswordService>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IActiveStrategy, ActiveStrategy>();
        services.AddScoped<IAuthLogic, AuthLogic>();
        services.AddScoped<IUserLogic, UserLogic>();
        services.AddScoped<ITicketLogic, TicketLogic>();
        services.AddScoped<IAttractionService, AttractionService>();
        services.AddScoped<IAttractionServiceEntity, AttractionService>();
        services.AddScoped<IEventService, EventService>();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IAttractionRepository, AttractionRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
    }
}