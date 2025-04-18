namespace GamaEdtech.Presentation.Api.Controllers
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using Asp.Versioning;

    using GamaEdtech.Application.Interface;
    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.Data;
    using GamaEdtech.Common.Data.Enumeration;
    using GamaEdtech.Common.Identity;
    using GamaEdtech.Data.Dto.Identity;
    using GamaEdtech.Domain.Entity.Identity;
    using GamaEdtech.Domain.Enumeration;
    using GamaEdtech.Presentation.ViewModel.Identity;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.JsonWebTokens;
    using Microsoft.IdentityModel.Tokens;

    using static GamaEdtech.Common.Core.Constants;

    using Void = Common.Data.Void;

    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class IdentitiesController(Lazy<ILogger<IdentitiesController>> logger, Lazy<IIdentityService> identityService, UserManager<ApplicationUser> userManager) : ApiControllerBase<IdentitiesController>(logger)
    {
        [HttpPost("login"), Produces(typeof(ApiResponse<AuthenticationResponseViewModel>))]
        [AllowAnonymous]
        public async Task<IActionResult<AuthenticationResponseViewModel>> Login([NotNull] AuthenticationRequestViewModel request)
        {
            try
            {
                var authenticateResult = await identityService.Value.AuthenticateAsync(new AuthenticationRequestDto
                {
                    Username = request.Username!,
                    Password = request.Password!,
                    AuthenticationProvider = AuthenticationProvider.Local,
                });
                if (authenticateResult.Data?.User is null)
                {
                    return Ok<AuthenticationResponseViewModel>(new(authenticateResult.Errors));
                }

                var signInResult = await identityService.Value.SignInAsync(new SignInRequestDto { RememberMe = request.RememberMe, User = authenticateResult.Data.User });
                return Ok<AuthenticationResponseViewModel>(new(signInResult.Errors)
                {
                    Data = signInResult.OperationResult is OperationResult.Succeeded ?
                    new() { Roles = signInResult.Data?.Roles?.ListToFlagsEnum<Role>(), }
                    : null,
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<AuthenticationResponseViewModel>(new(new Error { Message = exc.Message }));
            }
        }

        [HttpPost("register"), Produces(typeof(ApiResponse<Void>))]
        [AllowAnonymous]
        public async Task<IActionResult<Void>> Register([NotNull] RegistrationRequestViewModel request)
        {
            try
            {
                var result = await identityService.Value.RegisterAsync(new()
                {
                    Username = request.Email!,
                    Password = request.Password!,
                    Email = request.Email!,
                });

                return Ok<Void>(new(result.Errors));
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<Void>(new(new Error { Message = exc.Message }));
            }
        }

        [HttpGet("logout"), Produces(typeof(ApiResponse<Void>))]
        [Permission(policy: null)]
        public async Task<IActionResult<Void>> Logout()
        {
            try
            {
                var result = await identityService.Value.SignOutAsync();

                return Ok<Void>(new(result.Errors)
                {
                    Data = result.Data,
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<Void>(new(new Error { Message = exc.Message }));
            }
        }

        [HttpPut("password"), Produces(typeof(ApiResponse<Void>))]
        [Permission(policy: null)]
        public async Task<IActionResult<Void>> ChangePassword([NotNull] ChangePasswordRequestViewModel request)
        {
            try
            {
                var result = await identityService.Value.ChangePasswordAsync(new ChangePasswordRequestDto
                {
                    CurrentPassword = request.CurrentPassword,
                    NewPassword = request.NewPassword,
                });
                return Ok<Void>(new(result.Errors));
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<Void>(new(new Error { Message = exc.Message }));
            }
        }

        [HttpPost("tokens"), Produces(typeof(ApiResponse<GenerateTokenResponseViewModel>))]
        public async Task<IActionResult<GenerateTokenResponseViewModel>> GenerateToken([NotNull] GenerateTokenRequestViewModel request)
        {
            try
            {
                var authenticateResult = await identityService.Value.AuthenticateAsync(new AuthenticationRequestDto
                {
                    Username = request.Username!,
                    Password = request.Password!,
                    AuthenticationProvider = AuthenticationProvider.Local,
                });
                if (authenticateResult.Data?.User is null)
                {
                    return Ok<GenerateTokenResponseViewModel>(new(authenticateResult.Errors));
                }

                var result = await identityService.Value.GenerateUserTokenAsync(new GenerateUserTokenRequestDto
                {
                    UserId = authenticateResult.Data.User.Id,
                    TokenProvider = PermissionConstants.ApiDataProtectorTokenProvider,
                    Purpose = PermissionConstants.ApiDataProtectorTokenProviderAccessToken,
                });
                return Ok<GenerateTokenResponseViewModel>(new(result.Errors)
                {
                    Data = new()
                    {
                        Token = result.Data?.Token,
                        ExpirationTime = result.Data?.ExpirationTime,
                    }
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<GenerateTokenResponseViewModel>(new(new Error { Message = exc.Message }));
            }
        }

        [HttpPost("tokens/old"), Produces(typeof(ApiResponse<GenerateTokenResponseViewModel>))]
        public async Task<IActionResult<GenerateTokenResponseViewModel>> GenerateTokenWithOld([NotNull, FromBody] GenerateTokenWithOldRequestViewModel request)
        {
            try
            {
                const string endpoint = "https://core.gamatrain.com/";
                var data = await new JsonWebTokenHandler().ValidateTokenAsync(request.Token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = endpoint,
                    RequireExpirationTime = true,
                    ValidateActor = false,
                    ValidateIssuerSigningKey = false,
                    ValidateSignatureLast = false,
                    SignatureValidator = (string token, TokenValidationParameters parameters) => new JsonWebToken(token),
                    ValidAudience = endpoint,
                });
                if (!data.IsValid)
                {
                    return Ok<GenerateTokenResponseViewModel>(new(new Error { Message = "Invalid Token" }));
                }

                _ = data.Claims.TryGetValue("identity", out var email);

                var user = await userManager.FindByEmailAsync(email?.ToString()!);
                if (user is null)
                {
                    return Ok<GenerateTokenResponseViewModel>(new(new Error { Message = "Invalid Token" }));
                }

                var result = await identityService.Value.GenerateUserTokenAsync(new GenerateUserTokenRequestDto
                {
                    UserId = user.Id,
                    TokenProvider = PermissionConstants.ApiDataProtectorTokenProvider,
                    Purpose = PermissionConstants.ApiDataProtectorTokenProviderAccessToken,
                });
                return Ok<GenerateTokenResponseViewModel>(new(result.Errors)
                {
                    Data = new()
                    {
                        Token = result.Data?.Token,
                        ExpirationTime = result.Data?.ExpirationTime,
                    }
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<GenerateTokenResponseViewModel>(new(new Error { Message = exc.Message }));
            }
        }

        [HttpPost("tokens/google"), Produces(typeof(ApiResponse<GenerateTokenResponseViewModel>))]
        public async Task<IActionResult<GenerateTokenResponseViewModel>> GenerateTokenWithGoogle([NotNull] GenerateTokenWithGoogleRequestViewModel request)
        {
            try
            {
                var authenticateResult = await identityService.Value.AuthenticateAsync(new AuthenticationRequestDto
                {
                    Username = request.Code!,
                    AuthenticationProvider = AuthenticationProvider.Google,
                });
                if (authenticateResult.Data?.User is null)
                {
                    return Ok<GenerateTokenResponseViewModel>(new(authenticateResult.Errors));
                }

                var result = await identityService.Value.GenerateUserTokenAsync(new GenerateUserTokenRequestDto
                {
                    UserId = authenticateResult.Data.User.Id,
                    TokenProvider = PermissionConstants.ApiDataProtectorTokenProvider,
                    Purpose = PermissionConstants.ApiDataProtectorTokenProviderAccessToken,
                });
                return Ok<GenerateTokenResponseViewModel>(new(result.Errors)
                {
                    Data = new()
                    {
                        Token = result.Data?.Token,
                        ExpirationTime = result.Data?.ExpirationTime,
                    }
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<GenerateTokenResponseViewModel>(new(new Error { Message = exc.Message }));
            }
        }

        [HttpPost("tokens/revoke"), Produces(typeof(ApiResponse<RevokeTokenResponseViewModel>))]
        [Permission(policy: null)]
        public async Task<IActionResult<RevokeTokenResponseViewModel>> RevokeToken()
        {
            try
            {
                var result = await identityService.Value.RemoveUserTokenAsync(new RemoveUserTokenRequestDto
                {
                    UserId = User.UserId(),
                    TokenProvider = PermissionConstants.ApiDataProtectorTokenProvider,
                    Purpose = PermissionConstants.ApiDataProtectorTokenProviderAccessToken,
                });

                return Ok<RevokeTokenResponseViewModel>(new(result.Errors)
                {
                    Data = new()
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<RevokeTokenResponseViewModel>(new(new Error { Message = exc.Message }));
            }
        }

        [HttpGet("authenticated"), Produces(typeof(ApiResponse<bool>))]
        public IActionResult<bool> Authenticated()
        {
            try
            {
                return Ok<bool>(new()
                {
                    Data = User.Identity?.IsAuthenticated is true,
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<bool>(new(new Error { Message = exc.Message }));
            }
        }

        [HttpGet("profiles"), Produces(typeof(ApiResponse<ProfileSettingsResponseViewModel>))]
        [Permission(policy: null)]
        public async Task<IActionResult<ProfileSettingsResponseViewModel>> GetProfileSettings()
        {
            try
            {
                var result = await identityService.Value.GetProfileSettingsAsync();

                return Ok<ProfileSettingsResponseViewModel>(new(result.Errors)
                {
                    Data = new()
                    {
                        TimeZoneId = result.Data?.TimeZoneId,
                    },
                });
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<ProfileSettingsResponseViewModel>(new(new Error { Message = exc.Message }));
            }
        }

        [HttpPut("profiles"), Produces(typeof(ApiResponse<Void>))]
        [Permission(policy: null)]
        public async Task<IActionResult<Void>> UpdateProfileSettings([NotNull] ProfileSettingsRequestViewModel request)
        {
            try
            {
                var result = await identityService.Value.UpdateProfileSettingsAsync(new ProfileSettingsDto
                {
                    TimeZoneId = request.TimeZoneId,
                });

                return Ok<Void>(new(result.Errors));
            }
            catch (Exception exc)
            {
                Logger.Value.LogException(exc);

                return Ok<Void>(new(new Error { Message = exc.Message }));
            }
        }
    }
}
