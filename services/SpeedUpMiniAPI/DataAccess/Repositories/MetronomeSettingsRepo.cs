using DataAccess.Models;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess.Repositories;

public class MetronomeSettingsRepo : IMetronomeSettingsRepo
{
    private readonly AppDbContext _context;

    public MetronomeSettingsRepo(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MetronomeSettings>> GetAllAsync(CancellationToken cancellationToken = default) => await _context.MetronomeSettings.Include(m => m.User).Include(m => m.MetronomeCollection).ToListAsync(cancellationToken);

    public async Task<MetronomeSettings?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => await _context.MetronomeSettings.Include(m => m.User).Include(m => m.MetronomeCollection).FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<IEnumerable<MetronomeSettings>> GetByUserInfoIdAsync(int userInfoId, CancellationToken cancellationToken = default)
    {
        return await _context.MetronomeSettings.Where(m => m.UserInfoId == userInfoId).Include(m => m.MetronomeCollection).ToListAsync(cancellationToken);
    }

    public async Task<MetronomeSettings> CreateAsync(MetronomeSettings settings, CancellationToken cancellationToken = default)
    {
        await _context.MetronomeSettings.AddAsync(settings, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return settings;
    }

    public async Task<bool> UpdateAsync(MetronomeSettings settings, CancellationToken cancellationToken = default)
    {
        _context.MetronomeSettings.Update(settings);
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var settings = await _context.MetronomeSettings.FindAsync([id], cancellationToken);

        if (settings is null) return false;

        _context.MetronomeSettings.Remove(settings);
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<int> CountAsync(Expression<Func<MetronomeSettings, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.MetronomeSettings.CountAsync(predicate, cancellationToken);
    }
}