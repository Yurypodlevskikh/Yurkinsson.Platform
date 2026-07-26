namespace DataAccess.Interfaces;
using DataAccess.Models;

public interface IUserInfoRepo
{
    Task<bool> CreateUserInfoAsync(UserInfo userInfo, CancellationToken cancellationToken = default);
    Task<UserInfo?> GetUserInfoByGuidIdAsync(string userGuidId, CancellationToken cancellationToken);
    Task<UserInfo?> GetUserInfoByTokenAsync(string token, CancellationToken cancellationToken);
    Task<bool> AreThereAnyUsersByEmailAsync(string email, CancellationToken cancellationToken);
    Task<string?> GetRefreshTokenByTokenAsync(string token, CancellationToken cancellationToken);
    Task<bool> UpdateUserInfoAsync(UserInfo userInfo, CancellationToken cancellationToken = default);
    Task DeleteUserInfoAsync(UserInfo userInfo, CancellationToken cancellationToken = default);
    // Task<IEnumerable<UserInfo>> GetAllUserInfosAsync(CancellationToken cancellationToken = default);
}