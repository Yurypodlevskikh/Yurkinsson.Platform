using BusinessLogic.DTOs;

namespace BusinessLogic.Interfaces;

public interface IMetronomeCollectNameService
{
    Task CreateCollectionNameAsync(CollectionNameCreateDto collectionName, CancellationToken cancellationToken = default);
    Task<CollectionNameReadDto> GetCollectionNameByIdAsync(int id, CancellationToken cancellationToken = default);
    Task UpdateCollectionNameAsync(CollectionNameUpdateDto collectionName, CancellationToken cancellationToken = default);
    Task<IEnumerable<CollectionNameReadDto>> GetAllCollectionNamesAsync(CancellationToken cancellationToken = default);
    Task DeleteCollectionNameAsync(int id, CancellationToken cancellationToken = default);
}