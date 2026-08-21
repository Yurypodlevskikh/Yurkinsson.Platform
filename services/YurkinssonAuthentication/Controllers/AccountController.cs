using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using YurkinssonAuthentication.Data;
using YurkinssonAuthentication.DTOs;
using YurkinssonAuthentication.DTOs.User;
using YurkinssonAuthentication.Models;
using YurkinssonAuthentication.Services;
using YurkinssonAuthentication.Services.Interfaces;

namespace YurkinssonAuthentication.Controllers
{
    [Route("api/account")]
    [ApiController]
    [EnableRateLimiting("ControllerTokenIpLimiter")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IAccountService _accountService;
        private readonly IAudienceService _audienceService;
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<AccountController> _logger;
        private readonly IWebHostEnvironment _env;

        public AccountController(UserManager<AppUser> userManager, 
            IAccountService accountService, 
            IAudienceService audienceService, AppDbContext appDbContext, 
            ILogger<AccountController> logger, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _accountService = accountService;
            _audienceService = audienceService;
            _appDbContext = appDbContext;
            _logger = logger;
            _env = env;
        }

        [HttpPost("register")]
        [EnableRateLimiting("SingleSlidingLimiter")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            _logger.LogInformation("Received registration request for email: {Email}", registerDto.Email);
            

            if (registerDto == null)
            {
                return BadRequest(new ApiResponse
                { 
                    Success = false, 
                    Message = "Invalid request", 
                    ErrorCode = ErrorCodes.INVALID_MODEL.ToString(),
                });
            }

            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToArray();

                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid input data",
                    ErrorCode = ErrorCodes.VALIDATION_ERROR.ToString(),
                    ValidationErrors = validationErrors,
                    DeveloperMessage = _env.IsDevelopment() ? "Model validation failed. Check ValidationErrors for details." : null
                });
            }

            // DB connectivity check
            try
            {
                await _appDbContext.Database.OpenConnectionAsync();
                await _appDbContext.Database.CloseConnectionAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(503, new ApiResponse
                {
                    Success = false,
                    Message = "Database is currently unavailable. Please try again later.",
                    ErrorCode = ErrorCodes.REGISTRATION_FAILED.ToString(),
                    DeveloperMessage = _env.IsDevelopment() ? $"Failed to connect to the database: {ex.Message}" : null
                });
            }

            // Migration
            try
            {
                await _appDbContext.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse 
                { 
                    Success = false, 
                    Message = "Unexpected error",
                    ErrorCode = ErrorCodes.REGISTRATION_FAILED.ToString(),
                    DeveloperMessage = _env.IsDevelopment() ? ex.Message : null
                });
            }

            var (succeeded, errors) = await _accountService.RegisterUserAsync(registerDto);
            if (succeeded)
            {
                return Ok(new ApiResponse 
                { 
                    Success = true, 
                    Message = "User registered successfully. Please check your email to confirm your account." 
                });
            }

            if(errors.Contains("A user with that email already exists."))
            {
                return Conflict(new ApiResponse 
                { 
                    Success = false,
                    Message = "A user with that email already exists.",
                    ErrorCode = ErrorCodes.EMAIL_ALREADY_EXISTS.ToString(),
                });
            }

            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "User registration failed. Please check the errors for details.",
                ErrorCode = ErrorCodes.REGISTRATION_FAILED.ToString(),
                ValidationErrors = errors.ToArray(),
                DeveloperMessage = _env.IsDevelopment() ? "User registration failed. Check ValidationErrors for details." : null
            });
        }

        [HttpGet("confirm-email")]
        [EnableRateLimiting("SingleSlidingLimiter")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "One or more validation errors occureed.",
                    ErrorCode = ErrorCodes.INVALID_TOKEN.ToString(),
                    DeveloperMessage = _env.IsDevelopment() ? "Missing userId or token query parameters." : null
                });
            }

            try
            {
                var confirmEmail = new ConfirmEmail() { userId = userId, token = token };
                var result = await _accountService.UserConfirmsEmail(confirmEmail);

                if (result == null)
                {
                    var respNull = new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid email confirmation request.",
                        ErrorCode = ErrorCodes.CONFIRM_FAILED.ToString(),
                        DeveloperMessage = _env.IsDevelopment() ? "Email confirmation result is null." : null
                    };
                    return StatusCode(500, respNull);
                }

                if (result.Succeeded)
                {
                    return Ok(new ApiResponse
                    {
                        Success = true,
                        Message = "Email successfully confirmed."
                    });
                }

                // Map known cases
                if (result.ErrorCode == ErrorCodes.USER_NOT_FOUND || (result.ErrorMessage?.IndexOf("not found", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                        return NotFound(new ApiResponse
                        {
                            Success = false,
                            Message = "User with the specified ID was not found.",
                            ErrorCode = ErrorCodes.USER_NOT_FOUND.ToString(),
                            DeveloperMessage = _env.IsDevelopment() ? result.ErrorMessage : null
                        });
                    }

                // Token invalid/expired and can resend -> TOKEN_EXPIRED
                    if(result.CanResend)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Confirmation link is invalid or has expired.",
                        ErrorCode = ErrorCodes.TOKEN_EXPIRED.ToString(),
                        UserId = result.UserId, // safe to return so frontend can call resend
                        DeveloperMessage = _env.IsDevelopment() ? result.ErrorMessage : null
                    });
                }    
                        
                    if(result.ErrorCode == ErrorCodes.ALREADY_CONFIRMED)
                {
                    return Ok(new ApiResponse
                    {
                        Success = true,
                        Message = "Email is already confirmed.",
                        ErrorCode = ErrorCodes.ALREADY_CONFIRMED.ToString(),
                        DeveloperMessage = _env.IsDevelopment() ? result.ErrorMessage : null
                    });
                }

                // Generic fallback for other errors
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Email confirmation failed.",
                    ErrorCode = ErrorCodes.CONFIRM_FAILED.ToString(),
                    DeveloperMessage = _env.IsDevelopment() ? result.ErrorMessage : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while confirming email for userId={UserId}", userId);

                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An unexpected error occurred during email confirmation.",
                    ErrorCode = ErrorCodes.CONFIRM_FAILED.ToString(),
                    DeveloperMessage = _env.IsDevelopment() ? ex.ToString() : null
                });
            }
        }

        [HttpPost("login")]
        [EnableRateLimiting("SingleSlidingLimiter")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            //var audServisOrigin = _audienceService.ExtractAudiecneFromOrigin(HttpContext.Request);
            //if (string.IsNullOrEmpty(audServisOrigin) || audServisOrigin == "fake-audience")
            //    return BadRequest(new {message = "Entry is not permitted."});
            Console.WriteLine($"Login Audience: {loginDto.Audience}");

            var result = await _accountService.LoginUser(loginDto);
            if (result.SignedIn.Succeeded)
            {
                TokenResponse tr = new()
                {
                    JwtToken = result.JwtToken,
                    RefreshToken = result.RefreshToken,
                    RefreshTokenExpiry = result.RefreshTokenExpiry
                };
                return Ok(tr);
            }
            else if (result.SignedIn.IsLockedOut)
            {
                return BadRequest(new { message = "Account is locked. Please try again later." });
            }
            else if (result.SignedIn.IsNotAllowed)
            {
                return BadRequest(new { message = "Not allowed." });
            }

            return Unauthorized(new { message = "Invalid email or password." });
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(
            [FromHeader(Name = "Authorization")] string authorizationHeader,
            [FromBody] ChangePasswordRequest changePasswordRequest)
        {
            // Validate Authorization header
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                // Invalid or missing Authorization header
                return Unauthorized("Unauthorized.");
            }

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("Invalid user.");
            }
            var user = await _accountService.GetUserByIdAsync(userId, CancellationToken.None);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            if (string.IsNullOrEmpty(changePasswordRequest.CurrentPassword) || string.IsNullOrEmpty(changePasswordRequest.NewPassword))
            {
                return BadRequest("Current password and new password are required.");
            }

            if (changePasswordRequest.NewPassword.Length < 6)
            {
                return BadRequest("Password must be at least 6 characters long.");
            }

            var result = await _accountService.ChangePasswordAsync(user, changePasswordRequest);
            if (result.Succeeded)
            {
                return Ok("Password successfully changed.");
            }
            return BadRequest(result.Errors.Select(e => e.Description));
        }

        [HttpPost("change-nickname")]
        [Authorize]
        public async Task<IActionResult> ChangeNickname([FromHeader(Name = "Authorization")] string authorizationHeader, [FromBody] string nickname, CancellationToken cancellationToken)
        {
            if (authorizationHeader == null || !authorizationHeader.StartsWith("Bearer ") || nickname == null)
            {
                // Invalid request. Nickname cannot be empty.
                return Unauthorized();
            }

            // Get user ID from the token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var changeNicknameRequest = new ChangeNicknameRequest
            {
                UserId = userId,
                NewNickname = nickname
            };

            // Call the AccountService to update the nickname
            var result = await _accountService.ChangeUserNickname(changeNicknameRequest, cancellationToken);

            return Ok(result);
        }

        [HttpPost("refresh-token")]
        [EnableRateLimiting("SingleSlidingLimiter")]
        public async Task<IActionResult> RefreshToken([FromForm] RefreshTokenRequest refreshTokenRequest)
        {
            var jwtToken = Request.Headers.Authorization.ToString()?.Replace("Bearer ", string.Empty);

            Console.WriteLine("=== REFRESH REQUEST ===");
            Console.WriteLine($"JWT present: {!string.IsNullOrEmpty(jwtToken)}");
            Console.WriteLine(
                $"Refresh token present: " +
                $"{!string.IsNullOrEmpty(refreshTokenRequest.RefreshToken)}");
            Console.WriteLine("=======================");

            if (string.IsNullOrEmpty(jwtToken))
            {
                // JWT token is missing in the Authorization header
                return BadRequest("Invalid token.");
            }

            if (string.IsNullOrEmpty(refreshTokenRequest.RefreshToken))
            {
                // Refresh token is missing in the request
                return BadRequest("Invalid refresh token.");
            }

            var refreshTokenModel = new RefreshTokenModel
            {
                JwtToken = jwtToken,
                RefreshToken = refreshTokenRequest.RefreshToken
            };

            Console.WriteLine("=== REFRESH TOKEN REQUEST ===");
            Console.WriteLine($"JWT present: {!string.IsNullOrEmpty(jwtToken)}");
            Console.WriteLine($"Refresh token present: {!string.IsNullOrEmpty(refreshTokenRequest.RefreshToken)}");
            Console.WriteLine("=============================");

            var loginResult = await _accountService.RefreshToken(refreshTokenModel);
            if (loginResult.SignedIn.Succeeded)
            {
                TokenResponse tr = new()
                {
                    JwtToken = loginResult.JwtToken,
                    RefreshToken = loginResult.RefreshToken,
                    RefreshTokenExpiry = loginResult.RefreshTokenExpiry
                };
                return Ok(loginResult);
            }
            return Unauthorized();
        }

        [HttpGet("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmationEmail([FromQuery] string userId)
        {
            if(string.IsNullOrEmpty(userId))
            {
                return BadRequest("User with the specified ID was not found.");
            }

            var result = await _accountService.ResendConfirmationEmail(userId);
            
            if(!result.Succeeded)
            {
                return BadRequest("Failed to send confirmation email.");
            }

            return Ok("New confirmation email sent.");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPassword)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _accountService.ResetPasswordMessage(forgotPassword);
            if (!result)
            {
                return BadRequest("Invalid request");
            }

            return Ok();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPassword)
        {
            if (!ModelState.IsValid) return BadRequest();

            var result = await _accountService.ResetPassword(resetPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok();
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            Console.WriteLine("=== Logout controller reached ===");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("Invalid user.");
            }

            var user = await _accountService.GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiry = DateTime.MinValue;

            var result = await _accountService.UpdateUserAsync(user, cancellationToken);
            if (!result.Succeeded)
            {
                return BadRequest("Failed to log out.");
            }
            return Ok("User successfully logged out.");
        }

        [HttpDelete("delete-account")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("Invalid user.");
            }
            var user = await _accountService.GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return BadRequest("User not found.");
            }
            var result = await _accountService.DeleteUserAsync(user, cancellationToken);
            if (!result.Succeeded)
            {
                return BadRequest("Failed to delete account.");
            }
            return Ok("Account successfully deleted.");
        }

        [HttpPost("start-delete-account")]
        [Authorize]
        public async Task<IActionResult> StartDeleteAccount([FromBody] YurkinssonAuthentication.DTOs.User.DeleteAccountRequestDto model)
        {
            if (!ModelState.IsValid) return BadRequest(new { success = false, message = "Invalid request." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { success = false, message = "Invalid user." });
            }

            try
            {
                var emailSent = await _accountService.StartDeleteAccountAsync(userId, model.Password ?? string.Empty);
                if (!emailSent)
                {
                    // Either password verification failed or user/email not found
                    return BadRequest(new { success = false, message = "Failed to initiate account deletion. Check password and try again." });
                }

                return Ok(new { success = true, message = "Deletion confirmation email sent." });
            }
            catch (Exception ex)
            {
                // Do not leak internal details; log as needed
                _logger?.LogError(ex, "StartDeleteAccount failed for user {UserId}", userId);
                return StatusCode(500, new { success = false, message = "Unable to process account deletion at this time." });
            }
        }

        // New endpoint: POST /api/account/confirm-delete
        [HttpPost("confirm-delete")]
        public async Task<IActionResult> ConfirmDelete([FromBody] YurkinssonAuthentication.DTOs.User.ConfirmDeleteDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid request.",
                    ErrorCode = ErrorCodes.INVALID_MODEL.ToString()
                });
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "User not found.",
                    ErrorCode = ErrorCodes.USER_NOT_FOUND.ToString()
                });
            }

            try
            {
                var tokenValid = await _accountService.VerifyDeleteTokenAsync(model.UserId!, model.Token!);
                if (!tokenValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid or expired deletion token.",
                        ErrorCode = ErrorCodes.INVALID_TOKEN.ToString()
                    });
                }

                // Token is valid — but per task instructions do NOT delete the account yet.
                // Return success to caller so the BFF/frontend can proceed to orchestrate deletion next.
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Deletion token validated. Proceed to confirm deletion."
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error validating delete token for user {UserId}", model.UserId);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Server error while validating deletion token."
                });
            }
        }
    }
}
