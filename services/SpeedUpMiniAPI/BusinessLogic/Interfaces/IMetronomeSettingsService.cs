using BusinessLogic.DTOs;

namespace BusinessLogic.Interfaces;

public interface IMetronomeSettingsService
{
    Task<IEnumerable<MetronomeSettingsReadDto>> GetSettingsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MetronomeSettingsListItemDto>> GetSettingsByUserGuidIdAsync(string guidId, CancellationToken cancellationToken = default);
    Task<MetronomeSettingsReadDto?> GetSettingsByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<MetronomeSettingsReadDto?> CreateSettingsAsync(MetronomeSettingsCreateDto createDto, string guidId, CancellationToken cancellationToken = default);
    Task<bool> UpdateSettingsAsync(int id, MetronomeSettingsUpdateDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteSettingsAsync(int id, CancellationToken cancellationToken = default);
    Task<int> GetUserPresetsCountAsync(string guidUserId, CancellationToken cancellationToken = default);
}