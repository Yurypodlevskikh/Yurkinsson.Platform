using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YurkinssonAuthentication.Data;
using YurkinssonAuthentication.DTOs.User;
using YurkinssonAuthentication.Models;

namespace YurkinssonAuthentication.Services.Interfaces
{
    public interface IAccountService
    {
        Task<(bool Succeeded, string[] Errors)> RegisterUserAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);
        Task<TokenResult> LoginUser(LoginDto loginDto);
        Task<TokenResult> RefreshToken(RefreshTokenModel refreshTokenModel);
        Task<EmailConfirmationResult> UserConfirmsEmail(ConfirmEmail confirmEmail);
        Task<IdentityResult> ResendConfirmationEmail(string userId);
        Task<bool> ResetPasswordMessage(ForgotPasswordDto forgotPassword);
        Task<IdentityResult> ResetPassword(ResetPasswordDto resetPassword);
        Task<IdentityResult> ChangePasswordAsync(AppUser user, ChangePasswordRequest changePasswordRequest);
        Task<ChangeNicknameResponse> ChangeUserNickname(ChangeNicknameRequest changeNickname, CancellationToken cancellationToken);
        Task<AppUser> GetUserByIdAsync(string userId, CancellationToken cancellationToken);
        Task<IdentityResult> UpdateUserAsync(AppUser user, CancellationToken cancellationToken);
        Task<IdentityResult> DeleteUserAsync(AppUser user, CancellationToken cancellationToken);
        Task<bool> StartDeleteAccountAsync(string userId, string v);
    }
}