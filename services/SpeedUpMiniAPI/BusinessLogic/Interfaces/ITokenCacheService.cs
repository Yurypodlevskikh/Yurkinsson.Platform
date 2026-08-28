using BusinessLogic.Models;

namespace BusinessLogic.Interfaces
{
    public interface ITokenCacheService
    {
        void RemoveTokenInfo(string token);
        void SetTokenInfo(string token, CachedTokenInfo cachedTokenInfo);
        bool TryGetTokenInfo(string token, out CachedTokenInfo? cachedTokenInfo);
    }
}