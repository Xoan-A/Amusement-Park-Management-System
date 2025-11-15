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

        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string projectRoot = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", ".."));
        string pluginsPath = Path.Combine(projectRoot, "BusinessLogic", "Plugins");
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
        services.AddScoped<IScoreHistoryRepository, ScoreHistoryRepository>();
        services.AddScoped<IDailyScoreLogic, DailyScoreLogic>();
        services.AddScoped<IMaintenanceLogic, MaintenanceLogic>();
        services.AddScoped<IDateObserver, DailyScoreLogic>();
        services.AddScoped<IDateObserver, MaintenanceLogic>();
        services.AddScoped<IDateTimeLogic, DateTimeLogic>();
    }
}