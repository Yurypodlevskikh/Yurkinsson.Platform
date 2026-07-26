using AutoMapper;
using BusinessLogic.DTOs;
using BusinessLogic.Interfaces;
using DataAccess.Interfaces;
using DataAccess.Models;

namespace BusinessLogic.Services;

public class MetronomeSettingsService : IMetronomeSettingsService
{
    private readonly IMapper _mapper;
    private readonly IMetronomeSettingsRepo _settingRepo;
    private readonly IUserInfoRepo _userInfoRepo;

    public MetronomeSettingsService(IMapper mapper, IMetronomeSettingsRepo settingsRepo, IUserInfoRepo userInfoRepo)
    {
        _mapper = mapper;
        _settingRepo = settingsRepo;
        _userInfoRepo = userInfoRepo;
    }

    public async Task<IEnumerable<MetronomeSettingsReadDto>> GetSettingsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var settings = await _settingRepo.GetByUserInfoIdAsync(userId, cancellationToken);

        return _mapper.Map<IEnumerable<MetronomeSettingsReadDto>>(settings);
    }

    public async Task<IEnumerable<MetronomeSettingsListItemDto>> GetSettingsByUserGuidIdAsync(string guidId, CancellationToken cancellationToken = default)
    {
        var userInfo = await _userInfoRepo.GetUserInfoByGuidIdAsync(guidId, cancellationToken);
        if (userInfo == null)
        {
            return Enumerable.Empty<MetronomeSettingsListItemDto>();
        }

        var settings = await _settingRepo.GetByUserInfoIdAsync(userInfo.Id, cancellationToken);

        return _mapper.Map<IEnumerable<MetronomeSettingsListItemDto>>(settings);
    }

    public async Task<MetronomeSettingsReadDto?> GetSettingsByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var settings = await _settingRepo.GetByIdAsync(id, cancellationToken);
        if (settings == null)
        {
            return null;
        }

        return _mapper.Map<MetronomeSettingsReadDto>(settings);
    }

    public async Task<MetronomeSettingsReadDto?> CreateSettingsAsync(MetronomeSettingsCreateDto createDto, string guidId, CancellationToken cancellationToken = default)
    {
        if (createDto.SourceBpm <= 0 || createDto.TargetBpm <= 0)
        {
            throw new ArgumentException("Source and target BPM must be greater than 0.");
        }

        var userInfo = await _userInfoRepo.GetUserInfoByGuidIdAsync(guidId, cancellationToken);

        if (userInfo == null) return null;

        var entity = _mapper.Map<MetronomeSettings>(createDto);
        entity.UserInfoId = userInfo.Id;
        var createdEntity = await _settingRepo.CreateAsync(entity, cancellationToken);

        return _mapper.Map<MetronomeSettingsReadDto>(createdEntity);
    }

    public async Task<bool> UpdateSettingsAsync(int id, MetronomeSettingsUpdateDto updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await _settingRepo.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            return false;
        }

        if (updateDto.SourceBpm <= 0 || updateDto.TargetBpm <= 0)
        {
            throw new ArgumentException("Source and target BPM must be greater than 0.");
        }

        entity.SourceBpm = updateDto.SourceBpm;
        entity.TargetBpm = updateDto.TargetBpm;
        entity.BarsPerPattern = updateDto.BarsPerPattern;
        entity.TempoPeriods = updateDto.TempoPeriods;
        entity.IsReversed = updateDto.IsReversed;
        entity.IsRepeatPattern = updateDto.IsRepeatPattern;
        entity.BeatsPerBar = updateDto.BeatsPerBar;
        entity.IsTempoIncreace = updateDto.IsTempoIncreace;
        entity.MetronomeCollectionId = updateDto.MetronomeCollectionId;

        await _settingRepo.UpdateAsync(entity, cancellationToken);
        return true;
    }

    /// <summary>
    /// Deletes a metronome settings entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the metronome settings entity to delete.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the asyncronous operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains <c>true</c> if the entity was found and deleted successfully; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown if an error occurs during the delete operation.
    /// </exception>
    public async Task<bool> DeleteSettingsAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _settingRepo.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            return false;
        }

        await _settingRepo.DeleteAsync(id, cancellationToken);
        return true;
    }

    public async Task<int> GetUserPresetsCountAsync(string guidUserId, CancellationToken cancellationToken = default)
    {
        var userInfo = await _userInfoRepo.GetUserInfoByGuidIdAsync(guidUserId, cancellationToken);

        if (userInfo is null)
            return 0;

        return await _settingRepo.CountAsync(m => m.UserInfoId == userInfo.Id, cancellationToken);
    }
}