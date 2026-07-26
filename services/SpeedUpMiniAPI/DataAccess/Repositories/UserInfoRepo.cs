using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using DataAccess.Models;

namespace DataAccess.Repositories;

internal class UserInfoRepo(AppDbContext context) : IUserInfoRepo
{
    public async Task<bool> CreateUserInfoAsync(UserInfo userInfo, CancellationToken cancellationToken = default)
    {
        try
        {
            await context.UserInfos.AddAsync(userInfo, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception)
        {
            //Console.WriteLine(e);
            return false;
        }
    }

    public async Task<string?> GetRefreshTokenByTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await context.UserInfos.Where(x => x.JwtToken == token).Select(x => x.RefreshToken).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserInfo?> GetUserInfoByGuidIdAsync(string userGuidId, CancellationToken cancellationToken)
    {
        return await context.UserInfos.FirstOrDefaultAsync(x => x.GuidId == userGuidId, cancellationToken);
    }

    public async Task<UserInfo?> GetUserInfoByTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await context.UserInfos.FirstOrDefaultAsync(x => x.JwtToken == token, cancellationToken);
    }

    public async Task<bool> AreThereAnyUsersByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await context.UserInfos.AnyAsync(x => x.Email == email);
    }

    public async Task<bool> UpdateUserInfoAsync(UserInfo userInfo, CancellationToken cancellationToken = default)
    {
        context.UserInfos.Update(userInfo);
        var affectedRows = await context.SaveChangesAsync(cancellationToken);
        return affectedRows > 0;
    }
    
    public async Task DeleteUserInfoAsync(UserInfo userInfo, CancellationToken cancellationToken = default)
    {
        context.UserInfos.RemoveRange(userInfo);
        await context.SaveChangesAsync(cancellationToken);
    }
}