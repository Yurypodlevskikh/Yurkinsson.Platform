using BusinessLogic.DTOs;
using BusinessLogic.Interfaces;
using BusinessLogic.Models;
using System.Text.Json;
using WebApi.Extensions;

namespace WebApi.Endpoints;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var authEndpoints = endpoints.MapGroup("/api").RequireRateLimiting("IdentityLimiter");

        // Public endpoints
        authEndpoints.MapPost("/authenticate", async (IIdentityApiService identityApiService, AuthenticateRequest model, IConfiguration config) =>
        {
            model.Audience = config["IdentityApiSettings:Audience"];

            var response = await identityApiService.AuthenticateAsync(model);
            return Results.Ok(response);
        }).RequireRateLimiting("IdentityLimiter");

        authEndpoints.MapPost("/register", async (RegisterUserRequest model, IAuthService authService) =>
        {
            // Validate incoming registration data
            if (string.IsNullOrEmpty(model.Nickname) || 
            string.IsNullOrEmpty(model.Email) || 
            string.IsNullOrEmpty(model.Password))
            {
                return Results.BadRequest(new { success = false, message = "Nickname, email, and password are required" });
            }

            var (response, statusCode) = await authService.RegisterUserAsync(model);

            return Results.Json(response, statusCode: statusCode);
        }).RequireRateLimiting("IdentityLimiter");
        
        // POST /api/confirm-email (proxy)
        authEndpoints.MapPost("/confirm-email", async (IIdentityApiService identityApiService, ConfirmEmail model) =>
        {
            if (model == null || string.IsNullOrEmpty(model.userId) || string.IsNullOrEmpty(model.token))
                return Results.BadRequest(new { success = false, message = "userId and token are required." });

            var response = await identityApiService.ConfirmEmailAsync(model);
            var content = await response.Content.ReadAsStringAsync();

            MiniApiResponse? identityResp = null;
            try
            {
                identityResp = System.Text.Json.JsonSerializer.Deserialize<MiniApiResponse>(content, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                // ignore; fallback below
            }

            if (response.IsSuccessStatusCode && identityResp != null && identityResp.Success)
            {
                return Results.Ok(new { success = true, message = identityResp.Message ?? "Email confirmed successfully." });
            }

            var code = identityResp?.ErrorCode ?? string.Empty;
            var msg = identityResp?.Message ?? "Email confirmation failed.";

            return code switch
            {
                "TOKEN_EXPIRED" => Results.BadRequest(new { success = false, message = msg, errorCode = code, userId = identityResp?.UserId }),
                "INVALID_TOKEN" => Results.BadRequest(new { success = false, message = msg, errorCode = code }),
                "USER_NOT_FOUND" => Results.NotFound(new { success = false, message = msg, errorCode = code }),
                "ALREADY_CONFIRMED" => Results.Conflict(new { success = false, message = msg, errorCode = code }),
                _ => Results.BadRequest(new { success = false, message = msg, errorCode = identityResp?.ErrorCode ?? "CONFIRM_FAILED" })
            };
        }).RequireRateLimiting("IdentityLimiter");

        // POST /api/resend-confirmation (proxy) — body: { userId: "<id>" }
        authEndpoints.MapPost("/resend-confirmation", async (IIdentityApiService identityApiService, JsonElement payload) =>
        {
            if (!payload.TryGetProperty("userId", out var userIdProp) || string.IsNullOrWhiteSpace(userIdProp.GetString()))
                return Results.BadRequest(new { success = false, message = "userId is required" });

            var userId = userIdProp.GetString()!;
            var resp = await identityApiService.ResendConfirmationAsync(userId);

            if (!resp.IsSuccessStatusCode)
            {
                var error = await resp.Content.ReadAsStringAsync();
                return Results.BadRequest(new { success = false, message = "Failed to send confirmation email." });
            }

            return Results.Ok(new { success = true, message = "New confirmation email sent." });
        }).RequireRateLimiting("IdentityLimiter");

        authEndpoints.MapPost("/forgot-password", async (IIdentityApiService identityApiService, ForgotPasswordDto model) =>
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                return Results.BadRequest("Email is required");
            }

            var response = await identityApiService.ForgotPasswordAsync(model);

            if (!response.IsSuccessStatusCode)
            {
                return Results.Problem("Password reset failed", statusCode: (int)response.StatusCode);
            }

            return Results.Ok("Password reset link sent to your email");
        });

        authEndpoints.MapPost("/reset-password", async (IIdentityApiService identityApiService, ResetPasswordRequestdDto model) =>
        {
            if (string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Token))
            {
                return Results.BadRequest("Invalid request");
            }

            var result = await identityApiService.ResetPasswordAsync(model);
            if (!result.IsSuccessStatusCode)
            {
                return Results.Problem("Password reset failed", statusCode: (int)result.StatusCode);
            }

            return Results.Ok("Password has been reset successfully.");
        });

        authEndpoints.MapPost("/refresh-token", async (IIdentityApiService identityApiService, TokenRequestModel model, CancellationToken cancellationToken) =>
        {
            var refreshTokens = await identityApiService.RefreshTokenAsync(model, cancellationToken);

            if (refreshTokens == null)
                return Results.Unauthorized();

            return Results.Ok(refreshTokens);
        }).RequireRateLimiting("IdentityLimiter");

        var protectedEndpoints = endpoints.MapGroup("/api").MapProtectedGroup().RequireRateLimiting("IdentityLimiter");

        // Protected endpoints
        protectedEndpoints.MapPost("/logout", async (HttpContext httpContext, IIdentityApiService identityApiService) =>
        {
            try
            {
                var accessToken = httpContext.GetAccessToken();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Results.BadRequest("Access token is missing");
                }
                
                var loggedOutResult = await identityApiService.LogoutAsync(accessToken);
                        if (loggedOutResult.IsSuccessStatusCode)
                        {
                            return Results.Ok("You have been logged out");
                        }
                        else
                        {
                            return Results.BadRequest("Failed to logout");
                        }
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        }).RequireRateLimiting("IdentityLimiter");

        protectedEndpoints.MapDelete("/delete-account", async (HttpContext httpContext, IIdentityApiService identityApiService) =>
        {
            try
            {
                var accessToken = httpContext.GetAccessToken();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Results.BadRequest("Access token is missing");
                }

                var deleted = await identityApiService.DeleteAccountAsync(accessToken);
                    if (deleted.IsSuccessStatusCode)
                    {
                        return Results.Ok("Your account has been deleted");
                    }
                    else
                    {
                        return Results.BadRequest("Failed to delete account");
                    }
            }
            catch (Exception /*ex*/)
            {
                return Results.BadRequest(/*ex.Message*/);
            }
        }).RequireRateLimiting("IdentityLimiter");

        protectedEndpoints.MapPost("/change-password", async (
            HttpContext httpContext,
            IIdentityApiService identityApiService,
            ChangePasswordRequest model,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var accessToken = httpContext.GetAccessToken();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Results.BadRequest("Access token is missing");
                }

                var response = await identityApiService.ChangePasswordAsync(accessToken, model);
                if (response.IsSuccessStatusCode)
                {
                    return Results.Ok("Password changed successfully");
                }
                else
                {
                    return Results.BadRequest("Failed to change password");
                }
            }
            catch (Exception /*ex*/)
            {
                return Results.BadRequest(/*ex.Message*/"An error occured while trying to change password.");
            }
        }).RequireRateLimiting("IdentityLimiter");

        protectedEndpoints.MapPost("/change-nickname", async (
            HttpContext httpContext,
            IIdentityApiService identityApiService,
            NicknameModel model, CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrEmpty(model.Nickname))
            {
                return Results.BadRequest("Nickname is required.");
            }

            try
            {
                var accessToken = httpContext.GetAccessToken();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Results.BadRequest("Access token is missing");
                }

                var changeNicknameRequest = new ChangeNicknameRequest
                {
                    AccessToken = accessToken,
                    Nickname = model.Nickname
                };

                var nicknameChanged = await identityApiService.ChangeNicknameAsync(changeNicknameRequest, cancellationToken);

                if (nicknameChanged.IsChanged)
                {
                    return Results.Ok(nicknameChanged);
                }
                else
                {
                    return Results.BadRequest("Failed to change nickname");
                }
            }
            catch (Exception /*ex*/)
            {
                return Results.BadRequest(/*ex.Message*/);
            }
        }).RequireRateLimiting("IdentityLimiter");
    }
}