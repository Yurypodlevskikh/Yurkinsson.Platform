using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AutoMapper;
using BusinessLogic.DTOs;
using BusinessLogic.Interfaces;
using BusinessLogic.Models;
using DataAccess.Models;
using DataAccess.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace BusinessLogic.Services;

internal class IdentityApiService : IIdentityApiService
{
    private readonly HttpClient _httpClient;
    private readonly IUserInfoRepo _repo;
    private readonly string _baseUrl;
    private readonly IMapper _mapper;
    private readonly TokenCacheService _tokenCacheService;
    private readonly IConfiguration _configuration;

    public IdentityApiService(HttpClient httpClient, IOptions<IdentityApiSettings> settings, IMapper mapper, IUserInfoRepo repo, TokenCacheService tokenCacheService, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _repo = repo;
        _baseUrl = settings.Value.BaseUrl;
        _mapper = mapper;

        if (string.IsNullOrEmpty(_baseUrl))
        {
            throw new Exception("IdentityApiSettings.BaseUrl is not set");
        }

        _httpClient.BaseAddress = new Uri(_baseUrl);
        _tokenCacheService = tokenCacheService;
        _configuration = configuration;
    }

    public async Task<HttpResponseMessage> RegisterAsync(RegisterUserProxyRequestDto model, CancellationToken cancellationToken = default)
    {
        var identityRequest = new HttpRequestMessage(HttpMethod.Post, "api/account/register")
        {
            Content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json")
        };
        var response = await _httpClient.SendAsync(identityRequest, cancellationToken);
        return response;
    }

    public async Task<ResponseToUserDto> AuthenticateAsync(AuthenticateRequest model, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage? response = await _httpClient.PostAsJsonAsync("api/account/login", model, cancellationToken);

        var responseToUser = new ResponseToUserDto();

        if (!response.IsSuccessStatusCode)
        {
            responseToUser.Message = "Authentication failed";
            return responseToUser;
        }

        var authenticateResponse = await response.Content.ReadFromJsonAsync<AuthenticateResponse>(cancellationToken: cancellationToken);

        if (authenticateResponse == null || string.IsNullOrEmpty(authenticateResponse.JwtToken))
        {
            responseToUser.Message = "Authentication failed";
            return responseToUser;
        }

        var jwtSecurityToken = DecodeJwtToken(authenticateResponse.JwtToken);

        if (jwtSecurityToken == null)
        {
            responseToUser.Message = "Authentication failed";
            return responseToUser;
        }

        var userInfoFromToken = GetUserInfoFromToken(jwtSecurityToken);

        if (userInfoFromToken.TokenExpiry < DateTime.UtcNow)
        {
            responseToUser.Message = "Token is expired";
            return responseToUser;
        }

        var userInfoByGuid = await _repo.GetUserInfoByGuidIdAsync(userInfoFromToken.GuidId, cancellationToken);

        if (userInfoByGuid != null)
        {
            userInfoByGuid.JwtToken = authenticateResponse.JwtToken;
            userInfoByGuid.RefreshToken = authenticateResponse.RefreshToken;
            userInfoByGuid.RefreshTokenExpiry = authenticateResponse.RefreshTokenExpiry;
            await _repo.UpdateUserInfoAsync(userInfoByGuid, cancellationToken);
        }
        else
        {
            var newUserInfo = new UserInfo
            {
                GuidId = userInfoFromToken.GuidId,
                NickName = userInfoFromToken.NickName,
                Email = userInfoFromToken.Email,
                JwtToken = authenticateResponse.JwtToken,
                RefreshToken = authenticateResponse.RefreshToken,
                RefreshTokenExpiry = authenticateResponse.RefreshTokenExpiry
            };

            await _repo.CreateUserInfoAsync(newUserInfo, cancellationToken);
        }

        var cachedTokenInfo = new CachedTokenInfo
        {
            UserId = userInfoFromToken.GuidId,
            TokenExpiry = userInfoFromToken.TokenExpiry,
            RefreshTokenExpiry = authenticateResponse.RefreshTokenExpiry
        };

        _tokenCacheService.SetTokenInfo(authenticateResponse.JwtToken, cachedTokenInfo);

        responseToUser.JwtToken = authenticateResponse.JwtToken;
        responseToUser.RefreshToken = authenticateResponse.RefreshToken;
        responseToUser.HueDegrees = userInfoByGuid?.HueDegrees ?? 0;
        responseToUser.Nickname = userInfoFromToken.NickName;
        responseToUser.Message = "Authentication successful";
        responseToUser.IsSuccess = true;

        return responseToUser;
    }

    public async Task<ChangeNicknameResponse> ChangeNicknameAsync(ChangeNicknameRequest changeNickname, CancellationToken cancellationToken)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "api/account/change-nickname");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", changeNickname.AccessToken);
        requestMessage.Content = new StringContent($"\"{changeNickname.Nickname}\"", Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        var changeNicknameResponse = new ChangeNicknameResponse();
        changeNicknameResponse.AccessToken = changeNickname.AccessToken;
        changeNicknameResponse.NewNickname = changeNickname.Nickname;

        if (!response.IsSuccessStatusCode)
        {
            changeNicknameResponse.ResultMessage = "Failed to change nickname.";
            return changeNicknameResponse;
        }

        var identityResponse = await response.Content.ReadFromJsonAsync<ChangeNicknameResponse>(cancellationToken: cancellationToken);

        if (identityResponse == null)
        {
            changeNicknameResponse.ResultMessage = "Failed to deserialize updated nickname.";
            return changeNicknameResponse;
        }

        var jwtSecurityToken = DecodeJwtToken(changeNickname.AccessToken);

        if (jwtSecurityToken == null)
        {
            changeNicknameResponse.ResultMessage = "Failed to read the token.";
            return changeNicknameResponse;
        }

        var userId = GetUserGuidId(jwtSecurityToken);

        if (string.IsNullOrEmpty(userId))
        {
            changeNicknameResponse.ResultMessage = "Failed to get user id.";
            return changeNicknameResponse;
        }

        var updatedUserInfo = await _repo.GetUserInfoByGuidIdAsync(userId, cancellationToken);

        if (updatedUserInfo == null)
        {
            changeNicknameResponse.ResultMessage = "Failed to get user info.";
            return changeNicknameResponse;
        }

        updatedUserInfo.NickName = changeNickname.Nickname;

        var updatedResult = await _repo.UpdateUserInfoAsync(updatedUserInfo, cancellationToken);

        if (!updatedResult)
        {
            changeNicknameResponse.ResultMessage = "Could not edit the nickname of the metronome user.";
            return changeNicknameResponse;
        }

        changeNicknameResponse.IsChanged = true;
        changeNicknameResponse.ResultMessage = "Nickname changed successfully.";

        return changeNicknameResponse;
    }

    public async Task<List<object>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetFromJsonAsync<List<object>>("api/values/get-metronom-settings", cancellationToken);

        if (response == null)
            throw new Exception("Failed to deserialize response");

        return response;
    }

    public async Task<AuthenticateResponse?> RefreshTokenAsync(TokenRequestModel tokenRequestModel, CancellationToken cancellationToken = default)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "api/account/refresh-token");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenRequestModel.JwtToken);
        requestMessage.Content = new FormUrlEncodedContent(new[]{
            new KeyValuePair<string?, string?>("refreshToken", tokenRequestModel.RefreshToken)
        });

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            Console.WriteLine($"Failed to refresh token. Status: {response.StatusCode}, Content: {errorContent}");
            return null;
        }

        var tokens = await response.Content.ReadFromJsonAsync<AuthenticateResponse>(cancellationToken: cancellationToken);

        if (tokens == null)
        {
            Console.WriteLine("Failed to deserialize token response");
            return null;
        }

        var jwtSecurityToken = DecodeJwtToken(tokens.JwtToken);

        if (jwtSecurityToken == null)
        {
            Console.WriteLine("Failed to decode JWT token");
            return null;
        }

        if(tokens.RefreshTokenExpiry < DateTime.UtcNow)
        {
            Console.WriteLine("Refresh token has already expired");
            return null;
        }

        var userGuidId = GetUserGuidId(jwtSecurityToken);

        if (userGuidId == null)
        {
            Console.WriteLine("Failed to get user GUID");
            return null;
        }

        var userInfo = await _repo.GetUserInfoByGuidIdAsync(userGuidId, cancellationToken);

        if (userInfo == null)
        {
            Console.WriteLine("Failed to get user info");
            return null;
        }

        // Update users data
        userInfo.JwtToken = tokens.JwtToken;
        userInfo.RefreshToken = tokens.RefreshToken;
        userInfo.RefreshTokenExpiry = tokens.RefreshTokenExpiry;

        await _repo.UpdateUserInfoAsync(userInfo, cancellationToken);

        var cackedTokenInfo = new CachedTokenInfo
        {
            UserId = userGuidId,
            TokenExpiry = jwtSecurityToken.ValidTo,
            RefreshTokenExpiry = tokens.RefreshTokenExpiry
        };
        _tokenCacheService.SetTokenInfo(tokens.JwtToken, cackedTokenInfo);

        return tokens;
    }

    public async Task<HttpResponseMessage> LogoutAsync(string jwtToken, CancellationToken cancellationToken = default)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "api/account/logout");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(jwtToken);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var userInfo = await _repo.GetUserInfoByTokenAsync(jwtToken, cancellationToken);

            if (userInfo != null)
            {
                userInfo.JwtToken = string.Empty;
                userInfo.RefreshToken = string.Empty;
                userInfo.RefreshTokenExpiry = DateTime.MinValue;

                await _repo.UpdateUserInfoAsync(userInfo, cancellationToken);
            }
            else
            {
                // Local user info not found for this token — proceed without failing.
                // This can happen if the MiniAPI never created the local record or the token was rotated.
                // Do not throw here; logout should be idempotent.
            }

            _tokenCacheService.RemoveTokenInfo(jwtToken);
        }

        return response;
    }

    public async Task<HttpResponseMessage> ConfirmEmailAsync(ConfirmEmail confirmEmail, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/account/confirm-email", new { confirmEmail.userId, confirmEmail.token }, cancellationToken);

        return response;
    }

    public async Task<HttpResponseMessage> ResendConfirmationAsync(string userId, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/account/resend-confirmation?userId={Uri.EscapeDataString(userId)}");
        var response = await _httpClient.SendAsync(request, cancellationToken);
        return response;
    }

    public async Task<HttpResponseMessage> ChangePasswordAsync(string jwtToken, ChangePasswordRequest changePasswordRequest, CancellationToken cancellationToken = default)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "api/account/change-password")
        {
            Content = new StringContent(JsonSerializer.Serialize(changePasswordRequest), Encoding.UTF8, "application/json")
        };

        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        return response;
    }

    public async Task<HttpResponseMessage> ForgotPasswordAsync(ForgotPasswordDto model, CancellationToken cancellationToken = default)
    {
        // Build ClientUri from configuration (prefer client-specific redirect, else Frontend:BaseUrl)
        string? clientBase = _configuration["ClientRedirectUrls:SpeedUpVue"];
        if (string.IsNullOrWhiteSpace(clientBase))
        {
            clientBase = _configuration["Frontend:BaseUrl"];
        }

        if (string.IsNullOrWhiteSpace(clientBase))
        {
            throw new InvalidOperationException("No frontend base URL configured. Set 'Frontend:BaseUrl' or 'ClientRedirectUrls:SpeedUpVue' in configuration.");
        }

        clientBase = clientBase.TrimEnd('/');
        var clientUri = $"{clientBase}/reset-password";

        var requestModel = new ForgotPasswordRequestDto
        {
            Email = model.Email,
            ClientUri = clientUri
        };

        var forgotPasswordRequest = new HttpRequestMessage(HttpMethod.Post, "api/account/forgot-password")
        {
            Content = new StringContent(JsonSerializer.Serialize(requestModel), Encoding.UTF8, "application/json")
        };

        var result = await _httpClient.SendAsync(forgotPasswordRequest, cancellationToken);
        return result;
    }

    public async Task<HttpResponseMessage> ResetPasswordAsync(ResetPasswordRequestdDto model, CancellationToken cancellationToken)
    {
        // Forward reset request to the Identity service; let Identity perform account validation and reset.
        return await _httpClient.PostAsJsonAsync("api/account/reset-password", model, cancellationToken);
    }

    public async Task<HttpResponseMessage> DeleteAccountAsync(string jwtToken, CancellationToken cancellationToken = default)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Delete, "api/account/delete-account");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var userInfo = await _repo.GetUserInfoByTokenAsync(jwtToken, cancellationToken);

            if (userInfo == null)
            {
                throw new Exception("Failed to get user info");
            }

            await _repo.DeleteUserInfoAsync(userInfo, cancellationToken);
            _tokenCacheService.RemoveTokenInfo(jwtToken);
        }

        return response;
    }

    public async Task<IsAuthenticateDto?> IsSignedInAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            return null;
        }

        var isAuthenticateDto = new IsAuthenticateDto();

        if (!_tokenCacheService.TryGetTokenInfo(accessToken, out var tokenInfo))
        {
            var userDbInfo = await _repo.GetUserInfoByTokenAsync(accessToken, cancellationToken);

            if (userDbInfo == null)
            {
                return null;
            }

            if (userDbInfo.RefreshTokenExpiry < DateTime.UtcNow)
            {
                return null;
            }
            else
            {
                isAuthenticateDto.SignedIn = false;
                isAuthenticateDto.RefreshToken = userDbInfo.RefreshToken;
            }
        }
        else
        {
            if (tokenInfo == null)
            {
                Console.WriteLine("Token information is missing");
                return null;
            }

            var expirationTime = tokenInfo.TokenExpiry;
            var timeRemaining = expirationTime - DateTime.UtcNow;

            isAuthenticateDto.SignedIn = timeRemaining > TimeSpan.FromMinutes(5);

            if (!isAuthenticateDto.SignedIn)
            {
                var refreshToken = await _repo.GetRefreshTokenByTokenAsync(accessToken, cancellationToken);
                if (refreshToken == null) return null;

                Console.WriteLine("User is logged in, but token needs to be refreshed");
                isAuthenticateDto.RefreshToken = refreshToken;
            }
        }

        return isAuthenticateDto;
    }

    private JwtSecurityToken DecodeJwtToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

        if (jsonToken == null)
            throw new Exception("Invalid JWT token");

        return jsonToken;
    }

    private UserInfoFromTokenDto GetUserInfoFromToken(JwtSecurityToken jwtSecurityToken)
    {
        var guidId = jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == "nameid")?.Value;
        var nickname = jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;
        var email = jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == "email")?.Value;
        var audience = jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == "aud")?.Value;

        if (guidId == null || nickname == null || email == null || audience == null)
            throw new Exception("Failed to decode token: one or more required claims are missing");

        var userInfo = new UserInfoFromTokenDto
        {
            GuidId = guidId,
            NickName = nickname,
            Email = email,
            TokenExpiry = jwtSecurityToken.ValidTo,
            Audience = audience
        };
        return userInfo;
    }

    public string? GetUserGuidId(JwtSecurityToken jwtSecurityToken)
    {
        return jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == "nameid")?.Value;
    }
    public string? GetUserNickName(JwtSecurityToken jwtSecurityToken)
    {
        return jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value ?? "Anonymous";
    }
}