using System.IdentityModel.Tokens.Jwt;
using BusinessLogic.DTOs;
using BusinessLogic.Models;

namespace BusinessLogic.Interfaces
{
    public interface IIdentityApiService
    {
        Task<HttpResponseMessage> RegisterAsync(RegisterUserProxyRequestDto model, CancellationToken cancellationToken = default);
        Task<ResponseToUserDto> AuthenticateAsync(AuthenticateRequest model, CancellationToken cancellationToken = default);
        Task<List<object>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> ForgotPasswordAsync(ForgotPasswordDto model, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> ResetPasswordAsync(ResetPasswordRequestdDto model, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> ChangePasswordAsync(string jwtToken, ChangePasswordRequest changePasswordRequest, CancellationToken cancellationToken = default);
        Task<ChangeNicknameResponse> ChangeNicknameAsync(ChangeNicknameRequest nickname, CancellationToken cancellationToken = default);
        Task<AuthenticateResponse?> RefreshTokenAsync(TokenRequestModel tokenRequestModelModel, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> LogoutAsync(string jwtToken, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> DeleteAccountAsync(string jwtToken, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> StartDeleteAccountAsync(string jwtToken, DeleteAccountRequestDto model, CancellationToken cancellationToken = default);
        Task<IsAuthenticateDto?> IsSignedInAsync(string accessToken, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> ConfirmEmailAsync(ConfirmEmail confirmEmail, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> ConfirmDeleteAsync(ConfirmDeleteRequestDto model, CancellationToken cancellationToken = default);
        string? GetUserGuidId(JwtSecurityToken jwtToken);
        string? GetUserNickName(JwtSecurityToken jwtToken);
        Task<HttpResponseMessage> ResendConfirmationAsync(string userId, CancellationToken cancellationToken = default);
    }
}