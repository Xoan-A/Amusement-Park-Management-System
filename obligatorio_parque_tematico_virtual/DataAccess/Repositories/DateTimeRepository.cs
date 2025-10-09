using DataAccess.Context;
using Domain;
using IDataAccess;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories;

public class DateTimeRepository : IDateTimeRepository
{
    private readonly AppDbContext _context;

    public DateTimeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DateTime?> GetConfiguredDateTime()
    {
        DateTimeConfiguration currentDate = await _context.DateTimeConfigurations.FirstOrDefaultAsync();
        return currentDate?.CurrentDateTime;
    }

    public async Task SetConfiguredDateTime(DateTime dateTime)
    {
        DateTimeConfiguration existingConfig = await _context.DateTimeConfigurations.FirstOrDefaultAsync();
        if (existingConfig == null)
        {
            await _context.DateTimeConfigurations.AddAsync(new DateTimeConfiguration(dateTime));
        }
        else
        {
            existingConfig.CurrentDateTime = dateTime;
            _context.DateTimeConfigurations.Update(existingConfig);
        }

        await _context.SaveChangesAsync();
    }
}