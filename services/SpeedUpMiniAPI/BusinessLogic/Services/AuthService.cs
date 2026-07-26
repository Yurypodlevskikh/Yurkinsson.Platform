using BusinessLogic.DTOs;
using BusinessLogic.Interfaces;
using BusinessLogic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class AuthService : IAuthService
    {
        private readonly IIdentityApiService _identityApiService;
        public AuthService(IIdentityApiService identityApiService)
        {
            _identityApiService = identityApiService;
        }

        public async Task<(ApiResponseDto Response, int StatusCode)> RegisterUserAsync(RegisterUserRequest model)
        {
            var proxyRequest = new RegisterUserProxyRequestDto
            {
                Nickname = model.Nickname,
                Email = model.Email,
                Password = model.Password,
                ClientApp = "SpeedUpVue"
            };

            try
            {
                var response = await _identityApiService.RegisterAsync(proxyRequest);

                var content = await response.Content.ReadAsStringAsync();

                IdentityApiResponse? identityResult = null;

                if (!string.IsNullOrEmpty(content))
                {
                    identityResult = JsonSerializer.Deserialize<IdentityApiResponse>(
                        content,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                }

                if (response.IsSuccessStatusCode)
                {
                    return (
                        ApiResponseDto.Ok(identityResult?.Message ?? "Registration Successful. Please check your email to confirm your account."),
                        200
                        );
                }

                // Mapping a message from Identity
                return (ApiResponseDto.Fail(identityResult?.Message ?? "Registration failed."), 400);
            }
            catch (HttpRequestException)
            {
                return (ApiResponseDto.Fail("Identity service is unavailable."), 503);
            }
        }
    }
}
