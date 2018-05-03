using AspNet.Security.OpenIdConnect.Primitives;
using AutoMapper;
using InvestorDashboard.Api.Models;
using InvestorDashboard.Api.Models.AccountViewModels;
using InvestorDashboard.Api.Models.ManageViewModels;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;

namespace InvestorDashboard.Api.Controllers
{
    [Authorize, Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly IMessageService _messageService;
        private readonly IKycService _kycService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        private readonly UrlEncoder _urlEncoder;

        public AccountController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IAuthorizationService authorizationService,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger,
            IOptions<IdentityOptions> identityOptions,
            IMessageService messageService,
            IKycService kycService,
            IMapper mapper,
            UrlEncoder urlEncoder)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _identityOptions = identityOptions;
            _authorizationService = authorizationService;
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            _kycService = kycService ?? throw new ArgumentNullException(nameof(kycService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _urlEncoder = urlEncoder;
        }

        [TempData]
        public string ErrorMessage { get; set; }

        [HttpGet("users/me")]
        [Produces(typeof(UserViewModel))]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                ApplicationUser appUser = await _userManager.FindByNameAsync(User.Identity.Name);

                if (appUser == null)
                {
                    return NotFound(User.Identity.Name);
                }

                var userVM = _mapper.Map<UserViewModel>(appUser);
                userVM.Roles = new string[0];

                if (userVM != null)
                {
                    return Ok(userVM);
                }
                else
                {
                    return NotFound(appUser.Id);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = ex.Message
                });
            }
        }

        [HttpPut("users/me")]
        public async Task<IActionResult> UpdateCurrentUser([FromBody]UserEditViewModel user)
        {
            ApplicationUser appUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (user == null)
            {
                return BadRequest($"{nameof(user)} cannot be null");
            }

            if (appUser == null)
            {
                return NotFound(appUser.Id);
            }

            if (!string.IsNullOrWhiteSpace(user.Photo) && Convert.FromBase64String(user.Photo).Length > 1024 * 1024)
            {
                return Unauthorized();
            }

            var profile = _mapper.Map<UserProfile>(appUser);
            profile.UserId = appUser.Id;
            _context.UserProfiles.Add(profile);
            _context.SaveChanges();

            appUser.FirstName = user.FirstName;
            appUser.LastName = user.LastName;
            appUser.City = user.City;
            appUser.CountryCode = user.CountryCode;
            appUser.PhoneCode = user.PhoneCode;
            appUser.PhoneNumber = user.PhoneNumber;
            appUser.Photo = user.Photo;
            appUser.TelegramUsername = user.TelegramUsername;

            var result = await _userManager.UpdateAsync(appUser);

            if (result.Succeeded)
            {
                await _kycService.UpdateKycTransactions(appUser.Id);
                return Ok();
            }

            return BadRequest(ModelState);
        }

        [HttpPost("forgot_password"), Produces("application/json")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] EmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = "user_not_exist"
                    });
                }
                if (!(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = "user_not_confirmed"
                    });
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = HttpUtility.UrlEncode(code);

                var callbackUrl = $"{Request.Scheme}://{Request.Host}/api/account/reset_password?email={HttpUtility.UrlEncode(user.Email)}&code={code}";

                await _messageService.SendPasswordResetMessage(user.Id, $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
                return Ok();
                //return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // If we got this far, something failed, redisplay form
            // return View(model);
            return BadRequest(new OpenIdConnectResponse
            {
                Error = OpenIdConnectConstants.Errors.ServerError
            });
        }

        [HttpGet("reset_password"), Produces("application/json")]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email, string code)
        {
            var options = new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(1)
            };

            Response.Cookies.Append("reset_token", code, options);
            Response.Cookies.Append("reset_email", email, options);

            return RedirectPermanent("/reset_password");
        }

        [HttpPost("reset_password"), Produces("application/json")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Empty;
                foreach (var i in ModelState.Values)
                {
                    foreach (var e in i.Errors)
                    {
                        errors += e.ErrorMessage;
                    }
                }

                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = errors
                });
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.ServerError
                });
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                Response.Cookies.Delete("reset_token");
                Response.Cookies.Delete("reset_email");

                return Ok();
            }
            else
            {
                var errors = string.Empty;

                foreach (var e in result.Errors)
                {
                    errors += $"{e.Description}";
                }

                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = errors
                });
            }
        }

        [HttpPost("change_password"), Produces("application/json")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.ServerError,
                        ErrorDescription = $"Unable to load user with ID '{_userManager.GetUserId(User)}'."
                    });
                }

                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.Password);
                if (!changePasswordResult.Succeeded)
                {
                    var errors = string.Empty;
                    foreach (var e in changePasswordResult.Errors)
                    {
                        errors += $"{e.Description}";
                    }
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.ServerError,
                        ErrorDescription = errors
                    });
                }

                await _signInManager.SignInAsync(user, isPersistent: false);

                return Ok();
            }

            return BadRequest(new OpenIdConnectResponse
            {
                Error = OpenIdConnectConstants.Errors.ServerError
            });
        }

        [HttpGet("tfa"), Produces("application/json")]
        public async Task<IActionResult> TwoFactorAuthentication()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = $"Unable to load user with ID '{_userManager.GetUserId(User)}'."
                });
            }

            var model = new TwoFactorAuthenticationViewModel
            {
                HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null,
                Is2faEnabled = user.TwoFactorEnabled,
                RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user),
            };

            return Ok(model);
        }

        [HttpPost("tfa_reset"), Produces("application/json")]
        public async Task<IActionResult> ResetAuthenticator()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = $"Unable to load user with ID '{_userManager.GetUserId(User)}'."
                });
            }

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _userManager.ResetAuthenticatorKeyAsync(user);
            _logger.LogInformation("User with id '{UserId}' has reset their authentication app key.", user.Id);

            return Ok();
        }

        [HttpPost("tfa_disable"), Produces("application/json")]
        public async Task<IActionResult> Disable2fa()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = $"Unable to load user with ID '{_userManager.GetUserId(User)}'."
                });
            }

            var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = $"Unexpected error occured disabling 2FA for user with ID '{user.Id}'."
                });
            }

            _logger.LogInformation("User with ID {UserId} has disabled 2fa.", user.Id);
            return Ok();
        }

        [HttpGet("tfa_enable"), Produces("application/json")]
        public async Task<IActionResult> EnableAuthenticator()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.ServerError
                    });
                }

                var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
                if (string.IsNullOrEmpty(unformattedKey))
                {
                    await _userManager.ResetAuthenticatorKeyAsync(user);
                    unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
                }

                var result = new StringBuilder();
                int currentPosition = 0;
                while (currentPosition + 4 < unformattedKey.Length)
                {
                    result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                    currentPosition += 4;
                }

                if (currentPosition < unformattedKey.Length)
                {
                    result.Append(unformattedKey.Substring(currentPosition));
                }

                var model = new EnableAuthenticatorViewModel
                {
                    SharedKey = result
                        .ToString()
                        .ToLowerInvariant(),
                    AuthenticatorUri = string.Format(
                        "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
                        _urlEncoder.Encode("DataTradingTokenSaleDashboard"),
                        _urlEncoder.Encode(user.Email),
                        unformattedKey)
                };

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    ErrorDescription = ex.Message
                });
            }
        }

        [HttpPost("tfa_enable"), Produces("application/json")]
        public async Task<IActionResult> EnableAuthenticator([FromBody] EnableAuthenticatorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Empty;
                foreach (var i in ModelState.Values)
                {
                    foreach (var e in i.Errors)
                    {
                        errors += $"{e.ErrorMessage}";
                    }
                }
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = errors
                });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = $"Unable to load user with ID '{_userManager.GetUserId(User)}'."
                });

            }

            // Strip spaces and hypens
            var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = $"Verification code is invalid."
                });
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            return Ok();
        }

        [HttpGet("get_tf_recovery_codes"), Produces("application/json")]
        public async Task<IActionResult> GenerateRecoveryCodes()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = $"Unable to load user with ID '{_userManager.GetUserId(User)}'."
                });
            }

            if (!user.TwoFactorEnabled)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = $"Cannot generate recovery codes for user with ID '{user.Id}' as they do not have 2FA enabled."
                });
            }

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            var model = new GenerateRecoveryCodesViewModel { RecoveryCodes = recoveryCodes.ToArray() };


            return Ok(model);
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }

            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }
    }
}
