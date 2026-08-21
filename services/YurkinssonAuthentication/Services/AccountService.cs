using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using MimeKit.Encodings;
using Org.BouncyCastle.Crypto.Parameters;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using YurkinssonAuthentication.Controllers;
using YurkinssonAuthentication.Data;
using YurkinssonAuthentication.DTOs;
using YurkinssonAuthentication.DTOs.User;
using YurkinssonAuthentication.Models;
using YurkinssonAuthentication.Services.Interfaces;

namespace YurkinssonAuthentication.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AccountService> _logger;

        public AccountService(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration config,
            IEmailSender emailSender,
            ILogger<AccountService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _config = config;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task<(bool Succeeded, string[] Errors)> RegisterUserAsync(RegisterDto registerDto, CancellationToken cancellationToken = default)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                _logger.LogWarning($"User {registerDto.Email} is already exsists.");

                return (false, new[] {"A user with that email already exists."} );
            }

            var user = new AppUser
            {
                Nickname = registerDto.Nickname,
                UserName = registerDto.Email,
                Email = registerDto.Email,
                RegistrationDate = DateTime.UtcNow,
                LastLoginTime = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            { var errors = result.Errors.Select(e => e.Description).ToArray();
                _logger.LogWarning($"User registration failed: {string.Join(", ", errors)}");
                return (false, errors);
            }

            if (!await _roleManager.RoleExistsAsync("User"))
            {
                var role = new IdentityRole("User");
                await _roleManager.CreateAsync(role);
            }

            await _userManager.AddToRoleAsync(user, "User");

            // Generate an email confirmation token
            var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Encode token using Base64 URL-safe encoding and then URL-escape it for safety in query string
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailToken));
            var safeToken = Uri.EscapeDataString(encodedToken);

            // Ensure userId is safe for use in URL
            var safeUserId = Uri.EscapeDataString(user.Id);

            // Resolve frontend base URL:
            // 1. Prefer a client-specific redirect configured at ClientRedirectUrls:{ClientApp}
            // 2. Fallback to a global Frontend:BaseUrl (environment-specific)
            string? clientBase = null;
            if (!string.IsNullOrWhiteSpace(registerDto.ClientApp))
            {
                clientBase = _config[$"ClientRedirectUrls:{registerDto.ClientApp}"];
            }

            if (string.IsNullOrWhiteSpace(clientBase))
            {
                clientBase = _config["Frontend:BaseUrl"];
            }

            // If still not found, fail fast (so configuration errors are fixed)
            if (string.IsNullOrWhiteSpace(clientBase))
            {
                throw new InvalidOperationException("No frontend base URL configured. Set 'Frontend:BaseUrl' or 'ClientRedirectUrls:{ClientApp}' in configuration.");
            }

            // Normalize and build link (no trailing slash)
            clientBase = clientBase.TrimEnd('/');
            var confirmLink = $"{clientBase}/email-confirmation?userId={safeUserId}&token={safeToken}";

                // Letter parameters
                var subject = "Confirmation of registration";
            var htmlMessage = $@"<h1>Hello {user.Nickname}!</h1>
                <h2>Welcome to our service!</h2>
                <p>Please confirm your account by clicking the link below.</p>
                <p><a href='{confirmLink}'>Confirm email link</a></p>
                <p>Once you confirm your registration, you can immediately try logging into your account using your email and password.</p>
                <p>If you did not register for this account, please ignore this email.</p><br/>
                <p>Best regards,<p/>
                <p>The SpeedUp team";

            // Email sending
            await _emailSender.SendEmailAsync(user.Email, subject, htmlMessage);

            return (true, Array.Empty<string>());
        }

        public async Task<TokenResult> LoginUser(LoginDto logoDto)
        {
            var user = await _userManager.FindByEmailAsync(logoDto.Email);
            var tokenForUserResult = new TokenResult();

            if (user == null)
            {
                // _logger.LogWarning($"Login failed: user {logoDto.Email} not found.");
                tokenForUserResult.SignedIn = SignInResult.Failed;
                return tokenForUserResult;
            }

            SignInResult? result = await _signInManager.PasswordSignInAsync(
                user, logoDto.Password, logoDto.RememberMe, lockoutOnFailure: true);
            
            tokenForUserResult.SignedIn = result;

            if (result.Succeeded)
            {
                // _logger.LogInformation($"User {logoDto.Email} logged in successfully.");

                var roles = await _userManager.GetRolesAsync(user);

                var tokenModel = new TokenModel
                {
                    NameIdentifier = user.Id,
                    Nickname = user.Nickname,
                    Email = user.Email,
                    UserRoles = roles.ToList(),
                    Audience = logoDto.Audience
                };

                tokenForUserResult.JwtToken = this.GenerateTokenString(tokenModel);
                tokenForUserResult.RefreshToken = this.GenerateRefreshTokenString();
                if (!int.TryParse(_config["JwtYurkIdentity:RefreshTokenExpiry"], out int hoursToRt))
                {
                    hoursToRt = 12;
                };
                tokenForUserResult.RefreshTokenExpiry = DateTime.UtcNow.AddHours(hoursToRt);

                user.LastLoginTime = DateTime.UtcNow;
                user.RefreshToken = tokenForUserResult.RefreshToken;
                user.RefreshTokenExpiry = tokenForUserResult.RefreshTokenExpiry;
                await _userManager.UpdateAsync(user);
            }
            else if (result.IsLockedOut)
            {
                // _logger.LogWarning($"User {logoDto.Email} account locked out.");
            }

            return tokenForUserResult;
        }

        public async Task<ChangeNicknameResponse> ChangeUserNickname(ChangeNicknameRequest changeNickname, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(changeNickname.UserId);
            if (user == null)
            {
                // _logger.LogWarning($"User not found.");
                return new ChangeNicknameResponse
                {
                    IsChanged = false,
                    NewNickname = ""
                };
            }
            user.Nickname = changeNickname.NewNickname;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                // _logger.LogWarning($"Nickname change failed.");
                return new ChangeNicknameResponse
                {
                    IsChanged = false,
                    NewNickname = ""
                };
            }
            // _logger.LogInformation($"Nickname changed successfully.");
            return new ChangeNicknameResponse
            {
                IsChanged = true,
                NewNickname = changeNickname.NewNickname
            };
        }

        public async Task<TokenResult> RefreshToken(RefreshTokenModel refreshTokenModel)
        {
            var tokenForUserResult = new TokenResult();
            var principal = GetTokenPrincipal(refreshTokenModel.JwtToken);
            
            string? audienceClaim = principal?.FindFirst("aud")?.Value;
            if (string.IsNullOrEmpty(audienceClaim))
            {
                //throw new Exception("Audience claim not found in token.");
                tokenForUserResult.SignedIn = SignInResult.Failed;
                return tokenForUserResult;
            }

            string? nameIdentifier = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            
            if(string.IsNullOrEmpty(nameIdentifier))
            {
                //throw new Exception("NameIdentifier not found in token.");
                tokenForUserResult.SignedIn = SignInResult.Failed;
                return tokenForUserResult;
            }
            
            var identityUser = await _userManager.FindByIdAsync(nameIdentifier);

            //if (identityUser == null || identityUser.RefreshToken != refreshTokenModel.RefreshToken ||
            //    identityUser.RefreshTokenExpiry < DateTime.UtcNow)
            //{
            //    //throw new Exception("Refresh token is not identical or has expired.");
            //    tokenForUserResult.SignedIn = SignInResult.Failed;
            //    return tokenForUserResult;
            //}

            Console.WriteLine("=== REFRESH VALIDATION ===");
            Console.WriteLine($"User found: {identityUser != null}");
            Console.WriteLine($"Refresh token matches: {identityUser?.RefreshToken == refreshTokenModel.RefreshToken}");
            Console.WriteLine($"Refresh token expiry: {identityUser?.RefreshTokenExpiry:O}");
            Console.WriteLine($"Current UTC: {DateTime.UtcNow:O}");
            Console.WriteLine("==========================");

            if (identityUser == null)
            {
                Console.WriteLine($"Refresh token failed: user not found. UserId: {nameIdentifier}");
                tokenForUserResult.SignedIn = SignInResult.Failed;
                return tokenForUserResult;
            }

            if (identityUser.RefreshToken != refreshTokenModel.RefreshToken)
            {
                Console.WriteLine("Refresh token failed: refresh token mismatch.");
                tokenForUserResult.SignedIn = SignInResult.Failed;
                return tokenForUserResult;
            }

            if (identityUser.RefreshTokenExpiry < DateTime.UtcNow)
            {
                Console.WriteLine(
                    $"Refresh token failed: token expired at {identityUser.RefreshTokenExpiry:u}.");
                tokenForUserResult.SignedIn = SignInResult.Failed;
                return tokenForUserResult;
            }

            tokenForUserResult.SignedIn = SignInResult.Success;
            var roles = await _userManager.GetRolesAsync(identityUser);

            DateTime refreshTokenExpiry;
            if (int.TryParse(_config["JwtYurkIdentity:RefreshTokenExpiry"], out int hoursToAdd)) {
                refreshTokenExpiry = DateTime.UtcNow.AddHours(hoursToAdd);
                tokenForUserResult.RefreshTokenExpiry = refreshTokenExpiry;
            }else
            {
                refreshTokenExpiry = DateTime.UtcNow.AddHours(12);
            }

            var tokenModel = new TokenModel
            {
                NameIdentifier = identityUser.Id,
                Nickname = identityUser.Nickname,
                Email = identityUser.Email,
                UserRoles = roles.ToList(),
                Audience = audienceClaim
            };
            tokenForUserResult.JwtToken = this.GenerateTokenString(tokenModel);
            tokenForUserResult.RefreshToken = this.GenerateRefreshTokenString();

            
            
            identityUser.RefreshToken = tokenForUserResult.RefreshToken;
            identityUser.RefreshTokenExpiry = refreshTokenExpiry;
            await _userManager.UpdateAsync(identityUser);

            return tokenForUserResult;
        }

        public async Task<EmailConfirmationResult> UserConfirmsEmail(ConfirmEmail confirmEmail)
        {
            var user = await _userManager.FindByIdAsync(confirmEmail.userId);
            if(user == null)
            {
                _logger.LogWarning($"User not found for id: {confirmEmail.userId}");
                return new EmailConfirmationResult 
                { 
                    Succeeded = false, 
                    CanResend = false, 
                    ErrorMessage = "User with the specified ID was not found",
                    ErrorCode = ErrorCodes.USER_NOT_FOUND
                };
            }

            if(user.EmailConfirmed)
            {
                _logger.LogInformation($"Email already confirmed for user id: {confirmEmail.userId}");
                return new EmailConfirmationResult 
                { 
                    Succeeded = true, 
                    CanResend = false, 
                    ErrorMessage = "Email is already confirmed.",
                    ErrorCode = ErrorCodes.ALREADY_CONFIRMED
                };
            }

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(confirmEmail.token));

            _logger.LogInformation($"Decoded token: {decodedToken}");

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                var errorMessages = result.Errors.Select(e => e.Description);

                if (errorMessages.Any(m => m.Contains("invalid", StringComparison.OrdinalIgnoreCase)))
                {
                    if (!user.EmailConfirmed)
                    {
                        return new EmailConfirmationResult
                        { 
                        Succeeded = false,
                        CanResend = true,
                        ErrorMessage = "Confirmation link has expired or is invalid.",
                        ErrorCode = ErrorCodes.INVALID_TOKEN,
                            UserId = user.Id
                        };
                    }
                }

                return new EmailConfirmationResult
                {
                    Succeeded = false,
                    CanResend = false,
                    ErrorMessage = "Email confirmation error.",
                    ErrorCode = ErrorCodes.CONFIRM_FAILED
                };
            }

            return new EmailConfirmationResult { Succeeded = true };
        }

        public async Task<bool> ResetPasswordMessage(ForgotPasswordDto forgotPassword)
        {
            var user = await _userManager.FindByEmailAsync(forgotPassword.Email!);
            if (user == null) { return false; }

            var emailToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Encode the password reset token using Base64 URL-safe encoding and then URL-escape it for query string
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailToken));
            var safeToken = Uri.EscapeDataString(encodedToken);

            // Resolve frontend base URL using the same configuration pattern as for email confirmation:
            // 1. Prefer a client-specific redirect at ClientRedirectUrls:SpeedUpVue
            // 2. Fallback to Frontend:BaseUrl
            string? clientBase = _config["ClientRedirectUrls:SpeedUpVue"];
            if (string.IsNullOrWhiteSpace(clientBase))
            {
                clientBase = _config["Frontend:BaseUrl"];
            }

            if (string.IsNullOrWhiteSpace(clientBase))
            {
                throw new InvalidOperationException("No frontend base URL configured. Set 'Frontend:BaseUrl' or 'ClientRedirectUrls:SpeedUpVue' in configuration.");
            }

            clientBase = clientBase.TrimEnd('/');

            var safeEmail = Uri.EscapeDataString(user.Email!);

            // Build reset link that matches SPA route: /reset-password?token=...&email=...
            var resetLink = $"{clientBase}/reset-password?token={safeToken}&email={safeEmail}";

            var subject = "Reset password";
            var htmlMessage = $@"<p>Please reset your password by clicking the link below.</p>
                        <p><a href='{resetLink}'>Reset password</a></p>";

            await _emailSender.SendEmailAsync(user.Email!, subject, htmlMessage);

            return true;
        }

        public async Task<IdentityResult> ResendConfirmationEmail(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                var error = new IdentityError
                {
                    Code = "UserNotFound",
                    Description = "User with the specified ID was not found."
                };
                return IdentityResult.Failed(error);
            }

            if (user.EmailConfirmed)
            {
                var error = new IdentityError
                {
                    Code = "BadRequest",
                    Description = "Email already confirmed"
                };

                return IdentityResult.Failed(error);
            }

            // Generate an email confirmation token
            var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Base64Url encode the token and escape for URL
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailToken));
            var safeToken = Uri.EscapeDataString(encodedToken);

            // Ensure userId is safe for use in URL
            var safeUserId = Uri.EscapeDataString(user.Id);

            // Resolve frontend base URL (ClientRedirectUrls:SpeedUpVue or Frontend:BaseUrl)
            string? clientBase = null;
            clientBase = _config["ClientRedirectUrls:SpeedUpVue"];

            if (string.IsNullOrWhiteSpace(clientBase))
            {
                clientBase = _config["Frontend:BaseUrl"];
            }

            if (string.IsNullOrWhiteSpace(clientBase))
            {
                throw new InvalidOperationException("No frontend base URL configured. Set 'Frontend:BaseUrl' or 'ClientRedirectUrls:SpeedUpVue' in configuration.");
            }

            clientBase = clientBase.TrimEnd('/');
            var confirmLink = $"{clientBase}/email-confirmation?userId={safeUserId}&token={safeToken}";

            var subject = "New confirmation email";
            var htmlMessage = $@"<p>Hello {user.Nickname}!</p>
                                <p>Your previous confirmation link has expired.</p>
                                <p><a href='{confirmLink}'>Confirm your email</a></p>";

            await _emailSender.SendEmailAsync(user.Email, subject, htmlMessage);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> ResetPassword(ResetPasswordDto resetPassword)
        {
            var user = await _userManager.FindByEmailAsync(resetPassword.Email!);
            if (user == null)
            {
                var error = new IdentityError
                {
                    Code = "RequestIncorrect",
                    Description = "Invalid request."
                };
                return IdentityResult.Failed(error);
            }

            // Try to decode token that was Base64Url-encoded and URL-escaped when sent in email.
            // If decoding fails, fall back to the original token for backward compatibility.
            string tokenToUse = resetPassword.Token!;
            try
            {
                // First, unescape any URL-escaping (in case callers pass the escaped token)
                var unescaped = Uri.UnescapeDataString(tokenToUse);
                var decodedBytes = WebEncoders.Base64UrlDecode(unescaped);
                var decodedString = Encoding.UTF8.GetString(decodedBytes);
                tokenToUse = decodedString;
            }
            catch
            {
                // If decoding fails, assume token was sent in raw form and proceed.
            }

            return await _userManager.ResetPasswordAsync(user, tokenToUse, resetPassword.Password!);
        }

        public async Task<AppUser> GetUserByIdAsync(string userId, CancellationToken cancellationToken)
        {
            var userInfo = await _userManager.FindByIdAsync(userId);
            if(userInfo == null || string.IsNullOrEmpty(userInfo.RefreshToken))
            {
                throw new InvalidOperationException("User is not logged in.");
            }

            return userInfo;
        }

        public async Task<IdentityResult> UpdateUserAsync(AppUser user, CancellationToken cancellationToken = default)
        {
            var result = await _userManager.UpdateAsync(user);

            return result;
        }

        public async Task<IdentityResult> DeleteUserAsync(AppUser user, CancellationToken cancellationToken = default)
        {
            return await _userManager.DeleteAsync(user);
        }

        public async Task<IdentityResult> ChangePasswordAsync(AppUser user, ChangePasswordRequest changePasswordRequest)
        {
            var passwordCheck = await _userManager.CheckPasswordAsync(user, changePasswordRequest.CurrentPassword);
            if(!passwordCheck)
            {
                var error = new IdentityError
                {
                    Code = "PasswordIncorrect",
                    Description = "The current password is incorrect."
                };
                return IdentityResult.Failed(error);
            }

            var result = await _userManager.ChangePasswordAsync(user, changePasswordRequest.CurrentPassword, changePasswordRequest.NewPassword);
            
            return result;
        }

        private ClaimsPrincipal? GetTokenPrincipal(string jwtToken)
        {
            var audiences = _config["JwtYurkIdentity:Audiences"]
    ?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtYurkIdentity:Key"]));
            var validation = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _config["JwtYurkIdentity:Issuer"],
                ValidateAudience = true,
                ValidAudiences = audiences,
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey
            };

            try
            {
                var claimsPrincipal = tokenHandler.ValidateToken(jwtToken, validation, out var validatedToken);

                if (validatedToken is JwtSecurityToken jwtSecToken &&
                    !jwtSecToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token algorithm");
                }

                return claimsPrincipal;
            }
            catch (Exception ex)
            {
                throw new SecurityTokenException($"Token validation failed: {ex.Message}");
            }
        }

        private string GenerateTokenString(TokenModel tokenModel)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtYurkIdentity:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, tokenModel.NameIdentifier),
                new (ClaimTypes.Name, tokenModel.Nickname),
                new (ClaimTypes.Email, tokenModel.Email)
            };
            claims.AddRange(tokenModel.UserRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            DateTime tokenExpiry;
            if (int.TryParse(_config["JwtYurkIdentity:TokenExpirationInMinutes"], out int minutesToAdd))
            {
                tokenExpiry = DateTime.UtcNow.AddMinutes(minutesToAdd);
            }
            else
            {
                tokenExpiry = DateTime.UtcNow.AddMinutes(5);
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = tokenExpiry,
                Issuer = _config["JwtYurkIdentity:Issuer"],
                Audience = tokenModel.Audience,
                SigningCredentials = creds
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[64];
            using (var numberGenerator = RandomNumberGenerator.Create())
            {
                numberGenerator.GetBytes(randomNumber);
            }
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<bool> StartDeleteAccountAsync(string userId, string password)
        {
            if (string.IsNullOrEmpty(userId)) return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            // Verify the provided current password before generating the deletion token
            var passwordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!passwordValid) return false;

            // Generate a deletion token with a distinct purpose to avoid confusion with other tokens
            // Use the default token provider and a dedicated purpose string "DeleteAccount"
            var deletionToken = await _userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, "DeleteAccount");

            // Encode the deletion token using the same pattern used for password reset/confirmation:
            // Base64UrlEncode(UTF8(token)) and then Uri.EscapeDataString
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(deletionToken));
            var safeToken = Uri.EscapeDataString(encodedToken);

            // Ensure frontend base URL is resolved from configuration (prefer client-specific redirect)
            string? clientBase = _config["ClientRedirectUrls:SpeedUpVue"];
            if (string.IsNullOrWhiteSpace(clientBase))
            {
                clientBase = _config["Frontend:BaseUrl"];
            }

            if (string.IsNullOrWhiteSpace(clientBase))
            {
                throw new InvalidOperationException("No frontend base URL configured. Set 'Frontend:BaseUrl' or 'ClientRedirectUrls:SpeedUpVue' in configuration.");
            }

            clientBase = clientBase.TrimEnd('/');

            // Build confirmation link for SPA to call the BFF confirm-delete endpoint (SPA route example: /confirm-delete)
            var safeUserId = Uri.EscapeDataString(user.Id);
            var deleteLink = $"{clientBase}/confirm-delete?userId={safeUserId}&token={safeToken}";

            var subject = "Confirm account deletion";
            var htmlMessage = $@"<h3>Account deletion requested</h3>
<p>You (or someone with access to your account) requested account deletion. This operation is permanent and will remove your account and associated data.</p>
<p>If you initiated this request, confirm account deletion by clicking the link below. This link is valid for a short time:</p>
<p><a href='{deleteLink}'>Confirm account deletion</a></p>
<p>If you did not request deletion, ignore this message or contact support.</p>";

    await _emailSender.SendEmailAsync(user.Email, subject, htmlMessage);

    return true;
}
    }
}
