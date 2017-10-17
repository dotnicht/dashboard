

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using InvestorDashboard.Backend.Core.Interfaces;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Web.Controllers;
using InvestorDashboard.Web.Models.AccountViewModels;
using InvestorDashboard.Web.Server.Models;
using InvestorDashboard.Web.Server.Models.AccountViewModels;
using InvestorDashboard.Web.Server.Policies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InvestorDashboard.Web.Server.RestAPI
{
  [Authorize]
  [Route("api/[controller]")]
  public class AccountController : Controller
  {
    private readonly IAccountManager _accountManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IOptions<IdentityOptions> _identityOptions;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    //private readonly IEmailService _emailSender;
    private readonly ILogger _logger;
    private const string GetUserByIdActionName = "GetUserById";
    private const string GetRoleByIdActionName = "GetRoleById";

    public AccountController(
      UserManager<ApplicationUser> userManager,
      IAccountManager accountManager,
      IAuthorizationService authorizationService,
      SignInManager<ApplicationUser> signInManager,
      //IEmailService emailSender,
      ILogger<AccountController> logger,
      IOptions<IdentityOptions> identityOptions)
    {
      _userManager = userManager;
      _signInManager = signInManager;
      //_emailSender = emailSender;
      _logger = logger;
      _identityOptions = identityOptions;
      _accountManager = accountManager;
      _authorizationService = authorizationService;
    }

    [TempData]
    public string ErrorMessage { get; set; }

    [HttpGet("users/me")]
    [Produces(typeof(UserViewModel))]
    public async Task<IActionResult> GetCurrentUser()
    {
      return await GetUserByUserName(this.User.Identity.Name);
    }
    [HttpGet("users/username/{userName}")]
    [Produces(typeof(UserViewModel))]
    public async Task<IActionResult> GetUserByUserName(string userName)
    {
      ApplicationUser appUser = await _accountManager.GetUserByUserNameAsync(userName);

      //if (!(await _authorizationService.AuthorizeAsync(this.User, appUser?.Id ?? "", AuthPolicies.ViewUserByUserIdPolicy)).Succeeded)
      //  return new ChallengeResult();

      if (appUser == null)
        return NotFound(userName);

      return await GetUserById(appUser.Id);
    }
    [HttpGet("users/{id}", Name = GetUserByIdActionName)]
    [Produces(typeof(UserViewModel))]
    public async Task<IActionResult> GetUserById(string id)
    {
      //if (!(await _authorizationService.AuthorizeAsync(this.User, id, AuthPolicies.ViewUserByUserIdPolicy)).Succeeded)
      //  return new ChallengeResult();


      UserViewModel userVM = await GetUserViewModelHelper(id);

      if (userVM != null)
        return Ok(userVM);
      else
        return NotFound(id);
    }

    [HttpGet("roles/{id}", Name = GetRoleByIdActionName)]
    [Produces(typeof(RoleViewModel))]
    public async Task<IActionResult> GetRoleById(string id)
    {
      var appRole = await _accountManager.GetRoleByIdAsync(id);

      //if (!(await _authorizationService.AuthorizeAsync(this.User, appRole?.Name ?? "", AuthPolicies.ViewRoleByRoleNamePolicy)).Succeeded)
      //  return new ChallengeResult();

      if (appRole == null)
        return NotFound(id);

      return await GetRoleByName(appRole.Name);
    }




    [HttpGet("roles/name/{name}")]
    [Produces(typeof(RoleViewModel))]
    public async Task<IActionResult> GetRoleByName(string name)
    {
      //if (!(await _authorizationService.AuthorizeAsync(this.User, name, AuthPolicies.ViewRoleByRoleNamePolicy)).Succeeded)
      //  return new ChallengeResult();


      RoleViewModel roleVM = await GetRoleViewModelHelper(name);

      if (roleVM == null)
        return NotFound(name);

      return Ok(roleVM);
    }




    [HttpGet("roles")]
    [Produces(typeof(List<RoleViewModel>))]
    //[Authorize(AuthPolicies.ViewRolesPolicy)]
    public async Task<IActionResult> GetRoles()
    {
      return await GetRoles(-1, -1);
    }



    [HttpGet("roles/{page:int}/{pageSize:int}")]
    [Produces(typeof(List<RoleViewModel>))]
    //[Authorize(AuthPolicies.ViewRolesPolicy)]
    public async Task<IActionResult> GetRoles(int page, int pageSize)
    {
      var roles = await _accountManager.GetRolesLoadRelatedAsync(page, pageSize);
      return Ok(Mapper.Map<List<RoleViewModel>>(roles));
    }

    [HttpPost("~/register"), Produces("application/json")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterViewModel user)
    {
      try
      {
        user.FullName = "TestLogin";
        user.UserName = "TestName";
        user.IsEnabled = true;

        ApplicationUser appUser = Mapper.Map<ApplicationUser>(user);

        //var appUser = new ApplicationUser { UserName = model.Email, Email = model.Email };


        var result = await _accountManager.CreateUserAsync(appUser, new string[] { "user" }, user.Password);
        if (result.Item1)
        {
          return Json(new { message = "ok" });
        }
        return Json(new { error = "reg er" });
      }
      catch (Exception ex)
      {
        return Json(ex.Message);
      }
    }
    //[HttpPost("~/connect/token")]
    //[Produces("application/json")]
    //public async Task<IActionResult> Exchange(OpenIdConnectRequest request)
    //{
    //  if (request.IsPasswordGrantType())
    //  {
    //    var user = await _userManager.FindByEmailAsync(request.Username) ??
    //               await _userManager.FindByNameAsync(request.Username);
    //    if (user == null)
    //    {
    //      return BadRequest(new OpenIdConnectResponse
    //      {
    //        Error = OpenIdConnectConstants.Errors.InvalidGrant,
    //        ErrorDescription = "Please check that your email and password is correct"
    //      });
    //    }

    //    // Ensure the user is enabled.
    //    if (!user.LockoutEnabled)
    //    {
    //      return BadRequest(new OpenIdConnectResponse
    //      {
    //        Error = OpenIdConnectConstants.Errors.InvalidGrant,
    //        ErrorDescription = "The specified user account is disabled"
    //      });
    //    }

    //    // Validate the username/password parameters and ensure the account is not locked out.
    //    var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);

    //    // Ensure the user is not already locked out.
    //    if (result.IsLockedOut)
    //    {
    //      return BadRequest(new OpenIdConnectResponse
    //      {
    //        Error = OpenIdConnectConstants.Errors.InvalidGrant,
    //        ErrorDescription = "The specified user account has been suspended"
    //      });
    //    }

    //    // Reject the token request if two-factor authentication has been enabled by the user.
    //    if (result.RequiresTwoFactor)
    //    {
    //      return BadRequest(new OpenIdConnectResponse
    //      {
    //        Error = OpenIdConnectConstants.Errors.InvalidGrant,
    //        ErrorDescription = "Invalid login procedure"
    //      });
    //    }

    //    // Ensure the user is allowed to sign in.
    //    if (result.IsNotAllowed)
    //    {
    //      return BadRequest(new OpenIdConnectResponse
    //      {
    //        Error = OpenIdConnectConstants.Errors.InvalidGrant,
    //        ErrorDescription = "The specified user is not allowed to sign in"
    //      });
    //    }

    //    if (!result.Succeeded)
    //    {
    //      return BadRequest(new OpenIdConnectResponse
    //      {
    //        Error = OpenIdConnectConstants.Errors.InvalidGrant,
    //        ErrorDescription = "Please check that your email and password is correct"
    //      });
    //    }

    //    // Create a new authentication ticket.
    //    var ticket = await CreateTicketAsync(request, user);

    //    return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
    //  }
    //  else if (request.IsRefreshTokenGrantType())
    //  {
    //    // Retrieve the claims principal stored in the refresh token.
    //    var info = await HttpContext.AuthenticateAsync(OpenIdConnectServerDefaults.AuthenticationScheme);

    //    // Retrieve the user profile corresponding to the refresh token.
    //    // Note: if you want to automatically invalidate the refresh token
    //    // when the user password/roles change, use the following line instead:
    //    // var user = _signInManager.ValidateSecurityStampAsync(info.Principal);
    //    var user = await _userManager.GetUserAsync(info.Principal);
    //    if (user == null)
    //    {
    //      return BadRequest(new OpenIdConnectResponse
    //      {
    //        Error = OpenIdConnectConstants.Errors.InvalidGrant,
    //        ErrorDescription = "The refresh token is no longer valid"
    //      });
    //    }

    //    // Ensure the user is still allowed to sign in.
    //    if (!await _signInManager.CanSignInAsync(user))
    //    {
    //      return BadRequest(new OpenIdConnectResponse
    //      {
    //        Error = OpenIdConnectConstants.Errors.InvalidGrant,
    //        ErrorDescription = "The user is no longer allowed to sign in"
    //      });
    //    }

    //    // Create a new authentication ticket, but reuse the properties stored
    //    // in the refresh token, including the scopes originally granted.
    //    var ticket = await CreateTicketAsync(request, user);

    //    return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
    //  }
    //  return BadRequest(new OpenIdConnectResponse
    //  {
    //    Error = OpenIdConnectConstants.Errors.UnsupportedGrantType,
    //    ErrorDescription = "The specified grant type is not supported"
    //  });
    //}

    //private async Task<AuthenticationTicket> CreateTicketAsync(OpenIdConnectRequest request, ApplicationUser user)
    //{
    //  // Create a new ClaimsPrincipal containing the claims that
    //  // will be used to create an id_token, a token or a code.
    //  var principal = await _signInManager.CreateUserPrincipalAsync(user);

    //  // Create a new authentication ticket holding the user identity.
    //  var ticket = new AuthenticationTicket(principal, new AuthenticationProperties(),
    //    OpenIdConnectServerDefaults.AuthenticationScheme);

    //  //ticket.HasScope();

    //  //if (!request.IsRefreshTokenGrantType())
    //  //{
    //  // Set the list of scopes granted to the client application.
    //  // Note: the offline_access scope must be granted
    //  // to allow OpenIddict to return a refresh token.
    //  ticket.SetScopes(new[]
    //  {
    //    OpenIdConnectConstants.Scopes.OpenId,
    //    OpenIdConnectConstants.Scopes.Email,
    //    OpenIdConnectConstants.Scopes.Phone,
    //    OpenIdConnectConstants.Scopes.Profile,
    //    OpenIdConnectConstants.Scopes.OfflineAccess,
    //    //OpenIddictConstants.Scopes.Roles
    //  }.Intersect(request.GetScopes()));

    //  ticket.SetResources(request.GetResources());

    //  // Note: by default, claims are NOT automatically included in the access and identity tokens.
    //  // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
    //  // whether they should be included in access tokens, in identity tokens or in both.

    //  foreach (var claim in ticket.Principal.Claims)
    //  {
    //    // Never include the security stamp in the access and identity tokens, as it's a secret value.
    //    if (claim.Type == _identityOptions.Value.ClaimsIdentity.SecurityStampClaimType)
    //      continue;

    //    var destinations = new List<string> { OpenIdConnectConstants.Destinations.AccessToken };

    //    // Only add the iterated claim to the id_token if the corresponding scope was granted to the client application.
    //    // The other claims will only be added to the access_token, which is encrypted when using the default format.
    //    //if ((claim.Type == OpenIdConnectConstants.Claims.Subject &&
    //    //     ticket.HasScope(OpenIdConnectConstants.Scopes.OpenId)) ||
    //    //    (claim.Type == OpenIdConnectConstants.Claims.Name &&
    //    //     ticket.HasScope(OpenIdConnectConstants.Scopes.Profile)) ||
    //    //    (claim.Type == OpenIdConnectConstants.Claims.Role && ticket.HasScope(OpenIddictConstants.Claims.Roles)) ||
    //    //    (claim.Type == CustomClaimTypes.Permission && ticket.HasScope(OpenIddictConstants.Claims.Roles)))
    //    //{
    //    //  destinations.Add(OpenIdConnectConstants.Destinations.IdentityToken);
    //    //}

    //    claim.SetDestinations(destinations);
    //  }

    //  var identity = principal.Identity as ClaimsIdentity;

    //  if (ticket.HasScope(OpenIdConnectConstants.Scopes.Profile))
    //  {
    //    //if (!string.IsNullOrWhiteSpace(user.JobTitle))
    //    //  identity.AddClaim(CustomClaimTypes.JobTitle, user.JobTitle,
    //    //    OpenIdConnectConstants.Destinations.IdentityToken);

    //    //if (!string.IsNullOrWhiteSpace(user.FullName))
    //    //  identity.AddClaim(CustomClaimTypes.FullName, user.FullName,
    //    //    OpenIdConnectConstants.Destinations.IdentityToken);

    //    //if (!string.IsNullOrWhiteSpace(user.Configuration))
    //    //  identity.AddClaim(CustomClaimTypes.Configuration, user.Configuration,
    //    //    OpenIdConnectConstants.Destinations.IdentityToken);
    //  }

    //  //if (ticket.HasScope(OpenIdConnectConstants.Scopes.Email))
    //  //{
    //  //  if (!string.IsNullOrWhiteSpace(user.Email))
    //  //    identity.AddClaim(CustomClaimTypes.Email, user.Email, OpenIdConnectConstants.Destinations.IdentityToken);
    //  //}

    //  //if (ticket.HasScope(OpenIdConnectConstants.Scopes.Phone))
    //  //{
    //  //  if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
    //  //    identity.AddClaim(CustomClaimTypes.Phone, user.PhoneNumber,
    //  //      OpenIdConnectConstants.Destinations.IdentityToken);
    //  //}

    //  return ticket;
    //}

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string returnUrl = null)
    {
      // Clear the existing external cookie to ensure a clean login process
      await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

      ViewData["ReturnUrl"] = returnUrl;
      return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
      ViewData["ReturnUrl"] = returnUrl;
      if (ModelState.IsValid)
      {
        // This doesn't count login failures towards account lockout
        // To enable password failures to trigger account lockout, set lockoutOnFailure: true
        var result =
          await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe,
            lockoutOnFailure: false);
        if (result.Succeeded)
        {
          _logger.LogInformation("User logged in.");
          return RedirectToLocal(returnUrl);
        }
        if (result.RequiresTwoFactor)
        {
          return RedirectToAction(nameof(LoginWith2fa), new {returnUrl, model.RememberMe});
        }
        if (result.IsLockedOut)
        {
          _logger.LogWarning("User account locked out.");
          return RedirectToAction(nameof(Lockout));
        }
        else
        {
          ModelState.AddModelError(string.Empty, "Invalid login attempt.");
          return View(model);
        }
      }

      // If we got this far, something failed, redisplay form
      return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> LoginWith2fa(bool rememberMe, string returnUrl = null)
    {
      // Ensure the user has gone through the username & password screen first
      var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

      if (user == null)
      {
        throw new ApplicationException($"Unable to load two-factor authentication user.");
      }

      var model = new LoginWith2faViewModel {RememberMe = rememberMe};
      ViewData["ReturnUrl"] = returnUrl;

      return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, bool rememberMe, string returnUrl = null)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
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
        return RedirectToLocal(returnUrl);
      }
      else if (result.IsLockedOut)
      {
        _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
        return RedirectToAction(nameof(Lockout));
      }
      else
      {
        _logger.LogWarning("Invalid authenticator code entered for user with ID {UserId}.", user.Id);
        ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
        return View();
      }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> LoginWithRecoveryCode(string returnUrl = null)
    {
      // Ensure the user has gone through the username & password screen first
      var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
      if (user == null)
      {
        throw new ApplicationException($"Unable to load two-factor authentication user.");
      }

      ViewData["ReturnUrl"] = returnUrl;

      return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model,
      string returnUrl = null)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }

      var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
      if (user == null)
      {
        throw new ApplicationException($"Unable to load two-factor authentication user.");
      }

      var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);

      var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

      if (result.Succeeded)
      {
        _logger.LogInformation("User with ID {UserId} logged in with a recovery code.", user.Id);
        return RedirectToLocal(returnUrl);
      }
      if (result.IsLockedOut)
      {
        _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
        return RedirectToAction(nameof(Lockout));
      }
      else
      {
        _logger.LogWarning("Invalid recovery code entered for user with ID {UserId}", user.Id);
        ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
        return View();
      }
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Lockout()
    {
      return View();
    }

 
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
      await _signInManager.SignOutAsync();
      _logger.LogInformation("User logged out.");
      return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider, string returnUrl = null)
    {
      // Request a redirect to the external login provider.
      var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new {returnUrl});
      var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
      return Challenge(properties, provider);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
    {
      if (remoteError != null)
      {
        ErrorMessage = $"Error from external provider: {remoteError}";
        return RedirectToAction(nameof(Login));
      }
      var info = await _signInManager.GetExternalLoginInfoAsync();
      if (info == null)
      {
        return RedirectToAction(nameof(Login));
      }

      // Sign in the user with this external login provider if the user already has a login.
      var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey,
        isPersistent: false, bypassTwoFactor: true);
      if (result.Succeeded)
      {
        _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
        return RedirectToLocal(returnUrl);
      }
      if (result.IsLockedOut)
      {
        return RedirectToAction(nameof(Lockout));
      }
      else
      {
        // If the user does not have an account, then ask the user to create an account.
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["LoginProvider"] = info.LoginProvider;
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        return View("ExternalLogin", new ExternalLoginViewModel {Email = email});
      }
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string returnUrl = null)
    {
      if (ModelState.IsValid)
      {
        // Get the information about the user from the external login provider
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
          throw new ApplicationException("Error loading external login information during confirmation.");
        }
        var user = new ApplicationUser {UserName = model.Email, Email = model.Email};
        var result = await _userManager.CreateAsync(user);
        if (result.Succeeded)
        {
          result = await _userManager.AddLoginAsync(user, info);
          if (result.Succeeded)
          {
            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
            return RedirectToLocal(returnUrl);
          }
        }
        AddErrors(result);
      }

      ViewData["ReturnUrl"] = returnUrl;
      return View(nameof(ExternalLogin), model);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
      if (userId == null || code == null)
      {
        return RedirectToAction(nameof(HomeController.Index), "Home");
      }
      var user = await _userManager.FindByIdAsync(userId);
      if (user == null)
      {
        throw new ApplicationException($"Unable to load user with ID '{userId}'.");
      }
      var result = await _userManager.ConfirmEmailAsync(user, code);
      return View(result.Succeeded ? "ConfirmEmail" : "Error");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
      return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
      if (ModelState.IsValid)
      {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        {
          // Don't reveal that the user does not exist or is not confirmed
          return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        // For more information on how to enable account confirmation and password reset please
        // visit https://go.microsoft.com/fwlink/?LinkID=532713
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
       //var callbackUrl = Url.ResetPasswordCallbackLink(user.Id, code, Request.Scheme);
        //await _emailSender.SendEmailAsync(model.Email, "Reset Password",
        //  $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
        return RedirectToAction(nameof(ForgotPasswordConfirmation));
      }

      // If we got this far, something failed, redisplay form
      return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPasswordConfirmation()
    {
      return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string code = null)
    {
      if (code == null)
      {
        throw new ApplicationException("A code must be supplied for password reset.");
      }
      var model = new ResetPasswordViewModel {Code = code};
      return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }
      var user = await _userManager.FindByEmailAsync(model.Email);
      if (user == null)
      {
        // Don't reveal that the user does not exist
        return RedirectToAction(nameof(ResetPasswordConfirmation));
      }
      var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
      if (result.Succeeded)
      {
        return RedirectToAction(nameof(ResetPasswordConfirmation));
      }
      AddErrors(result);
      return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPasswordConfirmation()
    {
      return View();
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
      return View();
    }

    #region Helpers
    private async Task<UserViewModel> GetUserViewModelHelper(string userId)
    {
      var userAndRoles = await _accountManager.GetUserAndRolesAsync(userId);
      if (userAndRoles == null)
        return null;

      var userVM = Mapper.Map<UserViewModel>(userAndRoles.Item1);
      userVM.Roles = userAndRoles.Item2;

      return userVM;
    }
    private async Task<RoleViewModel> GetRoleViewModelHelper(string roleName)
    {
      var role = await _accountManager.GetRoleLoadRelatedAsync(roleName);
      if (role != null)
        return Mapper.Map<RoleViewModel>(role);


      return null;
    }
    private void AddErrors(IdentityResult result)
    {
      foreach (var error in result.Errors)
      {
        ModelState.AddModelError(string.Empty, error.Description);
      }
    }

    private IActionResult RedirectToLocal(string returnUrl)
    {
      if (Url.IsLocalUrl(returnUrl))
      {
        return Redirect(returnUrl);
      }
      else
      {
        return RedirectToAction(nameof(HomeController.Index), "Home");
      }
    }

    #endregion
  }
}
