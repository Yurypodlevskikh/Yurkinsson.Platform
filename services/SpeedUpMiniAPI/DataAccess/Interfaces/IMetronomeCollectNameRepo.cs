using DataAccess.Models;

namespace DataAccess.Interfaces;

public interface IMetronomeCollectNameRepo
{
    Task CreateCollectionNameAsync(MetronomeCollectionName metronomeCollectName, CancellationToken cancellationToken = default);
    Task<MetronomeCollectionName?> GetCollectionNameByIdAsync(int id, CancellationToken cancellationToken = default);
    Task UpdateCollectionNameAsync(MetronomeCollectionName collectionName, CancellationToken cancellationToken = default);
    Task<IEnumerable<MetronomeCollectionName>> GetAllCollectionNamesAsync(CancellationToken cancellationToken = default);
    Task DeleteCollectionNameAsync(MetronomeCollectionName collectionName, CancellationToken cancellationToken = default);
}