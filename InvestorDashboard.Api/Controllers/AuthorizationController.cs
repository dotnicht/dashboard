using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using AutoMapper;
using Hangfire;
using InvestorDashboard.Api.Helpers;
using InvestorDashboard.Api.Models;
using InvestorDashboard.Api.Models.AccountViewModels;
using InvestorDashboard.Api.Models.AuthorizationViewModels;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OpenIddict.Core;
using OpenIddict.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace InvestorDashboard.Api.Controllers
{
    [Route("[controller]/[action]")]
    public class AuthorizationController : Controller
    {
        private readonly OpenIddictApplicationManager<OpenIddictApplication> _applicationManager;
        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly IOptions<CaptchaSettings> _captchaOptions;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IMessageService _messageService;
        private readonly IReferralService _referralService;
        private readonly IGenericAddressService _genericAddressService;
        private readonly IKycService _kycService;
        private readonly IAffiliateService _affiliateService;
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public AuthorizationController(
            OpenIddictApplicationManager<OpenIddictApplication> applicationManager,
            IOptions<IdentityOptions> identityOptions,
            IOptions<CaptchaSettings> captchaOptions,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<AuthorizationController> logger,
            IMapper mapper,
            IMessageService messageService,
            IReferralService referralService,
            IGenericAddressService genericAddressService,
            IKycService kycService,
            IAffiliateService affiliateService,
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _applicationManager = applicationManager ?? throw new ArgumentNullException(nameof(applicationManager));
            _identityOptions = identityOptions ?? throw new ArgumentNullException(nameof(identityOptions));
            _captchaOptions = captchaOptions ?? throw new ArgumentNullException(nameof(captchaOptions));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            _referralService = referralService ?? throw new ArgumentNullException(nameof(referralService));
            _genericAddressService = genericAddressService ?? throw new ArgumentNullException(nameof(genericAddressService));
            _kycService = kycService ?? throw new ArgumentNullException(nameof(kycService));
            _affiliateService = affiliateService ?? throw new ArgumentNullException(nameof(affiliateService));
            _viewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
            _tempDataProvider = tempDataProvider ?? throw new ArgumentNullException(nameof(tempDataProvider));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        [AllowAnonymous, HttpPost("~/connect/register"), Produces("application/json")]
        public async Task<IActionResult> Register([FromBody]RegisterViewModel user)
        {
            try
            {
                var client = new WebClient();
                var address = $"https://www.google.com/recaptcha/api/siteverify?secret={_captchaOptions.Value.GoogleSecretKey}&response={user.ReCaptchaToken}";
                var recaptchaResult = client.DownloadString(address);
                var obj = JObject.Parse(recaptchaResult);
                var status = (bool)obj.SelectToken("success");

                if (!status)
                {
                    address = $"https://dp-captcha2.azurewebsites.net/api/CaptchaApi/IsApproved?captchaid={user.ReCaptchaToken}";
                    recaptchaResult = client.DownloadString(address);
                    obj = JObject.Parse(recaptchaResult);
                    status = (bool)obj.SelectToken("IsApproved");
                }

                if (status)
                {
                    user.UserName = user.Email;
                    var appUser = _mapper.Map<ApplicationUser>(user);
                    var result = await _userManager.CreateAsync(appUser, user.Password);

                    if (result.Succeeded)
                    {
                        BackgroundJob.Enqueue(() => _referralService.PopulateReferralData(appUser.Id, user.Referral));
                        BackgroundJob.Enqueue(() => _affiliateService.NotifyUserRegistered(appUser.Id));
                        BackgroundJob.Enqueue(() => _kycService.UpdateKycTransactions(appUser.Id));

                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
                        var emailBody = Render("EmailBody", $"{Request.Scheme}://{Request.Host}/api/connect/confirm_email?userId={appUser.Id}&code={HttpUtility.UrlEncode(code)}");
                        await _messageService.SendRegistrationConfirmationRequiredMessage(appUser.Id, emailBody);

                        return Ok();
                    }

                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = "user_exist"
                    });
                }

                return BadRequest(new OpenIdConnectResponse
                {
                    ErrorDescription = "Invalid captcha"
                });
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

        [HttpPost("~/connect/resend_email_confirm_code"), Produces("application/json")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendEmailConfirmCode([FromBody]EmailViewModel model)
        {
            try
            {
                var appUser = await _userManager.FindByEmailAsync(model.Email);
                if (appUser != null)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
                    var emailBody = Render("EmailBody", $"{Request.Scheme}://{Request.Host}/api/connect/confirm_email?userId={appUser.Id}&code={HttpUtility.UrlEncode(code)}");
                    await _messageService.SendRegistrationConfirmationRequiredMessage(appUser.Id, emailBody);

                    return Ok();
                }

                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = "The username/password couple is invalid."
                });
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

        #region Authorization code, implicit and implicit flows

        // Note: to support interactive flows like the code flow,
        // you must provide your own authorization endpoint action:
        //[HttpGet]
        //public async Task<IActionResult> LoginWith2fa()
        //{
        //    // Ensure the user has gone through the username & password screen first
        //    var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        //    if (user == null)
        //    {
        //        // throw new ApplicationException($"Unable to load two-factor authentication user.");
        //        return BadRequest(new OpenIdConnectResponse
        //        {
        //            Error = OpenIdConnectConstants.Errors.InvalidGrant,
        //            ErrorDescription = "The username/password couple is invalid."
        //        });
        //    }

        //    return Ok();
        //}

        [HttpPost("~/connect/login2fa"), Produces("application/json")]
        public async Task<IActionResult> LoginWith2fa([FromBody] LoginWith2faViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errorrs = string.Empty;
                foreach (var values in ModelState.Values)
                {
                    foreach (var error in values.Errors)
                    {
                        errorrs += $"{error.ErrorMessage};";
                    }
                }
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = errorrs
                });
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = $"Unable to load user with ID '{_userManager.GetUserId(User)}'."
                });
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            // var result =              await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, false, false);
            var result = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, authenticatorCode);

            if (result)
            {
                _logger.LogInformation("User with ID {UserId} logged in with 2fa.", user.Id);
                return Ok();
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID {UserId}.", user.Id);
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = "Invalid authenticator code."
                });

            }
        }
        [HttpPost("~/connect/login_with_recovery_code"), Produces("application/json")]
        public async Task<IActionResult> LoginWithRecoveryCode([FromBody]LoginWithRecoveryCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errorrs = string.Empty;
                foreach (var values in ModelState.Values)
                {
                    foreach (var error in values.Errors)
                    {
                        errorrs += $"{error.ErrorMessage};";
                    }
                }
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = errorrs
                });
            }

            var user = _userManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = "Unable to load two-factor authentication user."
                });
            }

            var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);
            var recoveryCodeLe = await _userManager.CountRecoveryCodesAsync(await user);

            var result = await _userManager.RedeemTwoFactorRecoveryCodeAsync(await user, recoveryCode);

            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = "Invalid recovery code"
                });
            }
        }

        [Authorize, HttpGet("~/connect/authorize")]
        public async Task<IActionResult> Authorize(OpenIdConnectRequest request)
        {
            Debug.Assert(request.IsAuthorizationRequest(),
              "The OpenIddict binder for ASP.NET Core MVC is not registered. " +
              "Make sure services.AddOpenIddict().AddMvcBinders() is correctly called.");

            // Retrieve the application details from the database.
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId, HttpContext.RequestAborted);
            if (application == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = "Details concerning the calling client application cannot be found in the database"
                });
            }

            // Flow the request_id to allow OpenIddict to restore
            // the original authorization request from the cache.
            return View(new AuthorizeViewModel
            {
                ApplicationName = application.DisplayName,
                RequestId = request.RequestId,
                Scope = request.Scope
            });
        }

        [Authorize, FormValueRequired("submit.Accept")]
        [HttpPost("~/connect/authorize"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(OpenIdConnectRequest request)
        {
            Debug.Assert(request.IsAuthorizationRequest(),
              "The OpenIddict binder for ASP.NET Core MVC is not registered. " +
              "Make sure services.AddOpenIddict().AddMvcBinders() is correctly called.");

            // Retrieve the profile of the logged in user.
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = "An internal error has occurred"
                });
            }

            // Create a new authentication ticket.
            var ticket = await CreateTicketAsync(request, user);

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

        [Authorize, FormValueRequired("submit.Deny")]
        [HttpPost("~/connect/authorize"), ValidateAntiForgeryToken]
        public IActionResult Deny()
        {
            // Notify OpenIddict that the authorization grant has been denied by the resource owner
            // to redirect the user agent to the client application using the appropriate response_mode.
            return Forbid(OpenIdConnectServerDefaults.AuthenticationScheme);
        }

        // Note: the logout action is only useful when implementing interactive
        // flows like the authorization code flow or the implicit flow.

        [HttpGet("~/connect/logout")]
        public IActionResult Logout(OpenIdConnectRequest request)
        {
            Debug.Assert(request.IsLogoutRequest(),
              "The OpenIddict binder for ASP.NET Core MVC is not registered. " +
              "Make sure services.AddOpenIddict().AddMvcBinders() is correctly called.");

            // Flow the request_id to allow OpenIddict to restore
            // the original logout request from the distributed cache.
            return View(new LogoutViewModel
            {
                RequestId = request.RequestId
            });
        }

        [HttpPost("~/connect/logout"), Produces("application/json")]
        public async Task<IActionResult> Logout()
        {
            // Ask ASP.NET Core Identity to delete the local and external cookies created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow (e.g Google or Facebook).
            await _signInManager.SignOutAsync();

            // Returning a SignOutResult will ask OpenIddict to redirect the user agent
            // to the post_logout_redirect_uri specified by the client application.
            return SignOut(OpenIdConnectServerDefaults.AuthenticationScheme);
        }

        #endregion

        #region Password, authorization code and refresh token flows

        // Note: to support non-interactive flows like password,
        // you must provide your own token endpoint action:
        [HttpPost("~/connect/token"), Produces("application/json")]
        public async Task<IActionResult> Exchange(OpenIdConnectRequest request)
        {
            Debug.Assert(request.IsTokenRequest(),
              "The OpenIddict binder for ASP.NET Core MVC is not registered. " +
              "Make sure services.AddOpenIddict().AddMvcBinders() is correctly called.");
            request.Username = HttpUtility.UrlDecode(request.Username);
            try
            {
                if (request.IsPasswordGrantType())
                {
                    var user = await _userManager.FindByEmailAsync(request.Username) ?? await _userManager.FindByNameAsync(request.Username);
                    if (user == null)
                    {
                        return BadRequest(new OpenIdConnectResponse
                        {
                            Error = OpenIdConnectConstants.Errors.InvalidGrant,
                            ErrorDescription = "The username/password couple is invalid."
                        });
                    }
                    if (!user.EmailConfirmed)
                    {
                        return BadRequest(new OpenIdConnectResponse
                        {
                            Error = "confirm_email",
                            ErrorDescription = "You must confirm your email."
                        });
                    }
                    if (user.LockoutEnd != null)
                    {
                        return BadRequest(new OpenIdConnectResponse
                        {
                            Error = OpenIdConnectConstants.Errors.InvalidGrant,
                            ErrorDescription = $"Too many login attempts. Try again {user.LockoutEnd.Value.ToString("G")} "
                        });
                    }
                    // Validate the username/password parameters and ensure the account is not locked out.
                    var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
                    // var result = await _signInManager.PasswordSignInAsync(request.Username, request.Password, false, lockoutOnFailure: true);
                    if (!result.Succeeded)
                    {
                        return BadRequest(new OpenIdConnectResponse
                        {
                            Error = OpenIdConnectConstants.Errors.InvalidGrant,
                            ErrorDescription = "The username/password couple is invalid."
                        });
                    }


                    if (result.RequiresTwoFactor)
                    {
                        return BadRequest(new OpenIdConnectResponse
                        {
                            Error = "need_2fa",
                            ErrorDescription = "Invalid login procedure"
                        });
                    }

                    // Create a new authentication ticket.
                    var ticket = await CreateTicketAsync(request, user);

                    return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
                }
                else if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
                {
                    // Retrieve the claims principal stored in the authorization code/refresh token.
                    var info = await HttpContext.AuthenticateAsync(OpenIdConnectServerDefaults.AuthenticationScheme);

                    // Retrieve the user profile corresponding to the authorization code/refresh token.
                    // Note: if you want to automatically invalidate the authorization code/refresh token
                    // when the user password/roles change, use the following line instead:
                    // var user = _signInManager.ValidateSecurityStampAsync(info.Principal);
                    var user = await _userManager.GetUserAsync(info.Principal);
                    if (user == null)
                    {
                        return BadRequest(new OpenIdConnectResponse
                        {
                            Error = OpenIdConnectConstants.Errors.InvalidGrant,
                            ErrorDescription = "The token is no longer valid."
                        });
                    }

                    // Ensure the user is still allowed to sign in.
                    if (!await _signInManager.CanSignInAsync(user))
                    {
                        return BadRequest(new OpenIdConnectResponse
                        {
                            Error = OpenIdConnectConstants.Errors.InvalidGrant,
                            ErrorDescription = "The user is no longer allowed to sign in."
                        });
                    }

                    // Create a new authentication ticket, but reuse the properties stored in the
                    // authorization code/refresh token, including the scopes originally granted.
                    var ticket = await CreateTicketAsync(request, user, info.Properties);

                    return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
                }

                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.UnsupportedGrantType,
                    ErrorDescription = "The specified grant type is not supported."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.UnsupportedGrantType,
                    ErrorDescription = ex.Message
                });
            }
        }

        #endregion

        private async Task<AuthenticationTicket> CreateTicketAsync(OpenIdConnectRequest request, ApplicationUser user, AuthenticationProperties properties = null)
        {
            // Create a new ClaimsPrincipal containing the claims that
            // will be used to create an id_token, a token or a code.
            var principal = await _signInManager.CreateUserPrincipalAsync(user);

            // Create a new authentication ticket holding the user identity.
            var ticket = new AuthenticationTicket(principal, properties, OpenIdConnectServerDefaults.AuthenticationScheme);

            //if (!request.IsAuthorizationCodeGrantType() && !request.IsRefreshTokenGrantType())
            //{
            // Set the list of scopes granted to the client application.
            // Note: the offline_access scope must be granted
            // to allow OpenIddict to return a refresh token.
            ticket.SetScopes(new[]
            {
                OpenIdConnectConstants.Scopes.OpenId,
                OpenIdConnectConstants.Scopes.Email,
                OpenIdConnectConstants.Scopes.Phone,
                OpenIdConnectConstants.Scopes.Profile,
                OpenIdConnectConstants.Scopes.OfflineAccess,
                OpenIddictConstants.Scopes.Roles
              }.Intersect(request.GetScopes()));
            //}

            ticket.SetResources(request.GetResources());

            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.

            foreach (var claim in ticket.Principal.Claims)
            {
                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                if (claim.Type == _identityOptions.Value.ClaimsIdentity.SecurityStampClaimType)
                {
                    continue;
                }

                var destinations = new List<string>
              {
                OpenIdConnectConstants.Destinations.AccessToken
              };

                // Only add the iterated claim to the id_token if the corresponding scope was granted to the client application.
                // The other claims will only be added to the access_token, which is encrypted when using the default format.
                if ((claim.Type == OpenIdConnectConstants.Claims.Name && ticket.HasScope(OpenIdConnectConstants.Scopes.Profile))
                    || (claim.Type == OpenIdConnectConstants.Claims.Email && ticket.HasScope(OpenIdConnectConstants.Scopes.Email))
                    || (claim.Type == OpenIdConnectConstants.Claims.Role && ticket.HasScope(OpenIddictConstants.Claims.Roles))
                    || (claim.Type == CustomClaimTypes.Permission && ticket.HasScope(OpenIddictConstants.Claims.Roles)))
                {
                    destinations.Add(OpenIdConnectConstants.Destinations.IdentityToken);
                }

                claim.SetDestinations(destinations);
            }

            var identity = principal.Identity as ClaimsIdentity;

            if (ticket.HasScope(OpenIdConnectConstants.Scopes.Profile))
            {
                if (!string.IsNullOrWhiteSpace(user.FirstName))
                    identity.AddClaim(CustomClaimTypes.JobTitle, user.FirstName, OpenIdConnectConstants.Destinations.IdentityToken);

                if (!string.IsNullOrWhiteSpace(user.LastName))
                    identity.AddClaim(CustomClaimTypes.FullName, user.LastName, OpenIdConnectConstants.Destinations.IdentityToken);

                if (!string.IsNullOrWhiteSpace(user.Configuration))
                    identity.AddClaim(CustomClaimTypes.Configuration, user.Configuration, OpenIdConnectConstants.Destinations.IdentityToken);
            }

            if (ticket.HasScope(OpenIdConnectConstants.Scopes.Email))
            {
                if (!string.IsNullOrWhiteSpace(user.Email))
                    identity.AddClaim(CustomClaimTypes.Email, user.Email, OpenIdConnectConstants.Destinations.IdentityToken);
            }

            if (ticket.HasScope(OpenIdConnectConstants.Scopes.Phone))
            {
                if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
                    identity.AddClaim(CustomClaimTypes.Phone, user.PhoneNumber, OpenIdConnectConstants.Destinations.IdentityToken);

            }

            identity.AddClaim(CustomClaimTypes.TwoFactorEnabled, user.TwoFactorEnabled.ToString().ToLower(), OpenIdConnectConstants.Destinations.IdentityToken);

            if (user.TwoFactorEnabled)
            {
                var b = await _signInManager.GetTwoFactorAuthenticationUserAsync();
                var tokenProvider = await _userManager.GetValidTwoFactorProvidersAsync(user);
                var s = await _userManager.GetTwoFactorEnabledAsync(user);
                var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
                var a = code;
            }
            return ticket;
        }

        [HttpGet("~/connect/isauthorization")]
        public IActionResult IsAuthorization()
        {
            return Json(true);
        }

        [HttpGet("~/connect/confirm_email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            var options = new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddMinutes(30)
            };

            if (userId == null || code == null)
            {
                Response.Cookies.Append("confirm_status", "invalid_parametrs", options);
                return RedirectPermanent("/email_confirmed");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                Response.Cookies.Append("confirm_status", "invalid_user", options);
                return RedirectPermanent("/email_confirmed");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                Response.Cookies.Append("confirm_status", "success", options);
                return RedirectPermanent("/email_confirmed");
            }
            else
            {
                var errors = "";
                foreach (var e in result.Errors)
                {
                    errors += e.Description;
                }
                Response.Cookies.Append("confirm_status", errors, options);
                return RedirectPermanent("/email_confirmed");
            }
        }

        private string Render<TModel>(string name, TModel model)
        {
            var httpContext = new DefaultHttpContext
            {
                RequestServices = _serviceProvider
            };

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            var viewEngineResult = _viewEngine.FindView(actionContext, name, false);

            if (!viewEngineResult.Success)
            {
                throw new InvalidOperationException(string.Format("Couldn't find view '{0}'", name));
            }

            var view = viewEngineResult.View;

            using (var output = new StringWriter())
            {
                var viewContext = new ViewContext(
                    actionContext,
                    view,
                    new ViewDataDictionary<TModel>(
                        metadataProvider: new EmptyModelMetadataProvider(),
                        modelState: new ModelStateDictionary())
                    {
                        Model = model
                    },
                    new TempDataDictionary(
                        actionContext.HttpContext,
                        _tempDataProvider),
                    output,
                    new HtmlHelperOptions());

                view.RenderAsync(viewContext).GetAwaiter().GetResult();

                return output.ToString();
            }
        }
    }
}
