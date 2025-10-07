using DataAccess.Context;
using Domain;
using IDataAccess;

namespace DataAccess.Repositories;

public class DateTimeRepository : IDateTimeRepository
{
    private readonly AppDbContext _context;

    public DateTimeRepository(AppDbContext context)
    {
        _context = context;
    }

    public DateTime? GetConfiguredDateTime()
    {
        DateTimeConfiguration currentDate = _context.DateTimeConfigurations.FirstOrDefault();
        return currentDate?.CurrentDateTime;
    }

    public void SetConfiguredDateTime(DateTime dateTime)
    {
        DateTimeConfiguration existingConfig = _context.DateTimeConfigurations.FirstOrDefault();
        if (existingConfig == null)
        {
            _context.DateTimeConfigurations.Add(new DateTimeConfiguration(dateTime));
        }
        else
        {
            existingConfig.CurrentDateTime = dateTime;
            _context.DateTimeConfigurations.Update(existingConfig);
        }

        _context.SaveChanges();
    }
}