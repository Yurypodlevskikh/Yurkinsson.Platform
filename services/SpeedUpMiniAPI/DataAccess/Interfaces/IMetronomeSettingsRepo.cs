using System.Linq.Expressions;
using DataAccess.Models;

namespace DataAccess.Interfaces;

public interface IMetronomeSettingsRepo
{
    Task<IEnumerable<MetronomeSettings>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MetronomeSettings?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<MetronomeSettings>> GetByUserInfoIdAsync(int userInfoId, CancellationToken cancellationToken = default);
    Task<MetronomeSettings> CreateAsync(MetronomeSettings settings, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(MetronomeSettings settings, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<MetronomeSettings, bool>> predicate, CancellationToken cancellationToken = default);
}