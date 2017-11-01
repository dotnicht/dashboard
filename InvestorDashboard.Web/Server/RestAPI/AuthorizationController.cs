using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using AutoMapper;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Services;
using InvestorDashboard.Web.Models;
using InvestorDashboard.Web.Models.AccountViewModels;
using InvestorDashboard.Web.Server.Helpers;
using InvestorDashboard.Web.Server.Models.AccountViewModels;
using InvestorDashboard.Web.Server.Models.AuthorizationViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Core;
using OpenIddict.Models;

namespace InvestorDashboard.Web.Server.RestAPI
{
    [Route("[controller]/[action]")]
    public class AuthorizationController : Controller
    {
        private readonly OpenIddictApplicationManager<OpenIddictApplication> _applicationManager;
        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger _logger;
        private readonly IEnumerable<ICryptoService> _cryptoServices;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public AuthorizationController(
          OpenIddictApplicationManager<OpenIddictApplication> applicationManager,
          IOptions<IdentityOptions> identityOptions,
          SignInManager<ApplicationUser> signInManager,
          UserManager<ApplicationUser> userManager,
          ILogger<AuthorizationController> loger,
          IEnumerable<ICryptoService> cryptoServices,
          IMapper mapper,
          IEmailService emailService)
        {
            _applicationManager = applicationManager;
            _identityOptions = identityOptions;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = loger;
            _cryptoServices = cryptoServices ?? throw new ArgumentNullException(nameof(cryptoServices));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        [HttpPost("~/connect/register"), Produces("application/json")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel user)
        {
            try
            {
                //user.FirstName = "FirstName";
                //user.LastName = "LastName";
                user.UserName = user.Email;
                //user.Address = "Address";
                //user.PhoneCode = "+380";
                //user.PhoneNumber = "00 000 0000";
                //user.IsEligibleForTokenSale = true;
                //user.CountryCode = "UKR".ToUpper();
                //user.City = "Boston".ToUpper();
                user.IsEnabled = true;

                ApplicationUser appUser = _mapper.Map<ApplicationUser>(user);
                var result = await _userManager.CreateAsync(appUser, user.Password);

                if (result.Succeeded)
                {
                    Parallel.ForEach(_cryptoServices, async x => await x.UpdateUserDetails(appUser.Id));

                    // TODO: replace token with URI.
                    appUser = await _userManager.FindByEmailAsync(appUser.Email);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
                    code = System.Web.HttpUtility.UrlEncode(code);
                    await _emailService.SendEmailConfirmationAsync(appUser.Email, $"{Request.Scheme}://{Request.Host}/connect/confirm_email?userId={appUser.Id}&code={code}");

                    return Ok();
                }
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = "user_exist"
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
        [HttpGet]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                // throw new ApplicationException($"Unable to load two-factor authentication user.");
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = "The username/password couple is invalid."
                });
            }

            var model = new LoginWith2faViewModel { RememberMe = rememberMe };
            ViewData["ReturnUrl"] = returnUrl;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                var errorrs = string.Empty;
                foreach (var values in ModelState.Values)
                {
                    foreach (var error in values.Errors)
                    {
                        errorrs += $"{error};";
                    }
                }
                return Ok(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = errorrs
                });
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result =
              await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} logged in with 2fa.", user.Id);
                return Ok();
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
                return Ok(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = "User with ID {UserId} account locked out."
                });
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID {UserId}.", user.Id);
                return Ok(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = "Invalid authenticator code."
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
            request.Username = System.Web.HttpUtility.UrlDecode(request.Username);
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
                    //if(!user.EmailConfirmed){
                    //    return BadRequest(new OpenIdConnectResponse
                    //    {
                    //        Error = OpenIdConnectConstants.Errors.AccessDenied,
                    //        ErrorDescription = "The username/password couple is invalid."
                    //    });
                    //}
                    // Validate the username/password parameters and ensure the account is not locked out.
                    var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
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
            catch
            {
                return null;
            }
        }
        #endregion

        private async Task<AuthenticationTicket> CreateTicketAsync(
          OpenIdConnectRequest request, ApplicationUser user,
              AuthenticationProperties properties = null)
        {
            // Create a new ClaimsPrincipal containing the claims that
            // will be used to create an id_token, a token or a code.
            var principal = await _signInManager.CreateUserPrincipalAsync(user);

            // Create a new authentication ticket holding the user identity.
            var ticket = new AuthenticationTicket(principal, properties,
              OpenIdConnectServerDefaults.AuthenticationScheme);

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
                    || (claim.Type == OpenIdConnectConstants.Claims.Role && ticket.HasScope(OpenIddictConstants.Claims.Roles)) ||
                    (claim.Type == CustomClaimTypes.Permission && ticket.HasScope(OpenIddictConstants.Claims.Roles)))
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
            return ticket;
        }

        [HttpGet("~/connect/isauthorization")]
        public IActionResult IsAuthorization()
        {
            return Json(User.Identity.IsAuthenticated);
        }

        [HttpGet("~/connect/confirm_email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.AccessDenied,
                    ErrorDescription = ""
                });
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.AccessDenied,
                    ErrorDescription = ""
                });
                throw new ApplicationException($"Unable to load user with ID '{userId}'.");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                return RedirectPermanent("/");
            }
            else
            {
                var errors = "";
                foreach(var e in result.Errors){
                    errors += e.Description;
                }
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = errors
                });
            }
        }
    }
}
