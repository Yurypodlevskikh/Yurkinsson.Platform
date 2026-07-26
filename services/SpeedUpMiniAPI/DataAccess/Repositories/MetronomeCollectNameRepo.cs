using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using DataAccess.Models;

namespace DataAccess.Repositories;

internal class MetronomeCollectNameRepo(AppDbContext context) : IMetronomeCollectNameRepo
{
    public async Task CreateCollectionNameAsync(MetronomeCollectionName metronomeCollectName, CancellationToken cancellationToken = default)
    {
        metronomeCollectName.CreatedAt = DateTime.Now;
        await context.MetronomeCollectionNames.AddAsync(metronomeCollectName, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<MetronomeCollectionName>> GetAllCollectionNamesAsync(CancellationToken cancellationToken = default)
    {
        return await context.MetronomeCollectionNames.ToListAsync(cancellationToken);
    }

    public async Task<MetronomeCollectionName?> GetCollectionNameByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.MetronomeCollectionNames.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task UpdateCollectionNameAsync(MetronomeCollectionName collectionName, CancellationToken cancellationToken = default)
    {
        collectionName.UpdatedAt = DateTime.UtcNow;
        context.MetronomeCollectionNames.Update(collectionName);
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task DeleteCollectionNameAsync(MetronomeCollectionName collectionName, CancellationToken cancellationToken = default)
    {
        context.MetronomeCollectionNames.Remove(collectionName);
        await context.SaveChangesAsync(cancellationToken);
    }
}