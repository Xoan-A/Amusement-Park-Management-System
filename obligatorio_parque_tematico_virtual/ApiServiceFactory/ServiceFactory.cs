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
        services.AddSingleton<IPasswordLogic, PasswordLogic>();
        services.AddSingleton<ITokenLogic, TokenLogic>();
        services.AddScoped<IStrategyRepository, StrategyRepository>();
        services.AddScoped<IActiveStrategy, ActiveStrategy>();
        services.AddScoped<IAuthLogic, AuthLogic>();
        services.AddScoped<IUserLogic, UserLogic>();
        services.AddScoped<ITicketLogic, TicketLogic>();
        services.AddScoped<IAttractionLogic, AttractionLogic>();
        services.AddScoped<IAttractionLogicEntity, AttractionLogic>();
        services.AddScoped<IEventLogic, EventLogic>();
        services.AddScoped<IRewardLogic, RewardLogic>();
        services.AddScoped<IRedemptionLogic, RedemptionLogic>();
        services.AddScoped<IScoreHistoryLogic, ScoreHistoryLogic>();
        services.AddScoped<IClaimsLogic, ClaimsLogic>();

        string pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
        services.AddSingleton<IPluginLoader>(new BusinessLogic.Plugins.PluginLoader(pluginsPath));

        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IAttractionRepository, AttractionRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IDateTimeRepository, DateTimeRepository>();
        services.AddScoped<IRewardRepository, RewardRepository>();
        services.AddScoped<IRedemptionHistoryRepository, RedemptionHistoryRepository>();
        services.AddScoped<IMaintenanceScheduleRepository, MaintenanceScheduleRepository>();
        services.AddScoped<IMaintenanceRecordRepository, MaintenanceRecordRepository>();
        services.AddScoped<IScoreHistoryRepository, ScoreHistoryRepository>();
        services.AddScoped<IDateTimeLogic, DateTimeLogic>();
        services.AddScoped<IDailyScoreLogic, DailyScoreLogic>();
        services.AddScoped<IMaintenanceLogic, MaintenanceLogic>();
        services.AddScoped<IDateObserver>(sp => sp.GetRequiredService<IDailyScoreLogic>() as IDateObserver);
        services.AddScoped<IDateObserver>(sp => sp.GetRequiredService<IMaintenanceLogic>() as IDateObserver);
    }

    public static void ConfigureObservers(IServiceProvider serviceProvider)
    {
        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            IDateSubject dateTimeLogic = scope.ServiceProvider.GetRequiredService<IDateTimeLogic>() as IDateSubject;
            IDateObserver dailyScoreLogic =
                scope.ServiceProvider.GetRequiredService<IDailyScoreLogic>() as IDateObserver;
            IDateObserver maintenanceLogic =
                scope.ServiceProvider.GetRequiredService<IMaintenanceLogic>() as IDateObserver;

            if (dateTimeLogic != null && dailyScoreLogic != null)
            {
                dateTimeLogic.Attach(dailyScoreLogic);
            }

            if (dateTimeLogic != null && maintenanceLogic != null)
            {
                dateTimeLogic.Attach(maintenanceLogic);
            }
        }
    }
}