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
        services.AddScoped<IMaintenanceLogic, MaintenanceLogic>();
        services.AddScoped<IScoreHistoryLogic, ScoreHistoryLogic>();

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
        services.AddScoped<IDateTimeLogic, DateTimeLogic>();
        services.AddScoped<IRewardRepository, RewardRepository>();
        services.AddScoped<IRedemptionHistoryRepository, RedemptionHistoryRepository>();
        services.AddScoped<IMaintenanceScheduleRepository, MaintenanceScheduleRepository>();
        services.AddScoped<IMaintenanceRecordRepository, MaintenanceRecordRepository>();
        services.AddScoped<IScoreHistoryRepository, ScoreHistoryRepository>();
    }
}