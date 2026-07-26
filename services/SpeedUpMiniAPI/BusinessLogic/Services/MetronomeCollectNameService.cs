using System.Diagnostics;
using AutoMapper;
using BusinessLogic.DTOs;
using BusinessLogic.Interfaces;
using DataAccess.Models;
using DataAccess.Interfaces;

namespace BusinessLogic.Services;

internal class MetronomeCollectNameService : IMetronomeCollectNameService
{
    private readonly IMapper _mapper;
    private readonly IMetronomeCollectNameRepo _repo;

    public MetronomeCollectNameService(IMapper mapper, IMetronomeCollectNameRepo metronomeCollectNameRepo)
    {
        _mapper = mapper;
        _repo = metronomeCollectNameRepo;
    }
    public async Task CreateCollectionNameAsync(CollectionNameCreateDto collectionName, CancellationToken cancellationToken = default)
    {
        var metronomeCollection = _mapper.Map<MetronomeCollectionName>(collectionName);
        await _repo.CreateCollectionNameAsync(metronomeCollection, cancellationToken);
    }

    public async Task<CollectionNameReadDto> GetCollectionNameByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        /// <summary>
        /// Asynchronously retrieves the collection name by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the collection.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the name of the metronome settings collection.</returns>
        var collectionName = await _repo.GetCollectionNameByIdAsync(id, cancellationToken);
        if (collectionName is null)
            throw new Exception("Collection name not found");

        var collectionNameReadDto = _mapper.Map<CollectionNameReadDto>(collectionName);
        return collectionNameReadDto;
    }

    public async Task UpdateCollectionNameAsync(CollectionNameUpdateDto collectionNameUpdateDto, CancellationToken cancellationToken = default)
    {
        var collectionName = await _repo.GetCollectionNameByIdAsync(collectionNameUpdateDto.Id, cancellationToken);
        if (collectionName is null)
            throw new Exception("Collection name not found");

        _mapper.Map(collectionNameUpdateDto, collectionName);
        await _repo.UpdateCollectionNameAsync(collectionName, cancellationToken);
    }

    public async Task<IEnumerable<CollectionNameReadDto>> GetAllCollectionNamesAsync(CancellationToken cancellationToken = default)
    {
        var collectionNames = await _repo.GetAllCollectionNamesAsync(cancellationToken);
        if(collectionNames is null)
            throw new Exception("Collection names not found");

        var collectionNamesReadDtos = _mapper.Map<IEnumerable<CollectionNameReadDto>>(collectionNames);
        return collectionNamesReadDtos;
    }

    public async Task DeleteCollectionNameAsync(int id, CancellationToken cancellationToken = default)
    {
        var collectionName = await _repo.GetCollectionNameByIdAsync(id, cancellationToken);
        if (collectionName is null)
            throw new Exception("Collection name not found");

        await _repo.DeleteCollectionNameAsync(collectionName, cancellationToken);
    }
}
