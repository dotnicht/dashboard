

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using AutoMapper;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Web.Controllers;
using InvestorDashboard.Web.Models.AccountViewModels;
using InvestorDashboard.Web.Server.Models;
using InvestorDashboard.Web.Server.Models.AccountViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.JsonPatch;

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
            try
            {
                ApplicationUser appUser = await _accountManager.GetUserByUserNameAsync(this.User.Identity.Name);

                if (appUser == null)
                    return NotFound(this.User.Identity.Name);

                UserViewModel userVM = await GetUserViewModelHelper(appUser.Id);

                if (userVM != null)
                    return Ok(userVM);
                else
                    return NotFound(appUser.Id);
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

        [HttpGet("users/username/{userName}")]
        [Produces(typeof(UserViewModel))]
        public async Task<IActionResult> GetUserByUserName(string userName)
        {
            ApplicationUser appUser = await _userManager.FindByNameAsync(userName);
        [HttpPut("users/me")]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UserEditViewModel user)
        {
            ApplicationUser appUser = await _userManager.FindByNameAsync(User.Identity.Name);

            if (ModelState.IsValid)
            {
                if (user == null)
                    return BadRequest($"{nameof(user)} cannot be null");


                if (appUser == null)
                    return NotFound(appUser.Id);
                    Mapper.Map<UserViewModel, ApplicationUser>(user, appUser);

                    var result = await _userManager.UpdateAsync(appUser);

                return Ok();
                
            }

            return BadRequest(ModelState);
        }
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<IActionResult> LoginWithRecoveryCode(string returnUrl = null)
        //{
        //  // Ensure the user has gone through the username & password screen first
        //  var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        //  if (user == null)
        //  {
        //    throw new ApplicationException($"Unable to load two-factor authentication user.");
        //  }

        //  ViewData["ReturnUrl"] = returnUrl;

        //  return View();
        //}

        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model,
        //  string returnUrl = null)
        //{
        //  if (!ModelState.IsValid)
        //  {
        //    return View(model);
        //  }

        //  var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        //  if (user == null)
        //  {
        //    throw new ApplicationException($"Unable to load two-factor authentication user.");
        //  }

        //  var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);

        //  var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

        //  if (result.Succeeded)
        //  {
        //    _logger.LogInformation("User with ID {UserId} logged in with a recovery code.", user.Id);
        //    return RedirectToLocal(returnUrl);
        //  }
        //  if (result.IsLockedOut)
        //  {
        //    _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
        //    return RedirectToAction(nameof(Lockout));
        //  }
        //  else
        //  {
        //    _logger.LogWarning("Invalid recovery code entered for user with ID {UserId}", user.Id);
        //    ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
        //    return View();
        //  }
        //}


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
            var model = new ResetPasswordViewModel { Code = code };
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

        #region Helpers
        private async Task<UserViewModel> GetUserViewModelHelper(string userId)
        {
            var userVM = Mapper.Map<UserViewModel>(_context.Users.SingleOrDefault());
            userVM.Roles = new string[0];

            return userVM;
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
